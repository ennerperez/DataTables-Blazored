using System;
using System.Collections.Generic;
// ReSharper disable InconsistentNaming

namespace BlazorServer.Models
{
    public class AjaxViewModel
    {

        public AjaxViewModel()
        {
            Columns = new ColumnViewModel[0];
            Order = new ColumnOrderViewModel[0];
            Filters = new ColumnFilterViewModel[0];
        }
        
        public int Draw { get; set; }

        public ColumnViewModel[] Columns { get; set; }

        public ColumnOrderViewModel[] Order { get; set; }

        public int Start { get; set; }

        public int Length { get; set; }

        public SearchViewModel Search { get; set; }

        public ColumnFilterViewModel[] Filters { get; set; }
    }

    public class ColumnViewModel
    {
        public string Data { get; set; }

        public string Name { get; set; }

        public bool Searchable { get; set; }

        public bool Orderable { get; set; }

        public SearchViewModel Search { get; set; }
    }

    public class SearchViewModel
    {
        public string Value { get; set; }

        public bool Regex { get; set; }
    }

    public class ColumnOrderViewModel
    {
        public int Column { get; set; }

        public string Dir { get; set; }
    }

    public class ColumnFilterViewModel
    {
        public string Column { get; set; }

        public string Value { get; set; }
    }
    
    public class AjaxResult<TResult>
    {
        public int iTotalRecords { get; set; }
        public int iTotalDisplayRecords { get; set; }
        public List<TResult> aaData { get; set; }
    }

    public class AjaxResult : AjaxResult<object>
    {
        
    }
}
