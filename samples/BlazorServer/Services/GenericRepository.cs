using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using BlazorServer.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
#if ENABLE_BULK
using EFCore.BulkExtensions;
#endif
namespace BlazorServer.Services
{
    public abstract class GenericRepository<TEntity> : GenericRepository<TEntity, int>, IGenericRepository<TEntity> where TEntity : class, IEntity<int>
    {
        protected GenericRepository(DbContext context, ILoggerFactory logger, IConfiguration configuration) : base(context, logger, configuration)
        {
            
        }
    }

    public abstract class GenericRepository<TEntity, TKey> : IGenericRepository<TEntity, TKey> where TEntity : class, IEntity<TKey> where TKey : struct, IComparable<TKey>, IEquatable<TKey>
    {
        protected DbContext DbContext;
        protected DbSet<TEntity> DbSet;
        protected readonly ILogger Logger;
        protected readonly IConfiguration Configuration;

#if ENABLE_BULK
        public ushort MinRowsToBulk { get; set; }
#endif

#if ENABLE_SPLIT
        public ushort MinRowsToSplit { get; set; }
#endif

        private Expression<Func<TEntity, bool>> PreparePredicate(Expression<Func<TEntity, bool>> predicate = null)
        {
            if (typeof(ISoftDelete).IsAssignableFrom(typeof(TEntity)))
            {
                if (predicate == null)
                {
                    predicate = m => (m as ISoftDelete).IsDeleted == false;
                }
                else
                {
                    var prop = typeof(TEntity).GetProperty("IsDeleted");
                    var type = prop?.PropertyType;
                    var constant = Expression.Constant(false);
                    var methodInfo = type?.GetMethod("Equals", new[] { type });
                    var member = Expression.Property(predicate.Parameters[0], prop);
                    var callExp = Expression.Call(member, methodInfo, constant);
                    var body = Expression.AndAlso(callExp, predicate.Body);
                    var lambda = Expression.Lambda<Func<TEntity, bool>>(body, predicate.Parameters[0]);
                    predicate = lambda;
                }
            }
            return predicate;
        }

        protected GenericRepository(DbContext context, ILoggerFactory logger, IConfiguration configuration)
        {
            DbContext = context;
            DbSet = DbContext.Set<TEntity>();
            Logger = logger.CreateLogger(GetType());
#if ENABLE_BULK
            MinRowsToBulk = ushort.Parse(configuration["RepositorySettings:MinRowsToBulk"] ?? "1000");
#endif
#if ENABLE_SPLIT
            MinRowsToSplit = ushort.Parse(configuration["RepositorySettings:MinRowsToSplit"] ?? "100");
#endif
            Configuration = configuration;
        }

        public virtual Task<IQueryable<TResult>> ReadAsync<TResult>(
            Expression<Func<TEntity, TResult>> selector,
            Expression<Func<TEntity, bool>> predicate = null,
            Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null,
            Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>> include = null,
            int? skip = 0, int? take = null,
            bool disableTracking = false,
            bool ignoreQueryFilters = false,
            bool includeDeleted = false)
        {
            IQueryable<TEntity> query = DbSet;
            if (disableTracking) query = query.AsNoTracking();
            if (include != null) query = include(query);
            if (ignoreQueryFilters) query = query.IgnoreQueryFilters();
            if (predicate != null) query = query.Where(predicate);

            if (typeof(ISoftDelete).IsAssignableFrom(typeof(TEntity)) && !includeDeleted)
                query = query.Where(m => (m as ISoftDelete).IsDeleted == false);

            var stprop = typeof(TEntity)
                .GetProperties()
                .FirstOrDefault(m => m.PropertyType == typeof(string) || (!typeof(IEnumerable).IsAssignableFrom(m.PropertyType) && !m.PropertyType.IsClass))?.Name;
            var result = orderBy != null ? orderBy(query).Select(selector) : !string.IsNullOrWhiteSpace(stprop) ? query.OrderBy(stprop).Select(selector) : query.Select(selector);

            if (skip != null && skip <= 0) skip = null;
            if (take != null && take <= 0) take = null;
            if (skip != null) result = result.Skip(skip.Value);
            if (take != null) result = result.Take(take.Value);

            return Task.FromResult(result);
        }

        public virtual async Task<IQueryable<TResult>> SearchAsync<TResult>(
            Expression<Func<TEntity, TResult>> selector = null,
            Expression<Func<TEntity, bool>> predicate = null,
            string criteria = "",
            Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null,
            Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>> include = null,
            int? skip = 0, int? take = null,
            bool disableTracking = false,
            bool ignoreQueryFilters = false,
            bool includeDeleted = false)
        {
            var searchs = criteria.Split(System.Globalization.CultureInfo.CurrentCulture.TextInfo.ListSeparator);
            var parameter = Expression.Parameter(typeof(TEntity));

            Expression searchPredicate = null;
            if (selector != null && selector.Body is NewExpression)
            {
                var args = ((NewExpression)selector.Body).Arguments.OfType<MemberExpression>().ToArray();

                ParameterExpression NestedMember(MemberExpression me)
                {
                    if (me.Expression is ParameterExpression)
                        return (ParameterExpression)me.Expression;
                    else if (me.Expression is MemberExpression)
                        return NestedMember((MemberExpression)me.Expression);
                    else
                        return null;
                }

                parameter = NestedMember(args.First());

                ConstantExpression constant;
                foreach (var search in searchs)
                {
                    foreach (var item in args)
                    {
                        var type = item.Type;
                        object value = null;
                        try
                        {
                            value = Convert.ChangeType(search, type);
                        }
                        catch (Exception)
                        {
                            // ignore
                        }

                        if (value != null && value != type.GetDefault())
                        {
                            constant = Expression.Constant(value);
                            var methods = new[] { "Contains", "Equals", "CompareTo" };
                            foreach (var method in methods)
                            {
                                var methodInfo = type.GetMethod(method, new[] { type });
                                if (methodInfo != null)
                                {
                                    var member = item;
                                    var callExp = Expression.Call(member, methodInfo, constant);
                                    searchPredicate = searchPredicate == null ? (Expression)callExp : Expression.OrElse(searchPredicate, callExp);
                                    break;
                                }
                            }
                        }
                    }
                }
            }

            Expression<Func<TEntity, bool>> expression = null;
            if (searchPredicate != null)
                expression = Expression.Lambda<Func<TEntity, bool>>(searchPredicate, parameter);

            IQueryable<TEntity> query = DbSet;
            if (disableTracking) query = query.AsNoTracking();
            if (include != null) query = include(query);
            if (ignoreQueryFilters) query = query.IgnoreQueryFilters();
            if (predicate != null) query = query.Where(predicate);
            if (expression != null) query = query.Where(expression);

            if (typeof(ISoftDelete).IsAssignableFrom(typeof(TEntity)) && !includeDeleted)
                query = query.Where(m => (m as ISoftDelete).IsDeleted == false);

            var stprop = typeof(TEntity)
                .GetProperties()
                .FirstOrDefault(m => m.PropertyType == typeof(string) || (!typeof(IEnumerable).IsAssignableFrom(m.PropertyType) && !m.PropertyType.IsClass))?.Name;
            var result = orderBy != null ? orderBy(query).Select(selector) : !string.IsNullOrWhiteSpace(stprop) ? query.OrderBy(stprop).Select(selector) : query.Select(selector);

            if (skip != null && skip <= 0) skip = null;
            if (take != null && take <= 0) take = null;
            if (skip != null) result = result.Skip(skip.Value);
            if (take != null) result = result.Take(take.Value);

            return await Task.FromResult(result);
        }

        public virtual async Task<int> CountAsync(Expression<Func<TEntity, bool>> predicate = null)
        {
            predicate = PreparePredicate(predicate);
            if (predicate != null) return await DbSet.CountAsync(predicate);
            return await DbSet.CountAsync();
        }

        public virtual async Task<long> LongCountAsync(Expression<Func<TEntity, bool>> predicate = null)
        {
            predicate = PreparePredicate(predicate);
            if (predicate != null) return await DbSet.LongCountAsync(predicate);
            return await DbSet.LongCountAsync();
        }

        public virtual async Task<bool> AnyAsync(Expression<Func<TEntity, bool>> predicate = null)
        {
            predicate = PreparePredicate(predicate);
            if (predicate != null) return await DbSet.AnyAsync(predicate);
            return await DbSet.AnyAsync();
        }

        public virtual async Task CreateAsync(params TEntity[] entities)
        {
#if ENABLE_BULK
            if (entities.Length < MinRowsToBulk || MinRowsToBulk == 0)
#endif
            {
#if ENABLE_SPLIT
                if (entities.Length < MinRowsToSplit || MinRowsToSplit == 0)
                {
                    await _dbSet.AddRangeAsync(entities);
                }
                else
                {
                    var size = (int)Math.Ceiling(Math.Sqrt(entities.Length));
                    var parts = entities.Split(size);
                    foreach (var item in parts)
                    {
                        await _dbSet.AddRangeAsync(item);
                        await _dbContext.SaveChangesAsync();
                    }

                    return;
                }
#else
                await DbSet.AddRangeAsync(entities);
#endif
            }
#if ENABLE_BULK
            else
                await _dbContext.BulkInsertAsync(entities);
#endif
            await DbContext.SaveChangesAsync();
        }

        public virtual async Task UpdateAsync(params TEntity[] entities)
        {
#if ENABLE_BULK
            if (entities.Length < MinRowsToBulk || MinRowsToBulk == 0)
#endif
            {
#if ENABLE_SPLIT
                if (entities.Length < MinRowsToSplit || MinRowsToSplit == 0)
                {
                    _dbSet.UpdateRange(entities);
                }
                else
                {
                    var size = (int)Math.Ceiling(Math.Sqrt(entities.Length));
                    var parts = entities.Split(size);
                    foreach (var item in parts)
                    {
                        _dbSet.UpdateRange(item);
                        await _dbContext.SaveChangesAsync();
                    }

                    return;
                }
#else
                DbSet.UpdateRange(entities);
#endif
            }
#if ENABLE_BULK
            else
                await _dbContext.BulkUpdateAsync(entities);
#endif
            await DbContext.SaveChangesAsync();
        }

        public virtual async Task DeleteAsync(params object[] keys)
        {
            var list = new List<TEntity>();
            foreach (var item in keys)
            {
                var entity = await DbSet.FindAsync(item);
                if (entity != null)
                    list.Add(entity);
            }

            if (typeof(ISoftDelete).IsAssignableFrom(typeof(TEntity)))
            {
                foreach (var entity in list.Cast<ISoftDelete>())
                {
                    // ReSharper disable once SuspiciousTypeConversion.Global
                    if (entity != null && !((ISoftDelete)entity).IsDeleted)
                    {
                        // ReSharper disable once SuspiciousTypeConversion.Global
                        ((ISoftDelete)entity).IsDeleted = true;
                        // ReSharper disable once SuspiciousTypeConversion.Global
                        ((ISoftDelete)entity).DeletedAt = DateTime.Now;
                    }
                }

                await UpdateAsync(list.ToArray());
                return;
            }

#if ENABLE_BULK
            if (keys.Length < MinRowsToBulk || MinRowsToBulk == 0)
#endif
            DbSet.RemoveRange(list);
#if ENABLE_BULK
            else
                await _dbContext.BulkDeleteAsync(list);
#endif

            await DbContext.SaveChangesAsync();
        }

        public virtual async Task CreateOrUpdateAsync(params TEntity[] entities)
        {
#if ENABLE_BULK
            if (entities.Length < MinRowsToBulk || MinRowsToBulk == 0)
#endif
            foreach (var item in entities)
            {
                if (item.Id.Equals(default))
                    await CreateAsync(item);
                else
                    await UpdateAsync(item);
            }
#if ENABLE_BULK
            else
            {
                await _dbContext.BulkUpdateAsync(entities.Where(m => !m.Id.Equals(default)).ToArray());
                await _dbContext.BulkInsertAsync(entities.Where(m => m.Id.Equals(default)).ToArray());
                await _dbContext.SaveChangesAsync();
            }
#endif
        }

        // * //

        public virtual async Task<TResult> FirstOrDefaultAsync<TResult>(
            Expression<Func<TEntity, TResult>> selector = null,
            Expression<Func<TEntity, bool>> predicate = null,
            Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null,
            Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>> include = null,
            bool disableTracking = false,
            bool ignoreQueryFilters = false,
            bool includeDeleted = false)
        {
            IQueryable<TEntity> query = DbSet;
            if (disableTracking)
                query = query.AsNoTracking();
            if (include != null)
                query = include(query);
            if (predicate != null)
                query = query.Where(predicate);
            if (ignoreQueryFilters)
                query = query.IgnoreQueryFilters();

            if (typeof(ISoftDelete).IsAssignableFrom(typeof(TEntity)) && !includeDeleted)
                query = query.Where(m => (m as ISoftDelete).IsDeleted == false);

            var result = orderBy != null ? orderBy(query).Select(selector) : query.Select(selector);
            return await result.FirstOrDefaultAsync();
        }

        public virtual async Task<TResult> LastOrDefaultAsync<TResult>(
            Expression<Func<TEntity, TResult>> selector = null,
            Expression<Func<TEntity, bool>> predicate = null,
            Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null,
            Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>> include = null,
            bool disableTracking = false,
            bool ignoreQueryFilters = false,
            bool includeDeleted = false)
        {
            IQueryable<TEntity> query = DbSet;
            if (disableTracking)
                query = query.AsNoTracking();
            if (include != null)
                query = include(query);
            if (predicate != null)
                query = query.Where(predicate);
            if (ignoreQueryFilters)
                query = query.IgnoreQueryFilters();

            if (typeof(ISoftDelete).IsAssignableFrom(typeof(TEntity)) && !includeDeleted)
                query = query.Where(m => (m as ISoftDelete).IsDeleted == false);

            query = query.OrderByDescending(m => m.Id);

            var result = orderBy != null ? orderBy(query).Select(selector) : query.Select(selector);
            return await result.FirstOrDefaultAsync();
        }

#if ENABLE_NONASYNC
        public virtual IQueryable<TResult> Read<TResult>(
            Expression<Func<TEntity, TResult>> selector,
            Expression<Func<TEntity, bool>> predicate = null,
            Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null,
            Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>> include = null,
            int? skip = 0, int? take = null,
            bool disableTracking = false,
            bool ignoreQueryFilters = false,
            bool includeDeleted = false)
        {
            IQueryable<TEntity> query = _dbSet;
            if (disableTracking) query = query.AsNoTracking();
            if (include != null) query = include(query);
            if (ignoreQueryFilters) query = query.IgnoreQueryFilters();
            if (predicate != null) query = query.Where(predicate);

            if (typeof(ISoftDelete).IsAssignableFrom(typeof(TEntity)) && !includeDeleted)
                query = query.Where(m => (m as ISoftDelete).IsDeleted == false);

            var stprop = typeof(TEntity)
                .GetProperties()
                .FirstOrDefault(m => m.PropertyType == typeof(string) || (!typeof(IEnumerable).IsAssignableFrom(m.PropertyType) && !m.PropertyType.IsClass))?.Name;
            var result = orderBy != null ? orderBy(query).Select(selector) : !string.IsNullOrWhiteSpace(stprop) ? query.OrderBy(stprop).Select(selector) : query.Select(selector);

            if (skip != null && skip <= 0) skip = null;
            if (take != null && take <= 0) take = null;
            if (skip != null) result = result.Skip(skip.Value);
            if (take != null) result = result.Take(take.Value);

            return result;
        }

        public virtual IQueryable<TResult> Search<TResult>(
            Expression<Func<TEntity, TResult>> selector = null,
            Expression<Func<TEntity, bool>> predicate = null,
            string criteria = "",
            Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null,
            Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>> include = null,
            int? skip = 0, int? take = null,
            bool disableTracking = false,
            bool ignoreQueryFilters = false,
            bool includeDeleted = false)
        {
            var searchs = criteria.Split(System.Globalization.CultureInfo.CurrentCulture.TextInfo.ListSeparator);
            var parameter = Expression.Parameter(typeof(TEntity));

            Expression searchPredicate = null;
            if (selector != null && selector.Body is NewExpression)
            {
                var args = ((NewExpression)selector.Body).Arguments.OfType<MemberExpression>().ToArray();

                ParameterExpression NestedMember(MemberExpression me)
                {
                    if (me.Expression is ParameterExpression)
                        return (ParameterExpression)me.Expression;
                    else if (me.Expression is MemberExpression)
                        return NestedMember((MemberExpression)me.Expression);
                    else
                        return null;
                }

                parameter = NestedMember(args.First());

                ConstantExpression constant;
                foreach (var search in searchs)
                {
                    foreach (var item in args)
                    {
                        var type = item.Type;
                        object value = null;
                        try
                        {
                            value = Convert.ChangeType(search, type);
                        }
                        catch (Exception)
                        {
                            // ignore
                        }

                        if (value != null && value != type.GetDefault())
                        {
                            constant = Expression.Constant(value);
                            var methods = new[] { "Contains", "Equals", "CompareTo" };
                            foreach (var method in methods)
                            {
                                var methodInfo = type.GetMethod(method, new[] { type });
                                if (methodInfo != null)
                                {
                                    var member = item;
                                    var callExp = Expression.Call(member, methodInfo, constant);
                                    searchPredicate = searchPredicate == null ? callExp : Expression.OrElse(searchPredicate, callExp);
                                    break;
                                }
                            }
                        }
                    }
                }
            }

            Expression<Func<TEntity, bool>> expression = null;
            if (searchPredicate != null)
                expression = Expression.Lambda<Func<TEntity, bool>>(searchPredicate, parameter);

            IQueryable<TEntity> query = _dbSet;
            if (disableTracking) query = query.AsNoTracking();
            if (include != null) query = include(query);
            if (ignoreQueryFilters) query = query.IgnoreQueryFilters();
            if (predicate != null) query = query.Where(predicate);
            if (expression != null) query = query.Where(expression);

            if (typeof(ISoftDelete).IsAssignableFrom(typeof(TEntity)) && !includeDeleted)
                query = query.Where(m => (m as ISoftDelete).IsDeleted == false);

            var stprop = typeof(TEntity)
                .GetProperties()
                .FirstOrDefault(m => m.PropertyType == typeof(string) || (!typeof(IEnumerable).IsAssignableFrom(m.PropertyType) && !m.PropertyType.IsClass))?.Name;
            var result = orderBy != null ? orderBy(query).Select(selector) : !string.IsNullOrWhiteSpace(stprop) ? query.OrderBy(stprop).Select(selector) : query.Select(selector);

            if (skip != null && skip <= 0) skip = null;
            if (take != null && take <= 0) take = null;
            if (skip != null) result = result.Skip(skip.Value);
            if (take != null) result = result.Take(take.Value);

            return result;
        }

        public virtual int Count(Expression<Func<TEntity, bool>> predicate = null)
        {
            predicate = PreparePredicate(predicate);
            if (predicate != null) return _dbSet.Count(predicate);
            return _dbSet.Count();
        }

        public virtual long LongCount(Expression<Func<TEntity, bool>> predicate = null)
        {
            predicate = PreparePredicate(predicate);
            if (predicate != null) return _dbSet.LongCount(predicate);
            return _dbSet.LongCount();
        }

        public virtual bool Any(Expression<Func<TEntity, bool>> predicate = null)
        {
            predicate = PreparePredicate(predicate);
            if (predicate != null) return _dbSet.Any(predicate);
            return _dbSet.Any();
        }

        public virtual void Create(params TEntity[] entities)
        {
#if ENABLE_BULK
            if (entities.Length < MinRowsToBulk || MinRowsToBulk == 0)
#endif
            {
#if ENABLE_SPLIT
                if (entities.Length < MinRowsToSplit || MinRowsToSplit == 0)
                {
                    _dbSet.AddRange(entities);
                }
                else
                {
                    var size = (int)Math.Ceiling(Math.Sqrt(entities.Length));
                    var parts = entities.Split(size);
                    foreach (var item in parts)
                    {
                        _dbSet.AddRange(item);
                        _dbContext.SaveChanges();
                    }

                    return;
                }
#else
                _dbSet.AddRange(entities);
#endif
            }
#if ENABLE_BULK
            else
                _dbContext.BulkInsert(entities);
#endif

            _dbContext.SaveChanges();
        }

        public virtual void Update(params TEntity[] entities)
        {
#if ENABLE_BULK
            if (entities.Length < MinRowsToBulk || MinRowsToBulk == 0)
#endif
            {
#if ENABLE_SPLIT
                if (entities.Length < MinRowsToSplit || MinRowsToSplit == 0)
                {
                    _dbSet.UpdateRange(entities);
                }
                else
                {
                    var size = (int)Math.Ceiling(Math.Sqrt(entities.Length));
                    var parts = entities.Split(size);
                    foreach (var item in parts)
                    {
                        _dbSet.UpdateRange(item);
                        _dbContext.SaveChanges();
                    }

                    return;
                }
#else
                _dbSet.UpdateRange(entities);
#endif
            }
#if ENABLE_BULK
            else
                _dbContext.BulkUpdate(entities);
#endif

            _dbContext.SaveChanges();
        }

        public virtual void Delete(params object[] keys)
        {
            var list = new List<TEntity>();
            foreach (var item in keys)
            {
                var entity = _dbSet.Find(item);
                if (entity != null)
                    list.Add(entity);
            }

            if (typeof(ISoftDelete).IsAssignableFrom(typeof(TEntity)))
            {
                foreach (var entity in list.Cast<ISoftDelete>())
                {
                    // ReSharper disable once SuspiciousTypeConversion.Global
                    if (entity != null && !((ISoftDelete)entity).IsDeleted)
                    {
                        // ReSharper disable once SuspiciousTypeConversion.Global
                        ((ISoftDelete)entity).IsDeleted = true;
                        // ReSharper disable once SuspiciousTypeConversion.Global
                        ((ISoftDelete)entity).DeletedAt = DateTime.Now;
                    }
                }

                Update(list.ToArray());
                return;
            }

#if ENABLE_BULK
            if (keys.Length < MinRowsToBulk || MinRowsToBulk == 0)
#endif
                _dbSet.RemoveRange(list);
#if ENABLE_BULK
            else
                _dbContext.BulkDelete(list);
#endif

            _dbContext.SaveChanges();
        }

        public virtual void CreateOrUpdate(params TEntity[] entities)
        {
#if ENABLE_BULK
            if (entities.Length < MinRowsToBulk || MinRowsToBulk == 0)
#endif
                foreach (var item in entities)
                {
                    if (item.Id.Equals(default))
                        Create(item);
                    else
                        Update(item);
                }
#if ENABLE_BULK
            else
            {
                _dbContext.BulkUpdate(entities.Where(m => !m.Id.Equals(default)).ToArray());
                _dbContext.BulkInsert(entities.Where(m => m.Id.Equals(default)).ToArray());
                _dbContext.SaveChanges();
            }
#endif
        }


        // * //


        public virtual TResult FirstOrDefault<TResult>(
            Expression<Func<TEntity, TResult>> selector = null,
            Expression<Func<TEntity, bool>> predicate = null,
            Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null,
            Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>> include = null,
            bool disableTracking = false,
            bool ignoreQueryFilters = false,
            bool includeDeleted = false)
        {
            IQueryable<TEntity> query = _dbSet;
            if (disableTracking)
                query = query.AsNoTracking();
            if (include != null)
                query = include(query);
            if (predicate != null)
                query = query.Where(predicate);
            if (ignoreQueryFilters)
                query = query.IgnoreQueryFilters();

            if (typeof(ISoftDelete).IsAssignableFrom(typeof(TEntity)) && !includeDeleted)
                query = query.Where(m => (m as ISoftDelete).IsDeleted == false);

            var result = orderBy != null ? orderBy(query).Select(selector) : query.Select(selector);
            return result.FirstOrDefault();
        }

        public virtual TResult LastOrDefault<TResult>(
            Expression<Func<TEntity, TResult>> selector = null,
            Expression<Func<TEntity, bool>> predicate = null,
            Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null,
            Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>> include = null,
            bool disableTracking = false,
            bool ignoreQueryFilters = false,
            bool includeDeleted = false)
        {
            IQueryable<TEntity> query = _dbSet;
            if (disableTracking)
                query = query.AsNoTracking();
            if (include != null)
                query = include(query);
            if (predicate != null)
                query = query.Where(predicate);
            if (ignoreQueryFilters)
                query = query.IgnoreQueryFilters();

            if (typeof(ISoftDelete).IsAssignableFrom(typeof(TEntity)) && !includeDeleted)
                query = query.Where(m => (m as ISoftDelete).IsDeleted == false);

            query = query.OrderByDescending(m => m.Id);

            var result = orderBy != null ? orderBy(query).Select(selector) : query.Select(selector);
            return result.FirstOrDefault();
        }

#endif
    }
}
