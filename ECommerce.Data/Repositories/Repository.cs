using ECommerce.Data.DbContexts;
using ECommerce.Data.IRepositories;
using ECommerce.Domain.Commons;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace ECommerce.Data.Repositories;

public class Repository<TEntity> : IRepository<TEntity> where TEntity : Auditable
{
    protected readonly ECommerceDbContext dbContext;
    protected readonly DbSet<TEntity> dbSet;
    public Repository(ECommerceDbContext dbContext)
    {
        this.dbContext = dbContext;
        dbSet = dbContext.Set<TEntity>();
    }

    public async Task<bool> DeleteAsync(Expression<Func<TEntity, bool>> expression, bool deleted = false)
    {
        var entity = await this.SelectAsync(expression, deleted: deleted);

        if (entity is null)
            return false;
        if (deleted)
            dbSet.Remove(entity);
        else
        {
            entity.IsDeleted = true;
            entity.UpdatedAt = DateTime.UtcNow;
        }

        return true;
    }

    public bool DeleteMany(Expression<Func<TEntity, bool>> expression, bool deleted = false)
    {
        IQueryable<TEntity> entities = this.SelectAll(expression, deleted: deleted);
        if (entities.Any())
        {
            if (deleted)
                entities.ExecuteDelete();
            else
                entities.ExecuteUpdate(e => e.SetProperty(
                    p => new
                    {
                        p.IsDeleted,
                        p.UpdatedAt
                    },
                    v => new
                    {
                        IsDeleted = true,
                        UpdatedAt = (DateTime?)DateTime.UtcNow
                    }));
            return true;
        }
        return false;
    }

    public async Task<TEntity> InsertAsync(TEntity entity)
        => (await this.dbSet.AddAsync(entity)).Entity;

    public async Task<bool> InsertAsync(IEnumerable<TEntity> entity)
    {
        await this.dbSet.AddRangeAsync(entity);
        return true;
    }

    public async Task<int> SaveChangesAsync()
        => await this.dbContext.SaveChangesAsync();

    public IQueryable<TEntity> SelectAll(Expression<Func<TEntity, bool>>? expression = null, string[]? includes = null, bool deleted = false)
    {
        IQueryable<TEntity> query = deleted ? dbSet.IgnoreQueryFilters() : this.dbSet;
        if (expression is not null)
            query = query.Where(expression);

        if (includes is not null)
            foreach (string include in includes)
            {
                query = query.Include(include);
            }

        return query;
    }

    public async Task<TEntity?> SelectAsync(Expression<Func<TEntity, bool>> expression, string[]? includes = null, bool deleted = false)
        => await this.SelectAll(expression, includes, deleted).FirstOrDefaultAsync();

    public TEntity Update(TEntity entity, bool deleted = false)
    {
        entity.UpdatedAt = DateTime.UtcNow;
        if (deleted)
        {
            this.dbSet.IgnoreQueryFilters()
                .Where(e => e.Id == entity.Id)
                .ExecuteUpdate(e => e.SetProperty(p => p, v => entity));
            return entity;
        }
        return this.dbContext.Update(entity).Entity;
    }
}
