using Marten;

namespace Kafe.Data;

public interface IKafeDocumentSession : IDocumentSession, IKafeQuerySession
{
}
