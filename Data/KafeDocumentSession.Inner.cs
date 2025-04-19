using System;
using System.Collections.Generic;
using System.Data.Common;
using System.IO;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Marten;
using Marten.Events;
using Marten.Internal.Operations;
using Marten.Linq;
using Marten.Services;
using Marten.Services.BatchQuerying;
using Marten.Storage;
using Marten.Storage.Metadata;
using Npgsql;

namespace Kafe.Data;

#pragma warning disable CS8714

public partial class KafeDocumentSession : IKafeDocumentSession
{
    public KafeDocumentSession(
        IDocumentSession inner,
        KafeTypeRegistry typeRegistry,
        DiagnosticFactory diagnosticFactory
    )
    {
        Inner = inner;
        this.typeRegistry = typeRegistry;
        this.diagnosticFactory = diagnosticFactory;
    }

    public IDocumentSession Inner { get; }

    public IMartenDatabase Database => Inner.Database;

    public NpgsqlConnection Connection => Inner.Connection;

    public IMartenSessionLogger Logger { get => Inner.Logger; set => Inner.Logger = value; }

    public int RequestCount => Inner.RequestCount;

    public IDocumentStore DocumentStore => Inner.DocumentStore;

    public IQueryEventStore Events => Inner.Events;

    public IJsonLoader Json => Inner.Json;

    public string? CausationId { get => Inner.CausationId; set => Inner.CausationId = value; }
    public string? CorrelationId { get => Inner.CorrelationId; set => Inner.CorrelationId = value; }

    public string TenantId => Inner.TenantId;

    public IAdvancedSql AdvancedSql => Inner.AdvancedSql;

    public IUnitOfWork PendingChanges => Inner.PendingChanges;

    public ConcurrencyChecks Concurrency => Inner.Concurrency;

    public IList<IDocumentSessionListener> Listeners => Inner.Listeners;

    public string? LastModifiedBy { get => Inner.LastModifiedBy; set => Inner.LastModifiedBy = value; }

    IEventStore IDocumentSession.Events => Inner.Events;

    IEventStore IDocumentOperations.Events => ((IDocumentOperations)Inner).Events;

    public void BeginTransaction()
    {
        Inner.BeginTransaction();
    }

    public ValueTask BeginTransactionAsync(CancellationToken token)
    {
        return Inner.BeginTransactionAsync(token);
    }

    public IBatchedQuery CreateBatchQuery()
    {
        return Inner.CreateBatchQuery();
    }

    public void Delete<T>(T entity) where T : notnull
    {
        Inner.Delete(entity);
    }

    public void Delete<T>(int id) where T : notnull
    {
        Inner.Delete<T>(id);
    }

    public void Delete<T>(long id) where T : notnull
    {
        Inner.Delete<T>(id);
    }

    public void Delete<T>(object id) where T : notnull
    {
        Inner.Delete<T>(id);
    }

    public void Delete<T>(Guid id) where T : notnull
    {
        Inner.Delete<T>(id);
    }

    public void Delete<T>(string id) where T : notnull
    {
        Inner.Delete<T>(id);
    }

    public void DeleteObjects(IEnumerable<object> documents)
    {
        Inner.DeleteObjects(documents);
    }

    public void DeleteWhere<T>(Expression<Func<T, bool>> expression) where T : notnull
    {
        Inner.DeleteWhere(expression);
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
        Inner.Dispose();
    }

    public ValueTask DisposeAsync()
    {
        GC.SuppressFinalize(this);
        return Inner.DisposeAsync();
    }

    public void Eject<T>(T document) where T : notnull
    {
        Inner.Eject(document);
    }

    public void EjectAllOfType(Type type)
    {
        Inner.EjectAllOfType(type);
    }

    public void EjectAllPendingChanges()
    {
        Inner.EjectAllPendingChanges();
    }

    public Task<int> ExecuteAsync(NpgsqlCommand command, CancellationToken token = default)
    {
        return Inner.ExecuteAsync(command, token);
    }

    public Task<DbDataReader> ExecuteReaderAsync(NpgsqlCommand command, CancellationToken token = default)
    {
        return Inner.ExecuteReaderAsync(command, token);
    }

    public ITenantQueryOperations ForTenant(string tenantId)
    {
        return ((IQuerySession)Inner).ForTenant(tenantId);
    }

    public object? GetHeader(string key)
    {
        return Inner.GetHeader(key);
    }

    public void HardDelete<T>(T entity) where T : notnull
    {
        Inner.HardDelete(entity);
    }

    public void HardDelete<T>(int id) where T : notnull
    {
        Inner.HardDelete<T>(id);
    }

    public void HardDelete<T>(long id) where T : notnull
    {
        Inner.HardDelete<T>(id);
    }

    public void HardDelete<T>(Guid id) where T : notnull
    {
        Inner.HardDelete<T>(id);
    }

    public void HardDelete<T>(string id) where T : notnull
    {
        Inner.HardDelete<T>(id);
    }

    public void HardDeleteWhere<T>(Expression<Func<T, bool>> expression) where T : notnull
    {
        Inner.HardDeleteWhere(expression);
    }

    public void Insert<T>(IEnumerable<T> entities) where T : notnull
    {
        Inner.Insert(entities);
    }

    public void Insert<T>(params T[] entities) where T : notnull
    {
        Inner.Insert(entities);
    }

    public void InsertObjects(IEnumerable<object> documents)
    {
        Inner.InsertObjects(documents);
    }

    public Task<T?> LoadAsync<T>(string id, CancellationToken token = default) where T : notnull
    {
        return ((IQuerySession)Inner).LoadAsync<T>(id, token);
    }

    public Task<T?> LoadAsync<T>(object id, CancellationToken token = default) where T : notnull
    {
        return Inner.LoadAsync<T>(id, token);
    }

    public Task<T?> LoadAsync<T>(int id, CancellationToken token = default) where T : notnull
    {
        return Inner.LoadAsync<T>(id, token);
    }

    public Task<T?> LoadAsync<T>(long id, CancellationToken token = default) where T : notnull
    {
        return Inner.LoadAsync<T>(id, token);
    }

    public Task<T?> LoadAsync<T>(Guid id, CancellationToken token = default) where T : notnull
    {
        return Inner.LoadAsync<T>(id, token);
    }

    public Task<IReadOnlyList<T>> LoadManyAsync<T>(params string[] ids) where T : notnull
    {
        return Inner.LoadManyAsync<T>(ids);
    }

    public Task<IReadOnlyList<T>> LoadManyAsync<T>(IEnumerable<string> ids) where T : notnull
    {
        return Inner.LoadManyAsync<T>(ids);
    }

    public Task<IReadOnlyList<T>> LoadManyAsync<T>(params Guid[] ids) where T : notnull
    {
        return Inner.LoadManyAsync<T>(ids);
    }

    public Task<IReadOnlyList<T>> LoadManyAsync<T>(IEnumerable<Guid> ids) where T : notnull
    {
        return Inner.LoadManyAsync<T>(ids);
    }

    public Task<IReadOnlyList<T>> LoadManyAsync<T>(params int[] ids) where T : notnull
    {
        return Inner.LoadManyAsync<T>(ids);
    }

    public Task<IReadOnlyList<T>> LoadManyAsync<T>(IEnumerable<int> ids) where T : notnull
    {
        return Inner.LoadManyAsync<T>(ids);
    }

    public Task<IReadOnlyList<T>> LoadManyAsync<T>(params long[] ids) where T : notnull
    {
        return Inner.LoadManyAsync<T>(ids);
    }

    public Task<IReadOnlyList<T>> LoadManyAsync<T>(IEnumerable<long> ids) where T : notnull
    {
        return Inner.LoadManyAsync<T>(ids);
    }

    public Task<IReadOnlyList<T>> LoadManyAsync<T>(CancellationToken token, params string[] ids) where T : notnull
    {
        return Inner.LoadManyAsync<T>(token, ids);
    }

    public Task<IReadOnlyList<T>> LoadManyAsync<T>(CancellationToken token, IEnumerable<string> ids) where T : notnull
    {
        return Inner.LoadManyAsync<T>(token, ids);
    }

    public Task<IReadOnlyList<T>> LoadManyAsync<T>(CancellationToken token, params Guid[] ids) where T : notnull
    {
        return Inner.LoadManyAsync<T>(token, ids);
    }

    public Task<IReadOnlyList<T>> LoadManyAsync<T>(CancellationToken token, IEnumerable<Guid> ids) where T : notnull
    {
        return Inner.LoadManyAsync<T>(token, ids);
    }

    public Task<IReadOnlyList<T>> LoadManyAsync<T>(CancellationToken token, params int[] ids) where T : notnull
    {
        return Inner.LoadManyAsync<T>(token, ids);
    }

    public Task<IReadOnlyList<T>> LoadManyAsync<T>(CancellationToken token, IEnumerable<int> ids) where T : notnull
    {
        return Inner.LoadManyAsync<T>(token, ids);
    }

    public Task<IReadOnlyList<T>> LoadManyAsync<T>(CancellationToken token, params long[] ids) where T : notnull
    {
        return Inner.LoadManyAsync<T>(token, ids);
    }

    public Task<IReadOnlyList<T>> LoadManyAsync<T>(CancellationToken token, IEnumerable<long> ids) where T : notnull
    {
        return Inner.LoadManyAsync<T>(token, ids);
    }

    public Task<DocumentMetadata> MetadataForAsync<T>(T entity, CancellationToken token = default) where T : notnull
    {
        return Inner.MetadataForAsync(entity, token);
    }

    public IReadOnlyList<TDoc> PhraseSearch<TDoc>(string searchTerm, string regConfig = "english")
    {
        return Inner.PhraseSearch<TDoc>(searchTerm, regConfig);
    }

    public Task<IReadOnlyList<TDoc>> PhraseSearchAsync<TDoc>(string searchTerm, string regConfig = "english", CancellationToken token = default)
    {
        return Inner.PhraseSearchAsync<TDoc>(searchTerm, regConfig, token);
    }

    public IReadOnlyList<TDoc> PlainTextSearch<TDoc>(string searchTerm, string regConfig = "english")
    {
        return Inner.PlainTextSearch<TDoc>(searchTerm, regConfig);
    }

    public Task<IReadOnlyList<TDoc>> PlainTextSearchAsync<TDoc>(string searchTerm, string regConfig = "english", CancellationToken token = default)
    {
        return Inner.PlainTextSearchAsync<TDoc>(searchTerm, regConfig, token);
    }

    public IMartenQueryable<T> Query<T>()
    {
        return Inner.Query<T>();
    }

    public Task<IReadOnlyList<T>> QueryAsync<T>(string sql, CancellationToken token, params object[] parameters)
    {
        return Inner.QueryAsync<T>(sql, token, parameters);
    }

    public Task<IReadOnlyList<T>> QueryAsync<T>(char placeholder, string sql, CancellationToken token, params object[] parameters)
    {
        return Inner.QueryAsync<T>(placeholder, sql, token, parameters);
    }

    public Task<IReadOnlyList<T>> QueryAsync<T>(string sql, params object[] parameters)
    {
        return Inner.QueryAsync<T>(sql, parameters);
    }

    public Task<IReadOnlyList<T>> QueryAsync<T>(char placeholder, string sql, params object[] parameters)
    {
        return Inner.QueryAsync<T>(placeholder, sql, parameters);
    }

    public Task<TOut> QueryAsync<TDoc, TOut>(ICompiledQuery<TDoc, TOut> query, CancellationToken token = default)
    {
        return Inner.QueryAsync(query, token);
    }

    public Task<T> QueryByPlanAsync<T>(IQueryPlan<T> plan, CancellationToken token = default)
    {
        return Inner.QueryByPlanAsync(plan, token);
    }

    public IMartenQueryable<T> QueryForNonStaleData<T>(TimeSpan timeout)
    {
        return Inner.QueryForNonStaleData<T>(timeout);
    }

    public void QueueOperation(IStorageOperation storageOperation)
    {
        Inner.QueueOperation(storageOperation);
    }

    public void QueueSqlCommand(string sql, params object[] parameterValues)
    {
        Inner.QueueSqlCommand(sql, parameterValues);
    }

    public void QueueSqlCommand(char placeholder, string sql, params object[] parameterValues)
    {
        Inner.QueueSqlCommand(placeholder, sql, parameterValues);
    }

    void IDocumentSession.SaveChanges()
    {
        throw new NotImplementedException("Stop using. Will be deprecated in Marten 8.0.");
    }

    public Task SaveChangesAsync(CancellationToken token = default)
    {
        return Inner.SaveChangesAsync(token);
    }

    public Task<IReadOnlyList<TDoc>> SearchAsync<TDoc>(string queryText, string regConfig = "english", CancellationToken token = default)
    {
        return Inner.SearchAsync<TDoc>(queryText, regConfig, token);
    }

    public void SetHeader(string key, object value)
    {
        Inner.SetHeader(key, value);
    }

    public void Store<T>(IEnumerable<T> entities) where T : notnull
    {
        Inner.Store(entities);
    }

    public void Store<T>(params T[] entities) where T : notnull
    {
        Inner.Store(entities);
    }

    public void StoreObjects(IEnumerable<object> documents)
    {
        Inner.StoreObjects(documents);
    }

    public Task<int> StreamJson<T>(Stream destination, CancellationToken token, string sql, params object[] parameters)
    {
        return Inner.StreamJson<T>(destination, token, sql, parameters);
    }

    public Task<int> StreamJson<T>(Stream destination, CancellationToken token, char placeholder, string sql, params object[] parameters)
    {
        return Inner.StreamJson<T>(destination, token, placeholder, sql, parameters);
    }

    public Task<int> StreamJson<T>(Stream destination, string sql, params object[] parameters)
    {
        return Inner.StreamJson<T>(destination, sql, parameters);
    }

    public Task<int> StreamJson<T>(Stream destination, char placeholder, string sql, params object[] parameters)
    {
        return Inner.StreamJson<T>(destination, placeholder, sql, parameters);
    }

    public Task<int> StreamJsonMany<TDoc, TOut>(ICompiledQuery<TDoc, TOut> query, Stream destination, CancellationToken token = default)
    {
        return Inner.StreamJsonMany(query, destination, token);
    }

    public Task<bool> StreamJsonOne<TDoc, TOut>(ICompiledQuery<TDoc, TOut> query, Stream destination, CancellationToken token = default)
    {
        return Inner.StreamJsonOne(query, destination, token);
    }

    public Task<string> ToJsonMany<TDoc, TOut>(ICompiledQuery<TDoc, TOut> query, CancellationToken token = default)
    {
        return Inner.ToJsonMany(query, token);
    }

    public Task<string?> ToJsonOne<TDoc, TOut>(ICompiledQuery<TDoc, TOut> query, CancellationToken token = default)
    {
        return Inner.ToJsonOne(query, token);
    }

    public void TryUpdateRevision<T>(T entity, int revision)
    {
        Inner.TryUpdateRevision(entity, revision);
    }

    public void UndoDeleteWhere<T>(Expression<Func<T, bool>> expression) where T : notnull
    {
        Inner.UndoDeleteWhere(expression);
    }

    public void Update<T>(IEnumerable<T> entities) where T : notnull
    {
        Inner.Update(entities);
    }

    public void Update<T>(params T[] entities) where T : notnull
    {
        Inner.Update(entities);
    }

    public void UpdateExpectedVersion<T>(T entity, Guid version) where T : notnull
    {
        Inner.UpdateExpectedVersion(entity, version);
    }

    public void UpdateRevision<T>(T entity, int revision)
    {
        Inner.UpdateRevision(entity, revision);
    }

    public void UseIdentityMapFor<T>()
    {
        Inner.UseIdentityMapFor<T>();
    }

    public Guid? VersionFor<TDoc>(TDoc entity) where TDoc : notnull
    {
        return Inner.VersionFor(entity);
    }

    public IReadOnlyList<TDoc> WebStyleSearch<TDoc>(string searchTerm, string regConfig = "english")
    {
        return Inner.WebStyleSearch<TDoc>(searchTerm, regConfig);
    }

    public Task<IReadOnlyList<TDoc>> WebStyleSearchAsync<TDoc>(string searchTerm, string regConfig = "english", CancellationToken token = default)
    {
        return Inner.WebStyleSearchAsync<TDoc>(searchTerm, regConfig, token);
    }

    IReadOnlyList<T> IQuerySession.AdvancedSqlQuery<T>(string sql, params object[] parameters)
    {
        throw new NotImplementedException("Stop using. Will be deprecated in Marten 8.0.");
    }

    IReadOnlyList<(T1, T2)> IQuerySession.AdvancedSqlQuery<T1, T2>(string sql, params object[] parameters)
    {
        throw new NotImplementedException("Stop using. Will be deprecated in Marten 8.0.");
    }

    IReadOnlyList<(T1, T2, T3)> IQuerySession.AdvancedSqlQuery<T1, T2, T3>(string sql, params object[] parameters)
    {
        throw new NotImplementedException("Stop using. Will be deprecated in Marten 8.0.");
    }

    Task<IReadOnlyList<T>> IQuerySession.AdvancedSqlQueryAsync<T>(
        string sql,
        CancellationToken token,
        params object[] parameters
    )
    {
        throw new NotImplementedException("Stop using. Will be deprecated in Marten 8.0.");
    }

    Task<IReadOnlyList<(T1, T2)>> IQuerySession.AdvancedSqlQueryAsync<T1, T2>(string sql, CancellationToken token, params object[] parameters)
    {
        throw new NotImplementedException("Stop using. Will be deprecated in Marten 8.0.");
    }

    Task<IReadOnlyList<(T1, T2, T3)>> IQuerySession.AdvancedSqlQueryAsync<T1, T2, T3>(string sql, CancellationToken token, params object[] parameters)
    {
        throw new NotImplementedException("Stop using. Will be deprecated in Marten 8.0.");
    }

    int IQuerySession.Execute(NpgsqlCommand cmd)
    {
        throw new NotImplementedException("Stop using. Will be deprecated in Marten 8.0.");
    }

    DbDataReader IQuerySession.ExecuteReader(NpgsqlCommand command)
    {
        throw new NotImplementedException("Stop using. Will be deprecated in Marten 8.0.");
    }

    ITenantOperations IDocumentSession.ForTenant(string tenantId)
    {
        return Inner.ForTenant(tenantId);
    }

    T? IQuerySession.Load<T>(string id) where T : default
    {
        throw new NotImplementedException("Stop using. Will be deprecated in Marten 8.0.");
    }

    T? IQuerySession.Load<T>(int id) where T : default
    {
        throw new NotImplementedException("Stop using. Will be deprecated in Marten 8.0.");
    }

    T? IQuerySession.Load<T>(long id) where T : default
    {
        throw new NotImplementedException("Stop using. Will be deprecated in Marten 8.0.");
    }

    T? IQuerySession.Load<T>(Guid id) where T : default
    {
        throw new NotImplementedException("Stop using. Will be deprecated in Marten 8.0.");
    }

    IReadOnlyList<T> IQuerySession.LoadMany<T>(params string[] ids)
    {
        throw new NotImplementedException("Stop using. Will be deprecated in Marten 8.0.");
    }

    IReadOnlyList<T> IQuerySession.LoadMany<T>(IEnumerable<string> ids)
    {
        throw new NotImplementedException("Stop using. Will be deprecated in Marten 8.0.");
    }

    IReadOnlyList<T> IQuerySession.LoadMany<T>(params Guid[] ids)
    {
        throw new NotImplementedException("Stop using. Will be deprecated in Marten 8.0.");
    }

    IReadOnlyList<T> IQuerySession.LoadMany<T>(IEnumerable<Guid> ids)
    {
        throw new NotImplementedException("Stop using. Will be deprecated in Marten 8.0.");
    }

    IReadOnlyList<T> IQuerySession.LoadMany<T>(params int[] ids)
    {
        throw new NotImplementedException("Stop using. Will be deprecated in Marten 8.0.");
    }

    IReadOnlyList<T> IQuerySession.LoadMany<T>(IEnumerable<int> ids)
    {
        throw new NotImplementedException("Stop using. Will be deprecated in Marten 8.0.");
    }

    IReadOnlyList<T> IQuerySession.LoadMany<T>(params long[] ids)
    {
        throw new NotImplementedException("Stop using. Will be deprecated in Marten 8.0.");
    }

    IReadOnlyList<T> IQuerySession.LoadMany<T>(IEnumerable<long> ids)
    {
        throw new NotImplementedException("Stop using. Will be deprecated in Marten 8.0.");
    }

    DocumentMetadata? IQuerySession.MetadataFor<T>(T entity)
    {
        throw new NotImplementedException("Stop using. Will be deprecated in Marten 8.0.");
    }

    IReadOnlyList<T> IQuerySession.Query<T>(string sql, params object[] parameters)
    {
        throw new NotImplementedException("Stop using. Will be deprecated in Marten 8.0.");
    }

    TOut IQuerySession.Query<TDoc, TOut>(ICompiledQuery<TDoc, TOut> query)
    {
        throw new NotImplementedException("Stop using. Will be deprecated in Marten 8.0.");
    }

    IReadOnlyList<TDoc> IQuerySession.Search<TDoc>(string queryText, string regConfig)
    {
        throw new NotImplementedException("Stop using. Will be deprecated in Marten 8.0.");
    }
}
