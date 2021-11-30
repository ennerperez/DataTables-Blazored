using System;
using BlazorServer.Interfaces;

namespace BlazorServer.Data.Entities
{
    public class Employee : IEntity<int>
    {
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Position { get; set; }
        public string Office { get; set; }
        public DateTime StartDate { get; set; }
        public decimal Salary { get; set; }
    }
}
