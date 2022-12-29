using System.Linq.Expressions;

using Microsoft.EntityFrameworkCore;

using static Library.Core.DatabaseExtensions;

namespace Library.Core;

public static class DatabaseExtensions
{
    public static async Task<PagedList<T>> QueryAsync<T, O>(
        this IQueryable<T> q,
        Expression<Func<T, bool>> where,
        IPagedListRequest<O> pageRequest,
        Expression<Func<T, object>> order = null,
        Action<IQueryable<T>> postQueryAction = null
    ) where T : class
    {
        IQueryable<T> items = q.Where(where);

        if (order != null)
        {
            if (q.IsOrdered() && q is IOrderedQueryable<T> oq)
            {
                items = pageRequest.OrderDir == OrderByDirection.Asc ? oq.ThenBy(order) : oq.ThenByDescending(order);
            }
            else
            {
                items = pageRequest.OrderDir == OrderByDirection.Asc ? items.OrderBy(order) : items.OrderByDescending(order);
            }
        }

        postQueryAction?.Invoke(items);

        return await PagedList<T>.CreateAsync(items, pageRequest.PageNumber, pageRequest.PageSize);
    }

    public static async Task<(T, bool exists)> FindWithDefaultAsync<T>(this IQueryable<T> query, Expression<Func<T, bool>> where, T useDefault) where T : class
    {
        var result = await query.FindAsync(where, null);
        return (result ?? useDefault, result != null);
    }

    public static async Task<T> FindOrNullAsync<T>(this IQueryable<T> query, Expression<Func<T, bool>> where) where T : class
    {
        return await query.FindAsync(where, null);
    }

    public static bool IsOrdered<T>(this IQueryable<T> queryable)
    {
        if (queryable == null) throw new ArgumentNullException(nameof(queryable));

        return queryable.Expression.Type == typeof(IOrderedQueryable<T>);
    }

    public static async Task<T> FindAsync<T>(this IQueryable<T> query, Expression<Func<T, bool>> where, ErrorCode? throwError = ErrorCode.MissingEntity) where T : class
    {
        var value = await query.FirstOrDefaultAsync(where);
        if (value == null && throwError.HasValue)
        {
            if (throwError.Value == ErrorCode.MissingEntity) throw new SiteException(ErrorCode.MissingEntity, typeof(T).Name);
            else throw new SiteException(throwError.Value);
        }

        return value;
    }

    public static async Task<T> CreateAsync<T>(this DbContext ctx, T model) where T : class
    {
        if (model == null) return model;

        var add = (await ctx.Set<T>().AddAsync(model)).Entity;
        await ctx.SaveChangesAsync();

        return add;
    }

    public static async Task<IEnumerable<T>> CreateManyAsync<T>(this DbContext ctx, IEnumerable<T> models) where T : class
    {
        if (!models.Any()) return models;

        await ctx.Set<T>().AddRangeAsync(models);
        await ctx.SaveChangesAsync();

        return models;
    }

    public static async Task<T> UpdateAsync<T>(this DbContext ctx, T model) where T : class
    {
        if (model == null) return model;

        var update = ctx.Set<T>().Update(model);
        await ctx.SaveChangesAsync();

        return update.Entity;
    }

    public static async Task<IEnumerable<T>> UpdateManyAsync<T>(this DbContext ctx, IEnumerable<T> models) where T : class
    {
        if (!models.Any())
        {
            return models;
        }

        ctx.Set<T>().UpdateRange(models);
        await ctx.SaveChangesAsync();

        return models;
    }

    public static async Task RemoveAsync<T>(this DbContext ctx, T model) where T : class
    {
        if (model == null) return;

        ctx.Set<T>().Remove(model);
        await ctx.SaveChangesAsync();
    }

    public static async Task RemoveManyAsync<T>(this DbContext ctx, IEnumerable<T> models) where T : class
    {
        if (!models.Any())
        {
            return;
        }

        ctx.RemoveRange(models);
        await ctx.SaveChangesAsync();
    }

    public static async Task UpdateUsernameAsync<U>(this DbContext ctx, Guid userId, string username) where U : class, IUser
    {
        // Get user set
        var set = ctx.Set<U>();

        // Update record immediately
        var data = await set
            .Where(x => x.UserId == userId)
            .ExecuteUpdateAsync(prop => prop.SetProperty(p => p.Username, username));
    }

    public static Task DeleteUserAsync<U>(this DbContext ctx, Guid userId) where U : class, IUserId
    {
        return ctx.ExecuteDeleteAsync<U>(u => u.UserId == userId);
    }

    public static async Task ExecuteUpdateAsync<U>(this DbContext ctx, Expression<Func<U, bool>> expression, Action<U> updateAction) where U : class
    {
        // Get db set
        var set = ctx.Set<U>();

        // Get data from db
        var entries = await set.Where(expression).ToListAsync();

        foreach (var entry in entries)
        {
            updateAction.Invoke(entry);
        }

        await UpdateManyAsync(ctx, entries);
    }

    public static async Task ExecuteDeleteAsync<U>(this DbContext ctx, Expression<Func<U, bool>> expression) where U : class
    {
        // Get db set
        var set = ctx.Set<U>();

        // Get data from db
        var results = set.Where(expression);

        // This does not work when using in memory
        if (!ctx.Database.IsInMemory())
        {
            await results.ExecuteDeleteAsync();
        }

        // Do it the old way
        else
        {
            set.RemoveRange(results);
            await ctx.SaveChangesAsync();
        }
    }
}
