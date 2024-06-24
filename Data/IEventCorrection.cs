using System;
using System.Threading;
using System.Threading.Tasks;
using Kafe.Data.Documents;
using Marten;

namespace Kafe.Data;

/// <summary>
/// A "migration" of sorts. Fixes a mistake by appending new events to the DB.
/// </summary>
public interface IEventCorrection
{
    /// <summary>
    /// Apply a series of corrective events.
    /// </summary>
    /// 
    /// <remarks>
    /// This method should never call <see cref="IDocumentSession.SaveChanges"/>, so that the appended events can be
    /// recorded in <see cref="EventCorrectionInfo"/>.
    /// </remarks>
    /// 
    /// <param name="db">The DB.</param>
    /// 
    /// <returns>A JSON-serializable object that will be included in the EventCorrectionInfo document.</returns>
    Task Apply(IDocumentSession db, CancellationToken ct = default);
}
