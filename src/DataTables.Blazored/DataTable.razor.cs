using System;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System.Threading.Tasks;
using System.Collections.Generic;
using DataTables.Blazored.Models;

namespace DataTables.Blazored
{
    public partial class DataTable : IDisposable
    {
        [Inject]
        private IJSRuntime JsRuntime { get; set; }

        public string Id { get; set; } = string.Empty;

        [Parameter]
        public Func<TableRequestViewModel, Task<TableResult>> OnLoad { get; set; }

        [Parameter]
        public IEnumerable<object> DataSource { get; set; }

        [Parameter]
        public List<Column> Columns { get; set; }

        [Parameter]
        public TableSettings Settings { get; set; }

        [Parameter]
        public string Url { get; set; } = string.Empty;

        [Parameter]
        public string Type { get; set; } = "POST";

        [Parameter]
        public string ContentType { get; set; } = "application/json";

        private DotNetObjectReference<DataTable> _objRef;

        protected override async Task OnInitializedAsync()
        {
            if (string.IsNullOrWhiteSpace(Id))
                Id = Guid.NewGuid().ToString();

            await base.OnInitializedAsync();
        }

        public async Task Reload()
        {
            await JsRuntime.InvokeVoidAsync("Table.reload");
        }

        [JSInvokable]
        public async Task<TableResult> OnLoadAsync(TableRequestViewModel tableRequest)
        {
            return await OnLoad(tableRequest);
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                await InitializeTable();
            }

            await base.OnAfterRenderAsync(firstRender);
        }

        private async Task InitializeTable()
        {
            Settings = new TableSettings()
            {
                Columns = Columns,
                Ordering = true,
                DeferRender = true,
                Scroller = true,
                ScrollY = "350px",
                ServerSide = false
            };

            var identifier = $"Table.create";

            if (Settings.Columns != null)
            {
                if (OnLoad != null)
                {
                    Settings.ServerSide = true;
                    if(_objRef == null)
                        _objRef = DotNetObjectReference.Create(this);
                    await JsRuntime.InvokeVoidAsync(identifier, Id, Settings, null, null, _objRef);
                }
                else if (DataSource != null)
                {
                    Settings.ServerSide = false;
                    await JsRuntime.InvokeVoidAsync(identifier, Id, Settings, null, DataSource, null);
                }
                else if (!string.IsNullOrWhiteSpace(Url))
                {
                    Settings.ServerSide = true;
                    Settings.Processing = true;
                    await JsRuntime.InvokeVoidAsync(identifier, Id, Settings, new { Url, Type, ContentType }, null, null);
                }
                else
                {
                    await JsRuntime.InvokeVoidAsync(identifier, Id, Settings, null, null, null);
                }
            }
        }

        public void Dispose()
        {
            _objRef?.Dispose();
        }
    }
}
