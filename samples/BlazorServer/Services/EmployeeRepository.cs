using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;
using BlazorServer.Data.Contexts;
using BlazorServer.Data.Entities;
using BlazorServer.Interfaces;
using BlazorServer.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace BlazorServer.Services
{
    public class EmployeeRepository : GenericRepository<Employee>, IEmployeeRepository
    {
        public EmployeeRepository(DefaultContext context, ILoggerFactory logger, IConfiguration configuration) : base(context, logger, configuration)
        {
        }
        
        public async Task<JsonResult> GetDataAsync<TResult>([FromBody]AjaxViewModel model, Expression<Func<Employee, TResult>> selector)
        {
            object rows;

            var props = typeof(Employee).GetProperties().ToArray();
            var orderBy = new Dictionary<string, string>();
            foreach (var item in model.Order)
            {
                var column = model.Columns[item.Column];
                var prop = props.FirstOrDefault(m => m.Name.ToLower() == column.Name.ToLower());
                if (prop != null) orderBy.Add(prop.Name, item.Dir);
            }

            var order = orderBy.Select(m => new[] { m.Key, m.Value }).ToArray();

            Expression predicate = null;
            ParameterExpression parameter;

            var filters = model.Columns.Where(m => m.Searchable && m.Search != null && !string.IsNullOrWhiteSpace(m.Search.Value))
                .Select(column =>
                {
                    var prop = props.FirstOrDefault(m => m.Name.ToLower() == column.Name.ToLower());
                    if (prop != null)
                        return new { Property = prop, column.Name, column.Search.Value };

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
                        var methods = new[] { "Contains", "Equals", "CompareTo" };
                        foreach (var method in methods)
                        {
                            var methodInfo = type.GetMethod(method, new[] { type });
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
                rows = await SearchAsync(selector, expression, model.Search.Value, o => o.SortDynamically(order),
                    skip: model.Start, take: model.Length);
            }
            else
            {
                rows = await ReadAsync(selector, expression, o => o.SortDynamically(order),
                    skip: model.Start, take: model.Length);
            }

            var data = await ((IQueryable<object>)rows).ToListAsync();

            var total = await CountAsync();
            var isFiltered = (model.Search != null && !string.IsNullOrWhiteSpace(model.Search.Value));
            var stotal = isFiltered ? data.Count : total;

            return new JsonResult(new { iTotalRecords = total, iTotalDisplayRecords = stotal, aaData = data });
        }
    }
}
