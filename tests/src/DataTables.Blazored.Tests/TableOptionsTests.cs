using DataTables.Blazored.Tests.Assets;
using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;

namespace DataTables.Blazored.Tests
{
    public class TableOptionsTests : TestContext
    {
        public TableOptionsTests()
        {
            Services.AddScoped<NavigationManager, MockNavigationManager>();
            JSInterop.Mode = JSRuntimeMode.Loose;
        }

    }
}
