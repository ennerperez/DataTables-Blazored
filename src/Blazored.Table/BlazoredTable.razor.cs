using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.Linq;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Reflection;
using Newtonsoft.Json;
using Blazored.Table.Models;

namespace Blazored.Table
{
    public partial class BlazoredTable
    {
        [Inject]
        private IJSRuntime JsRuntime { get; set; }

        [Parameter]
        public string Id { get; set; } = string.Empty;

        [Parameter]
        public string Method { get; set; } = string.Empty;

        [Parameter]
        public ObservableCollection<object> DataSource { get; set; }

        [Parameter]
        public ObservableCollection<TableColumn> Columns { get; set; }

        [Parameter]
        public TableSettings Settings { get; set; }

        [Parameter]
        public string Url { get; set; } = string.Empty;

        [Parameter]
        public string Type { get; set; } = "POST";

        [Parameter]
        public string ContentType { get; set; } = "application/json";

        private string _entryAssembly;
        private string _executingAssembly;

        public BlazoredTable()
        {
            _executingAssembly = Assembly.GetExecutingAssembly()?.GetName().Name;
            _entryAssembly = Assembly.GetEntryAssembly()?.GetName().Name;
        }

        protected override async Task OnInitializedAsync()
        {
            if (string.IsNullOrWhiteSpace(Id))
                Id = Guid.NewGuid().ToString();

            if (Settings == null)
                Settings = new TableSettings()
                {
                    Columns = Columns,
                    Ordering = true,
                    DeferRender = true,
                    Scroller = true,
                    ScrollY = "350px",
                    ServerSide = false
                };

            await base.OnInitializedAsync();
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            await base.OnAfterRenderAsync(firstRender);
            if (firstRender)
            {
                var identifier = $"BlazoredTable.create";

                if (Settings.Columns != null)
                {
                    if (!string.IsNullOrWhiteSpace(Method))
                    {
                        Settings.ServerSide = true;
                        await JsRuntime.InvokeVoidAsync(identifier, Id, Settings, _entryAssembly, Method, null, null);
                    }
                    else if (DataSource != null)
                    {
                        Settings.ServerSide = false;
                        await JsRuntime.InvokeVoidAsync(identifier, Id, Settings, _entryAssembly, null, null, DataSource);
                    }
                    else if (!string.IsNullOrWhiteSpace(Url))
                    {
                        Settings.ServerSide = true;
                        Settings.Processing = true;
                        await JsRuntime.InvokeVoidAsync(identifier, Id, Settings, _entryAssembly, null, new { Url, Type, ContentType }, null);
                    }
                    else
                    {
                        await JsRuntime.InvokeVoidAsync(identifier, Id, Settings, _entryAssembly, null, null, null);
                    }
                }
            }
        }
    }
}
