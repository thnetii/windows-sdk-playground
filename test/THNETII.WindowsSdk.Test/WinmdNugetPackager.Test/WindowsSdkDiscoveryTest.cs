using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using Xunit;
using Xunit.Abstractions;

namespace THNETII.WindowsSdk.WindmdNugetPackager.Test
{
    public sealed class WindowsSdkDiscoveryTest : IDisposable
    {
        private readonly ServiceProvider serviceProvider;

        public WindowsSdkDiscoveryTest(ITestOutputHelper outputHelper)
        {
            serviceProvider = new ServiceCollection()
                .AddLogging(logging => logging.AddXUnit(outputHelper))
                .BuildServiceProvider();
        }

        [SkippableFact]
        public void ConstructsWindowsSdkDiscovery()
        {
            var discovery = ActivatorUtilities.GetServiceOrCreateInstance<WindowsSdkDiscovery>(serviceProvider);

            Assert.NotNull(discovery);
        }

        #region IDisposable implementation
        public void Dispose()
        {
            serviceProvider.Dispose();
        }
        #endregion
    }
}
