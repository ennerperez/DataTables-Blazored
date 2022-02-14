namespace Blazored.Table.Models
{
    public class TableRequestViewModel
    {
        public TableRequestViewModel()
        {
            Columns = new ColumnViewModel[0];
            Order = new ColumnOrderViewModel[0];
            Filters = new ColumnFilterViewModel[0];
            Search = new SearchViewModel();
        }

        public int Draw { get; set; }

        public ColumnViewModel[] Columns { get; set; }

        public ColumnOrderViewModel[] Order { get; set; }

        public int Start { get; set; }

        public int Length { get; set; }

        public SearchViewModel Search { get; set; }

        public ColumnFilterViewModel[] Filters { get; set; }
    }
}
