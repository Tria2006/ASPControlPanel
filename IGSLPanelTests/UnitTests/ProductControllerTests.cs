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
	public class ProductControllerTests
	{
	    private readonly IGSLContext _context;

        public ProductControllerTests()
		{
		    var options = new DbContextOptionsBuilder<IGSLContext>()
		        .UseInMemoryDatabase(Guid.NewGuid().ToString())
		        .Options;
		    _context = new IGSLContext(options);
        }

	    [Fact]
	    public void GetIndexViewFromController()
	    {
	        // arrange
	        var controller = new ProductsController(_context, new FolderDataHelper(), new ProductsHelper(), new HttpContextAccessor
	        {
	            HttpContext = new DefaultHttpContext()
	        });

	        // act
	        var result = controller.Index(Guid.Empty) as ViewResult;

	        // assert
	        Assert.NotNull(result);
	        Assert.Equal(typeof(FolderTreeEntry), result.Model.GetType());
	    }

		[Fact]
		public void GetAddViewWithNewProduct()
		{
            // arrange
		    var products = new List<Product>
		    {
                new Product
                {
                    Name = "Product 1",
                    FolderId = Guid.Empty
                },
		        new Product
		        {
		            Name = "Product 2",
		            FolderId = Guid.Empty
		        }
		    };
		    _context.Products.AddRange(products);
		    _context.SaveChanges();

		    var controller = new ProductsController(_context, new FolderDataHelper(), new ProductsHelper(), new HttpContextAccessor
		    {
		        HttpContext = new DefaultHttpContext()
		    });

            // act
		    var result = controller.CreateProduct(Guid.Empty) as ViewResult;

            // assert
			Assert.NotNull(result);
            Assert.Equal(typeof(Product), result.Model.GetType());
            Assert.Equal(Guid.Empty, (result.Model as Product)?.Id);
		}

	    [Theory]
        [InlineData("create")]
        [InlineData("createAndExit")]
	    public async Task ConfirmCreateProductMustAddProductToContext(string createMethod)
	    {
	        // arrange
	        var product = new Product
	        {
                Id = Guid.NewGuid(),
                Name = "Product 1"
	        };
            
	        var controller = new ProductsController(_context, new FolderDataHelper(), new ProductsHelper(), new HttpContextAccessor
	            {
                    HttpContext = new DefaultHttpContext()
	            });

	        // act
	        RedirectToActionResult result;

	        if (createMethod == "create")
	            result = await controller.CreateProduct(product, createMethod, null) as RedirectToActionResult;
	        else
	            result = await controller.CreateProduct(product, null, createMethod) as RedirectToActionResult;

	        // assert
            Assert.NotNull(await _context.Products.FindAsync(product.Id));
            Assert.NotNull(result);
	        Assert.Equal(createMethod == "create" ? "Edit" : "Index", result.ActionName);
	    }

		[Fact]
		public async Task GetEditView()
		{
            // arrange
		    var product = new Product
		    {
		        Id = Guid.NewGuid(),
		        Name = "Product 1"
		    };
		    _context.Products.Add(product);
		    _context.SaveChanges();
            
		    var controller = new ProductsController(_context, new FolderDataHelper(), new ProductsHelper(), new HttpContextAccessor
		    {
		        HttpContext = new DefaultHttpContext()
		    });

            // act
		    var result = await controller.Edit(product.Id) as ViewResult;

            // assert
			Assert.NotNull(result);
            Assert.Equal(typeof(Product), result.Model.GetType());
            Assert.Equal(product.Id, (result.Model as Product)?.Id);
		}

	    [Theory]
        [InlineData("save")]
        [InlineData("saveAndExit")]
	    public async Task ConfirmEditProduct(string saveMethod)
	    {
	        // arrange
	        var product = new Product
	        {
                Id = Guid.NewGuid(),
                Name = "Product 1"
	        };
	        await _context.Products.AddAsync(product);
	        await _context.SaveChangesAsync();
            
	        var controller = new ProductsController(_context, new FolderDataHelper(), new ProductsHelper(), new HttpContextAccessor
	            {
                    HttpContext = new DefaultHttpContext()
	            });
	        controller.Index(Guid.Empty);

	        // act
	        RedirectToActionResult result;

	        product.Name = "Edited Product";

	        if (saveMethod == "save")
	            result = await controller.Edit(product, saveMethod, null) as RedirectToActionResult;
	        else
	            result = await controller.Edit(product, null, saveMethod) as RedirectToActionResult;

	        // assert
            Assert.NotNull(await _context.Products.FindAsync(product.Id));
            Assert.NotNull(result);
            Assert.Equal("Edited Product", _context.Products.Find(product.Id)?.Name);
	        Assert.Equal(saveMethod == "save" ? "Edit" : "Index", result.ActionName);
	    }

	    [Fact]
	    public async Task ClearFolderItemsMustSetFolderAllProductsFolderIdToEmpty()
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
	        //act
	        controller.Index(Guid.Empty);
	        await controller.ClearFolderItems(new List<FolderTreeEntry> {folder});

	        //assert
            Assert.NotNull(await _context.Products.FindAsync(product1.Id));
            Assert.NotNull(await _context.Products.FindAsync(product2.Id));
            Assert.Equal(Guid.Empty, _context.Products.Find(product1.Id).FolderId);
            Assert.Equal(Guid.Empty, _context.Products.Find(product2.Id).FolderId);
	    }

        [Fact]
        public async Task ProductCheckBoxClickShouldAddProductToSelectedCollection()
        {
            //arrange
            var product1 = new Product
            {
                Id = Guid.NewGuid(),
                Name = "Product 1"
            };
            var product2 = new Product
            {
                Id = Guid.NewGuid(),
                Name = "Product 2"
            };

            _context.Products.AddRange(new List<Product>{product1, product2});
            await _context.SaveChangesAsync();
            
            var controller = new ProductsController(_context, new FolderDataHelper(), new ProductsHelper(), new HttpContextAccessor
            {
                HttpContext = new DefaultHttpContext()
            });

            //act
            var result = controller.ProductCheckBoxClick(product1.Id);

            //assert
            Assert.True(result);
            Assert.True(controller.GetFolderOrProductSelected());
        }

        [Fact]
        public async Task DeleteProductMustSetIsDeletedTrue()
        {
            //arrange
            var product = new Product
            {
                Id = Guid.NewGuid(),
                Name = "Product 1"
            };
            _context.Products.Add(product);
            _context.SaveChanges();
            
            var controller = new ProductsController(_context, new FolderDataHelper(), new ProductsHelper(), new HttpContextAccessor
            {
                HttpContext = new DefaultHttpContext()
            });

            //act
            controller.Index(Guid.Empty);
            controller.ProductCheckBoxClick(product.Id);
            var result = await controller.DeleteProduct(product.FolderId);

            //assert
            Assert.NotNull(result);
            Assert.NotNull(await _context.Products.FindAsync(product.Id));
            Assert.True(_context.Products.Find(product.Id).IsDeleted);
        }

        [Fact]
        public async Task SaveTempDataShouldChangeOnlyModelOfView()
        {
            //arrange
            var product = new Product
            {
                Id = Guid.NewGuid(),
                Name = "Product 1"
            };
            _context.Products.Add(product);
            _context.SaveChanges();
            
            var controller = new ProductsController(_context, new FolderDataHelper(), new ProductsHelper(), new HttpContextAccessor
            {
                HttpContext = new DefaultHttpContext()
            });

            //act
            await controller.Edit(product.Id);
            controller.SaveTempData(product.FolderId, "Saved Product", null, null);
            var result = await controller.Edit(product.Id) as ViewResult;

            //assert
            Assert.NotNull(result?.Model);
            Assert.Equal("Saved Product", (result.Model as Product)?.Name);
        }

        [Fact]
        public async Task MoveSelectedItemsShouldChangeProductsFolderId()
        {
            //arrange
            var folder = new FolderTreeEntry
            {
                Id = Guid.NewGuid(),
                Name = "Test Folder"
            };
            var childFolder = new FolderTreeEntry
            {
                Id = Guid.NewGuid(),
                Name = "Child Folder",
                ParentFolderId = folder.Id
            };
            await _context.FolderTreeEntries.AddRangeAsync(new List<FolderTreeEntry> {folder, childFolder});
            await _context.SaveChangesAsync();

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
            await _context.SaveChangesAsync();
            
            var controller = new ProductsController(_context, new FolderDataHelper(), new ProductsHelper(), new HttpContextAccessor
            {
                HttpContext = new DefaultHttpContext()
            });

            //act
            // выбираем кого перемещать
            controller.ProductCheckBoxClick(product1.Id);
            controller.ProductCheckBoxClick(product2.Id);
            controller.FolderCheckBoxClick(childFolder.Id);

            // выбираем куда
            controller.FolderClick(Guid.Empty, ModelTypes.Products.ToString());

            //перемещаем
            var result = controller.MoveSelectedItems();

            //assert
            Assert.NotNull(result);
            Assert.Equal(Guid.Empty, _context.FolderTreeEntries.Find(childFolder.Id).ParentFolderId);
            Assert.Equal(Guid.Empty, _context.Products.Find(product1.Id).FolderId);
            Assert.Equal(Guid.Empty, _context.Products.Find(product2.Id).FolderId);
            Assert.False(controller.GetFolderOrProductSelected());
        }
	}
}
