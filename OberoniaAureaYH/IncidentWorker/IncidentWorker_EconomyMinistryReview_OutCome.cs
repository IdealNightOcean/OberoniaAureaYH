using OberoniaAurea_Frame;
using RimWorld;
using System;
using System.Collections.Generic;
using Verse;

namespace OberoniaAurea;

internal class IncidentWorker_EconomyMinistryReview_Outcome : IncidentWorker
{
    protected override bool CanFireNowSub(IncidentParms parms)
    {
        return ScienceDepartmentInteractHandler.IsInteractAvailable()
            && ScienceDepartmentInteractHandler.Instance.LastEconomyReviewOutcome != EconomyMinistryReviewOutcome.Unknown
            && (Find.TickManager.TicksGame - ScienceDepartmentInteractHandler.Instance.LastEconomyReviewTick).TicksToDays() < 10f;
    }

    protected override bool TryExecuteWorker(IncidentParms parms)
    {
        if (!CanFireNowSub(parms))
        {
            return false;
        }
        parms.faction ??= ModUtility.OAFaction;
        OutcomeNodeTree(parms);
        return true;
    }

    private void OutcomeNodeTree(IncidentParms parms)
    {

        DiaNode waitNode = new("OARK_EconomyMinistryReview_OutcomeWait".Translate());
        DiaOption waitOpt = new("Confirm".Translate())
        {
            linkLateBind = () => ResultNode(parms)
        };
        waitNode.options.Add(waitOpt);


        DiaNode rootNode = new("OARK_EconomyMinistryReview_OutcomeRoot".Translate());
        DiaOption rootOpt = new("Confirm".Translate())
        {
            link = waitNode
        };
        rootNode.options.Add(rootOpt);

        Dialog_NodeTreeWithFactionInfo nodeTree = new(rootNode, parms.faction)
        {
            draggable = true
        };

        Find.WindowStack.Add(nodeTree);
    }

    private DiaNode ResultNode(IncidentParms parms)
    {
        Map map = Find.AnyPlayerHomeMap;
        Faction faction = parms.faction ?? ModUtility.OAFaction;
        int pointsGain = (int)parms.points;

        TaggedString resultText;
        Action resultAction;

        switch (ScienceDepartmentInteractHandler.Instance.LastEconomyReviewOutcome)
        {
            case EconomyMinistryReviewOutcome.ScienceDepartment:
                {
                    resultText = "OARK_EconomyMinistryReview_OutcomeSD".Translate(pointsGain);
                    resultAction = delegate
                    {
                        Outcome_SD(map, faction, pointsGain);
                    };
                    break;
                }
            case EconomyMinistryReviewOutcome.EconomyMinistry:
                {
                    resultText = "OARK_EconomyMinistryReview_OutcomeEM".Translate(pointsGain);
                    resultAction = delegate
                    {
                        Outcome_EM(map, faction, pointsGain);
                    };
                    break;
                }
            case EconomyMinistryReviewOutcome.WinWin:
                {
                    int apGain = pointsGain / 10000;
                    int gtapGain = pointsGain % 10000;
                    resultText = "OARK_EconomyMinistryReview_OutcomeWinWin".Translate(apGain, gtapGain);
                    resultAction = delegate
                    {
                        Outcome_WinWin(map, faction, apGain, gtapGain);
                    };
                    break;
                }
            default:
                {
                    resultText = "OARK_EconomyMinistryReview_OutcomeUnknown".Translate(pointsGain);
                    resultAction = null;
                    break;
                }
        }

        DiaNode resultNode = new(resultText);
        DiaOption resultOpt = new("Confirm".Translate())
        {
            action = resultAction,
            resolveTree = true
        };
        resultNode.options.Add(resultOpt);
        return resultNode;
    }

    private void Outcome_SD(Map map, Faction faction, int pointGain)
    {
        ScienceDepartmentInteractHandler.Instance.AdjustGravTechAssistPoint(pointGain);

        if (map is not null)
        {
            OARK_DropPodUtility.DefaultDropSingleThingOfDef(OARK_ThingDefOf.OARK_SDCommunicationEquipment, map, faction);
        }
    }

    private void Outcome_EM(Map map, Faction faction, int pointGain)
    {
        OAInteractHandler.Instance.AdjustAssistPoints(pointGain);

        if (map is not null)
        {
            OARK_DropPodUtility.DefaultDropThingOfDef(OARK_ThingDefOf.Oberonia_Aurea_Chanwu_AC, 20, map, faction);
        }
    }

    private void Outcome_WinWin(Map map, Faction faction, int apGain, int gtapGain)
    {
        OAInteractHandler.Instance.AdjustAssistPoints(apGain);
        ScienceDepartmentInteractHandler.Instance.AdjustGravTechAssistPoint(gtapGain);

        if (map is not null)
        {
            List<Thing> rewards = OAFrame_MiscUtility.TryGenerateThing(OARK_ThingDefOf.Oberonia_Aurea_Chanwu_AC, 20);
            rewards.Add(ThingMaker.MakeThing(OARK_ThingDefOf.OARK_SDCommunicationEquipment));
            OARK_DropPodUtility.DefaultDropThing(rewards, map, faction);
        }
    }
}
