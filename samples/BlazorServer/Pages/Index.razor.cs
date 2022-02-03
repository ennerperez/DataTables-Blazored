using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading.Tasks;
using BlazorServer.Data.Entities;

namespace BlazorServer.Pages
{
    public partial class Index
    {
        public Index()
        {

        }
        /*public ObservableCollection<object> Employees { get; set; }

        protected override async Task OnInitializedAsync()
        {
            var dir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "data");
            var json = await File.ReadAllTextAsync($"{dir}/employees.json");
            var data = Newtonsoft.Json.JsonConvert.DeserializeObject<IEnumerable<Employee>>(json);
            if (data != null) Employees = new ObservableCollection<object>(data);
            await base.OnInitializedAsync();


        } */
    }
}
