﻿using System;
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
        public InsuranceRule CurrentRule { get; set; }
        private List<Tariff> _tariffs { get; set; }
        public List<Tariff> RootTariffs { get; set; }
        private List<Tariff> _checkedTariffs { get; } = new List<Tariff>();
        public bool HasSelectedTariffs => _checkedTariffs.Any();

        public void Initialize(IGSLContext _context, FolderTreeEntry rootFolder)
        {
            // продукты получаем вместе со связанными параметрами 
            _tariffs = _context.Tariffs.Include(x => x.InsRuleTariffLink)
                .ThenInclude(s => s.InsRule)
                .Include(x => x.RiskFactorsTariffLinks)
                .ThenInclude(x => x.RiskFactor).
                ThenInclude(x => x.FactorValues)
                .Where(s => !s.IsDeleted).ToList();

            // продукты, не привязанные ни к какой папке
            RootTariffs = _tariffs.Where(x => x.FolderId == Guid.Empty && !x.IsDeleted).ToList();

            BuildTariffs(rootFolder);
        }

        public void BuildTariffs(FolderTreeEntry parent)
        {
            var tariffs = _tariffs.Where(x => x.FolderId == parent.Id && !x.IsDeleted);
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

        public async Task RemoveFolderId(Tariff tariff, IGSLContext _context)
        {
            // отвязываем продукт от папки
            var contextTariff = _context.Tariffs.FirstOrDefault(x => x.Id == tariff.Id);
            if (contextTariff == null) return;
            contextTariff.FolderId = Guid.Empty;
            await _context.SaveChangesAsync();
        }

        public void CheckTariff(Guid id, IGSLContext _context)
        {
            // получаем продукт из контекста
            var tariff = _context.Tariffs.SingleOrDefault(x => x.Id == id);
            if (tariff == null) return;
            // добавляем или удаляем продукт из списка _checkedFolders
            if (_checkedTariffs.Any(p => p.Id == tariff.Id))
                _checkedTariffs.RemoveAll(p => p.Id == tariff.Id);
            else
                _checkedTariffs.Add(tariff);
        }

        public void MoveSelectedTariffs(IGSLContext context, Guid selectedDestFolderId)
        {
            foreach (var tariff in _checkedTariffs)
            {
                var contextTariff = context.Tariffs.SingleOrDefault(p => p.Id == tariff.Id);
                if (contextTariff == null) continue;
                contextTariff.FolderId = selectedDestFolderId;
            }
            _checkedTariffs.Clear();
            context.SaveChanges();
        }

        public async Task RemoveTariffs(IGSLContext _context, FolderTreeEntry parentFolder, ILog logger, IHttpContextAccessor _httpAccessor)
        {
            // двигаемся по списку выбранных тарифов
            foreach (var f in _checkedTariffs)
            {
                // получаем тариф из контекста и далее работавем с ним
                var contextTariff = _context.Tariffs
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
                logger.Info($"{_httpAccessor.HttpContext.Connection.RemoteIpAddress} deleted(set IsDeleted=true) Tariff (id={f.Id})");
            }

            // удаляются тарифы только из FoldersTree
            // в контексте БД они остаются
            parentFolder?.Tariffs.RemoveAll(x => x.IsDeleted);

            // удалить тарифы нужно и из _productsWOFolder
            await _context.SaveChangesAsync();

            // очищаем список выбранных тарифов
            _checkedTariffs.Clear();
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
    }
}
