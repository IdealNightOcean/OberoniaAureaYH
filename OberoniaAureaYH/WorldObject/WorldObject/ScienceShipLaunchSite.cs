using OberoniaAurea_Frame;
using RimWorld;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using Verse.Grammar;

namespace OberoniaAurea;

public class ScienceShipLaunchSite : WorldObject_InteractWithFixedCaravanBase
{
    private string shipName;
    private ScienceShipRecord? scienceShip;
    private bool shipLaunched;
    private int quizCount;
    private bool quizDialogOpen;
    public override int TicksNeeded => 12500;
    protected override WorldObjectDef FixedCaravanDef => OARK_WorldObjectDefOf.OARK_FixedCaravan_ScienceShipLaunch;

    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_Values.Look(ref shipLaunched, "shipLaunched", defaultValue: false);
        Scribe_Values.Look(ref quizCount, "quizCount", 0);
        Scribe_Values.Look(ref quizDialogOpen, "quizDialogOpen", defaultValue: false);
        Scribe_Values.Look(ref shipName, "shipName");

        Scribe_Deep.Look(ref scienceShip, "scienceShip");
    }

    public void InitShip(string shipName)
    {
        this.shipName = shipName;
        scienceShip = ScienceShipRecord.GenerateShip(shipName);
    }

    public override string GetInspectString()
    {
        StringBuilder sb = new(base.GetInspectString());
        sb.AppendInNewLine("OARK_ScienceShipName".Translate());
        sb.Append(": ");
        sb.Append(shipName);

        sb.AppendInNewLine("OARK_ScienceShipLaunchSite_TimeLeft".Translate(ticksRemaining.ToStringTicksToPeriod()));

        return sb.ToString();
    }

    protected override void FinishWork()
    {
        int maxLevel = 0;

        SkillRecord skillRecord;
        int skillLevel;
        Pawn maxSkillPawn = null;
        foreach (Pawn pawn in associatedFixedCaravan.PawnsListForReading)
        {
            skillRecord = pawn.skills?.GetSkill(SkillDefOf.Intellectual);
            skillLevel = skillRecord is null ? 0 : skillRecord.GetLevel();
            if (skillLevel > maxLevel)
            {
                maxLevel = skillLevel;
                maxSkillPawn = pawn;
            }

            if (pawn.needs.mood is not null)
            {
                Thought_Memory thought = ThoughtMaker.MakeThought(OARK_ThoughtDefOf.OARK_Thought_ScienceShipLaunch, skillLevel < 10 ? 0 : 1);
                pawn.needs.mood.thoughts.memories.TryGainMemory(thought);
            }
        }

        maxSkillPawn ??= associatedFixedCaravan.PawnsListForReading.RandomElement();

        List<Thing> rewards = OAFrame_MiscUtility.TryGenerateThing(OARK_ThingDefOf.Oberonia_Aurea_Chanwu_AB, associatedFixedCaravan.PawnsListForReading.Count * 3);
        OAFrame_FixedCaravanUtility.GiveThings(associatedFixedCaravan, rewards);

        LaunchShip();

        Find.WindowStack.Add(OAFrame_DiaUtility.DefaultConfirmDiaNodeTree(GetShipLaunchText(maxSkillPawn, maxLevel)));

        Destroy();
    }

    protected override void InterruptWork()
    {
        LaunchShip();
        Find.WindowStack.Add(OAFrame_DiaUtility.DefaultConfirmDiaNodeTree("OARK_ScienceShipLaunch_Interrupt".Translate()));
        Destroy();
    }

    private void LaunchShip()
    {
        shipLaunched = true;
        if (!scienceShip.HasValue)
        {
            scienceShip = ScienceShipRecord.GenerateShip(shipName);
        }
        ScienceDepartmentInteractHandler.Instance.ScienceShipRecord = scienceShip;
        IncidentParms parms = new()
        {
            target = Find.World,
            faction = ModUtility.OAFaction
        };
        OAFrame_MiscUtility.AddNewQueuedIncident(OARK_IncidentDefOf.OARK_ScienceShipRecycle, Rand.RangeInclusive(20, 40) * 60000, parms);
    }

    public override void Destroy()
    {
        if (!shipLaunched)
        {
            LaunchShip();
        }
        base.Destroy();
    }

    public Command_Action QuizCommand()
    {
        Command_Action command_Quiz = null;
        if (isWorking && !quizDialogOpen && scienceShip.HasValue)
        {
            command_Quiz = new()
            {
                defaultLabel = "OARK_ScienceShipLaunch_Quiz".Translate(),
                defaultDesc = "OARK_ScienceShipLaunch_QuizDesc".Translate(),
                action = QuizResult,
                icon = IconUtility.OADipIcon
            };
        }
        return command_Quiz;
    }

    private void QuizResult()
    {
        if (associatedFixedCaravan is null)
        {
            return;
        }

        quizCount++;

        bool sendLetter = false;
        TaggedString letterLabel = TaggedString.Empty;
        TaggedString letterText = TaggedString.Empty;
        LetterDef letterDef = LetterDefOf.PositiveEvent;

        if (quizCount < 6)
        {
            letterLabel = "OARK_ScienceShipLaunch_QuizLabel".Translate();

            GrammarRequest grammarRequest = new();
            grammarRequest.Includes.Add(OARK_ModDefOf.OARK_PackScienceShipQuiz);
            grammarRequest.Constants.Add("envirType", scienceShip.Value.EnvironmentAffect.ToString());
            grammarRequest.Constants.Add("shipType", scienceShip.Value.TypeOfShip.ToString());
            letterText = GenText.CapitalizeAsTitle(GrammarResolver.Resolve("r_text", grammarRequest));
            sendLetter = true;
        }
        else if (quizCount == 6)
        {
            float maxNegotiationAbility = associatedFixedCaravan.PawnsListForReading.Where(p => p.IsColonist && p.Awake())?.Select(p => p.GetStatValue(StatDefOf.NegotiationAbility))?.Max() ?? 0f;
            if (maxNegotiationAbility >= 3f)
            {
                sendLetter = false;
                quizDialogOpen = true;
                Dialog_NodeTree nodeTree = OAFrame_DiaUtility.ConfirmDiaNodeTree(text: "OARK_ScienceShipLaunch_Quiz6Special".Translate(),
                                                                                 acceptText: "OARK_ScienceShipLaunch_FlowCake".Translate(),
                                                                                 acceptAction: delegate
                                                                                 {
                                                                                     quizDialogOpen = false;
                                                                                     if (associatedFixedCaravan is not null)
                                                                                     {
                                                                                         List<Thing> flowCakes = OAFrame_MiscUtility.TryGenerateThing(OARK_ThingDefOf.Oberonia_Aurea_Chanwu_AB, 5 * associatedFixedCaravan.PawnsCount);
                                                                                         OAFrame_FixedCaravanUtility.GiveThings(associatedFixedCaravan, flowCakes);
                                                                                         Find.LetterStack.ReceiveLetter(label: "OARK_ScienceShipLaunch_Quiz6GainLabel".Translate(5),
                                                                                                                        text: "OARK_ScienceShipLaunch_Quiz6Gain".Translate(),
                                                                                                                        textLetterDef: LetterDefOf.PositiveEvent,
                                                                                                                        lookTargets: associatedFixedCaravan,
                                                                                                                        relatedFaction: Faction,
                                                                                                                        quest: quest);
                                                                                     }
                                                                                 },
                                                                                 rejectText: "Close".Translate(),
                                                                                 rejectAction: delegate { quizDialogOpen = false; });
                Find.WindowStack.Add(nodeTree);
            }
            else
            {
                sendLetter = true;
                letterLabel = "OARK_ScienceShipLaunch_QuizLabelAbove6".Translate();
                letterText = "OARK_ScienceShipLaunch_QuizAbove6".Translate();
            }
        }
        else
        {
            sendLetter = true;
            letterLabel = "OARK_ScienceShipLaunch_QuizLabelAbove6".Translate();
            letterText = "OARK_ScienceShipLaunch_QuizAbove6".Translate();
        }

        if (quizCount <= 6)
        {
            foreach (Pawn pawn in associatedFixedCaravan.PawnsListForReading)
            {
                pawn.skills?.GetSkill(SkillDefOf.Intellectual).Learn(50f, direct: true);
            }

            letterText += ("\n\n" + "OARK_ScienceShipLaunch_QuizGainXP".Translate(50f));
        }

        if (sendLetter)
        {
            Find.LetterStack.ReceiveLetter(label: letterLabel,
                                           text: letterText,
                                           textLetterDef: letterDef,
                                           lookTargets: this,
                                           relatedFaction: Faction,
                                           quest: quest);
        }


    }

    private static string GetShipLaunchText(Pawn pawn, int skillLevel)
    {
        StringBuilder sb = new("OARK_ScienceShipLaunchFinished".Translate(3000, 3));
        ScienceShipRecord shipRecord = ScienceDepartmentInteractHandler.Instance.ScienceShipRecord.Value;

        if (skillLevel >= 15)
        {
            sb.AppendLine();
            sb.AppendInNewLine("OARK_ScienceShipEnvir".Translate(pawn));
            sb.AppendInNewLine(ScienceShipRecord.GetShipEnvirLabel(shipRecord.EnvironmentAffect).Translate());

            if (skillLevel >= 18)
            {
                sb.AppendLine();
                sb.AppendInNewLine(ScienceShipRecord.GetShipLaunchText(shipRecord.TypeOfShip).Translate());
            }
        }

        return sb.ToString();
    }
}
