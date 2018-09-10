using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DBModels.Models;
using IGSLControlPanel.Controllers;
using IGSLControlPanel.Data;
using IGSLControlPanel.Enums;
using IGSLControlPanel.Helpers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace IGSLPanelTests.UnitTests
{
    public class FoldersBaseControllerTests
    {
        private readonly IGSLContext _context;

        public FoldersBaseControllerTests()
        {
            var options = new DbContextOptionsBuilder<IGSLContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
            _context = new IGSLContext(options);
        }

        [Fact]
        public async Task CreateFolderMustAddFolderToParent()
        {
            //arrange
            var controller = new ProductsController(_context, new FolderDataHelper(), new ProductsHelper(), new HttpContextAccessor
            {
                HttpContext = new DefaultHttpContext()
            });
            controller.Index(Guid.Empty);

            //act
            var result = await controller.CreateFolder("Test folder 1", Guid.Empty, ModelTypes.Products) as RedirectToActionResult;

            //assert
            Assert.NotNull(result);
            Assert.Equal("Index", result.ActionName);
            Assert.NotNull(await _context.FolderTreeEntries.SingleOrDefaultAsync(x => x.Name == "Test folder 1"));
        }

        [Fact]
        public async Task DeleteFolderShouldSetIsDeletedAndMoveAllItemsToRoot()
        {
            //arrange
            var folder = new FolderTreeEntry
            {
                Id = Guid.NewGuid(),
                Name = "Test Folder"
            };
            var product1 = new Product
            {
                Id = Guid.NewGuid(),
                Name = "Product 1",
                FolderId = folder.Id
            };
            var product2 = new Product
            {
                Id = Guid.NewGuid(),
                Name = "Product 2",
                FolderId = folder.Id
            };

            _context.Products.AddRange(new List<Product>{product1, product2});
            _context.FolderTreeEntries.Add(folder);
            await _context.SaveChangesAsync();
            var controller = new ProductsController(_context, new FolderDataHelper(), new ProductsHelper(), new HttpContextAccessor
            {
                HttpContext = new DefaultHttpContext()
            });
            controller.Index(Guid.Empty);
            controller.FolderCheckBoxClick(folder.Id);
            
            //act
            var result = await controller.DeleteFolder(folder.Id, ModelTypes.Products.ToString()) as RedirectToActionResult;

            //assert
            Assert.NotNull(result);
            Assert.Equal("Index", result.ActionName);
            Assert.True(_context.FolderTreeEntries.Find(folder.Id).IsDeleted);
            Assert.Equal(Guid.Empty, _context.Products.Find(product1.Id).FolderId);
            Assert.Equal(Guid.Empty, _context.Products.Find(product2.Id).FolderId);
        }

        [Fact]
        public async Task OneLevelUpShouldReturnDestFolderView()
        {
            //arrange
            var folder = new FolderTreeEntry
            {
                Id = Guid.NewGuid(),
                Name = "Test Folder",
                ParentFolderId = Guid.Empty
            };

            _context.FolderTreeEntries.Add(folder);
            await _context.SaveChangesAsync();
            var controller = new ProductsController(_context, new FolderDataHelper(), new ProductsHelper(), new HttpContextAccessor
            {
                HttpContext = new DefaultHttpContext()
            });
            controller.Index(folder.Id);

            //act
            var result = controller.OneLevelUp(Guid.Empty, ModelTypes.Products.ToString(), true) as PartialViewResult;

            //assert
            Assert.NotNull(result);
            Assert.Equal(Guid.Empty, (result.Model as FolderTreeEntry)?.Id);
        }
    }
}
