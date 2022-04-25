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
        Task<Result> GetDataAsync<TResult>([FromBody] Request model, Expression<Func<Employee, TResult>> selector = null);
    }
}
