using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;
using DataTables.Blazored.Models;
using BlazorShared.Data.Contexts;
using BlazorShared.Data.Entities;
using BlazorServer.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;

namespace BlazorServer.Services
{
    public class EmployeeRepository : IEmployeeRepository
    {
        private readonly DefaultContext _context;

        public EmployeeRepository(DefaultContext context)
        {
            _context = context;
        }

        public async Task<TableResult> GetDataAsync<TResult>(TableRequestViewModel model, Expression<Func<Employee, TResult>> selector = null)
        {
            var dbSet = _context.Set<Employee>();
            object rows;

            var props = typeof(Employee).GetProperties().ToArray();
            var orderBy = new Dictionary<string, string>();
            foreach (var item in model.Order)
            {
                var column = model.Columns[item.Column];
                var prop = props.FirstOrDefault(m => m.Name.ToLower() == column.Name.ToLower());
                if (prop != null) orderBy.Add(prop.Name, item.Dir);
            }

            var order = orderBy.Select(m => new[] {m.Key, m.Value}).ToArray();

            Expression predicate = null;
            ParameterExpression parameter;

            var filters = model.Columns.Where(m => m.Searchable && m.Search != null && !string.IsNullOrWhiteSpace(m.Search.Value))
                .Select(column =>
                {
                    var prop = props.FirstOrDefault(m => m.Name.ToLower() == column.Name.ToLower());
                    if (prop != null)
                        return new {Property = prop, column.Name, column.Search.Value};

                    return null;
                });

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
                                predicate = predicate == null ? (Expression)callExp : Expression.AndAlso(predicate, callExp);
                                break;
                            }
                        }
                    }
                }
            }

            Expression<Func<Employee, bool>> expression = null;
            if (predicate != null)
                expression = Expression.Lambda<Func<Employee, bool>>(predicate, parameter);

            if (model.Search != null && !string.IsNullOrWhiteSpace(model.Search.Value))
            {
                rows = await dbSet.SearchAsync(selector, expression, model.Search.Value, o => o.SortDynamically(order),
                    skip: model.Start, take: model.Length);
            }
            else
            {
                rows = await dbSet.ReadAsync(selector, expression, o => o.SortDynamically(order),
                    skip: model.Start, take: model.Length);
            }

            var data = await ((IQueryable<object>)rows).ToListAsync();

            var total = await dbSet.CountAsync();
            var isFiltered = (model.Search != null && !string.IsNullOrWhiteSpace(model.Search.Value));
            var stotal = isFiltered ? data.Count : total;

            return new TableResult {iTotalRecords = total, iTotalDisplayRecords = stotal, aaData = data};
        }
    }

    public static class Extensions
    {
        #region References

        public static async Task<IQueryable<TResult>> SearchAsync<TResult, TEntity>(
            this DbSet<TEntity> dbSet,
            Expression<Func<TEntity, TResult>> selector = null,
            Expression<Func<TEntity, bool>> predicate = null,
            string criteria = "",
            Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null,
            Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>> include = null,
            int? skip = 0, int? take = null,
            bool disableTracking = false,
            bool ignoreQueryFilters = false,
            bool includeDeleted = false) where TEntity : class
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

            IQueryable<TEntity> query = dbSet;
            if (disableTracking) query = query.AsNoTracking();
            if (include != null) query = include(query);
            if (ignoreQueryFilters) query = query.IgnoreQueryFilters();
            if (predicate != null) query = query.Where(predicate);
            if (expression != null) query = query.Where(expression);

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


        public static Task<IQueryable<TResult>> ReadAsync<TResult, TEntity>(
            this DbSet<TEntity> dbSet,
            Expression<Func<TEntity, TResult>> selector,
            Expression<Func<TEntity, bool>> predicate = null,
            Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null,
            Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>> include = null,
            int? skip = 0, int? take = null,
            bool disableTracking = false,
            bool ignoreQueryFilters = false,
            bool includeDeleted = false) where TEntity : class
        {
            IQueryable<TEntity> query = dbSet;
            if (disableTracking) query = query.AsNoTracking();
            if (include != null) query = include(query);
            if (ignoreQueryFilters) query = query.IgnoreQueryFilters();
            if (predicate != null) query = query.Where(predicate);

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

        #endregion
    }
}
