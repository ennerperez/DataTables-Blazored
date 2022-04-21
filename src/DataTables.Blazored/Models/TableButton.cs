using System;
using System.Collections.Generic;
using System.Text;

namespace DataTables.Blazored.Models
{
    public class TableColumnDefs
    {
        public string Targets { get; set; }

        public bool Visible { get; set; }

        public object Data { get; set; }

        public string DefaultContent { get; set; }

        public string ClassName { get; set; }
    }

}
