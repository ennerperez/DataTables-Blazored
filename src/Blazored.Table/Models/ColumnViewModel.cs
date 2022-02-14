namespace Blazored.Table.Models
{
    public class ColumnViewModel
    {
        public string Data { get; set; }

        public string Name { get; set; }

        public bool Searchable { get; set; }

        public bool Orderable { get; set; }

        public SearchViewModel Search { get; set; }
    }
}
