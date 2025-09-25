using OberoniaAurea_Frame;
using RimWorld;
using RimWorld.QuestGen;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace OberoniaAurea;

public class QuestNode_GravTechAssistWatcher : QuestNode
{
    [NoTranslate]
    public SlateRef<string> inSignalEnable;

    protected override bool TestRunInt(Slate slate)
    {
        return true;
    }

    protected override void RunInt()
    {
        Slate slate = QuestGen.slate;
        QuestPart_GravTechAssistWatcher questPart_GravTechAssistWatcher = new()
        {
            inSignalEnable = QuestGenUtility.HardcodedSignalWithQuestID(inSignalEnable.GetValue(slate)) ?? slate.Get<string>("inSignal")
        };
        QuestGen.quest.AddPart(questPart_GravTechAssistWatcher);
    }
}


public class QuestPart_GravTechAssistWatcher : QuestPartActivable
{
    private bool enabled;
    private int ticksLeft;

    private int dailyGTP;

    public int TotalGTP;
    public int ExternalGTAP;
    public int GainGTAP => Mathf.RoundToInt(TotalGTP / 10f) + ExternalGTAP;

    protected override void Enable(SignalArgs receivedArgs)
    {
        base.Enable(receivedArgs);
        enabled = true;

        IEnumerable<Pawn> LentColonists = ModUtility.GetLendColonistsFromQuest(quest);
        Pawn researcher = LentColonists.FirstOrFallback(null);
        if (researcher is not null)
        {
            ScienceDepartmentInteractHandler.Instance?.Notify_AssistGravResearchQuestStateChange(true, researcher);
        }

        float dailyPoints = 0;
        foreach (Pawn pawn in LentColonists)
        {
            dailyPoints += pawn.GetStatValue(StatDefOf.ResearchSpeed) * 40f;
        }
        dailyGTP = Mathf.Clamp((int)dailyPoints, 0, 200);
        ticksLeft = 60000;

        TotalGTP = dailyGTP;
        ScienceDepartmentInteractHandler.Instance?.AddGravTechPoints(dailyGTP, byPlayer: true);
    }

    public override void QuestPartTick()
    {
        base.QuestPartTick();
        if (!enabled)
        {
            return;
        }
        if (ticksLeft-- < 0)
        {
            ticksLeft = 60000;
            TotalGTP += dailyGTP;
            ScienceDepartmentInteractHandler.Instance?.AddGravTechPoints(dailyGTP, byPlayer: true);
        }
    }

    public override void Notify_PreCleanup()
    {
        base.Notify_PreCleanup();
        ScienceDepartmentInteractHandler.Instance?.Notify_AssistGravResearchQuestStateChange(false);

        ScienceDepartmentInteractHandler.Instance?.AdjustGravTechAssistPoint(GainGTAP);
        Map map = Find.AnyPlayerHomeMap;
        if (map is null)
        {
            return;
        }

        if (TotalGTP > 1500)
        {
            ThingDef techCore = DefDatabase<ThingDef>.GetNamedSilentFail("TechprofSubpersonaCore");
            Dialog_NodeTreeWithFactionInfo nodeTree = OAFrame_DiaUtility.ConfirmDiaNodeTreeWithFactionInfo(
                text: "OARK_GravTechAssist_RewardChoice".Translate(),
                faction: ModUtility.OAFaction,
                acceptText: techCore.LabelCap,
                acceptAction: delegate
                {
                    OAFrame_DropPodUtility.DefaultDropSingleThingOfDef(techCore, map, ModUtility.OAFaction);
                },
                rejectText: OARK_RimWorldDefOf.VanometricPowerCell.LabelCap,
                rejectAction: delegate
                {
                    Thing building = ThingMaker.MakeThing(OARK_RimWorldDefOf.VanometricPowerCell);
                    Thing item = MinifyUtility.TryMakeMinified(building);
                    OAFrame_DropPodUtility.DefaultDropSingleThing(item, map, ModUtility.OAFaction);
                });
            Find.WindowStack.Add(nodeTree);
        }
    }


    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_Values.Look(ref enabled, "enabled", defaultValue: false);
        Scribe_Values.Look(ref ticksLeft, "ticksLeft", 0);
        Scribe_Values.Look(ref dailyGTP, "dailyGTP", 0);
        Scribe_Values.Look(ref TotalGTP, "totalGTP", 0);
        Scribe_Values.Look(ref ExternalGTAP, "externalGTAP", 0);
    }
}
