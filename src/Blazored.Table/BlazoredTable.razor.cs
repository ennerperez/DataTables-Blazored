using Blazored.Table.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Routing;
using Microsoft.JSInterop;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace Blazored.Table
{
    public partial class BlazoredTable
    {
        [Inject] private NavigationManager NavigationManager { get; set; }
        [Inject] private IJSRuntime JsRuntime { get; set; }

        [CascadingParameter] private ITableService CascadedTableService { get; set; }

        [Parameter] public string Class { get; set; }

        private readonly Collection<TableReference> _tables = new Collection<TableReference>();
        private readonly TableOptions _globalTableOptions = new TableOptions();

        protected override void OnInitialized()
        {
            if (CascadedTableService == null)
            {
                throw new InvalidOperationException($"{GetType()} requires a cascading parameter of type {nameof(ITableService)}.");
            }

            NavigationManager.LocationChanged += CancelTables;

            _globalTableOptions.Class = Class;

        }

        internal async void CloseInstance(TableReference table, TableResult result)
        {
            if (table.TableInstanceRef != null)
            {
                // Gracefully close the table
                await table.TableInstanceRef.CloseAsync(result);
            }
            else
            {
                await DismissInstance(table, result);
            }
        }

        internal void CloseInstance(Guid id)
        {
            var reference = GetTableReference(id);
            CloseInstance(reference, TableResult.Ok<object>(null));
        }

        internal void CancelInstance(Guid id)
        {
            var reference = GetTableReference(id);
            CloseInstance(reference, TableResult.Cancel());
        }

        internal Task DismissInstance(Guid id, TableResult result)
        {
            var reference = GetTableReference(id);
            return DismissInstance(reference, result);
        }

        internal async Task DismissInstance(TableReference table, TableResult result)
        {
            if (table != null)
            {
                await JsRuntime.InvokeVoidAsync("BlazoredTable.deactivateFocusTrap", table.Id);
                table.Dismiss(result);
                _tables.Remove(table);
                await InvokeAsync(StateHasChanged);
            }
        }

        private async void CancelTables(object sender, LocationChangedEventArgs e)
        {
            foreach (var tableReference in _tables.ToList())
            {
                await Task.Yield();
                tableReference.Dismiss(TableResult.Cancel());
            }

            _tables.Clear();
            await InvokeAsync(StateHasChanged);
        }

        private async void Update(TableReference tableReference)
        {
            await Task.Yield();
            _tables.Add(tableReference);
            await InvokeAsync(StateHasChanged);
        }

        private TableReference GetTableReference(Guid id)
        {
            return _tables.SingleOrDefault(x => x.Id == id);
        }
    }
}
