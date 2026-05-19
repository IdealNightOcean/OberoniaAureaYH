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
    /// 多发会谈的派系选择界面，允许玩家选择3-5个派系参与会谈，提供每个派系的基本信息和与玩家的关系概况，并在确认界面展示综合谈判能力评估和相关说明
    /// </summary>
    private static DiaNode FactionSelectionNode_MultiPartyTalks(FactionDialogCache dialogCache, HashSet<Faction> selFactions)
    {
        string selFactionsNames = string.Join(", ", selFactions.Select(f => f.NameColored));
        DiaNode diaNode = new("OARK_MultiPartyTalks_FactionSelectionNode".Translate(selFactionsNames.Named("FactionNames")).CapitalizeFirst());
        Faction ofPlayer = Faction.OfPlayer;
        Faction oaFaction = ModUtility.OAFaction;


        foreach (DiaOption option in FactionSelectionOptions(dialogCache, selFactions, () => FactionSelectionNode_MultiPartyTalks(dialogCache, selFactions)))
        {
            if (selFactions.Count >= 5)
            {
                option.Disable("OARK_MultiPartyTalks_FactionSelection_ReachMax".Translate(5.Named(KeyLibrary_NamedArgument.Value)));
            }
            diaNode.options.Add(option);
        }

        if (selFactions.Count >= 3)
        {
            DiaOption confirmOption = new("Confirm".Translate())
            {
                linkLateBind = () => FactionConfirmNode_MultiPartyTalks(dialogCache, selFactions)
            };
            diaNode.options.Add(confirmOption);
        }

        return diaNode;
    }

    /// <summary>
    /// 多方会谈派系确认界面，在玩家选择3-5个派系后，展示所选派系的综合谈判能力评估和相关说明，帮助玩家做出是否继续进行会谈的决策，并提供返回修改选择的选项
    /// </summary>
    private static DiaNode FactionConfirmNode_MultiPartyTalks(FactionDialogCache dialogCache, HashSet<Faction> selFactions)
    {
        string selFactionsNames = string.Join(", ", selFactions.Select(f => f.NameColored));
        StringBuilder nodeTextBuilder = new(128);
        nodeTextBuilder.AppendLine("OARK_MultiPartyTalks_FactionConfirmNode".Translate(selFactionsNames.Named("FactionNames")).CapitalizeFirst());

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

        if (baselineNegotiationAbility > 2f && OAInteractHandler.Instance.AssistPoints < 40)
        {
            confirmOpt.Disable("OARK_AssistPointsNotEnough".Translate(40));
        }

        diaNode.options.Add(new DiaOption("GoBack".Translate())
        {
            linkLateBind = () => FactionSelectionNode_MultiPartyTalks(dialogCache, selFactions)
        });


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
        DiaNode diaNode = new("OARK_MultiPartyTalks_ConfirmNode".Translate(selFactionsNames.Named("FactionNames")).CapitalizeFirst());
        diaNode.options.Add(FactionDialogUtility.OKToRoot(dialogCache.Faction, dialogCache.Negotiator));

        return diaNode;
    }

    private static IEnumerable<DiaOption> FactionSelectionOptions(FactionDialogCache dialogCache, HashSet<Faction> selFactions, Func<DiaNode> linkLateBind)
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
                diaOption.SetText(faction.NameColored + (" " + selectedText));
                diaOption.action = () => selFactions.Remove(faction);
            }
            else
            {
                diaOption.SetText(faction.NameColored);
                diaOption.action = () => selFactions.Add(faction);
            }

            yield return diaOption;
        }
    }
}