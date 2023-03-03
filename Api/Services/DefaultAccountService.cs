using Kafe.Api.Transfer;
using Kafe.Common;
using Kafe.Data;
using Kafe.Data.Aggregates;
using Kafe.Data.Events;
using Marten;
using Marten.Events;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Kafe.Api.Services;

public class DefaultAccountService : IAccountService, IDisposable
{
    public const string EmailConfirmationPurpose = "EmailConfirmation";
    public static readonly TimeSpan ConfirmationTokenExpiration = TimeSpan.FromHours(24);
    public static readonly Hrib DebugAccountId = "AAAAbadf00d";
    
    // TODO: Obtain this from ASP.NET Core somehow.
    public const string EmailConfirmationEndpoint = "/api/v1/tmp-account/";

    private readonly RandomNumberGenerator rng = RandomNumberGenerator.Create();
    private readonly IDataProtector dataProtector;
    private readonly IDocumentSession db;
    private readonly IEmailService emailService;
    private readonly IHostEnvironment environment;
    private readonly IOptions<KafeOptions> kafeOptions;

    public DefaultAccountService(
        IDocumentSession db,
        IDataProtectionProvider dataProtectionProvider,
        IEmailService emailService,
        IHostEnvironment environment,
        IOptions<KafeOptions> kafeOptions)
    {
        dataProtector = dataProtectionProvider.CreateProtector(nameof(DefaultAccountService));
        this.db = db;
        this.emailService = emailService;
        this.environment = environment;
        this.kafeOptions = kafeOptions;
    }

    public async Task CreateTemporaryAccount(
        TemporaryAccountCreationDto dto,
        CancellationToken token = default)
    {
        var account = await db.Query<TemporaryAccountInfo>()
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
            db.Events.StartStream<TemporaryAccountInfo>(id, created);
            await db.SaveChangesAsync(token);
            account = await db.LoadAsync<TemporaryAccountInfo>(id, token)!;
        }
        else
        {
            id = account.Id;
        }

        var eventStream = await db.Events.FetchForExclusiveWriting<TemporaryAccountInfo>(id, token)
            ?? throw new InvalidOperationException($"Could not obtain the event stream for account '{id}' " +
                "despite the account existing.");
        var refreshed = new TemporaryAccountRefreshed(
            AccountId: id,
            SecurityStamp: Guid.NewGuid().ToString());
        eventStream.AppendOne(refreshed);
        await db.SaveChangesAsync(token);

        var confirmationToken = EncodeAccountToken(new(id, EmailConfirmationPurpose, refreshed.SecurityStamp));
        var confirmationUrl = new Uri(new Uri(kafeOptions.Value.BaseUrl), $"{EmailConfirmationEndpoint}{confirmationToken}");
        var emailSubject = Const.ConfirmationEmailSubject[account!.PreferredCulture]!;
        var emailMessage = string.Format(
            Const.ConfirmationEmailMessageTemplate[account!.PreferredCulture]!,
            confirmationUrl,
            Const.EmailSignOffs[RandomNumberGenerator.GetInt32(0, Const.EmailSignOffs.Length)][account!.PreferredCulture]);
        await emailService.SendEmail(account.EmailAddress, emailSubject, emailMessage);
    }

    public async Task<TemporaryAccountInfoDto?> ConfirmTemporaryAccount(
        string confirmationToken,
        CancellationToken token = default)
    {
        if (environment.IsDevelopment()
            && !string.IsNullOrEmpty(kafeOptions.Value.DebugAccountToken)
            && confirmationToken == kafeOptions.Value.DebugAccountToken)
        {
            return new TemporaryAccountInfoDto(
                Id: DebugAccountId,
                EmailAddress: "kafe@example.com",
                PreferredCulture: Const.InvariantCultureCode);
        }

        if (!TryDecodeAccountToken(confirmationToken, out var dto))
        {
            return null;
        }

        if (dto.Purpose != EmailConfirmationPurpose)
        {
            return null;
        }

        var account = await db.LoadAsync<TemporaryAccountInfo>(dto.AccountId, token);
        if (account is null)
        {
            return null;
        }

        if (account.RefreshedOn + ConfirmationTokenExpiration < DateTimeOffset.UtcNow)
        {
            var closed = new TemporaryAccountClosed(account.Id);
            db.Events.Append(account.Id, closed);
            await db.SaveChangesAsync(token);
            return null;
        }

        if (account.SecurityStamp != dto.SecurityStamp)
        {
            return null;
        }

        var closed = new TemporaryAccountClosed(account.Id);
        db.Events.Append(account.Id, closed);
        await db.SaveChangesAsync(token);
        return TransferMaps.ToTemporaryAccountInfoDto(account);
    }

    public void Dispose()
    {
        ((IDisposable)rng).Dispose();
        GC.SuppressFinalize(this);
    }

    private string EncodeAccountToken(TemporaryAccountTokenDto dto)
    {
        var token = $"{dto.Purpose}:{dto.AccountId}:{dto.SecurityStamp}";
        var protectedBytes = dataProtector.Protect(Encoding.UTF8.GetBytes(token));
        return WebEncoders.Base64UrlEncode(protectedBytes);
    }

    private bool TryDecodeAccountToken(string encodedToken, [NotNullWhen(true)] out TemporaryAccountTokenDto? dto)
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
}
