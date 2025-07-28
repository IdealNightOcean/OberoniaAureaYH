using OberoniaAurea_Frame;
using RimWorld;
using UnityEngine;
using Verse;

namespace OberoniaAurea;

public class ScienceDepartmentInteractHandler : IExposable
{
    private const int MaxGravTechPoints = 100000000;
    public static readonly int[] GravTechStageBoundary = [0, 25000, 50000, 80000];
    public static readonly int MaxGravTechStage = GravTechStageBoundary.Length - 1;

    public static ScienceDepartmentInteractHandler Instance { get; private set; }

    private int curGravTechStageIndex;
    private int gravTechPoints;
    private int playerTechPoints;
    private int gravTechAssistPoints;
    private bool isInitGravQuestCompleted;
    private Pawn gravResearchAssistLendPawn;

    public ScienceShipRecord? ScienceShipRecord;

    public int CurGravTechStage => curGravTechStageIndex + 1;
    public int GravTechPoints => gravTechPoints;
    public int PlayerTechPoints => playerTechPoints;
    public int GravTechAssistPoints => gravTechAssistPoints;
    public bool IsInitGravQuestCompleted => isInitGravQuestCompleted;
    public Pawn GravResearchAssistLendPawn => gravResearchAssistLendPawn;

    public ScienceDepartmentInteractHandler() => Instance = this;
    public static void ClearStaticCache() => Instance = null;
    public static void OpenDevWindow() => Find.WindowStack.Add(new DevWin_SDInteractHandler());

    public static bool IsScienceDepartmentInteractAvailable()
    {
        return ModsConfig.OdysseyActive && Instance is not null && ModUtility.OAFaction is not null;
    }

    public void TickDay()
    {
        AddGravTechPoints(Rand.RangeInclusive(50, 150), byPlayer: false);
        if (!isInitGravQuestCompleted)
        {
            TryTriggerGravInitQuest();
        }

        TryTriggerAnnualInteractionQuest();
    }

    public void AddGravTechPoints(int change, bool byPlayer, bool showMessage = true)
    {
        if (change <= 0)
        {
            Log.Warning("Try to add a non positive gravity tech point.");
            return;
        }

        int trueChange = gravTechPoints; Mathf.Min(change, MaxGravTechPoints - gravTechPoints);
        gravTechPoints = Mathf.Clamp(gravTechPoints + change, 0, MaxGravTechPoints);
        trueChange = gravTechPoints - trueChange;

        if (trueChange == 0)
        {
            return;
        }

        if (byPlayer)
        {
            playerTechPoints += trueChange;
        }

        if (showMessage)
        {
            Messages.Message("OARK_ScienceDepartment_GravTechPointsAdded".Translate(trueChange.ToString("F0")), MessageTypeDefOf.PositiveEvent);
        }

        if (curGravTechStageIndex < MaxGravTechStage && gravTechPoints > GravTechStageBoundary[curGravTechStageIndex + 1])
        {
            curGravTechStageIndex++;
            if (isInitGravQuestCompleted)
            {
                InteractUtility.SendGravTechStageUpgradeLetter(curGravTechStageIndex);
            }
        }
    }

    public void AdjustGravTechAssistPoint(int change, bool showMessage = true)
    {
        if (change == 0)
        {
            return;
        }

        int trueChange = gravTechAssistPoints;
        gravTechAssistPoints = Mathf.Max(0, gravTechAssistPoints + change);
        trueChange = gravTechAssistPoints - trueChange;

        if (showMessage && trueChange != 0)
        {
            if (trueChange > 0)
            {
                Messages.Message("OARK_ScienceDepartment_GravTechAssistPointAdded".Translate(trueChange), MessageTypeDefOf.PositiveEvent);
            }
            else
            {
                Messages.Message("OARK_ScienceDepartment_GravTechAssistPointReduced".Translate(-trueChange), MessageTypeDefOf.NeutralEvent);
            }
        }
    }

    public void Notify_InitGravQuestCompleted()
    {
        isInitGravQuestCompleted = true;
        OAInteractHandler.Instance.DeregisterCDRecord("GravInitQuestTrigger");

        AddGravTechPoints(100, byPlayer: true);
        AdjustGravTechAssistPoint(100);
    }

    public void Notify_AssistGravResearchQuestStateChange(bool active, Pawn pawn = null)
    {
        if (active && pawn is null)
        {
            Log.Error("Try active gravResearchAssistLendPawn with a null pawn.");
            return;
        }

        OAInteractHandler.Instance.DeregisterCDRecord("QuestGravlitePanel");
        gravResearchAssistLendPawn = active ? pawn : null;
    }

    private void TryTriggerGravInitQuest()
    {
        if (!CanTriggerGravInitQuest())
        {
            return;
        }

        OAInteractHandler.Instance.RegisterCDRecord("GravInitQuestTrigger", cdTicks: 3 * 60000);

        IncidentParms parms = new()
        {
            target = Find.World,
            faction = ModUtility.OAFaction,
            questScriptDef = OARK_QuestScriptDefOf.OARK_InitGravQuest,
            points = 5000f, //不重要
            forced = true
        };

        OAFrame_MiscUtility.TryFireIncidentNow(OAFrameDefOf.OAFrame_GiveQuest, parms, force: true);

        static bool CanTriggerGravInitQuest()
        {
            if (ModUtility.OAFaction is null || ModUtility.OAFaction.PlayerRelationKind != FactionRelationKind.Ally)
            {
                return false;
            }
            if (OAInteractHandler.Instance.IsInCooldown("GravInitQuestTrigger"))
            {
                return false;
            }
            return true;
        }
    }

    private void TryTriggerAnnualInteractionQuest()
    {
        if (!CanTriggerGravInitQuest())
        {
            return;
        }

        IncidentParms parms = new()
        {
            target = Find.World,
            faction = ModUtility.OAFaction,
            points = 5000f, //不重要
            forced = true
        };

        if (OAFrame_MiscUtility.TryFireIncidentNow(OARK_IncidentDefOf.OARK_ScienceDepartmentAnnualInteraction, parms, force: true))
        {
            OAInteractHandler.Instance.RegisterCDRecord("SDAnnualInteraction", cdTicks: Rand.RangeInclusive(70, 90) * 60000);
        }
        else
        {
            OAInteractHandler.Instance.RegisterCDRecord("SDAnnualInteraction", cdTicks: 5 * 60000);
        }

        static bool CanTriggerGravInitQuest()
        {
            if (ModUtility.OAFaction is null || ModUtility.OAFaction.PlayerRelationKind != FactionRelationKind.Ally)
            {
                return false;
            }
            if (OAInteractHandler.Instance.IsInCooldown("SDAnnualInteraction"))
            {
                return false;
            }
            return true;
        }
    }

    public void ExposeData()
    {
        Scribe_Values.Look(ref curGravTechStageIndex, "curGravTechStageIndex", 0);
        Scribe_Values.Look(ref gravTechPoints, "gravTechPoints", 0);
        Scribe_Values.Look(ref playerTechPoints, "playerTechPoints", 0);
        Scribe_Values.Look(ref gravTechAssistPoints, "gravTechAssistPoints", 0);
        Scribe_Values.Look(ref isInitGravQuestCompleted, "isInitGravQuestCompleted", defaultValue: false);

        Scribe_References.Look(ref gravResearchAssistLendPawn, "gravResearchAssistLendPawn");

        Scribe_Deep.Look(ref ScienceShipRecord, "scienceShipRecord");
    }
}
