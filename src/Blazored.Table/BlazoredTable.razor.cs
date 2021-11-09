using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.Linq;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System.Threading.Tasks;

namespace Blazored.Table
{
    public partial class BlazoredTable
    {
        [Inject]
        private IJSRuntime JsRuntime { get; set; }

        [Parameter]
        public string Id { get; set; } = string.Empty;
        
        [Parameter]
        public Type Type { get; set; }
        
        [Parameter]
        public ObservableCollection<dynamic> DataSource { get; set; }

        public string[] Columns => Type.GetProperties().Select(x => x.Name).ToArray();

        public object[] Rows => DataSource?.Select(m => m).ToArray() ?? new object[]{};

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            await base.OnAfterRenderAsync(firstRender);
            if (firstRender)
            {
                await JsRuntime.InvokeVoidAsync("BlazoredTable.create", Id);
            }
        }

    }
}
