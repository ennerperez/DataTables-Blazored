using Blazored.Table.Tests.Assets;
using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;

namespace Blazored.Table.Tests
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
