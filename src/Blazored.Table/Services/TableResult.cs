using System;

namespace Blazored.Table.Services
{
    public class TableResult
    {
        public object Data { get; }
        public Type DataType { get; }
        public bool Cancelled { get; }

        protected TableResult(object data, Type resultType, bool cancelled)
        {
            Data = data;
            DataType = resultType;
            Cancelled = cancelled;
        }

        public static TableResult Ok<T>(T result) => Ok(result, default);

        public static TableResult Ok<T>(T result, Type tableType) => new TableResult(result, tableType, false);

        public static TableResult Cancel() => new TableResult(default, typeof(object), true);
    }
}
