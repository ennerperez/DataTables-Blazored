using System;
using System.Threading.Tasks;
using BlazorServer.Data.Entities;

namespace BlazorServer.Interfaces
{
    public interface IEmployeeRepository : IGenericRepository<Employee>
    {

    }
}
