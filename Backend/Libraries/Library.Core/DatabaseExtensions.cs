using System.Linq.Expressions;
using System.Text.Json;

using Library.Core.Models;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;

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
        var result = await FindAsync(query, where, null);
        return (result ?? useDefault, result != null);
    }

    public static async Task<T> FindOrNullAsync<T>(this IQueryable<T> query, Expression<Func<T, bool>> where) where T : class
    {
        return await FindAsync(query, where, null);
    }

    public static bool IsOrdered<T>(this IQueryable<T> queryable)
    {
        if (queryable == null)
        {
            throw new ArgumentNullException(nameof(queryable));
        }

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

    public static Task Set<T>(this IDistributedCache cache, string key, T value, DistributedCacheEntryOptions options)
    {
        return cache.SetStringAsync(key, JsonSerializer.Serialize(value), options);
    }

    public static Task Set<T>(this IDistributedCache cache, string key, T value)
    {
        return cache.SetStringAsync(key, JsonSerializer.Serialize(value));
    }

    public static async Task<T> Get<T>(this IDistributedCache cache, string key)
    {
        var value = await cache.GetStringAsync(key);
        if (value == null) return default;

        try
        {
            return JsonSerializer.Deserialize<T>(value);
        }
        catch
        {
            return default;
        }
    }

    public static async Task<T> Post<T>(this DbContext ctx, T model) where T : class
    {
        if (model == null)
        {
            return model;
        }

        var add = (await ctx.Set<T>().AddAsync(model)).Entity;
        await ctx.SaveChangesAsync();

        return add;
    }

    public static async Task<IEnumerable<T>> PostRange<T>(this DbContext ctx, IEnumerable<T> models) where T : class
    {
        if (!models.Any())
        {
            return models;
        }

        await ctx.Set<T>().AddRangeAsync(models);
        await ctx.SaveChangesAsync();

        return models;
    }

    public static async Task<T> Put<T>(this DbContext ctx, T model) where T : class
    {
        if (model == null)
        {
            return model;
        }

        var update = ctx.Set<T>().Update(model);
        await ctx.SaveChangesAsync();

        return update.Entity;
    }

    public static async Task<IEnumerable<T>> PutRange<T>(this DbContext ctx, IEnumerable<T> models) where T : class
    {
        if (!models.Any())
        {
            return models;
        }

        ctx.Set<T>().UpdateRange(models);
        await ctx.SaveChangesAsync();

        return models;
    }

    public static async Task Delete<T>(this DbContext ctx, T model) where T : class
    {
        if (model == null)
        {
            return;
        }

        ctx.Set<T>().Remove(model);
        await ctx.SaveChangesAsync();
    }

    public static async Task DeleteRange<T>(this DbContext ctx, IEnumerable<T> models) where T : class
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

        // Fetch data from set
        var data = await set.Where(x => x.UserId == userId).ToListAsync();

        // Update username of all records in set
        data.ForEach(x => x.Username = username);

        // Save records
        await ctx.Put(data);
    }

    public static async Task DeleteUserAsync<U>(this DbContext ctx, Guid userId) where U : class, IUserId
    {
        // Get user set
        var set = ctx.Set<U>();

        // Fetch data from set
        var data = set.Where(x => x.UserId == userId);

        // Delete records
        await ctx.DeleteRange(data);
    }
}