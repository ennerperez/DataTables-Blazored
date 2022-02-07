using Microsoft.EntityFrameworkCore.Query;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections;
using Blazored.Table.Models;
using BlazorServer.Interfaces;

// ReSharper disable UnusedTypeParameter
// ReSharper disable RedundantCast
// ReSharper disable AssignNullToNotNullAttribute

namespace BlazorServer.Repositories
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
                    var methodInfo = type?.GetMethod("Equals", new[] {type});
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
                            var methods = new[] {"Contains", "Equals", "CompareTo"};
                            foreach (var method in methods)
                            {
                                var methodInfo = type.GetMethod(method, new[] {type});
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
            await DbSet.AddRangeAsync(entities);
            await DbContext.SaveChangesAsync();
        }

        public virtual async Task UpdateAsync(params TEntity[] entities)
        {
            DbSet.UpdateRange(entities);
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

            DbSet.RemoveRange(list);
            await DbContext.SaveChangesAsync();
        }

        public virtual async Task CreateOrUpdateAsync(params TEntity[] entities)
        {
            foreach (var item in entities)
            {
                if (item.Id.Equals(default))
                    await CreateAsync(item);
                else
                    await UpdateAsync(item);
            }
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

        public virtual async Task<DataResult> DataAsync<TResult>(AjaxViewModel model, Expression<Func<TEntity, TResult>> selector, Expression<Func<TEntity, bool>> predicate = null, Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>> include = null)
        {
            object rows;

            var props = typeof(TEntity).GetProperties().ToArray();
            var orderBy = new Dictionary<string, string>();
            if (model.Order != null && model.Columns != null)
            {
                foreach (var item in model.Order)
                {
                    var column = model.Columns[item.Column];
                    var prop = props.FirstOrDefault(m => m.Name.ToLower() == column.Name.ToLower());
                    if (prop != null) orderBy.Add(prop.Name, item.Dir);
                }
            }

            var order = orderBy.Select(m => new[] {m.Key, m.Value}).ToArray();

            Expression predicateExpression = null;
            ParameterExpression parameter = null;

            ParameterExpression NestedMember(MemberExpression me)
            {
                if (me.Expression is ParameterExpression)
                    return (ParameterExpression)me.Expression;
                else if (me.Expression is MemberExpression)
                    return NestedMember((MemberExpression)me.Expression);
                else
                    return null;
            }

            if (model.Columns != null)
            {
                var filters = model.Columns.Where(m => m.Searchable && m.Search != null && !string.IsNullOrWhiteSpace(m.Search.Value))
                    .Select(column =>
                    {
                        var prop = props.FirstOrDefault(m => m.Name.ToLower() == column.Name.ToLower());
                        if (prop != null)
                            return new {Property = prop, column.Name, column.Search.Value};

                        return null;
                    });

                var args = ((NewExpression)selector.Body).Arguments.OfType<MemberExpression>().ToArray();
                parameter = NestedMember(args.First());

                ConstantExpression constant;
                foreach (var filter in filters)
                {
                    foreach (var item in args.Where(m => filter != null && m.Member.Name.ToLower() == filter.Name.ToLower()))
                    {
                        var type = filter.Property.PropertyType;
                        object value = null;
                        try
                        {
                            value = Convert.ChangeType(filter.Value, type);
                        }
                        catch (Exception)
                        {
                            // ignore
                        }

                        if (value != null && value != type.GetDefault())
                        {
                            constant = Expression.Constant(value);
                            var methods = new[] {"Contains", "Equals", "CompareTo"};
                            foreach (var method in methods)
                            {
                                var methodInfo = type.GetMethod(method, new[] {type});
                                if (methodInfo != null)
                                {
                                    var member = item;
                                    var callExp = Expression.Call(member, methodInfo, constant);
                                    predicateExpression = predicateExpression == null ? (Expression)callExp : Expression.AndAlso(predicateExpression, callExp);
                                    break;
                                }
                            }
                        }
                    }
                }
            }

            Expression<Func<TEntity, bool>> expression = null;
            if (predicateExpression != null)
            {
                if (predicate != null)
                    predicateExpression = Expression.AndAlso(predicate, predicateExpression);

                expression = Expression.Lambda<Func<TEntity, bool>>(predicateExpression, parameter);
            }
            else
            {
                if (predicate != null)
                {
                    expression = predicate;
                }
            }

            if (model.Search != null && !string.IsNullOrWhiteSpace(model.Search.Value))
            {
                rows = await SearchAsync(selector, expression, model.Search.Value, o => o.SortDynamically(order), include: include,
                    skip: model.Start, take: model.Length);
            }
            else
            {
                rows = await ReadAsync(selector, expression, o => o.SortDynamically(order), include: include,
                    skip: model.Start, take: model.Length);
            }

            var data = await ((IQueryable<object>)rows).ToListAsync();

            var total = await CountAsync();
            var isFiltered = (model.Search != null && !string.IsNullOrWhiteSpace(model.Search.Value));
            var stotal = isFiltered ? data.Count : total;

            return new DataResult() {iTotalRecords = total, iTotalDisplayRecords = stotal, aaData = data};
        }
    }
}
