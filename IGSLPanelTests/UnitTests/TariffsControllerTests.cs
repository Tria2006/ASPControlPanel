using System;
using System.Collections.Generic;
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
    public class TariffsControllerTests
    {
        private readonly IGSLContext _context;

        public TariffsControllerTests()
        {
            var options = new DbContextOptionsBuilder<IGSLContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
            _context = new IGSLContext(options);
        }

        [Fact]
        public void GetIndexMustReturnFolderTree()
        {
            //arrange
            var controller = new TariffsController(_context, new FolderDataHelper(), new TariffsHelper(), new HttpContextAccessor
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
        public void GetCreateNewTariffView()
        {
            //arrange
            var controller = new TariffsController(_context, new FolderDataHelper(), new TariffsHelper(), new HttpContextAccessor
            {
                HttpContext = new DefaultHttpContext()
            });

            //act
            var result = controller.Create(Guid.Empty) as ViewResult;

            //assert
            Assert.NotNull(result);
            Assert.Equal(typeof(Tariff), result.Model.GetType());
            Assert.Equal(Guid.Empty, (result.Model as Tariff)?.Id);
        }

        [Theory]
        [InlineData("create")]
        [InlineData("createAndExit")]
        public async Task ConfirmCreateTariffShouldAddTariffToContext(string createMethod)
        {
            //arrange
            var tariff = new Tariff
            {
                Id = Guid.NewGuid(),
                Name = "New Tariff"
            };
            var controller = new TariffsController(_context, new FolderDataHelper(), new TariffsHelper(), new HttpContextAccessor
            {
                HttpContext = new DefaultHttpContext()
            });

            // act
            RedirectToActionResult result;

            if (createMethod == "create")
                result = await controller.Create(tariff, createMethod, null) as RedirectToActionResult;
            else
                result = await controller.Create(tariff, null, createMethod) as RedirectToActionResult;

            // assert
            Assert.NotNull(await _context.Tariffs.FindAsync(tariff.Id));
            Assert.NotNull(result);
            Assert.Equal(createMethod == "create" ? "Edit" : "Index", result.ActionName);
        }

        [Fact]
        public void GetEditView()
        {
            // arrange
            var tariff = new Tariff
            {
                Id = Guid.NewGuid(),
                Name = "Tariff 1"
            };
            _context.Tariffs.Add(tariff);
            _context.SaveChanges();
            
            var controller = new TariffsController(_context, new FolderDataHelper(), new TariffsHelper(), new HttpContextAccessor
            {
                HttpContext = new DefaultHttpContext()
            });

            // act
            var result = controller.Edit(tariff.Id) as ViewResult;

            // assert
            Assert.NotNull(result);
            Assert.Equal(typeof(Tariff), result.Model.GetType());
            Assert.Equal(tariff.Id, (result.Model as Tariff)?.Id);
        }

        [Theory]
        [InlineData("save")]
        [InlineData("saveAndExit")]
        public async Task ConfirmEditTariff(string saveMethod)
        {
            // arrange
            var tariff = new Tariff
            {
                Id = Guid.NewGuid(),
                Name = "Tariff 1"
            };
            await _context.Tariffs.AddAsync(tariff);
            await _context.SaveChangesAsync();
            
            var controller = new TariffsController(_context, new FolderDataHelper(), new TariffsHelper(), new HttpContextAccessor
            {
                HttpContext = new DefaultHttpContext()
            });
            controller.Index(Guid.Empty);

            // act
            RedirectToActionResult result;

            tariff.Name = "Edited Tariff";

            if (saveMethod == "save")
                result = await controller.Edit(tariff.Id, tariff, saveMethod, null) as RedirectToActionResult;
            else
                result = await controller.Edit(tariff.Id, tariff, null, saveMethod) as RedirectToActionResult;

            // assert
            Assert.NotNull(await _context.Tariffs.FindAsync(tariff.Id));
            Assert.NotNull(result);
            Assert.Equal("Edited Tariff", _context.Tariffs.Find(tariff.Id)?.Name);
            Assert.Equal(saveMethod == "save" ? "Edit" : "Index", result.ActionName);
        }

        [Fact]
        public async Task ClearFolderItemsMustSetFolderAllTariffsFolderIdToEmpty()
        {
            //arrange
            var folder = new FolderTreeEntry
            {
                Id = Guid.NewGuid(),
                Name = "Test Folder"
            };
            var tariff1 = new Tariff
            {
                Id = Guid.NewGuid(),
                Name = "Tariff 1",
                FolderId = folder.Id
            };
            var tariff2 = new Tariff
            {
                Id = Guid.NewGuid(),
                Name = "Tariff 2",
                FolderId = folder.Id
            };

            _context.Tariffs.AddRange(new List<Tariff>{tariff1, tariff2});
            _context.FolderTreeEntries.Add(folder);
            await _context.SaveChangesAsync();
            
            var controller = new TariffsController(_context, new FolderDataHelper(), new TariffsHelper(), new HttpContextAccessor
            {
                HttpContext = new DefaultHttpContext()
            });
            //act
            controller.Index(Guid.Empty);
            await controller.ClearFolderItems(new List<FolderTreeEntry> {folder});

            //assert
            Assert.NotNull(await _context.Tariffs.FindAsync(tariff1.Id));
            Assert.NotNull(await _context.Tariffs.FindAsync(tariff2.Id));
            Assert.Equal(Guid.Empty, _context.Tariffs.Find(tariff1.Id).FolderId);
            Assert.Equal(Guid.Empty, _context.Tariffs.Find(tariff2.Id).FolderId);
        }

        [Fact]
        public async Task TariffCheckBoxClickShouldAddTariffToSelectedCollection()
        {
            //arrange
            var tariff1 = new Tariff
            {
                Id = Guid.NewGuid(),
                Name = "Tariff 1"
            };
            var tariff2 = new Tariff
            {
                Id = Guid.NewGuid(),
                Name = "Tariff 2"
            };

            _context.Tariffs.AddRange(new List<Tariff>{tariff1, tariff2});
            await _context.SaveChangesAsync();
            
            var controller = new TariffsController(_context, new FolderDataHelper(), new TariffsHelper(), new HttpContextAccessor
            {
                HttpContext = new DefaultHttpContext()
            });

            //act
            var result = controller.TariffCheckBoxClick(tariff1.Id);

            //assert
            Assert.True(result);
            Assert.True(controller.GetFolderOrTariffSelected());
        }

        [Fact]
        public async Task DeleteTariffMustSetIsDeletedTrue()
        {
            //arrange
            var tariff = new Tariff
            {
                Id = Guid.NewGuid(),
                Name = "Tariff 1"
            };
            _context.Tariffs.Add(tariff);
            _context.SaveChanges();
            
            var controller = new TariffsController(_context, new FolderDataHelper(), new TariffsHelper(), new HttpContextAccessor
            {
                HttpContext = new DefaultHttpContext()
            });

            //act
            controller.Index(Guid.Empty);
            controller.TariffCheckBoxClick(tariff.Id);
            var result = await controller.Delete(tariff.FolderId);

            //assert
            Assert.NotNull(result);
            Assert.NotNull(await _context.Tariffs.FindAsync(tariff.Id));
            Assert.True(_context.Tariffs.Find(tariff.Id).IsDeleted);
        }

        [Fact]
        public void SaveTempDataShouldChangeOnlyModelOfView()
        {
            //arrange
            var tariff = new Tariff
            {
                Id = Guid.NewGuid(),
                Name = "Tariff 1"
            };
            _context.Tariffs.Add(tariff);
            _context.SaveChanges();
            
            var controller = new TariffsController(_context, new FolderDataHelper(), new TariffsHelper(), new HttpContextAccessor
            {
                HttpContext = new DefaultHttpContext()
            });

            //act
            controller.Edit(tariff.Id);
            controller.SaveTempData(tariff.FolderId, "Saved Tariff", null, null);
            var result = controller.Edit(tariff.Id) as ViewResult;

            //assert
            Assert.NotNull(result?.Model);
            Assert.Equal("Saved Tariff", (result.Model as Tariff)?.Name);
        }

        [Fact]
        public async Task MoveSelectedItemsShouldChangeTariffsFolderId()
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

            var tariff1 = new Tariff
            {
                Id = Guid.NewGuid(),
                Name = "Tariff 1",
                FolderId = folder.Id
            };
            var tariff2 = new Tariff
            {
                Id = Guid.NewGuid(),
                Name = "Tariff 2",
                FolderId = folder.Id
            };

            _context.Tariffs.AddRange(new List<Tariff>{tariff1, tariff2});
            await _context.SaveChangesAsync();
            
            var controller = new TariffsController(_context, new FolderDataHelper(), new TariffsHelper(), new HttpContextAccessor
            {
                HttpContext = new DefaultHttpContext()
            });

            //act
            // выбираем кого перемещать
            controller.TariffCheckBoxClick(tariff1.Id);
            controller.TariffCheckBoxClick(tariff2.Id);
            controller.FolderCheckBoxClick(childFolder.Id);

            // выбираем куда
            controller.FolderClick(Guid.Empty);

            //перемещаем
            var result = controller.MoveSelectedItems();

            //assert
            Assert.NotNull(result);
            Assert.Equal(Guid.Empty, _context.FolderTreeEntries.Find(childFolder.Id).ParentFolderId);
            Assert.Equal(Guid.Empty, _context.Tariffs.Find(tariff1.Id).FolderId);
            Assert.Equal(Guid.Empty, _context.Tariffs.Find(tariff2.Id).FolderId);
            Assert.False(controller.GetFolderOrTariffSelected());
        }
    }
}
