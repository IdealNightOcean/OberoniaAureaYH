using OberoniaAurea_Frame;
using RimWorld;
using RimWorld.QuestGen;
using UnityEngine;
using Verse;

namespace OberoniaAurea;

public class ScienceDepartmentInteractHandler : IExposable
{
    private const int MaxGravTechPoints = 100000000;
    public static readonly int[] GravTechStageBoundary = [0, 20000, 40000, 60000];
    public static readonly int MaxGravTechStageIndex = GravTechStageBoundary.Length - 1;

    public static ScienceDepartmentInteractHandler Instance { get; private set; }

    private int curGravTechStageIndex;
    private int gravTechPoints;
    private int playerTechPoints;
    private int gravTechAssistPoints;
    private bool isInitGravQuestCompleted;
    private Pawn gravResearchAssistLendPawn;

    public int CurGravTechStage => curGravTechStageIndex + 1;
    public int GravTechPoints => gravTechPoints;
    public int PlayerTechPoints => playerTechPoints;
    public int GravTechAssistPoints => gravTechAssistPoints;
    public bool IsInitGravQuestCompleted => isInitGravQuestCompleted;
    public Pawn GravResearchAssistLendPawn => gravResearchAssistLendPawn;

    private int nextEMReviewPointBoundary = 20000;
    private float emReviewChance = 0f;
    private int gravTechPointEMBlockDays;
    private EconomyMinistryReviewOutcome lastEconomyReviewOutcome = EconomyMinistryReviewOutcome.Unknown;
    private int lastEconomyReviewTick;

    public int GravTechPointEMBlockDays => gravTechPointEMBlockDays;
    internal EconomyMinistryReviewOutcome LastEconomyReviewOutcome => lastEconomyReviewOutcome;
    public int LastEconomyReviewTick => lastEconomyReviewTick;

    public ScienceShipRecord? ScienceShipRecord;
    public string TodayDutyText = string.Empty;
    public int NextShiftWorkDay = -1;

    public ScienceDepartmentInteractHandler()
    {
        OAFrame_MiscUtility.ValidateSingleton(Instance, nameof(Instance));
        Instance = this;
    }
    public static void ClearStaticCache() => Instance = null;
    public static void OpenDevWindow() => Find.WindowStack.Add(new DevWin_SDInteractHandler());

    public static bool IsInteractAvailable()
    {
        return ModsConfig.OdysseyActive && Instance is not null && ModUtility.OAFaction is not null;
    }

    public void ExposeData()
    {
        Scribe_Values.Look(ref curGravTechStageIndex, "curGravTechStageIndex", 0);
        Scribe_Values.Look(ref gravTechPoints, "gravTechPoints", 0);
        Scribe_Values.Look(ref playerTechPoints, "playerTechPoints", 0);
        Scribe_Values.Look(ref gravTechAssistPoints, "gravTechAssistPoints", 0);
        Scribe_Values.Look(ref isInitGravQuestCompleted, "isInitGravQuestCompleted", defaultValue: false);
        Scribe_Values.Look(ref TodayDutyText, "TodayDutyText", string.Empty);
        Scribe_Values.Look(ref NextShiftWorkDay, "NextShiftWorkDay", -1);

        Scribe_Values.Look(ref nextEMReviewPointBoundary, "nextEMReviewPointBoundary", 20000);
        Scribe_Values.Look(ref emReviewChance, "emReviewChance", 0f);
        Scribe_Values.Look(ref gravTechPointEMBlockDays, "gravTechPointEMBlockDays", 0);
        Scribe_Values.Look(ref lastEconomyReviewOutcome, "lastEconomyReviewOutcome", EconomyMinistryReviewOutcome.Unknown);
        Scribe_Values.Look(ref lastEconomyReviewTick, "lastEconomyReviewTick", -1);

        Scribe_References.Look(ref gravResearchAssistLendPawn, "gravResearchAssistLendPawn");

        Scribe_Deep.Look(ref ScienceShipRecord, "scienceShipRecord");
    }

    public void TickDay()
    {
        DailyGravTechPointsChange();
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
            Messages.Message("OARK_ScienceDepartment_GravTechPointsAdded".Translate(trueChange), MessageTypeDefOf.PositiveEvent);
        }

        if (curGravTechStageIndex < MaxGravTechStageIndex && gravTechPoints >= GravTechStageBoundary[curGravTechStageIndex + 1])
        {
            curGravTechStageIndex++;
            if (isInitGravQuestCompleted)
            {
                InteractUtility.SendGravTechStageUpgradeLetter(CurGravTechStage);
            }
        }

        if (gravTechPoints > nextEMReviewPointBoundary)
        {
            nextEMReviewPointBoundary += 10000;
            TryTriggerEMReview();
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
        OAInteractHandler.Instance.CooldownManager.DeregisterRecord("GravInitQuestTrigger");

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

        OAInteractHandler.Instance.CooldownManager.DeregisterRecord("QuestGravlitePanel");
        gravResearchAssistLendPawn = active ? pawn : null;
    }

    internal void SetEconomyReviewOutcome(EconomyMinistryReviewOutcome outcome)
    {
        lastEconomyReviewOutcome = outcome;
        lastEconomyReviewTick = Find.TickManager.TicksGame;
    }

    public void StartEMReviewBlock()
    {
        gravTechPointEMBlockDays = 15;
    }

    private void DailyGravTechPointsChange()
    {
        if (gravTechPointEMBlockDays > 0)
        {
            gravTechPointEMBlockDays--;
            if (gravTechPointEMBlockDays == 0)
            {
                AddGravTechPoints(1200, byPlayer: false, showMessage: false);
                Find.LetterStack.ReceiveLetter(label: "OARK_LetterLabel_EMBlockEnd".Translate(),
                                               text: "OARK_LetterLabel_EMBlockEnd".Translate(),
                                               textLetterDef: LetterDefOf.NegativeEvent,
                                               lookTargets: null,
                                               relatedFaction: ModUtility.OAFaction);
            }
        }
        else
        {
            AddGravTechPoints(Rand.RangeInclusive(50, 150), byPlayer: false, showMessage: false);
        }
    }

    private void TryTriggerEMReview()
    {
        if (!IsInitGravQuestCompleted || !ModUtility.IsOAFactionAlly() || OAInteractHandler.Instance.CooldownManager.IsInCooldown("EconomyMinistryReview"))
        {
            emReviewChance += 0.25f;
            return;
        }

        if (Rand.Chance(emReviewChance))
        {
            emReviewChance = 0f;
            OAInteractHandler.Instance.CooldownManager.RegisterRecord("EconomyMinistryReview", cdTicks: 15 * 60000, shouldRemoveWhenExpired: false);
            ChoiceLetter_EMReview letter = (ChoiceLetter_EMReview)LetterMaker.MakeLetter(label: "OARK_LetterLabel_EMReview".Translate(),
                                                                                         text: "OARK_Letter_EMReview".Translate(),
                                                                                         def: OARK_LetterDefOf.OARK_EMReviewLetter,
                                                                                         relatedFaction: ModUtility.OAFaction);
            int delayTicks = Rand.RangeInclusive(1 * 60000, 3 * 60000);
            letter.StartTimeout(delayTicks + 30000);
            Find.LetterStack.ReceiveLetter(letter, delayTicks: delayTicks);
        }
        else
        {
            emReviewChance += 0.25f;
        }
    }

    private void TryTriggerGravInitQuest()
    {
        if (!CanTriggerGravInitQuest())
        {
            return;
        }

        OAInteractHandler.Instance.CooldownManager.RegisterRecord("GravInitQuestTrigger", cdTicks: 3 * 60000, shouldRemoveWhenExpired: true);

        OAFrame_QuestUtility.TryGenerateQuestAndMakeAvailable(out _, OARK_QuestScriptDefOf.OARK_InitGravQuest, new Slate(), forced: false);

        static bool CanTriggerGravInitQuest()
        {
            if (!ModUtility.IsOAFactionAlly())
            {
                return false;
            }
            if (OAInteractHandler.Instance.CooldownManager.IsInCooldown("GravInitQuestTrigger"))
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
            OAInteractHandler.Instance.CooldownManager.RegisterRecord("SDAnnualInteraction", cdTicks: Rand.RangeInclusive(50, 70) * 60000, shouldRemoveWhenExpired: false);
        }
        else
        {
            OAInteractHandler.Instance.CooldownManager.RegisterRecord("SDAnnualInteraction", cdTicks: 2 * 60000, shouldRemoveWhenExpired: false);
        }

        static bool CanTriggerGravInitQuest()
        {
            if (ModUtility.OAFaction is null || ModUtility.OAFaction.PlayerRelationKind != FactionRelationKind.Ally)
            {
                return false;
            }
            if (OAInteractHandler.Instance.CooldownManager.IsInCooldown("SDAnnualInteraction"))
            {
                return false;
            }
            return true;
        }
    }
}
