using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.Linq;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System.Threading.Tasks;
using System.Collections.Generic;
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
        public Type? Type { get; set; }

        /*[Parameter]
        public ObservableCollection<dynamic> DataSource { get; set; }

        public string[] Columns => Type.GetProperties().Select(x => x.Name).ToArray();

        public string DatatableColumns => JsonConvert.SerializeObject(Type.GetProperties().Select(x => new TableColumn() { title = x.Name, data = x.Name }));

        public object[] Rows => DataSource?.Select(m => m).ToArray() ?? new object[] { };*/

        [Parameter]
        public string AjaxUrl { get; set; } = string.Empty;

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            await base.OnAfterRenderAsync(firstRender);
            if (firstRender)
            {
                var s = new Settings() {
                    columns = new[] { new TableColumn() { data = "first_name" }, new TableColumn() { data = "last_name" } },
                    ordering = true,
                    deferRender = false,
                    scroller = false,
                    scrollCollapse = false,
                    serverSide = true,
                    ajax = new AjaxObj() { url = AjaxUrl }
                };
                await JsRuntime.InvokeVoidAsync("BlazoredTable.create", Id, s);
            }
        }



    }
}
