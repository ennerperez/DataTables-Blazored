using System.Collections.Generic;

namespace DataTables.Blazored.Models
{
    public class Settings
    {

        public Settings()
        {
            Ordering = true;
            DeferRender = true;
        }
        public bool Ordering { get; set; }
        public string ScrollY { get; set; }
        
        public bool ScrollCollapse { get; set; }
        public bool DeferRender { get; set; }
        public bool Scroller { get; set; }
        public bool ServerSide { get; set; }
        public IEnumerable<object> Columns { get; set; }
        public bool Processing { get; set; }
        
        public bool Responsive { get; set; }
    }
}
