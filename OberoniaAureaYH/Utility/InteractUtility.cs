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

        Find.LetterStack.ReceiveLetter(label: "OARK_LetterLabel_GravTechStageUpgrade".Translate(),
                                       text: "OARK_Letter_GravTechStageUpgrade".Translate(curStage, gainGTAP),
                                       textLetterDef: LetterDefOf.PositiveEvent,
                                       lookTargets: null,
                                       relatedFaction: ModUtility.OAFaction);
    }
}