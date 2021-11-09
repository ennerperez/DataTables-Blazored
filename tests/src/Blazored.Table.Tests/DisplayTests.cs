using Bunit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Components;
using Blazored.Table.Tests.Assets;

namespace Blazored.Table.Tests
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
