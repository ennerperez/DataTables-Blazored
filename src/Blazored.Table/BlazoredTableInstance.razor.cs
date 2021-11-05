using Blazored.Table.Services;
using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.JSInterop;
using System.Diagnostics.CodeAnalysis;

namespace Blazored.Table
{
    public partial class BlazoredTableInstance
    {
        [Inject] private IJSRuntime JsRuntime { get; set; }
        [CascadingParameter] private BlazoredTable Parent { get; set; }
        [CascadingParameter] private TableOptions GlobalTableOptions { get; set; }

        [Parameter] public TableOptions Options { get; set; }
        [Parameter] public string Title { get; set; }
        [Parameter] public RenderFragment Content { get; set; }
        [Parameter] public Guid Id { get; set; }

        private string Class { get; set; }


        [SuppressMessage("Style", "IDE0044:Add readonly modifier", Justification = "This is assigned in Razor code and isn't currently picked up by the tooling.")]
#pragma warning disable 169
        private ElementReference _tableReference;
#pragma warning restore 169

        // Temporarily add a tabindex of -1 to the close button so it doesn't get selected as the first element by activateFocusTrap
        private readonly Dictionary<string, object> _closeBtnAttributes = new Dictionary<string, object> { { "tabindex", "-1" } };

        protected override void OnInitialized()
        {
            ConfigureInstance();
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                await Task.Yield();
                _closeBtnAttributes.Clear();
                StateHasChanged();
            }
        }

        /// <summary>
        /// Sets the title for the table being displayed
        /// </summary>
        /// <param name="title">Text to display as the title of the table</param>
        public void SetTitle(string title)
        {
            Title = title;
            StateHasChanged();
        }

        /// <summary>
        /// Closes the table with a default Ok result />.
        /// </summary>
        public async Task CloseAsync()
        {
            await CloseAsync(TableResult.Ok<object>(null));
        }

        /// <summary>
        /// Closes the table with the specified <paramref name="tableResult"/>.
        /// </summary>
        /// <param name="tableResult"></param>
        public async Task CloseAsync(TableResult tableResult)
        {
            await Parent.DismissInstance(Id, tableResult);
        }

        /// <summary>
        /// Closes the table and returns a cancelled TableResult.
        /// </summary>
        public async Task CancelAsync()
        {
            await CloseAsync(TableResult.Cancel());
        }

        private void ConfigureInstance()
        {
            Class = SetClass();
        }
        
        private string SetClass()
        {
            var tableClass = string.Empty;

            if (!string.IsNullOrWhiteSpace(Options.Class))
                tableClass = Options.Class;

            if (string.IsNullOrWhiteSpace(tableClass) && !string.IsNullOrWhiteSpace(GlobalTableOptions.Class))
                tableClass = GlobalTableOptions.Class;

            if (string.IsNullOrWhiteSpace(tableClass))
                tableClass = "blazored-table";

            return tableClass;
        }

    }
}
