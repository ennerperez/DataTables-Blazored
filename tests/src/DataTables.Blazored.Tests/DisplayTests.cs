using Bunit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Components;
using DataTables.Blazored.Tests.Assets;

namespace DataTables.Blazored.Tests
{
    public class DisplayTests : TestContext
    {
        public DisplayTests()
        {
            Services.AddScoped<NavigationManager, MockNavigationManager>();

            JSInterop.Mode = JSRuntimeMode.Loose;
        }
       
    }
}
