using System;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Blazored.Table.Models;
using BlazorServer.Data.Entities;
using Microsoft.AspNetCore.Mvc;

namespace BlazorServer.Interfaces
{
    public interface IEmployeeRepository
    {
        Task<TableResult> GetDataAsync<TResult>([FromBody] TableRequestViewModel model, Expression<Func<Employee, TResult>> selector);
    }
}
