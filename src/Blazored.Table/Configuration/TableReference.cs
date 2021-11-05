using Blazored.Table.Services;
using Microsoft.AspNetCore.Components;
using System;
using System.Threading.Tasks;

namespace Blazored.Table
{
    public class TableReference : ITableReference
    {
        private readonly TaskCompletionSource<TableResult> _resultCompletion = new TaskCompletionSource<TableResult>();
        private readonly Action<TableResult> _closed;
        private readonly TableService _tableService;

        public TableReference(Guid tableInstanceId, RenderFragment tableInstance, TableService tableService)
        {
            Id = tableInstanceId;
            TableInstance = tableInstance;
            _closed = HandleClosed;
            _tableService = tableService;
        }

        private void HandleClosed(TableResult obj)
        {
            _ = _resultCompletion.TrySetResult(obj);
        }

        internal Guid Id { get; }
        internal RenderFragment TableInstance { get; }
        internal BlazoredTableInstance TableInstanceRef { get; set; }

        public Task<TableResult> Result => _resultCompletion.Task;

        internal void Dismiss(TableResult result)
        {
            _closed.Invoke(result);
        }
    }
}
