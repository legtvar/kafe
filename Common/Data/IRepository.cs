using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Kafe;

/// <summary>
/// A simplified interface to the data access layer.
/// Allows uniform access and modification of KAFE acc
/// </summary>
public interface IRepository<T> where T : IEntity
{
    /// <summary>
    /// Creates a <typeparamref name="T"/> entity.
    /// <paramref name="entity"/> must have an <see cref="IEntity.Id"/> equal to <see cref="Hrib.Empty"/>.
    /// </summary>
    ValueTask<Err<Hrib>> Create(T entity, CancellationToken ct = default);

    /// <summary>
    /// Read one or more <typeparamref name="T"/> entities.
    /// </summary>
    /// 
    /// <remarks>
    /// Preserves order of entities in <paramref name="ids"/>.
    /// Upon error, returns all successfully read entities, sets failed reads to <see langword="null"/>,
    /// and returns a user-facing <see cref="Diagnostic"/> in <see cref="Err{T}"/>.
    /// </remarks>
    ValueTask<Err<IReadOnlyList<T?>>> Read(IEnumerable<Hrib> ids, CancellationToken ct = default);

    /// <summary>
    /// Inserts a <typeparamref name="T"/> entity.
    /// <paramref name="entity"/> must have a valid non-empty <see cref="IEntity.Id"/>.
    /// </summary>
    ValueTask<Err<Hrib>> Update(T entity, CancellationToken ct = default);

    /// <summary>
    /// Updates a <typeparamref name="T"/> entity if it already exists or inserts it if it does not.
    /// <paramref name="entity"/> must have a valid <see cref="IEntity.Id"/>.
    /// </summary>
    ValueTask<Err<Hrib>> Upsert(T entity, CancellationToken ct = default);

    /// <summary>
    /// Archive (i.e. soft-delete) a <typeparamref name="T"/> entity.
    /// </summary>
    ValueTask<Err<Hrib>> Archive(Hrib id, CancellationToken ct = default);

}
