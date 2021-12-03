using System;
using System.Linq.Expressions;
using System.Threading.Tasks;
using BlazorServer.Data.Entities;
using BlazorServer.Models;
using Microsoft.AspNetCore.Mvc;

namespace BlazorServer.Interfaces
{
    public interface IEmployeeRepository : IGenericRepository<Employee>
    {
        Task<JsonResult> GetDataAsync<TResult>([FromBody] AjaxViewModel model, Expression<Func<Employee, TResult>> selector);
    }
}
