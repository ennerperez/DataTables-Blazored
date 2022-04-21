using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using BlazorShared.Interfaces;

namespace BlazorShared.Data.Entities
{
    public class Employee : IEntity<int>
    {
        public int Id { get; set; }

        [DisplayName("First Name")]
        public string FirstName { get; set; }

        [DisplayName("Last Name")]
        public string LastName { get; set; }

        public string Position { get; set; }
        public string Office { get; set; }

        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        [DisplayName("Start Date")]
        public DateTime StartDate { get; set; }

        public decimal Salary { get; set; }
    }
}
