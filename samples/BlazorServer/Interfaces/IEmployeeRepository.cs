using System;
using System.Linq.Expressions;
using System.Threading.Tasks;
using DataTables.Blazored.Models;
using BlazorShared.Data.Entities;
using Microsoft.AspNetCore.Mvc;

namespace BlazorServer.Interfaces
{
    public interface IEmployeeRepository
    {
        Task<TableResult> GetDataAsync<TResult>([FromBody] TableRequestViewModel model, Expression<Func<Employee, TResult>> selector = null);
    }
}
