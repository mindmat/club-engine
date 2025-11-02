using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace AppEngine.DataAccess;

using System.Linq.Expressions;

public interface IRepository<TEntity> : IQueryable<TEntity>
    where TEntity : Entity
{
    Task<TEntity?> Get(Expression<Func<TEntity, bool>> predicate);
    Task Upsert(TEntity entity, CancellationToken cancellationToken = default);
    Task<TEntity> Upsert(Expression<Func<TEntity, bool>> findExisting, Func<TEntity> createNew, Action<TEntity> update, CancellationToken cancellationToken = default);
    TEntity Insert(TEntity rootEntity);
    EntityEntry<TEntity> Remove(TEntity entity);
    void Remove(Expression<Func<TEntity, bool>> predicate);
}

public class Repository<TEntity>(DbContext dbContext) : Queryable<TEntity>(dbContext), IRepository<TEntity>
    where TEntity : Entity, new()
{
    public Task<TEntity?> Get(Expression<Func<TEntity, bool>> predicate)
    {
        return DbSet.FirstOrDefaultAsync(predicate);
    }

    public async Task<TEntity> Upsert(Expression<Func<TEntity, bool>> findExisting, Func<TEntity> createNew, Action<TEntity> update, CancellationToken cancellationToken = default)
    {
        var entity = await DbSet.AsTracking()
                                .FirstOrDefaultAsync(findExisting, cancellationToken)
                  ?? Insert(createNew());
        update(entity);

        return entity;
    }

    public TEntity Insert(TEntity rootEntity)
    {
        var entry = DbSet.Add(rootEntity);

        return entry.Entity;
    }

    public async Task Upsert(TEntity entity, CancellationToken cancellationToken = default)
    {
        // prevent empty Guid
        if (entity.Id == Guid.Empty)
        {
            entity.Id = Guid.NewGuid();
        }

        // add entity to context
        DbSet.Attach(entity);

        var entry = dbContext.Entry(entity);
        var dbValues = await entry.GetDatabaseValuesAsync(cancellationToken);

        if (dbValues == null)
        {
            // new entity, INSERT
            entry.State = EntityState.Added;
        }
        else
        {
            // existing entity, UPDATE
            // in EF Core this lines resets the original values to the entity
            //entry.State = EntityState.Unchanged;

            // check with dbValues if modified or unchanged (DbContext sets state)
            entry.OriginalValues.SetValues(dbValues);

            // check concurrency
            // entry.CurrentValues[rowversionPropertyName] must be the value sent by the client in order that optimistic locking works
            entry.OriginalValues[nameof(Entity.RowVersion)] = entry.CurrentValues[nameof(Entity.RowVersion)];
        }
    }

    public EntityEntry<TEntity> Remove(TEntity entityToDelete)
    {
        // make sure the entity is in the context
        entityToDelete = DbSet.Find(entityToDelete.Id);

        return DbSet.Remove(entityToDelete);
    }

    public void Remove(Expression<Func<TEntity, bool>> predicate)
    {
        var entitiesToDelete = DbSet.Where(predicate);

        foreach (var entityToDelete in entitiesToDelete)
        {
            DbSet.Remove(entityToDelete);
        }
    }
}