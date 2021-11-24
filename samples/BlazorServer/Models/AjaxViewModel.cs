using System;
namespace BlazorServer.Models
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
}
