﻿using Microsoft.AspNetCore.Components;
using DataTables.Blazored.Interop;

namespace DataTables.Blazored
{
    public partial class Column
    {

        public Column()
        {
            Searchable = true;
            Orderable = true;
            Visible = true;
        }
        
        [CascadingParameter]
        public DataTable Table { get; set; }

        [Parameter]
        public string Title { get; set; }

        [Parameter]
        public string Name { get; set; }

        [Parameter]
        public string Data { get; set; }

        [Parameter]
        public string DefaultContent { get; set; }
        
        [Parameter]
        public string ClassName { get; set; }
        
        [Parameter]
        public string CellType { get; set; }
        
        [Parameter]
        public string ContentPadding { get; set; }
        
        [Parameter]
        public bool Searchable { get; set; }
        
        [Parameter]
        public bool Orderable { get; set; }
        
        [Parameter]
        public string OrderData { get; set; }
        
        [Parameter]
        public string OrderDataType { get; set; }
        
        [Parameter]
        public JSFunction CreatedCell { get; set; }
        
        [Parameter]
        public JSFunction Render { get; set; }
        
        [Parameter]
        public string Type { get; set; }
        
        [Parameter]
        public bool Visible { get; set; }
        
        [Parameter]
        public string Width { get; set; }

        protected override void OnInitialized() => Table.Columns.Add(this);

    }
}