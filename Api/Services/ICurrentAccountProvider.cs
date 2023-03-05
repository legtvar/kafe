namespace Kafe.Api.Services;

public interface ICurrentAccountProvider
{
    ApiAccount? User { get; }
}
