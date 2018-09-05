using System;
using IGSLControlPanel.Controllers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace IGSLPanelTests.UnitTests
{
    public class HomeControllerTests
    {
        [Fact]
        public void GetHomePage()
        {
            //arrange
            var controller = new HomeController();

            //act
            var result = controller.Index() as ViewResult;

            //assert
            Assert.NotNull(result);
        }

        [Fact]
        public void GetErrorPage()
        {
            //arrange
            var controller = new HomeController();
            
            //act
            var result = controller.Error(new Exception("Test Exception")) as ViewResult;

            //assert
            Assert.NotNull(result);
        }
    }
}
