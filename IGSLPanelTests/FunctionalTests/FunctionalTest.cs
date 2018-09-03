using System;
using System.Net;
using System.Threading.Tasks;
using IGSLControlPanel;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace IGSLPanelTests.FunctionalTests
{
    public class FunctionalTest : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly WebApplicationFactory<Program> _factory;

        public FunctionalTest(WebApplicationFactory<Program> factory)
        {
            _factory = factory;
        }

        [Fact]
        public async Task CanGetHomePage()
        {
            var client = _factory.CreateClient(new WebApplicationFactoryClientOptions
            {
                BaseAddress = new Uri("http://localhost:50116/")
            });

            var response = await client.GetAsync("/");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Theory]
        [InlineData("Products")]
        [InlineData("Tariffs")]
        [InlineData("ParameterGroups")]
        public async Task CanGetHomePageLinksPagesTask(string page)
        {
            var client = _factory.CreateClient(new WebApplicationFactoryClientOptions
            {
                BaseAddress = new Uri("http://localhost:50116/")
            });

            var response = await client.GetAsync(page);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }
    }
}
