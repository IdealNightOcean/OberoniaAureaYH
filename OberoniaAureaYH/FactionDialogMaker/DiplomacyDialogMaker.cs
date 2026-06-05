using OberoniaAurea_Frame;
using RimWorld;
using RimWorld.Planet;
using RimWorld.QuestGen;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace OberoniaAurea;

/// <summary>
/// 外交相关界面
/// </summary>
internal static partial class DiplomacyDialogMaker
{
    /// <summary>
    /// 外交交互选项
    /// </summary>
    internal static DiaOption Diplomacy(FactionDialogCache dialogCache)
    {
        DiaOption diaOption = new("OARK_CommsConsloe_Diplomacy".Translate())
        {
            linkLateBind = () => DiplomacyNode(dialogCache)
        };
        if (!dialogCache.NegotiatorSocialEnabled)
        {
            diaOption.Disable("WorkTypeDisablesOption".Translate(SkillDefOf.Social.label));
        }

        return diaOption;
    }

    /// <summary>
    /// 外交交互选择界面，提供多个外交相关选项，包括多方会谈等，并根据玩家与该派系的关系状态动态调整可用选项和相关说明，帮助玩家做出合理的外交决策
    /// </summary>
    private static DiaNode DiplomacyNode(FactionDialogCache dialogCache)
    {
        DiaNode diaNode = new("OARK_CommsConsloe_DiplomacyNode".Translate());

        diaNode.options.Add(ExtraSiteGuest.ExtraSiteTraderOpt(dialogCache));
        diaNode.options.Add(MultiPartyTalks.MultiPartyTalksOpt(dialogCache));

        //返回按钮
        diaNode.AddDefaultGoBackOption(linkLateBind: FactionDialogMaker.ResetToRoot(dialogCache.Faction, dialogCache.Negotiator));
        return diaNode;
    }

    private static IEnumerable<(Faction faction, DiaOption diaOption)> FactionSelectionOptions(Func<Faction, DiaNode> linkLateBind)
    {
        Faction ofPlayer = Faction.OfPlayer;
        Faction oaFaction = ModUtility.OAFaction;

        foreach (Faction faction in Find.FactionManager.AllFactions)
        {
            if (faction.Hidden || faction.temporary || faction.def.permanentEnemy || faction.IsPlayerSafe())
                continue;
            if (faction == ofPlayer || faction == oaFaction)
                continue;

            DiaOption diaOption = new(faction.Name)
            {
                linkLateBind = () => linkLateBind(faction)
            };

            yield return (faction, diaOption);
        }
    }

    private static IEnumerable<(Faction faction, DiaOption diaOption)> MultiFactionSelectionOptions(HashSet<Faction> selFactions, Func<DiaNode> linkLateBind, int maxSelections = int.MaxValue)
    {
        Faction ofPlayer = Faction.OfPlayer;
        Faction oaFaction = ModUtility.OAFaction;

        foreach (Faction faction in Find.FactionManager.AllFactions)
        {
            if (faction.Hidden || faction.temporary || faction.def.permanentEnemy || faction.IsPlayerSafe())
                continue;
            if (faction == ofPlayer || faction == oaFaction)
                continue;

            DiaOption diaOption = new()
            {
                linkLateBind = linkLateBind
            };

            string selectedText = "OARK_HasSelected".Translate();
            if (selFactions.Contains(faction))
            {
                diaOption.SetText($"{faction.Name} ({selectedText})");
                diaOption.action = () => selFactions.Remove(faction);
            }
            else
            {
                diaOption.SetText(faction.Name);
                diaOption.action = () => selFactions.Add(faction);
                if (selFactions.Count >= maxSelections)
                {
                    diaOption.Disable("OARK_ReachMax".Translate(maxSelections.Named(KeyLibrary_NamedArgument.Value)));
                }
            }
            yield return (faction, diaOption);
        }
    }
}

internal static partial class DiplomacyDialogMaker
{
    /// <summary>
    /// 访客派系相关
    /// </summary>
    protected static class ExtraSiteGuest
    {
        /// <summary>
        /// 要求访客派系延迟事件（天）
        /// </summary>
        private const int NextGusetDelayDays = 3;
        /// <summary>
        /// 邀请与金鼠敌对派系额外花费的支援点数
        /// </summary>
        private const int ExtraAssistPointsCostForOAHostileGuest = 5;

        /// <summary>
        /// 访客派系选项
        /// </summary>
        internal static DiaOption ExtraSiteTraderOpt(FactionDialogCache dialogCache)
        {
            DiaOption diaOption = FactionDialogUtility.DiaOptionWithCooldown(
               text: "OARK_DiplomacyOpt_ExtraSiteTrader".Translate(),
               key: KeyLibrary_CDRecord.RequestExtraSiteTrader,
               linkLateBind: () => ExtraSiteTrader_SettlementSelectNode(dialogCache));

            return diaOption;
        }

        private static DiaNode ExtraSiteTrader_SettlementSelectNode(FactionDialogCache dialogCache)
        {
            DiaNode diaNode = new("OARK_ExtraSiteTrader_SettlementSelectNode".Translate());
            PlanetTile mapTile = dialogCache.Map.Tile;
            PlanetLayerDef mapLayerDef = mapTile.LayerDef;
            List<Settlement> potentialSettlements = Find.WorldObjects.Settlements.Where(s => s.Faction == dialogCache.Faction && s.Tile.LayerDef == mapLayerDef)
                                                                                 .OrderBy(s => Find.WorldGrid.ApproxDistanceInTiles(s.Tile, mapTile))
                                                                                 .Take(5)
                                                                                 .ToList();

            foreach (Settlement settlement in potentialSettlements)
            {
                WorldObjectComp_ExtraGuest guestComp = settlement.GetComponent<WorldObjectComp_ExtraGuest>();
                if (guestComp is null || guestComp.ExtraSiteGuest is null)
                    continue;

                DiaOption diaOption = new(settlement.LabelCap)
                {
                    linkLateBind = () => ExtraSiteTrader_FactionSelectNode(dialogCache, settlement)
                };

                if (guestComp.ExtraSiteGuest.NextGuestFaction is not null)
                {
                    diaOption.Disable("OARK_ExtraSiteTrader_AlreadyHasNextGuest".Translate());
                }
                diaNode.options.Add(diaOption);
            }

            diaNode.AddDefaultGoBackOption(linkLateBind: () => DiplomacyNode(dialogCache));
            return diaNode;
        }

        private static DiaNode ExtraSiteTrader_FactionSelectNode(FactionDialogCache dialogCache, Settlement settlement)
        {
            DiaNode diaNode = new("OARK_ExtraSiteTrader_FactionSelectNode".Translate(settlement.Named(KeyLibrary_NamedArgument.SETTLEMENT)));
            Faction oaFaction = ModUtility.OAFaction;
            int assistPoints = OAInteractHandler.Instance.AssistPoints;
            foreach ((Faction faction, DiaOption diaOption) in FactionSelectionOptions(linkLateBind: f => ExtraSiteTrader_TraderKindSelectNode(dialogCache, settlement, f)))
            {
                if (faction.HostileTo(oaFaction) && assistPoints < ExtraAssistPointsCostForOAHostileGuest)
                {
                    diaOption.Disable("OARK_AssistPointsNotEnough".Translate(ExtraAssistPointsCostForOAHostileGuest));
                }
                diaNode.options.Add(diaOption);
            }

            diaNode.AddDefaultGoBackOption(linkLateBind: () => ExtraSiteTrader_SettlementSelectNode(dialogCache));

            return diaNode;
        }

        private static DiaNode ExtraSiteTrader_TraderKindSelectNode(FactionDialogCache dialogCache, Settlement settlement, Faction selFaction)
        {
            DiaNode diaNode = new("OARK_ExtraSiteTrader_TraderKindSelectNode".Translate(
                settlement.Named(KeyLibrary_NamedArgument.SETTLEMENT),
                selFaction.Named(KeyLibrary_NamedArgument.FACTION)));
            if (!selFaction.def.caravanTraderKinds.NullOrEmpty())
            {
                foreach (TraderKindDef traderKind in selFaction.def.caravanTraderKinds)
                {
                    DiaOption diaOption = new(traderKind.label)
                    {
                        linkLateBind = () => ExtraSiteTrader_SelectConfirmNode(dialogCache, settlement, selFaction, traderKind)
                    };
                    diaNode.options.Add(diaOption);
                }
            }

            diaNode.AddDefaultGoBackOption(linkLateBind: () => ExtraSiteTrader_FactionSelectNode(dialogCache, settlement));
            return diaNode;
        }

        private static DiaNode ExtraSiteTrader_SelectConfirmNode(FactionDialogCache dialogCache, Settlement settlement, Faction selFaction, TraderKindDef traderKind)
        {
            StringBuilder textBuilder = new(32);
            textBuilder.AppendLine("OARK_ExtraSiteTrader_SelectConfirmNode".Translate(
                settlement.Named(KeyLibrary_NamedArgument.SETTLEMENT),
                selFaction.Named(KeyLibrary_NamedArgument.FACTION),
                traderKind.Named(KeyLibrary_NamedArgument.DEF),
                NextGusetDelayDays.Named(KeyLibrary_NamedArgument.Value)));
            textBuilder.AppendLine();
            if (selFaction.HostileTo(dialogCache.Faction))
            {
                textBuilder.AppendLine("OARK_ExtraSiteTrader_HostileFaction".Translate(
                    selFaction.Named("SELFACTION"),
                    dialogCache.Named(KeyLibrary_NamedArgument.SETTLEMENT),
                    ExtraAssistPointsCostForOAHostileGuest.Named(KeyLibrary_NamedArgument.Value)));
            }

            DiaNode diaNode = new(textBuilder.ToTaggedString());
            DiaOption confirmOpt = new("Confirm".Translate())
            {
                action = delegate
                {
                    WorldObjectComp_ExtraGuest guestComp = settlement.GetComponent<WorldObjectComp_ExtraGuest>();
                    if (guestComp is null || guestComp.ExtraSiteGuest is null || guestComp.ExtraSiteGuest.NextGuestFaction is not null)
                        return;

                    guestComp.ExtraSiteGuest.SetNextTraderMeta(selFaction, traderKind, NextGusetDelayDays);
                    if (selFaction.HostileTo(ModUtility.OAFaction))
                    {
                        OAInteractHandler.Instance.AdjustAssistPoints(-ExtraAssistPointsCostForOAHostileGuest);
                    }
                },
                linkLateBind = () => ExtraSiteTrader_ConfirmNode(dialogCache, settlement, selFaction, traderKind)
            };
            diaNode.options.Add(confirmOpt);

            diaNode.AddDefaultGoBackOption(linkLateBind: () => ExtraSiteTrader_TraderKindSelectNode(dialogCache, settlement, selFaction));
            return diaNode;
        }

        private static DiaNode ExtraSiteTrader_ConfirmNode(FactionDialogCache dialogCache, Settlement settlement, Faction selFaction, TraderKindDef traderKind)
        {
            DiaNode diaNode = new("OARK_ExtraSiteTrader_ConfirmNode".Translate(
                settlement.Named(KeyLibrary_NamedArgument.SETTLEMENT),
                selFaction.Named(KeyLibrary_NamedArgument.FACTION),
                traderKind.Named(KeyLibrary_NamedArgument.DEF),
                NextGusetDelayDays.Named(KeyLibrary_NamedArgument.Value)));

            diaNode.options.Add(FactionDialogUtility.OKToRoot(dialogCache.Faction, dialogCache.Negotiator));
            return diaNode;
        }
    }
}

internal static partial class DiplomacyDialogMaker
{
    /// <summary>
    /// 多方会谈相关
    /// </summary>
    protected static class MultiPartyTalks
    {
        /// <summary>
        /// 多方会谈的最大参与派系数量
        /// </summary>
        private const int MaxMultiPartyTalksParticipants = 6;
        /// <summary>
        /// 多方会谈最小参与派系数量
        /// </summary>
        private const int MinMultiPartyTalksParticipants = 3;
        /// <summary>
        /// 举行多方会谈的最低好感度
        /// </summary>
        private const int GoodwillThresholdForMultiPartyTalks = 25;
        /// <summary>
        /// 表示因不必要的多人对话而额外扣减的金鸢尾兰协助点数
        /// </summary>
        private const int ExtraAssistPointsCostForUnnecessaryMultiPartyTalks = 40;
        /// <summary>
        /// 请求多方会谈的冷却时间（Tick）
        /// </summary>
        private const int CooldownTicks_MultiPartyTalks = 30 * 60000;

        /// <summary>
        /// 多方会谈选项
        /// </summary>
        internal static DiaOption MultiPartyTalksOpt(FactionDialogCache dialogCache)
        {
            DiaOption diaOption = FactionDialogUtility.DiaOptionWithCooldown(
                text: "OARK_DiplomacyOpt_MultiPartyTalks".Translate(),
                key: KeyLibrary_CDRecord.RequestMultiPartyTalks,
                linkLateBind: () => MultiPartyTalks_FactionSelectNode(dialogCache, [dialogCache.Faction]));

            if (dialogCache.Faction.PlayerGoodwill < GoodwillThresholdForMultiPartyTalks)
            {
                diaOption.Disable("NeedGoodwill".Translate(GoodwillThresholdForMultiPartyTalks));
            }
            return diaOption;
        }

        /// <summary>
        /// 多发会谈的派系选择界面，允许玩家选择3-5个派系参与会谈，提供每个派系的基本信息和与玩家的关系概况，并在确认界面展示综合谈判能力评估和相关说明
        /// </summary>
        private static DiaNode MultiPartyTalks_FactionSelectNode(FactionDialogCache dialogCache, HashSet<Faction> selFactions)
        {
            string selFactionsNames = string.Join(", ", selFactions.Select(f => f.NameColored));
            DiaNode diaNode = new("OARK_MultiPartyTalks_FactionSelectionNode".Translate(
                dialogCache.Faction.leader.Named(KeyLibrary_NamedArgument.PAWN),
                selFactionsNames.Named(KeyLibrary_NamedArgument.FactionNames)));
            Faction ofPlayer = Faction.OfPlayer;
            Faction oaFaction = ModUtility.OAFaction;

            foreach ((Faction faction, DiaOption diaOption) in MultiFactionSelectionOptions(selFactions: selFactions,
                                                                                            linkLateBind: () => MultiPartyTalks_FactionSelectNode(dialogCache, selFactions),
                                                                                            maxSelections: 5))
            {
                diaNode.options.Add(diaOption);
            }

            if (selFactions.Count >= 3)
            {
                DiaOption confirmOption = new("Confirm".Translate())
                {
                    linkLateBind = () => MultiPartyTalks_FactionConfirmNode(dialogCache, selFactions)
                };
                diaNode.options.Add(confirmOption);
            }

            diaNode.AddDefaultGoBackOption(linkLateBind: () => DiplomacyNode(dialogCache));

            return diaNode;
        }

        /// <summary>
        /// 多方会谈派系确认界面，在玩家选择3-5个派系后，展示所选派系的综合谈判能力评估和相关说明，帮助玩家做出是否继续进行会谈的决策，并提供返回修改选择的选项
        /// </summary>
        private static DiaNode MultiPartyTalks_FactionConfirmNode(FactionDialogCache dialogCache, HashSet<Faction> selFactions)
        {
            string selFactionsNames = string.Join(", ", selFactions.Select(f => f.NameColored));
            StringBuilder nodeTextBuilder = new(128);
            nodeTextBuilder.AppendLine("OARK_MultiPartyTalks_FactionConfirmNode".Translate(
                GoodwillThresholdForMultiPartyTalks.Named(KeyLibrary_NamedArgument.Goodwill),
                dialogCache.Faction.Named(KeyLibrary_NamedArgument.FACTION),
                selFactionsNames.Named(KeyLibrary_NamedArgument.FactionNames)).Resolve());

            float baselineNegotiationAbility = MultiPartyTalksUtility.CalculateBaselineNegotiationAbility(participantFactions: selFactions, resultOnly: false, explanation: out string explanation);
            if (baselineNegotiationAbility < 2f)
            {
                nodeTextBuilder.AppendLine();
                nodeTextBuilder.AppendLine("OARK_MultiPartyTalks_NotNecessary".Translate());
            }

            nodeTextBuilder.AppendLine();
            nodeTextBuilder.AppendLine(explanation);

            DiaNode diaNode = new(nodeTextBuilder.ToTaggedString());

            DiaOption confirmOpt = new("Confirm".Translate())
            {
                action = () =>
                {
                    Slate slate = new();
                    slate.Set("factions", selFactions.ToList());
                    slate.Set("map", dialogCache.Map);
                    if (OAFrame_QuestUtility.TryGenerateQuestAndMakeAvailable(out _, OARK_QuestScriptDefOf.OA_OpportunitySite_MultiPartyTalks, slate, forced: true))
                    {
                        ModUtility.CooldownManager.RegisterRecord(key: KeyLibrary_CDRecord.RequestMultiPartyTalks,
                                                                  cdTicks: CooldownTicks_MultiPartyTalks,
                                                                  removeWhenExpired: true);

                        Faction.OfPlayer.TryAffectGoodwillWith(other: dialogCache.Faction,
                                                               goodwillChange: -GoodwillThresholdForMultiPartyTalks,
                                                               reason: OARK_HistoryEventDefOf.OARK_DiplomacyInteraction);
                        if (baselineNegotiationAbility < 2f)
                        {
                            OAInteractHandler.Instance.AdjustAssistPoints(-ExtraAssistPointsCostForUnnecessaryMultiPartyTalks);
                        }
                    }
                },
                linkLateBind = () => MultiPartyTalks_ConfirmNode(dialogCache, selFactions)
            };

            if (baselineNegotiationAbility < 2f && OAInteractHandler.Instance.AssistPoints < ExtraAssistPointsCostForUnnecessaryMultiPartyTalks)
            {
                confirmOpt.Disable("OARK_AssistPointsNotEnough".Translate(ExtraAssistPointsCostForUnnecessaryMultiPartyTalks));
            }
            diaNode.options.Add(confirmOpt);

            diaNode.AddDefaultGoBackOption(linkLateBind: () => MultiPartyTalks_FactionSelectNode(dialogCache, selFactions));

            return diaNode;
        }

        /// <summary>
        /// 多方会谈最终确认界面，在玩家确认继续进行会谈后，展示最终的综合谈判能力评估结果和相关说明，并提供一个选项返回主界面
        /// </summary>
        private static DiaNode MultiPartyTalks_ConfirmNode(FactionDialogCache dialogCache, HashSet<Faction> selFactions)
        {
            string selFactionsNames = string.Join(", ", selFactions.Select(f => f.NameColored));
            DiaNode diaNode = new("OARK_MultiPartyTalks_ConfirmNode".Translate(
                dialogCache.Faction.leader.Named(KeyLibrary_NamedArgument.PAWN),
                dialogCache.Faction.Named(KeyLibrary_NamedArgument.FACTION),
                selFactionsNames.Named(KeyLibrary_NamedArgument.FactionNames)));
            diaNode.options.Add(FactionDialogUtility.OKToRoot(dialogCache.Faction, dialogCache.Negotiator));

            return diaNode;
        }

    }
}