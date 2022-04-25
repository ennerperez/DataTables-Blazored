using System;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using DataTables.Blazored.Models;

namespace DataTables.Blazored
{
    public partial class DataTable : IDisposable
    {
        public DataTable()
        {
            Columns = new List<Column>();
        }

        [Parameter] public RenderFragment ChildContent { get; set; }

        [Inject] private IJSRuntime JsRuntime { get; set; }

        public string Id { get; set; } = string.Empty;

        [Parameter] public Func<Request, Task<Result>> OnLoad { get; set; }
        [Parameter] public Func<object, Task<bool>> OnRowSelected { get; set; }
        [Parameter] public IEnumerable<object> DataSource { get; set; }

        [Parameter] public IEnumerable<Definition> ColumnsDefs { get; set; }
        [Parameter] public Settings Settings { get; set; }
        [Parameter] public string Url { get; set; } = string.Empty;
        [Parameter] public string Type { get; set; } = "POST";
        [Parameter] public string ContentType { get; set; } = "application/json";

        private DotNetObjectReference<DataTable> _objRef;

        [Parameter] public List<Column> Columns { get; set; }

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
        public async Task<Result> OnLoadAsync(Request request)
        {
            return await OnLoad(request);
        }

        [JSInvokable]
        public async Task<bool> OnRowSelectedAsync(object tableRequest)
        {
            return await OnRowSelected(tableRequest);
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
            Settings ??= new Settings();
            Settings.Columns = Columns.Select(m => new
            {
                m.Data,
                m.Name,
                m.Title,
                m.Visible,
                m.Orderable,
                m.Searchable,
                m.Type,
                m.Width,
                m.Render,
                m.CreatedCell
            }).ToList();
            var identifier = $"Table.create";
            if (Settings.Columns != null)
            {
                if (OnLoad != null)
                {
                    Settings.ServerSide = true;
                    _objRef ??= DotNetObjectReference.Create(this);
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
