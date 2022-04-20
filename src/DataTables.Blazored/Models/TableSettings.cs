using System.Collections.Generic;

namespace DataTables.Blazored.Models
{
    public class TableSettings
    {
        public bool? Ordering { get; set; }
        public string ScrollY { get; set; }
        public bool? ScrollCollapse { get; set; }
        public bool? DeferRender { get; set; }
        public bool? Scroller { get; set; }
        public bool? ServerSide { get; set; }
        public IEnumerable<TableColumn> Columns { get; set; }
        public IEnumerable<TableColumnDefs> ColumnsDefs { get; set; }
        public bool? Processing { get; set; }
        public bool? Responsive { get; set; }
    }
}
