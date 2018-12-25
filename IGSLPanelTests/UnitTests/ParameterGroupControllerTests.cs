using DBModels.Models;
using IGSLControlPanel.Controllers;
using IGSLControlPanel.Data;
using IGSLControlPanel.Helpers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
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

        [Fact]
        public async Task GetEditView()
        {
            // arrange
            var group = new ParameterGroup
            {
                Id = Guid.NewGuid(),
                Name = "Group 1"
            };
            await _context.AddAsync(group);
            await _context.SaveChangesAsync();

            var controller = new ParameterGroupsController(_context, new GroupsHelper(), new ProductsHelper());

            // act
            var result = await controller.Edit(group.Id) as ViewResult;

            // assert
            Assert.NotNull(result);
            Assert.Equal(typeof(ParameterGroup), result.Model.GetType());
            Assert.Equal(group.Id, (result.Model as ParameterGroup)?.Id);
        }

        [Theory]
        [InlineData("save")]
        [InlineData("saveAndExit")]
        [InlineData("")]
        public async Task ConfirmEditGroup(string saveMethod)
        {
            // arrange
            var group = new ParameterGroup
            {
                Id = Guid.NewGuid(),
                Name = "Group 1"
            };
            await _context.AddAsync(group);
            await _context.SaveChangesAsync();

            var controller = new ParameterGroupsController(_context, new GroupsHelper(), new ProductsHelper());

            // act
            RedirectToActionResult result;

            group.Name = "Edited Group";

            switch (saveMethod)
            {
                case "save":
                    result = await controller.Edit(group.Id, group, saveMethod, null) as RedirectToActionResult;
                    break;
                case "saveAndExit":
                    result = await controller.Edit(group.Id, group, null, saveMethod) as RedirectToActionResult;
                    break;
                default:
                    result = await controller.Edit(group.Id, group, null, null) as RedirectToActionResult;
                    break;
            }

            // assert
            Assert.NotNull(await _context.ParameterGroups.FindAsync(group.Id));
            Assert.NotNull(result);
            Assert.Equal("Edited Group", _context.ParameterGroups.Find(group.Id)?.Name);
            Assert.Equal(saveMethod == "save" || saveMethod == "" ? "Edit" : "Index", result.ActionName);
        }

        [Fact]
        public async Task GetEditGlobalGroupView()
        {
            // arrange
            var group = new ParameterGroup
            {
                Id = Guid.NewGuid(),
                Name = "Group 1"
            };
            await _context.AddAsync(group);
            await _context.SaveChangesAsync();

            for (int i = 1; i <= 5; i++)
            {
                var param = new ProductParameter
                {
                    GroupId = group.Id,
                    Name = $"Parameter {i}"
                };
                await _context.AddAsync(param);
            }

            var excludedParam = new ProductParameter
            {
                Id = Guid.NewGuid(),
                GroupId = Guid.NewGuid(),
                Name = "Excluded param"
            };
            await _context.AddAsync(excludedParam);
            await _context.SaveChangesAsync();

            var controller = new ParameterGroupsController(_context, new GroupsHelper(), new ProductsHelper());

            // act
            var result = await controller.EditGlobal(group.Id) as ViewResult;

            // assert
            Assert.NotNull(result);
            Assert.Equal(typeof(ParameterGroup), result.Model.GetType());
            Assert.Equal(group.Id, (result.Model as ParameterGroup)?.Id);

            var parameters = result.ViewData["Parameters"] as List<ProductParameter>;
            Assert.NotNull(parameters);
            Assert.Equal(5, parameters.Count);
            Assert.True(parameters.All(p => p.GroupId == group.Id));
            Assert.DoesNotContain(excludedParam, parameters);
        }

        [Theory]
        [InlineData("save")]
        [InlineData("saveAndExit")]
        [InlineData("")]
        public async Task ConfirmEditGlobalGroup(string saveMethod)
        {
            // arrange
            var group = new ParameterGroup
            {

                Id = Guid.NewGuid(),
                Name = "Group 1"
            };
            await _context.AddAsync(group);
            await _context.SaveChangesAsync();

            var controller = new ParameterGroupsController(_context, new GroupsHelper(), new ProductsHelper());

            // act
            RedirectToActionResult result;

            group.Name = "Edited Group";

            switch (saveMethod)
            {
                case "save":
                    result = await controller.EditGlobal(group.Id, group, saveMethod, null) as RedirectToActionResult;
                    break;
                case "saveAndExit":
                    result = await controller.EditGlobal(group.Id, group, null, saveMethod) as RedirectToActionResult;
                    break;
                default:
                    result = await controller.EditGlobal(group.Id, group, null, null) as RedirectToActionResult;
                    break;
            }

            // assert
            Assert.NotNull(await _context.ParameterGroups.FindAsync(group.Id));
            Assert.NotNull(result);
            Assert.Equal("Edited Group", _context.ParameterGroups.Find(group.Id)?.Name);
            Assert.Equal(saveMethod == "save" || saveMethod == "" ? "EditGlobal" : "Index", result.ActionName);
        }

        [Fact]
        public async Task GetDeleteView()
        {
            // arrange
            var group = new ParameterGroup
            {
                Id = Guid.NewGuid(),
                Name = "Group 1"
            };
            await _context.AddAsync(group);
            await _context.SaveChangesAsync();

            var controller = new ParameterGroupsController(_context, new GroupsHelper(), new ProductsHelper());

            // act
            var result = await controller.Delete(group.Id) as ViewResult;

            // assert
            Assert.NotNull(result);
            Assert.Equal(typeof(ParameterGroup), result.Model.GetType());
            Assert.Equal(group.Id, (result.Model as ParameterGroup)?.Id);
        }

        [Fact]
        public async Task DeleteGroupMustSetIsDeletedTrueAndClearParamsGroupId()
        {
            //arrange
            var group = new ParameterGroup
            {
                Id = Guid.NewGuid(),
                Name = "Group 1"
            };
            await _context.AddAsync(group);
            await _context.SaveChangesAsync();

            var param = new ProductParameter
            {
                Id = Guid.NewGuid(),
                GroupId = group.Id,
                Name = "Param1"
            };
            await _context.AddAsync(param);
            await _context.SaveChangesAsync();

            var controller = new ParameterGroupsController(_context, new GroupsHelper(), new ProductsHelper());

            //act
            var result = await controller.DeleteConfirmed(group.Id);

            //assert
            Assert.NotNull(result);
            Assert.True(_context.ParameterGroups.Find(group.Id).IsDeleted);
            Assert.Null(_context.ProductParameters.Find(param.Id).GroupId);
        }

        [Fact]
        public async Task AttachGroupShouldCreateParamsForProductFromTemplates()
        {
            //arrange
            #region GlobalGroup with params
            var globalGroup = new ParameterGroup
            {
                Id = Guid.NewGuid(),
                Name = "Group 1",
                IsGlobal = true
            };
            await _context.AddAsync(globalGroup);
            await _context.SaveChangesAsync();

            var template1 = new ProductParameter
            {
                Id = Guid.NewGuid(),
                GroupId = globalGroup.Id,
                Name = "Param1",
                IsParamTemplate = true
            };
            var template2 = new ProductParameter
            {
                Id = Guid.NewGuid(),
                GroupId = globalGroup.Id,
                Name = "Param2",
                IsParamTemplate = true
            };
            await _context.AddRangeAsync(new List<ProductParameter> { template1, template2 });
            await _context.SaveChangesAsync();

            var limit = new ValueLimit
            {
                Id = Guid.NewGuid(),
                Name = "Limit 1",
                ParameterDataType = 0,
                ParameterId = template1.Id
            };
            await _context.AddAsync(limit);
            await _context.SaveChangesAsync();

            template1.Limit = limit;
            #endregion

            #region Product arrange
            var product = new Product
            {
                Id = Guid.NewGuid(),
                Name = "Product 1"
            };
            _context.Products.Add(product);
            _context.SaveChanges();
            #endregion

            //act
            var productsController = new ProductsController(_context, new FolderDataHelper(), new ProductsHelper(), new HttpContextAccessor
            {
                HttpContext = new DefaultHttpContext()
            }, new ProductParamRiskFactorLinkHelper());
            await productsController.Edit(product.Id);

            var groupsController = new ParameterGroupsController(_context, new GroupsHelper(), new ProductsHelper());
            await groupsController.SelectGroup(globalGroup.Id);
            var result = groupsController.AttachGroup(product.Id) as PartialViewResult;

            //assert
            Assert.NotNull(result);
            var resultProduct = result.Model as Product;
            Assert.NotNull(resultProduct);
            Assert.Equal(2, resultProduct.LinkToProductParameters.Count);
            Assert.NotNull(resultProduct.LinkToProductParameters.SingleOrDefault(x => x.Parameter.Name == "Param1")?.Parameter.Limit);
        }
    }
}
