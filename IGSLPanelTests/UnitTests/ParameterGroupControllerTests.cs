using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using DBModels.Models;
using IGSLControlPanel.Controllers;
using IGSLControlPanel.Data;
using IGSLControlPanel.Helpers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace IGSLPanelTests.UnitTests
{
    public class ParameterGroupControllerTests
    {
        private readonly IGSLContext _context;

        public ParameterGroupControllerTests()
        {
            var options = new DbContextOptionsBuilder<IGSLContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
            _context = new IGSLContext(options);
        }

        [Fact]
        public void GetCreateGroupView()
        {
            //arrange
            var controller = new ParameterGroupsController(_context, new GroupsHelper(), new ProductsHelper());

            //act
            var result = controller.Create() as ViewResult;

            //assert
            Assert.NotNull(result);
        }

        [Theory]
        [InlineData("create")]
        [InlineData("createAndExit")]
        public async Task SubmitCreateShouldAddNewGroupToContext(string createMethod)
        {
            //arrange
            var newGroup = new ParameterGroup
            {
                Name = "Group 1"
            };
            var controller = new ParameterGroupsController(_context, new GroupsHelper(), new ProductsHelper());

            //act
            RedirectToActionResult result;

            if (createMethod == "create")
                result = await controller.Create(newGroup, createMethod, null) as RedirectToActionResult;
            else
                result = await controller.Create(newGroup, null, createMethod) as RedirectToActionResult;

            // assert
            Assert.NotNull(await _context.ParameterGroups.FindAsync(newGroup.Id));
            Assert.NotNull(result);
            Assert.Equal(createMethod == "create" ? "Edit" : "Index", result.ActionName);
        }
    }
}
