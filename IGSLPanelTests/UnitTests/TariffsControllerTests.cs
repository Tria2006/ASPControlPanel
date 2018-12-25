using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DBModels.Models;
using DBModels.Models.ManyToManyLinks;
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
    public class TariffsControllerTests
    {
        private readonly IGSLContext _context;

        private readonly HttpContextAccessor _httpAccessor = new HttpContextAccessor
        {
            HttpContext = new DefaultHttpContext()
        };

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
            var controller = new TariffsController(_context, new FolderDataHelper(), new TariffsHelper(), _httpAccessor, new FilesHelper(_httpAccessor));

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
            var controller = new TariffsController(_context, new FolderDataHelper(), new TariffsHelper(), _httpAccessor, new FilesHelper(_httpAccessor));

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
            var controller = new TariffsController(_context, new FolderDataHelper(), new TariffsHelper(), _httpAccessor, new FilesHelper(_httpAccessor));

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
            
            var controller = new TariffsController(_context, new FolderDataHelper(), new TariffsHelper(), _httpAccessor, new FilesHelper(_httpAccessor));

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
            
            var controller = new TariffsController(_context, new FolderDataHelper(), new TariffsHelper(), _httpAccessor, new FilesHelper(_httpAccessor));
            controller.Index(Guid.Empty);

            // act
            RedirectToActionResult result;

            tariff.Name = "Edited Tariff";

            if (saveMethod == "save")
                result = await controller.Edit(tariff, saveMethod, null) as RedirectToActionResult;
            else
                result = await controller.Edit(tariff, null, saveMethod) as RedirectToActionResult;

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
            
            var controller = new TariffsController(_context, new FolderDataHelper(), new TariffsHelper(), _httpAccessor, new FilesHelper(_httpAccessor));
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
            
            var controller = new TariffsController(_context, new FolderDataHelper(), new TariffsHelper(), _httpAccessor, new FilesHelper(_httpAccessor));

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
            
            var controller = new TariffsController(_context, new FolderDataHelper(), new TariffsHelper(), _httpAccessor, new FilesHelper(_httpAccessor));

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
            
            var controller = new TariffsController(_context, new FolderDataHelper(), new TariffsHelper(), _httpAccessor, new FilesHelper(_httpAccessor));

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
            
            var controller = new TariffsController(_context, new FolderDataHelper(), new TariffsHelper(), _httpAccessor, new FilesHelper(_httpAccessor));

            //act
            // выбираем кого перемещать
            controller.TariffCheckBoxClick(tariff1.Id);
            controller.TariffCheckBoxClick(tariff2.Id);
            controller.FolderCheckBoxClick(childFolder.Id);

            // выбираем куда
            controller.FolderClick(Guid.Empty, ModelTypes.Tariffs.ToString());

            //перемещаем
            var result = controller.MoveSelectedItems();

            //assert
            Assert.NotNull(result);
            Assert.Equal(Guid.Empty, _context.FolderTreeEntries.Find(childFolder.Id).ParentFolderId);
            Assert.Equal(Guid.Empty, _context.Tariffs.Find(tariff1.Id).FolderId);
            Assert.Equal(Guid.Empty, _context.Tariffs.Find(tariff2.Id).FolderId);
            Assert.False(controller.GetFolderOrTariffSelected());
        }


        [Fact]
        public async Task AttachTariffToProductShouldLinkTariffToProduct()
        {
            //arrange
            var product = new Product
            {
                Id = Guid.NewGuid(),
                Name = "Product 1"
            };
            await _context.AddAsync(product);
            await _context.SaveChangesAsync();

            var tariff = new Tariff
            {
                Id = Guid.NewGuid(),
                Name = "Tariff1"
            };
            await _context.AddAsync(tariff);
            await _context.SaveChangesAsync();

            //act
            var controller = new TariffsController(_context, new FolderDataHelper(), new TariffsHelper(), _httpAccessor, new FilesHelper(_httpAccessor));
            await controller.SelectTariff(tariff.Id);
            var result = await controller.AttachTariffToProduct(product.Id) as RedirectToActionResult;

            //assert
            Assert.NotNull(result);
            var resultProduct = await _context.Products.FindAsync(product.Id);
            Assert.NotNull(resultProduct);
            Assert.NotNull(resultProduct.TariffId);
        }

        [Fact]
        public async Task ReAttachTariffToProductShouldReLinkTariffToProduct()
        {
            //arrange
            var product = new Product
            {
                Id = Guid.NewGuid(),
                Name = "Product 1"
            };
            await _context.AddAsync(product);
            await _context.SaveChangesAsync();

            var attachedTariff = new Tariff
            {
                Id = Guid.NewGuid(),
                Name = "AttachedTariff"
            };
            await _context.AddAsync(attachedTariff);

            attachedTariff.LinkedProducts.Add(product);
            await _context.SaveChangesAsync();

            var tariff = new Tariff
            {
                Id = Guid.NewGuid(),
                Name = "Tariff1"
            };
            await _context.AddAsync(tariff);
            await _context.SaveChangesAsync();

            //act
            var controller = new TariffsController(_context, new FolderDataHelper(), new TariffsHelper(), _httpAccessor, new FilesHelper(_httpAccessor));
            await controller.SelectTariff(tariff.Id);
            var result = await controller.AttachTariffToProduct(product.Id) as RedirectToActionResult;

            //assert
            Assert.NotNull(result);
            var resultProduct = await _context.Products.FindAsync(product.Id);
            Assert.NotNull(resultProduct);
            Assert.NotNull(resultProduct.TariffId);
            Assert.Equal(tariff.Id, resultProduct.TariffId);
        }


        [Fact]
        public async Task GetDetailsView()
        {
            //arrange
            var tariff = new Tariff
            {
                Id = Guid.NewGuid(),
                Name = "Tariff1"
            };
            await _context.AddAsync(tariff);

            var insRule = new InsuranceRule
            {
                Id=Guid.NewGuid(),
                Name = "Ins Rule 1"
            };
            await _context.AddAsync(insRule);

            var risk = new Risk
            {
                Id=Guid.NewGuid(),
                Name = "Risk 1"
            };
            await _context.AddAsync(risk);

            var rf = new RiskFactor
            {
                Id = Guid.NewGuid(),
                Name = "RF 1"
            };
            await _context.AddAsync(rf);

            tariff.InsRuleTariffLink.Add(new InsRuleTariffLink
            {
                InsRuleId = insRule.Id,
                TariffId = tariff.Id
            });
            tariff.RiskFactorsTariffLinks.Add(new RiskFactorTariffLink
            {
                TariffId = tariff.Id,
                RiskFactorId = rf.Id
            });
            insRule.LinksToRisks.Add(new RiskInsRuleLink
            {
                RiskId = risk.Id,
                InsRuleId = insRule.Id
            });
            await _context.SaveChangesAsync();

            //act
            var controller = new TariffsController(_context, new FolderDataHelper(), new TariffsHelper(), _httpAccessor, new FilesHelper(_httpAccessor));
            var result = controller.Details(tariff.Id) as ViewResult;

            //assert
            Assert.NotNull(result);
            var resultTariff = result.Model as Tariff;
            Assert.NotNull(resultTariff);
            Assert.NotNull(resultTariff.InsRuleTariffLink.SingleOrDefault(x => x.InsRuleId == insRule.Id));
            Assert.NotNull(resultTariff.InsRuleTariffLink.SingleOrDefault(x => x.InsRuleId == insRule.Id)
                ?.InsRule.LinksToRisks.SingleOrDefault(x => x.RiskId == risk.Id));
            Assert.NotNull(resultTariff.RiskFactorsTariffLinks.SingleOrDefault(x => x.RiskFactorId == rf.Id));
        }
    }
}
