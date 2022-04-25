using System;

namespace DataTables.Blazored.Models
{
    [Obsolete]
    public class Definition
    {
        public string Targets { get; set; }

        public bool Visible { get; set; }

        public object Data { get; set; }

        public string DefaultContent { get; set; }

        public string ClassName { get; set; }
    }

}
