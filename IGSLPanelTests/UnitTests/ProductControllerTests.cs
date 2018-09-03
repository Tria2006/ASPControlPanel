using System;
using System.Collections.Generic;
using System.Linq;
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
		    var rootFolder = new FolderTreeEntry
		    {
		        Name = "RootFolder"
		    };
            var folders = new List<FolderTreeEntry> {rootFolder}.AsQueryable();
            _context.FolderTreeEntries.AddRange(folders);
		    _context.SaveChanges();

		    var controller = new ProductsController(_context, new FolderDataHelper(), new ProductsHelper(), new HttpContextAccessor());

            // act
		    var result = controller.Index(rootFolder.Id) as ViewResult;

            // assert
			Assert.NotNull(result);
            Assert.Equal(typeof(FolderTreeEntry), result.Model.GetType());
            Assert.Equal("RootFolder", (result.Model as FolderTreeEntry)?.Name);
		}
	}
}
