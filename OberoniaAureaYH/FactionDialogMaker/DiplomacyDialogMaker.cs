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
    /// 外交交互选项
    /// </summary>
    internal static DiaOption Diplomacy(FactionDialogCache dialogCache)
    {
        DiaOption diaOption = new("OARK_CommsConsloe_Diplomacy".Translate())
        {
            linkLateBind = () => DiplomacyNode(dialogCache)
        };

        return diaOption;
    }

    /// <summary>
    /// 外交交互选择界面，提供多个外交相关选项，包括多方会谈等，并根据玩家与该派系的关系状态动态调整可用选项和相关说明，帮助玩家做出合理的外交决策
    /// </summary>
    private static DiaNode DiplomacyNode(FactionDialogCache dialogCache)
    {
        DiaNode diaNode = new("OARK_CommsConsloe_DiplomacyNode".Translate());

        DiaOption multiPartyTalksOpt = new("OARK_DiplomacyOpt_MultiPartyTalks".Translate())
        {
            linkLateBind = () => FactionSelectionNode_MultiPartyTalks(dialogCache, [])
        };
        diaNode.options.Add(multiPartyTalksOpt);

        //返回按钮
        diaNode.AddDefaultGoBackOption(linkLateBind: FactionDialogMaker.ResetToRoot(dialogCache.Faction, dialogCache.Negotiator));
        return diaNode;
    }

    /// <summary>
    /// 多发会谈的派系选择界面，允许玩家选择3-5个派系参与会谈，提供每个派系的基本信息和与玩家的关系概况，并在确认界面展示综合谈判能力评估和相关说明
    /// </summary>
    private static DiaNode FactionSelectionNode_MultiPartyTalks(FactionDialogCache dialogCache, HashSet<Faction> selFactions)
    {
        string selFactionsNames = string.Join(", ", selFactions.Select(f => f.NameColored));
        DiaNode diaNode = new("OARK_MultiPartyTalks_FactionSelectionNode".Translate(selFactionsNames.Named("FactionNames")).CapitalizeFirst());
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
        nodeTextBuilder.AppendLine("OARK_MultiPartyTalks_FactionConfirmNode".Translate(selFactionsNames.Named("FactionNames")).CapitalizeFirst().Resolve());

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
                    if (baselineNegotiationAbility < 2f)
                    {
                        OAInteractHandler.Instance.AdjustAssistPoints(-40);
                    }
                }
            },
            linkLateBind = () => ConfirmNode_MultiPartyTalks(dialogCache, selFactions)
        };

        if (baselineNegotiationAbility < 2f && OAInteractHandler.Instance.AssistPoints < 40)
        {
            confirmOpt.Disable("OARK_AssistPointsNotEnough".Translate(40));
        }
        diaNode.options.Add(confirmOpt);

        diaNode.AddDefaultGoBackOption(linkLateBind: () => FactionSelectionNode_MultiPartyTalks(dialogCache, selFactions));

        return diaNode;
    }

    /// <summary>
    /// 多方会谈最终确认界面，在玩家确认继续进行会谈后，展示最终的综合谈判能力评估结果和相关说明，并提供一个选项返回主界面
    /// </summary>
    /// <param name="dialogCache"></param>
    /// <param name="selFactions"></param>
    /// <param name="baselineNegotiationAbility"></param>
    /// <returns></returns>
    private static DiaNode ConfirmNode_MultiPartyTalks(FactionDialogCache dialogCache, HashSet<Faction> selFactions)
    {
        string selFactionsNames = string.Join(", ", selFactions.Select(f => f.NameColored));
        DiaNode diaNode = new("OARK_MultiPartyTalks_ConfirmNode".Translate(dialogCache.Faction.Named(KeyLibrary_NamedArgument.FACTION), selFactionsNames.Named("FactionNames")).CapitalizeFirst());
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