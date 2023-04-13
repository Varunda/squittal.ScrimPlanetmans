using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using squittal.ScrimPlanetmans.Data;
using squittal.ScrimPlanetmans.ScrimMatch.Messages;
using squittal.ScrimPlanetmans.ScrimMatch.Models;
using squittal.ScrimPlanetmans.Services.Planetside;
using squittal.ScrimPlanetmans.Services.Rulesets;
using squittal.ScrimPlanetmans.Services.ScrimMatch;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace squittal.ScrimPlanetmans.ScrimMatch
{
    public class ScrimRulesetManager : IScrimRulesetManager
    {
        private readonly IDbContextHelper _dbContextHelper;
        private readonly IItemCategoryService _itemCategoryService;
        private readonly IItemService _itemService;
        private readonly IRulesetDataService _rulesetDataService;
        private readonly IScrimMessageBroadcastService _messageService;
        public ILogger<ScrimRulesetManager> _logger;

        public Ruleset ActiveRuleset { get; private set; }

        private readonly int _defaultRulesetId = 1;

        private readonly AutoResetEvent _activateRulesetAutoEvent = new AutoResetEvent(true);

        public ScrimRulesetManager(IDbContextHelper dbContextHelper, IItemCategoryService itemCategoryService,
            IItemService itemService, IRulesetDataService rulesetDataService,
            IScrimMessageBroadcastService messageService, ILogger<ScrimRulesetManager> logger)
        {
            _dbContextHelper = dbContextHelper;
            _itemCategoryService = itemCategoryService;
            _itemService = itemService;
            _rulesetDataService = rulesetDataService;
            _messageService = messageService;
            _logger = logger;

            _messageService.RaiseRulesetRuleChangeEvent += async (s, e) => await HandleRulesetRuleChangeMesssage(s, e);

            _messageService.RaiseRulesetSettingChangeEvent += HandleRulesetSettingChangeMessage;
            _messageService.RaiseRulesetOverlayConfigurationChangeEvent +=
                HandleRulesetOverlayConfigurationChangeMessage;
        }

        public async Task<IEnumerable<Ruleset>> GetRulesetsAsync(CancellationToken cancellationToken)
        {
            return await _rulesetDataService.GetAllRulesetsAsync(cancellationToken);
        }

        public async Task<Ruleset> GetActiveRulesetAsync(bool forceRefresh = false)
        {
            if (ActiveRuleset == null)
            {
                return await ActivateDefaultRulesetAsync();
            }
            else if (forceRefresh || ActiveRuleset.RulesetActionRules == null ||
                     !ActiveRuleset.RulesetActionRules.Any() || ActiveRuleset.RulesetItemCategoryRules == null ||
                     !ActiveRuleset.RulesetItemCategoryRules.Any())
            {
                await SetUpActiveRulesetAsync();
                return ActiveRuleset;
            }
            else
            {
                return ActiveRuleset;
            }
        }

        public async Task<Ruleset> ActivateRulesetAsync(int rulesetId)
        {
            _activateRulesetAutoEvent.WaitOne();

            try
            {
                using var factory = _dbContextHelper.GetFactory();
                var dbContext = factory.GetDbContext();

                Ruleset currentActiveRuleset = null;

                if (ActiveRuleset != null)
                {
                    currentActiveRuleset = ActiveRuleset;

                    if (rulesetId == currentActiveRuleset.Id)
                    {
                        _activateRulesetAutoEvent.Set();

                        return currentActiveRuleset;
                    }
                }

                var newActiveRuleset =
                    await _rulesetDataService.GetRulesetFromIdAsync(rulesetId, CancellationToken.None);

                if (newActiveRuleset == null)
                {
                    _activateRulesetAutoEvent.Set();

                    return null;
                }

                _rulesetDataService.SetActiveRulesetId(rulesetId);

                ActiveRuleset = newActiveRuleset;

                var message = currentActiveRuleset == null
                    ? new ActiveRulesetChangeMessage(ActiveRuleset)
                    : new ActiveRulesetChangeMessage(ActiveRuleset, currentActiveRuleset);

                _messageService.BroadcastActiveRulesetChangeMessage(message);

                _activateRulesetAutoEvent.Set();

                return ActiveRuleset;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());

                _activateRulesetAutoEvent.Set();

                return null;
            }
        }

        public async Task<Ruleset> ActivateDefaultRulesetAsync()
        {
            using var factory = _dbContextHelper.GetFactory();
            var dbContext = factory.GetDbContext();

            var ruleset = await dbContext.Rulesets.FirstOrDefaultAsync(r => r.IsCustomDefault);

            if (ruleset == null)
            {
                _logger.LogInformation($"No custom default ruleset found. Loading default ruleset...");
                ruleset = await dbContext.Rulesets.FirstOrDefaultAsync(r => r.IsDefault);
            }

            if (ruleset == null)
            {
                _logger.LogError($"Failed to activate default ruleset: no ruleset found");
                return null;
            }

            ActiveRuleset = await ActivateRulesetAsync(ruleset.Id);

            _logger.LogInformation($"Active ruleset loaded: {ActiveRuleset.Name}");

            return ActiveRuleset;
        }

        public async Task SetUpActiveRulesetAsync()
        {
            //WTF?
            //_activateRulesetAutoEvent.WaitOne();

            try
            {
                using var factory = _dbContextHelper.GetFactory();
                var dbContext = factory.GetDbContext();

                var currentActiveRuleset = ActiveRuleset;

                if (currentActiveRuleset == null)
                {
                    _logger.LogError($"Failed to set up active ruleset: no ruleset found");

                    _activateRulesetAutoEvent.Set();

                    return;
                }

                var tempRuleset =
                    await _rulesetDataService.GetRulesetFromIdAsync(currentActiveRuleset.Id, CancellationToken.None);

                if (tempRuleset == null)
                {
                    _logger.LogError($"Failed to set up active ruleset: temp ruleset is null");

                    _activateRulesetAutoEvent.Set();

                    return;
                }

                ActiveRuleset = tempRuleset;

                _rulesetDataService.SetActiveRulesetId(ActiveRuleset.Id);

                _logger.LogInformation($"Active ruleset collections loaded: {ActiveRuleset.Name}");

                _activateRulesetAutoEvent.Set();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to set up active ruleset: {ex}");

                _activateRulesetAutoEvent.Set();
            }
        }

        private async Task HandleRulesetRuleChangeMesssage(object sender,
            ScrimMessageEventArgs<RulesetRuleChangeMessage> e)
        {
            var changedRulesetId = e.Message.Ruleset.Id;

            if (changedRulesetId == ActiveRuleset.Id)
            {
                await SetUpActiveRulesetAsync();
            }
        }

        private void HandleRulesetSettingChangeMessage(object sender,
            ScrimMessageEventArgs<RulesetSettingChangeMessage> e)
        {
            var ruleset = e.Message.Ruleset;

            _activateRulesetAutoEvent.WaitOne();

            if (ruleset.Id != ActiveRuleset.Id)
            {
                _activateRulesetAutoEvent.Set();
                return;
            }

            ActiveRuleset.Name = ruleset.Name;
            ActiveRuleset.DefaultMatchTitle = ruleset.DefaultMatchTitle;
            ActiveRuleset.DefaultRoundLength = ruleset.DefaultRoundLength;
            ActiveRuleset.DefaultEndRoundOnFacilityCapture = ruleset.DefaultEndRoundOnFacilityCapture;

            _activateRulesetAutoEvent.Set();
        }

        private void HandleRulesetOverlayConfigurationChangeMessage(object sender,
            ScrimMessageEventArgs<RulesetOverlayConfigurationChangeMessage> e)
        {
            var ruleset = e.Message.Ruleset;
            var overlayConfiguration = e.Message.OverlayConfiguration;

            _activateRulesetAutoEvent.WaitOne();

            if (ruleset.Id != ActiveRuleset.Id)
            {
                _activateRulesetAutoEvent.Set();
                return;
            }

            ActiveRuleset.RulesetOverlayConfiguration.UseCompactLayout = overlayConfiguration.UseCompactLayout;
            ActiveRuleset.RulesetOverlayConfiguration.StatsDisplayType = overlayConfiguration.StatsDisplayType;
            ActiveRuleset.RulesetOverlayConfiguration.ShowStatusPanelScores =
                overlayConfiguration.ShowStatusPanelScores;

            _activateRulesetAutoEvent.Set();
        }

        public async Task<Ruleset> GetDefaultRulesetAsync()
        {
            using var factory = _dbContextHelper.GetFactory();
            var dbContext = factory.GetDbContext();

            var ruleset = await dbContext.Rulesets.FirstOrDefaultAsync(r => r.IsDefault);

            if (ruleset == null)
            {
                return null;
            }

            ruleset = await _rulesetDataService.GetRulesetFromIdAsync(ruleset.Id, CancellationToken.None);

            return ruleset;
        }

        public async Task SeedDefaultRuleset()
        {
            //Add default ruleset json from ruleset folder. So we can remove a lot of code.

            var rulesets = await _rulesetDataService.GetAllRulesetsAsync(CancellationToken.None);
            if (rulesets == null || !rulesets.Any())
            {
                var ruleset = await _rulesetDataService.ImportNewRulesetFromJsonFile("default_ruleset", false, false);
                await _rulesetDataService.SetCustomDefaultRulesetAsync(ruleset.Id);
            }
        }

        public async Task SeedScrimActionModels()
        {
            using var factory = _dbContextHelper.GetFactory();
            var dbContext = factory.GetDbContext();

            var createdEntities = new List<ScrimAction>();

            var allActionTypeValues = new List<ScrimActionType>();

            var enumValues = (ScrimActionType[])Enum.GetValues(typeof(ScrimActionType));

            allActionTypeValues.AddRange(enumValues);

            var storeEntities = await dbContext.ScrimActions.ToListAsync();

            allActionTypeValues.AddRange(storeEntities.Where(a => !allActionTypeValues.Contains(a.Action))
                .Select(a => a.Action).ToList());

            allActionTypeValues.Distinct().ToList();

            foreach (var value in allActionTypeValues)
            {
                try
                {
                    var storeEntity = storeEntities.FirstOrDefault(e => e.Action == value);
                    var isValidEnum = enumValues.Any(enumValue => enumValue == value);

                    if (storeEntity == null)
                    {
                        createdEntities.Add(ConvertToDbModel(value));
                    }
                    else if (isValidEnum)
                    {
                        storeEntity = ConvertToDbModel(value);
                        dbContext.ScrimActions.Update(storeEntity);
                    }
                    else
                    {
                        dbContext.ScrimActions.Remove(storeEntity);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex.ToString());
                }
            }

            if (createdEntities.Any())
            {
                await dbContext.ScrimActions.AddRangeAsync(createdEntities);
            }

            await dbContext.SaveChangesAsync();

            _logger.LogInformation($"Seeded Scrim Actions store");
        }

        private ScrimAction ConvertToDbModel(ScrimActionType value)
        {
            var name = Enum.GetName(typeof(ScrimActionType), value);

            return new ScrimAction
            {
                Action = value,
                Name = name,
                Description = Regex.Replace(name, @"(\p{Ll})(\p{Lu})", "$1 $2"),
                Domain = ScrimAction.GetDomainFromActionType(value)
            };
        }

        public IEnumerable<ScrimActionType> GetScrimActionTypes()
        {
            return (ScrimActionType[])Enum.GetValues(typeof(ScrimActionType));
        }
    }
}