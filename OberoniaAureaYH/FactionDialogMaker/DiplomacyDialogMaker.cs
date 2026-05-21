using OberoniaAurea_Frame;
using RimWorld;
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
internal static class DiplomacyDialogMaker
{
    /// <summary>
    /// 多方会谈的最大参与派系数量
    /// </summary>
    private const int MaxMultiPartyTalksParticipants = 5;
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

        diaNode.options.Add(MultiPartyTalksOpt(dialogCache));

        //返回按钮
        diaNode.AddDefaultGoBackOption(linkLateBind: FactionDialogMaker.ResetToRoot(dialogCache.Faction, dialogCache.Negotiator));
        return diaNode;
    }



    private static DiaOption ExtraSiteTrader(FactionDialogCache dialogCache)
    {
        throw new NotImplementedException();
    }

    private static DiaNode ExtraSiteTraderNode(FactionDialogCache dialogCache)
    {

        throw new NotImplementedException();
    }



    /// <summary>
    /// 多方会谈选项
    /// </summary>
    /// <param name="dialogCache"></param>
    private static DiaOption MultiPartyTalksOpt(FactionDialogCache dialogCache)
    {
        DiaOption diaOption = FactionDialogUtility.DiaOptionWithCooldown(
            text: "OARK_DiplomacyOpt_MultiPartyTalks".Translate(),
            key: KeyLibrary_CDRecord.RequestMultiPartyTalks,
            linkLateBind: () => FactionSelectionNode_MultiPartyTalks(dialogCache, []));

        if (dialogCache.Faction.PlayerGoodwill < GoodwillThresholdForMultiPartyTalks)
        {
            diaOption.Disable("NeedGoodwill".Translate(GoodwillThresholdForMultiPartyTalks));
        }
        return diaOption;
    }

    /// <summary>
    /// 多发会谈的派系选择界面，允许玩家选择3-5个派系参与会谈，提供每个派系的基本信息和与玩家的关系概况，并在确认界面展示综合谈判能力评估和相关说明
    /// </summary>
    private static DiaNode FactionSelectionNode_MultiPartyTalks(FactionDialogCache dialogCache, HashSet<Faction> selFactions)
    {
        string selFactionsNames = string.Join(", ", selFactions.Select(f => f.NameColored));
        DiaNode diaNode = new("OARK_MultiPartyTalks_FactionSelectionNode".Translate(selFactionsNames.Named(KeyLibrary_NamedArgument.FactionNames)).CapitalizeFirst());
        Faction ofPlayer = Faction.OfPlayer;
        Faction oaFaction = ModUtility.OAFaction;

        diaNode.options.AddRange(FactionSelectionOptions(
            dialogCache: dialogCache,
            selFactions: selFactions,
            linkLateBind: () => FactionSelectionNode_MultiPartyTalks(dialogCache, selFactions),
            maxSelections: 5));

        if (selFactions.Count >= 3)
        {
            DiaOption confirmOption = new("Confirm".Translate())
            {
                linkLateBind = () => FactionConfirmNode_MultiPartyTalks(dialogCache, selFactions)
            };
            diaNode.options.Add(confirmOption);
        }

        diaNode.AddDefaultGoBackOption(linkLateBind: () => DiplomacyNode(dialogCache));

        return diaNode;
    }

    /// <summary>
    /// 多方会谈派系确认界面，在玩家选择3-5个派系后，展示所选派系的综合谈判能力评估和相关说明，帮助玩家做出是否继续进行会谈的决策，并提供返回修改选择的选项
    /// </summary>
    private static DiaNode FactionConfirmNode_MultiPartyTalks(FactionDialogCache dialogCache, HashSet<Faction> selFactions)
    {
        string selFactionsNames = string.Join(", ", selFactions.Select(f => f.NameColored));
        StringBuilder nodeTextBuilder = new(128);
        nodeTextBuilder.AppendLine("OARK_MultiPartyTalks_FactionConfirmNode".Translate(
            GoodwillThresholdForMultiPartyTalks.Named(KeyLibrary_NamedArgument.Goodwill),
            dialogCache.Faction.Named(KeyLibrary_NamedArgument.FACTION),
            selFactionsNames.Named(KeyLibrary_NamedArgument.FactionNames)).CapitalizeFirst().Resolve());

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
            linkLateBind = () => ConfirmNode_MultiPartyTalks(dialogCache, selFactions)
        };

        if (baselineNegotiationAbility < 2f && OAInteractHandler.Instance.AssistPoints < ExtraAssistPointsCostForUnnecessaryMultiPartyTalks)
        {
            confirmOpt.Disable("OARK_AssistPointsNotEnough".Translate(ExtraAssistPointsCostForUnnecessaryMultiPartyTalks));
        }
        diaNode.options.Add(confirmOpt);

        diaNode.AddDefaultGoBackOption(linkLateBind: () => FactionSelectionNode_MultiPartyTalks(dialogCache, selFactions));

        return diaNode;
    }

    /// <summary>
    /// 多方会谈最终确认界面，在玩家确认继续进行会谈后，展示最终的综合谈判能力评估结果和相关说明，并提供一个选项返回主界面
    /// </summary>
    private static DiaNode ConfirmNode_MultiPartyTalks(FactionDialogCache dialogCache, HashSet<Faction> selFactions)
    {
        string selFactionsNames = string.Join(", ", selFactions.Select(f => f.NameColored));
        DiaNode diaNode = new("OARK_MultiPartyTalks_ConfirmNode".Translate(dialogCache.Faction.Named(KeyLibrary_NamedArgument.FACTION), selFactionsNames.Named(KeyLibrary_NamedArgument.FactionNames)).CapitalizeFirst());
        diaNode.options.Add(FactionDialogUtility.OKToRoot(dialogCache.Faction, dialogCache.Negotiator));

        return diaNode;
    }




    private static IEnumerable<DiaOption> FactionSelectionOptions(FactionDialogCache dialogCache, HashSet<Faction> selFactions, Func<DiaNode> linkLateBind, int maxSelections = int.MaxValue)
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
            yield return diaOption;
        }
    }
}