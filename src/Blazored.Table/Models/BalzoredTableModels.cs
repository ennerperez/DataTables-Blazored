using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Blazored.Table.Models
{
    public class AjaxViewModel
    {
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

    public class TableColumn
    {
        public string title { get; set; }
        public string data { get; set; }
        public string name { get; set; }
        
        public string format { get; set; }
    }

    public class Settings
    {
        public bool? ordering { get; set; }
        public string scrollY { get; set; }
        public bool? scrollCollapse { get; set; }
        public bool? deferRender { get; set; }
        public bool? scroller { get; set; }
        public bool? serverSide { get; set; }
        //public AjaxObj ajax { get; set; }
        //public ObservableCollection<object> data { get; set; }
        public ObservableCollection<TableColumn> columns { get; set; }
    }

    public class AjaxObj
    {
        public string url { get; set; }
        public string type { get; set; }
        public string contentType { get; set; }
     }

    public class DataResult : DataResult<object>
    {

    }

    public class DataResult<T> where T : class
    {
        public int iTotalRecords { get; set; }
        public int iTotalDisplayRecords { get; set; }
        public IEnumerable<T> aaData { get; set; }
    }
}
