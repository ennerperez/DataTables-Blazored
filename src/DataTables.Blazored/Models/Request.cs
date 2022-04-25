namespace DataTables.Blazored.Models
{
    public class Request
    {
        public Request()
        {
            Columns = new Column[0];
            Orders = new Order[0];
            Filters = new Filter[0];
            Search = new Search();
        }

        public int Draw { get; set; }

        public Column[] Columns { get; set; }

        public Order[] Orders { get; set; }

        public int Start { get; set; }

        public int Length { get; set; }

        public Search Search { get; set; }

        public Filter[] Filters { get; set; }
    }
}
