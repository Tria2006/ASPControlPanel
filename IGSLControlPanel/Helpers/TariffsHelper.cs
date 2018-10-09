using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DBModels.Models;
using IGSLControlPanel.Data;
using log4net;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace IGSLControlPanel.Helpers
{
    public class TariffsHelper
    {
        public Tariff CurrentTariff { get; set; }
        public Tariff SelectedTariffForProduct { get; set; }
        public InsuranceRule CurrentRule { get; set; }
        private List<Tariff> Tariffs { get; set; }
        public List<Tariff> RootTariffs { get; set; }
        private List<Tariff> CheckedTariffs { get; } = new List<Tariff>();
        public bool HasSelectedTariffs => CheckedTariffs.Any();

        public void Initialize(IGSLContext context, FolderTreeEntry rootFolder)
        {
            // продукты получаем вместе со связанными параметрами 
            Tariffs = context.Tariffs.Include(x => x.InsRuleTariffLink)
                .ThenInclude(s => s.InsRule)
                .Include(x => x.RiskFactorsTariffLinks)
                .ThenInclude(x => x.RiskFactor).
                ThenInclude(x => x.FactorValues)
                .Where(s => !s.IsDeleted).ToList();

            // продукты, не привязанные ни к какой папке
            RootTariffs = Tariffs.Where(x => x.FolderId == Guid.Empty && !x.IsDeleted).ToList();

            BuildTariffs(rootFolder);
        }

        public void BuildTariffs(FolderTreeEntry parent)
        {
            var tariffs = Tariffs.Where(x => x.FolderId == parent.Id && !x.IsDeleted);
            foreach (var tariff in tariffs)
            {
                if (parent.Tariffs.Contains(tariff)) continue;
                parent.Tariffs.Add(tariff);
            }

            foreach (var childFolder in parent.ChildFolders)
            {
                BuildTariffs(childFolder);
            }
        }

        public async Task RemoveFolderId(Tariff tariff, IGSLContext context)
        {
            // отвязываем продукт от папки
            var contextTariff = context.Tariffs.FirstOrDefault(x => x.Id == tariff.Id);
            if (contextTariff == null) return;
            contextTariff.FolderId = Guid.Empty;
            await context.SaveChangesAsync();
        }

        public void CheckTariff(Guid id, IGSLContext context)
        {
            // получаем продукт из контекста
            var tariff = context.Tariffs.SingleOrDefault(x => x.Id == id);
            if (tariff == null) return;
            // добавляем или удаляем продукт из списка _checkedFolders
            if (CheckedTariffs.Any(p => p.Id == tariff.Id))
                CheckedTariffs.RemoveAll(p => p.Id == tariff.Id);
            else
                CheckedTariffs.Add(tariff);
        }

        public void MoveSelectedTariffs(IGSLContext context, Guid selectedDestFolderId)
        {
            foreach (var tariff in CheckedTariffs)
            {
                var contextTariff = context.Tariffs.SingleOrDefault(p => p.Id == tariff.Id);
                if (contextTariff == null) continue;
                contextTariff.FolderId = selectedDestFolderId;
            }
            CheckedTariffs.Clear();
            context.SaveChanges();
        }

        public async Task RemoveTariffs(IGSLContext context, FolderTreeEntry parentFolder, ILog logger, IHttpContextAccessor httpAccessor)
        {
            // двигаемся по списку выбранных тарифов
            foreach (var f in CheckedTariffs)
            {
                // получаем тариф из контекста и далее работавем с ним
                var contextTariff = context.Tariffs
                    .Include(x => x.RiskFactorsTariffLinks)
                    .ThenInclude(x => x.RiskFactor)
                    .Include(x => x.InsRuleTariffLink)
                    .ThenInclude(x => x.InsRule)
                    .SingleOrDefault(x => x.Id == f.Id);
                if (contextTariff == null) continue;
                // проставляем IsDeleted всем связанным правилам
                contextTariff.InsRuleTariffLink.ForEach(l =>
                {
                    l.InsRule.IsDeleted = true;
                });
                // удаляем связи
                contextTariff.InsRuleTariffLink.Clear();
                contextTariff.IsDeleted = true;
                logger.Info($"{httpAccessor.HttpContext.Connection.RemoteIpAddress} deleted(set IsDeleted=true) Tariff (id={f.Id})");
            }

            // удаляются тарифы только из FoldersTree
            // в контексте БД они остаются
            parentFolder?.Tariffs.RemoveAll(x => x.IsDeleted);

            // удалить тарифы нужно и из _productsWOFolder
            await context.SaveChangesAsync();

            // очищаем список выбранных тарифов
            CheckedTariffs.Clear();
            BuildTariffs(parentFolder);
        }

        public void SelectUnselectRule(Guid ruleId)
        {
            CurrentRule = CurrentTariff.InsRuleTariffLink
                .SingleOrDefault(s => s.InsRuleId == ruleId)
                ?.InsRule;
        }

        public void RenewCurrentTariffLinks(IGSLContext context)
        {
            var tempTariff = context.Tariffs.Include(x => x.InsRuleTariffLink)
                .ThenInclude(s => s.InsRule)
                .Include(x => x.RiskFactorsTariffLinks)
                .ThenInclude(x => x.RiskFactor).
                ThenInclude(x => x.FactorValues)
                .SingleOrDefault(s => !s.IsDeleted && s.Id == CurrentTariff.Id);

            if(tempTariff == null) return;
            CurrentTariff.RiskFactorsTariffLinks = tempTariff.RiskFactorsTariffLinks;
            CurrentTariff.InsRuleTariffLink = tempTariff.InsRuleTariffLink;
        }

        public async Task SelectTariffForProduct(Guid tariffId, IGSLContext context)
        {
            SelectedTariffForProduct = await context.Tariffs.Include(x => x.LinkedProducts).SingleOrDefaultAsync(x => x.Id == tariffId);
        }
    }
}
