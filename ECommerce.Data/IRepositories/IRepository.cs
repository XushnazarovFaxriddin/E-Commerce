using ECommerce.Domain.Commons;
using System.Linq.Expressions;
using System.Security.AccessControl;

namespace ECommerce.Data.IRepositories;

public interface IRepository<TEntity> where TEntity : Auditable
{
    /// <summary>
    /// Inserts element to a table and keep track of it until change saved
    /// </summary>
    /// <param name="entity"></param>
    /// <returns></returns>
    Task<TEntity> InsertAsync(TEntity entity);

    /// <summary>
    /// Inserts elements to a table and keep track of them until change saved
    /// </summary>
    /// <param name="entity"></param>
    /// <returns></returns>
    Task<bool> InsertAsync(IEnumerable<TEntity> entity);

    /// <summary>
    /// Updates entity and keep track of it until change saved
    /// </summary>
    /// <param name="entity"></param>
    /// <param name="deleted"></param>
    /// <returns></returns>
    TEntity Update(TEntity entity, bool deleted = false);

    /// <summary>
    /// Selects all elements from table that matches condition and include relations
    /// </summary>
    /// <param name="expression"></param>
    /// <param name="includes"></param>
    /// <param name="deleted"></param>
    /// <returns></returns>
    IQueryable<TEntity> SelectAll(Expression<Func<TEntity, bool>>? expression = null, string[]? includes = null, bool deleted = false);

    /// <summary>
    /// selects element from a table specified with expression and can includes relations
    /// </summary>
    /// <param name="expression"></param>
    /// <param name="includes"></param>
    /// <param name="deleted"></param>
    /// <returns></returns>
    Task<TEntity?> SelectAsync(Expression<Func<TEntity, bool>> expression, string[]? includes = null, bool deleted = false);

    /// <summary>
    /// Deletes first item that matched expression and keep track of it until change saved 
    /// </summary>
    /// <param name="expression"></param>
    /// <param name="deleted"></param>
    /// <returns>true if action is successful, false if unable to delete</returns>
    Task<bool> DeleteAsync(Expression<Func<TEntity, bool>> expression, bool deleted = false);

    /// <summary>
    /// Deletes all elements if expression matches and keep track of them until change saved
    /// </summary>
    /// <param name="expression"></param>
    /// <param name="deleted"></param>
    /// <returns></returns>
    bool DeleteMany(Expression<Func<TEntity, bool>> expression, bool deleted = false);

    /// <summary>
    /// Saves tracking changes and write them to database permenantly
    /// </summary>
    /// <returns></returns>
    Task<int> SaveChangesAsync();
}