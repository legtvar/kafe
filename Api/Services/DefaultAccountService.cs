using Kafe.Api.Options;
using Kafe.Api.Transfer;
using Kafe.Data;
using Kafe.Data.Aggregates;
using Kafe.Data.Capabilities;
using Kafe.Data.Events;
using Marten;
using Marten.Events;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Kafe.Api.Services;

public class DefaultAccountService : IAccountService, IDisposable
{
    public const string EmailConfirmationPurpose = "EmailConfirmation";
    public static readonly TimeSpan ConfirmationTokenExpiration = TimeSpan.FromHours(24);

    private readonly RandomNumberGenerator rng = RandomNumberGenerator.Create();
    private readonly IDataProtector dataProtector;
    private readonly IDocumentSession db;
    private readonly IEmailService emailService;
    private readonly IHostEnvironment environment;
    private readonly IOptions<ApiOptions> apiOptions;

    public DefaultAccountService(
        IDocumentSession db,
        IDataProtectionProvider dataProtectionProvider,
        IEmailService emailService,
        IHostEnvironment environment,
        IOptions<ApiOptions> apiOptions)
    {
        dataProtector = dataProtectionProvider.CreateProtector(nameof(DefaultAccountService));
        this.db = db;
        this.emailService = emailService;
        this.environment = environment;
        this.apiOptions = apiOptions;
    }

    public async Task<AccountDetailDto?> Load(Hrib id, CancellationToken token = default)
    {
        var account = await db.LoadAsync<AccountInfo>(id, token);
        if (account is null)
        {
            return null;
        }

        var projects = await db.LoadManyAsync<ProjectInfo>(
            token,
            account.Capabilities.OfType<ProjectOwnership>().Select(c => (string)c.ProjectId));
        if (projects is null)
        {
            return null;
        }

        return TransferMaps.ToAccountDetailDto(account, projects);
    }

    public async Task<AccountDetailDto?> Load(string emailAddress, CancellationToken token = default)
    {
        // TODO: Dedupe with the overload above.

        var account = await db.Query<AccountInfo>()
            .SingleOrDefaultAsync(a => a.EmailAddress == emailAddress, token);

        if (account is null)
        {
            return null;
        }

        var projects = await db.LoadManyAsync<ProjectInfo>(
            token,
            account.Capabilities.OfType<ProjectOwnership>().Select(c => (string)c.ProjectId));
        if (projects is null)
        {
            return null;
        }

        return TransferMaps.ToAccountDetailDto(account, projects);
    }

    public async Task<ApiUser?> LoadApiAccount(Hrib id, CancellationToken token = default)
    {
        var account = await db.LoadAsync<AccountInfo>(id, token);
        if (account is null)
        {
            return null;
        }
        return ApiUser.FromAggregate(account);
    }

    public async Task<ApiUser?> LoadApiAccount(string emailAddress, CancellationToken token = default)
    {
        // TODO: Dedupe with the overload above.

        var account = await db.Query<AccountInfo>()
            .SingleOrDefaultAsync(a => a.EmailAddress == emailAddress, token);

        if (account is null)
        {
            return null;
        }

        return ApiUser.FromAggregate(account);
    }

    public async Task<Hrib> CreateTemporaryAccount(
        TemporaryAccountCreationDto dto,
        CancellationToken token = default)
    {
        var account = await db.Query<AccountInfo>()
            .SingleOrDefaultAsync(a => a.EmailAddress == dto.EmailAddress, token);
        Hrib? id;
        if (account is null)
        {
            id = Hrib.Create();
            var created = new TemporaryAccountCreated(
                AccountId: id,
                CreationMethod: CreationMethod.Api,
                EmailAddress: dto.EmailAddress,
                PreferredCulture: dto.PreferredCulture ?? Const.InvariantCultureCode);
            db.Events.StartStream<AccountInfo>(id, created);
            await db.SaveChangesAsync(token);
            account = await db.LoadAsync<AccountInfo>(id, token)!;
        }
        else
        {
            id = account.Id;
        }

        var eventStream = await db.Events.FetchForExclusiveWriting<AccountInfo>(id, token)
            ?? throw new InvalidOperationException($"Could not obtain the event stream for account '{id}' " +
                "despite the account existing.");
        var refreshed = new TemporaryAccountRefreshed(
            AccountId: id,
            SecurityStamp: Guid.NewGuid().ToString());
        eventStream.AppendOne(refreshed);
        await db.SaveChangesAsync(token);

        var confirmationToken = EncodeToken(new(id, EmailConfirmationPurpose, refreshed.SecurityStamp));
        var pathString = new PathString(apiOptions.Value.AccountConfirmPath)
            .Add(new PathString("/" + confirmationToken));
        var confirmationUrl = new Uri(new Uri(apiOptions.Value.BaseUrl), pathString);
        var emailSubject = Const.ConfirmationEmailSubject[account!.PreferredCulture]!;
        var emailMessage = string.Format(
            Const.ConfirmationEmailMessageTemplate[account!.PreferredCulture]!,
            confirmationUrl,
            Const.EmailSignOffs[RandomNumberGenerator.GetInt32(0, Const.EmailSignOffs.Length)][account!.PreferredCulture]);
        await emailService.SendEmail(account.EmailAddress, emailSubject, emailMessage, token);

        return id;
    }

    public async Task ConfirmTemporaryAccount(
        TemporaryAccountTokenDto dto,
        CancellationToken token = default)
    {
        if (dto.Purpose != EmailConfirmationPurpose)
        {
            throw new UnauthorizedAccessException("The token is meant for a different purpose.");
        }

        var account = await db.LoadAsync<AccountInfo>(dto.AccountId, token);
        if (account is null)
        {
            throw new UnauthorizedAccessException("The account does not exist.");
        }

        if (account.RefreshedOn + ConfirmationTokenExpiration < DateTimeOffset.UtcNow)
        {
            var closedExpired = new TemporaryAccountClosed(account.Id);
            db.Events.Append(account.Id, closedExpired);
            await db.SaveChangesAsync(token);
            throw new UnauthorizedAccessException("The token has expired.");
        }

        if (account.SecurityStamp != dto.SecurityStamp)
        {
            throw new UnauthorizedAccessException("The token has been already used or revoked.");
        }

        var closedSuccessfully = new TemporaryAccountClosed(account.Id);
        db.Events.Append(account.Id, closedSuccessfully);
        await db.SaveChangesAsync(token);
    }

    public async Task AddCapabilities(
        Hrib id,
        IEnumerable<AccountCapability> capabilities,
        CancellationToken token = default)
    {
        // TODO: Find a cheaper way of knowing that an account exists.
        var account = await db.LoadAsync<AccountInfo>(id, token);
        if (account is null)
        {
            throw new ArgumentOutOfRangeException(nameof(id));
        }

        var eventStream = await db.Events.FetchForExclusiveWriting<AccountInfo>(account.Id, token);
        eventStream.AppendMany(capabilities.Select(c => new AccountCapabilityAdded(account.Id, c)));
        await db.SaveChangesAsync(token);
    }

    public string EncodeToken(TemporaryAccountTokenDto dto)
    {
        var token = $"{dto.Purpose}:{dto.AccountId}:{dto.SecurityStamp}";
        var protectedBytes = dataProtector.Protect(Encoding.UTF8.GetBytes(token));
        return WebEncoders.Base64UrlEncode(protectedBytes);
    }

    public bool TryDecodeToken(string encodedToken, [NotNullWhen(true)] out TemporaryAccountTokenDto? dto)
    {
        try
        {
            var unprotectedBytes = dataProtector.Unprotect(WebEncoders.Base64UrlDecode(encodedToken));
            var token = Encoding.UTF8.GetString(unprotectedBytes);
            var fields = token.Split(':', 3);
            if (fields.Length != 3)
            {
                dto = null;
                return false;
            }

            dto = new(Purpose: fields[0], AccountId: fields[1], SecurityStamp: fields[2]);
            return true;
        }
        catch (Exception)
        {
            dto = null;
            return false;
        }
    }

    public void Dispose()
    {
        ((IDisposable)rng).Dispose();
        GC.SuppressFinalize(this);
    }
}
