using System.Threading;
using System.Threading.Tasks;
using Kafe.Data.Documents;
using Marten;

namespace Kafe.Data;

/// <summary>
/// A "migration" of sorts. Fixes a mistake by appending new events to the DB.
/// </summary>
public interface IEventCorrection<T>
{
    /// <summary>
    /// Apply a series of corrective events.
    /// </summary>
    /// <param name="db">The DB.</param>
    /// <returns>A JSON-serializable object that will be included in the EventCorrectionInfo document.</returns>
    Task<object?> Apply(IDocumentSession db, CancellationToken ct = default);

    /// <summary>
    /// Un-apply the correction.
    /// Use <see cref="EventCorrectionInfo.CustomData"/> to store data necessary to make this possible.
    /// </summary>
    /// <param name="db">The DB.</param>
    /// <param name="info">The document describing the </param>
    /// <returns></returns>
    Task Revert(IDocumentSession db, EventCorrectionInfo<T> info, CancellationToken ct = default);
}
