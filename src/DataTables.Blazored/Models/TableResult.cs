using System.Collections.Generic;

namespace DataTables.Blazored.Models
{
    public class TableResult : TableResult<object>
    {
    }

    public class TableResult<T> where T : class
    {
        // ReSharper disable once InconsistentNaming
        public int iTotalRecords { get; set; }

        // ReSharper disable once InconsistentNaming
        public int iTotalDisplayRecords { get; set; }

        // ReSharper disable once InconsistentNaming
        public IEnumerable<T> aaData { get; set; }
    }
}
