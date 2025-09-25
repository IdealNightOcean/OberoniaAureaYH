using OberoniaAurea_Frame;
using RimWorld;
using UnityEngine;
using Verse;

namespace OberoniaAurea;

public static class InteractUtility
{
    private const int BaseAssistPointCap = 40;
    private const int MaxAssistPointCap = 300;

    public static int GetDailyAssistPointChange(bool assistPointsStoppage, bool overCap)
    {
        Faction oaFaction = ModUtility.OAFaction;
        if (oaFaction is not null)
        {
            if (oaFaction.PlayerRelationKind == FactionRelationKind.Hostile)
            {
                return -25;
            }
            else if (oaFaction.PlayerRelationKind == FactionRelationKind.Ally)
            {
                if (!assistPointsStoppage && !overCap)
                {
                    return 1;
                }
            }
        }
        return 0;
    }

    public static int GetCurAssistPointsCap(float allianceDuration)
    {
        if (allianceDuration < 0)
        {
            return BaseAssistPointCap;
        }
        else
        {
            int num = BaseAssistPointCap + 20 * Mathf.FloorToInt(allianceDuration / 60f);
            return Mathf.Min(num, MaxAssistPointCap);
        }
    }

    public static void SendGravTechStageUpgradeLetter(int curStage)
    {
        int gainGTAP = Mathf.RoundToInt(ScienceDepartmentInteractHandler.Instance.PlayerTechPoints * 0.02f);
        ScienceDepartmentInteractHandler.Instance.AdjustGravTechAssistPoint(gainGTAP);

        Letter upgradeLetter = LetterMaker.MakeLetter(label: "OARK_LetterLabel_GravTechStageUpgrade".Translate(),
                                               text: "OARK_Letter_GravTechStageUpgrade".Translate(curStage, gainGTAP),
                                               def: LetterDefOf.PositiveEvent,
                                               relatedFaction: ModUtility.OAFaction);


        if (curStage == 2)
        {
            GiveSpecialTechPrint("OARK_Odyssey_GravTrap", upgradeLetter);
        }
        if (curStage == 4)
        {
            GiveSpecialTechPrint("OARK_Odyssey_GravFieldCore", upgradeLetter);
        }

        Find.LetterStack.ReceiveLetter(upgradeLetter);
        OAInteractHandler.Instance.CooldownManager.RegisterRecord("GravTechStageUpgrade", cdTicks: 0, shouldRemoveWhenExpired: false); //由于记录提升时间，非冷却

        static void GiveSpecialTechPrint(string defName, Letter letter)
        {
            Map map = Find.AnyPlayerHomeMap;
            DiaBuyableTechPrintDef techPrintDef = DefDatabase<DiaBuyableTechPrintDef>.GetNamedSilentFail(defName);
            if (techPrintDef is null)
            {
                return;
            }
            if (map is null)
            {
                ScienceDepartmentInteractHandler.Instance.AdjustGravTechAssistPoint(techPrintDef.gravTechAssistPoints);
            }
            else
            {
                Thing techPrint = ThingMaker.MakeThing(techPrintDef.TechPrintDef);
                OAFrame_DropPodUtility.DefaultDropSingleThing(techPrint, map, ModUtility.OAFaction);
                letter.lookTargets = techPrint;
            }
        }
    }
}