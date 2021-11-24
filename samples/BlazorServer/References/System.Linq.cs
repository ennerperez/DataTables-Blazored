using System.Collections;
using System.Reflection;

namespace System.Linq
{
    namespace Expressions
    {
        public static class Extensions
        {
            public static Expression<Func<TEntityType, TResult>> Select<TEntityType, TResult>(this TEntityType type, Expression<Func<TEntityType, TResult>> selector)
            {
                return selector;
            }

            public static IOrderedQueryable<TEntityType> SortDynamically<TEntityType>(this IQueryable<TEntityType> query, params string[][] args)
            {
                IOrderedQueryable<TEntityType> result = null;
                if (args != null)
                {
                    var typeParams = new[] { Expression.Parameter(typeof(TEntityType), "") };
                    var props = typeof(TEntityType).GetProperties()
                        .Where(m=> m.PropertyType == typeof(string) || (!typeof(IEnumerable).IsAssignableFrom(m.PropertyType) && !m.PropertyType.IsClass))
                        .ToArray();
                    if (args.Length == 0)
                    {
                        args = new[] { new[] { props.First().Name, "asc" } };
                    }

                    foreach (var item in args.Where(m => m.Length > 0))
                    {
                        var pn = item[0];
                        var pi = props.FirstOrDefault(m => m.Name.ToLower() == pn.ToLower());
                        if (pi == null) pi = props[0];
                        var direction = (item.Length > 1) ? item[1] : "asc";
                        result = (IOrderedQueryable<TEntityType>)query.Provider.CreateQuery(
                            Expression.Call(
                                typeof(Queryable),
                                direction == "asc" ? "OrderBy" : "OrderByDescending",
                                new[] { typeof(TEntityType), pi.PropertyType },
                                result != null ? result.Expression : query.Expression,
                                Expression.Lambda(Expression.Property(typeParams[0], pi), typeParams))
                        );
                    }
                }

                return result;
            }

            public static Expression<Func<T, TE>> GetPropertySelector<T, TE>(this Type @this, string propertyName)
            {
                var arg = Expression.Parameter(typeof(T), "x");
                var property = Expression.Property(arg, propertyName);
                //return the property as object
                var conv = Expression.Convert(property, typeof(TE));
                var exp = Expression.Lambda<Func<T, TE>>(conv, new ParameterExpression[] { arg });
                return exp;
            }

            public static Expression<Func<T, string>> GetPropertySelector<T>(this Type @this, PropertyInfo propertyInfo)
            {
                var arg = Expression.Parameter(typeof(T), "x");
                var property = Expression.Property(arg, propertyInfo.Name);
                //return the property as object
                var conv = Expression.Convert(property, typeof(string));
                var exp = Expression.Lambda<Func<T, string>>(conv, new ParameterExpression[] { arg });
                return exp;
            }

        }
    }
}