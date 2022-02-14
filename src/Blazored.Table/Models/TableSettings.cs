using System.Collections.Generic;

namespace Blazored.Table.Models
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
    }
}
