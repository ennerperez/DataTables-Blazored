namespace Blazored.Table.Models
{
    public class TableRequestViewModel
    {
        public int Draw { get; set; }

        public ColumnViewModel[] Columns { get; set; }

        public ColumnOrderViewModel[] Order { get; set; }

        public int Start { get; set; }

        public int Length { get; set; }

        public SearchViewModel Search { get; set; }

        public ColumnFilterViewModel[] Filters { get; set; }
    }
}
