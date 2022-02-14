using System;
using BlazorServer.Data.Contexts;
using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;

namespace BlazorServer.Controllers
{
    public class BaseComponent : ComponentBase, IDisposable
    {
		[Inject]
		protected IDbContextFactory<DefaultContext> DbContextFactory { get; set; }

		protected DefaultContext DbContext { get; set; }

		protected void InitDependencies()
		{
			DbContext = DbContextFactory.CreateDbContext();

			// setup logger, other things
		}

		// ... ... ...

		public void Dispose()
		{
			DbContext?.Dispose(); // DbContext will be disposed automatically
		}


	}
}
