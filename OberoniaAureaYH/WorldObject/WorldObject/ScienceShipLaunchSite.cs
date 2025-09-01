using OberoniaAurea_Frame;
using RimWorld;
using RimWorld.Planet;
using RimWorld.QuestGen;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using Verse.Grammar;

namespace OberoniaAurea;

[StaticConstructorOnStartup]
public class ScienceShipLaunchSite : WorldObject_InteractWithFixedCaravanBase
{
    private static readonly Texture2D quizIcon = ContentFinder<Texture2D>.Get("UI/OARK_ScienceShipLaunch_Quiz");
    private static readonly Texture2D inquiryIcon = ContentFinder<Texture2D>.Get("UI/OARK_ScienceShipLaunch_Inquiry");
    private static readonly Texture2D pryIcon = ContentFinder<Texture2D>.Get("UI/OARK_ScienceShipLaunch_Pry");

    private string shipName;
    private ScienceShipRecord? scienceShip;
    private bool shipLaunched;

    private bool blockInteraction;

    private int quizCount;
    private bool quizDialogOpen;

    private int nextCanInteractTick;

    public override int TicksNeeded => 20000;
    protected override WorldObjectDef FixedCaravanDef => OARK_WorldObjectDefOf.OARK_FixedCaravan_ScienceShipLaunch;

    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_Deep.Look(ref scienceShip, "scienceShip");

        Scribe_Values.Look(ref shipName, "shipName");
        Scribe_Values.Look(ref shipLaunched, "shipLaunched", defaultValue: false);

        Scribe_Values.Look(ref blockInteraction, "blockInteraction", defaultValue: false);

        Scribe_Values.Look(ref quizCount, "quizCount", 0);
        Scribe_Values.Look(ref quizDialogOpen, "quizDialogOpen", defaultValue: false);

        Scribe_Values.Look(ref nextCanInteractTick, "nextCanInteractTick", 0);

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

    public override bool StartWork(Caravan caravan)
    {
        bool result = base.StartWork(caravan);
        if (result)
        {
            Find.LetterStack.ReceiveLetter(label: "OARK_ScienceShipLaunchSite_StartLabel".Translate(),
                                           text: "OARK_ScienceShipLaunchSite_StartDesc".Translate(shipName),
                                           textLetterDef: LetterDefOf.PositiveEvent,
                                           lookTargets: this,
                                           relatedFaction: Faction,
                                           quest: quest);
        }
        return result;
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

        Find.WindowStack.Add(OAFrame_DiaUtility.DefaultConfirmDiaNodeTreeWithFactionInfo(GetShipLaunchText(maxSkillPawn, maxLevel), ModUtility.OAFaction));

        Destroy();
    }

    protected override void InterruptWork()
    {
        LaunchShip();
        Find.WindowStack.Add(OAFrame_DiaUtility.DefaultConfirmDiaNodeTreeWithFactionInfo("OARK_ScienceShipLaunch_Interrupt".Translate(), ModUtility.OAFaction));
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

    public IEnumerable<Gizmo> InteractGizmos()
    {
        if (isWorking && !quizDialogOpen)
        {
            int cooldownTicksLeft = nextCanInteractTick - Find.TickManager.TicksGame;
            if (scienceShip.HasValue)
            {
                Command_Action command_Quiz = new()
                {
                    defaultLabel = "OARK_ScienceShipLaunch_Quiz".Translate(),
                    defaultDesc = "OARK_ScienceShipLaunch_QuizDesc".Translate(),
                    action = QuizResult,
                    icon = quizIcon
                };
                if (blockInteraction)
                {
                    command_Quiz.Disable("OARK_ScienceShipLaunch_Block".Translate());
                }
                else if (cooldownTicksLeft > 0)
                {
                    command_Quiz.Disable("WaitTime".Translate(cooldownTicksLeft.ToStringTicksToPeriod()));
                }
                yield return command_Quiz;
            }

            Command_Action command_Inquiry = new()
            {
                defaultLabel = "OARK_ScienceShipLaunch_Inquiry".Translate(),
                defaultDesc = "OARK_ScienceShipLaunch_InquiryDesc".Translate(),
                action = InquiryResult,
                icon = inquiryIcon
            };
            if (ScienceDepartmentInteractHandler.Instance.GravTechAssistPoints < 5)
            {
                command_Inquiry.Disable("OARK_GravTechAssistPointsNotEnough".Translate(5));
            }

            Command_Action command_Pry = new()
            {
                defaultLabel = "OARK_ScienceShipLaunch_Pry".Translate(),
                defaultDesc = "OARK_ScienceShipLaunch_PryDesc".Translate(),
                action = PryDialog,
                icon = pryIcon
            };

            if (blockInteraction)
            {
                string disableReason = "OARK_ScienceShipLaunch_Block".Translate();
                command_Inquiry.Disable(disableReason);
                command_Pry.Disable(disableReason);
            }
            else if (cooldownTicksLeft > 0)
            {
                string disableReason = "WaitTime".Translate(cooldownTicksLeft.ToStringTicksToPeriod());
                command_Inquiry.Disable(disableReason);
                command_Pry.Disable(disableReason);
            }


            yield return command_Inquiry;
            yield return command_Pry;
        }

    }

    private void QuizResult()
    {
        quizCount++;
        nextCanInteractTick = Find.TickManager.TicksGame + 1250;

        if (associatedFixedCaravan is null)
        {
            return;
        }

        bool sendLetter = false;
        TaggedString letterLabel = TaggedString.Empty;
        TaggedString letterText = TaggedString.Empty;
        LetterDef letterDef = LetterDefOf.PositiveEvent;

        if (quizCount < 6)
        {
            letterLabel = "OARK_ScienceShipLaunch_QuizLabel".Translate();

            GrammarRequest grammarRequest = new();
            grammarRequest.Includes.Add(OARK_RulePackDef.OARK_RulePack_ScienceShipQuiz);
            grammarRequest.Constants.Add("envirType", scienceShip.Value.EnvironmentAffect.ToString());
            grammarRequest.Constants.Add("shipType", scienceShip.Value.TypeOfShip.ToString());
            letterText = GenText.CapitalizeAsTitle(GrammarResolver.Resolve("r_text", grammarRequest));
            sendLetter = true;
        }
        else if (quizCount == 6)
        {
            blockInteraction = true;
            float maxNegotiationAbility = associatedFixedCaravan.PawnsListForReading.Where(p => p.IsColonist && p.Awake())?.Select(p => p.GetStatValue(StatDefOf.NegotiationAbility))?.Max() ?? 0f;
            if (maxNegotiationAbility >= 3f)
            {
                sendLetter = false;
                quizDialogOpen = true;
                Dialog_NodeTreeWithFactionInfo nodeTree = OAFrame_DiaUtility.ConfirmDiaNodeTreeWithFactionInfo(
                    text: "OARK_ScienceShipLaunch_Quiz6Special".Translate(),
                    faction: Faction,
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
                letterLabel = "OARK_ScienceShipLaunch_QuizLabel6".Translate();
                letterText = "OARK_ScienceShipLaunch_Quiz6".Translate();
            }
        }
        else
        {
            sendLetter = false;
            blockInteraction = true;

            // letterLabel = "OARK_ScienceShipLaunch_QuizLabelAbove6".Translate();
            // letterText = "OARK_ScienceShipLaunch_QuizAbove6".Translate();
        }

        if (quizCount <= 5)
        {
            foreach (Pawn pawn in associatedFixedCaravan.PawnsListForReading)
            {
                pawn.skills?.GetSkill(SkillDefOf.Intellectual).Learn(50f, direct: true);
            }

            letterText += ("\n\n" + "OARK_ScienceShipLaunch_GainXP".Translate(50f, SkillDefOf.Intellectual.LabelCap));
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

    private void InquiryResult()
    {
        nextCanInteractTick = Find.TickManager.TicksGame + 2500;

        if (associatedFixedCaravan is null)
        {
            return;
        }

        StringBuilder sb = new("OARK_ScienceShipLaunch_InquiryInfo".Translate());
        sb.AppendLine();
        foreach (Pawn pawn in associatedFixedCaravan.PawnsListForReading)
        {
            pawn.skills?.GetSkill(SkillDefOf.Intellectual).Learn(1500f, direct: true);
        }
        sb.AppendInNewLine("OARK_ScienceShipLaunch_GainXP".Translate(1500, SkillDefOf.Intellectual.LabelCap));
        if (true)
        {
            ResearchProjectDef projectDef = ResearchUtility.GetSignalResearchableProject(50);
            if (projectDef is not null)
            {
                Find.ResearchManager.AddProgress(projectDef, 500f);
                sb.AppendLine();
                sb.AppendInNewLine("OAFrame_ResearchProgressAdded".Translate(500));
                sb.AppendInNewLine(projectDef.LabelCap);
            }
        }

        Find.LetterStack.ReceiveLetter(label: "OARK_ScienceShipLaunch_InquiryLabel".Translate(),
                                       text: sb.ToString(),
                                       textLetterDef: LetterDefOf.PositiveEvent,
                                       lookTargets: this,
                                       relatedFaction: Faction,
                                       quest: quest);
    }

    private void PryDialog()
    {
        StringBuilder sb = new("OARK_ScienceShipLaunch_PryDialog".Translate());

        QuestScriptDef questScriptDef;
        string rewardText;
        if (Rand.Chance(0.25f))
        {
            questScriptDef = OARK_RimWorldDefOf.Opportunity_AncientStructureGarrison;
            rewardText = "OARK_ScienceShipLaunch_PryReward_Weapon".Translate();
        }
        else
        {
            questScriptDef = OARK_RimWorldDefOf.OpportunitySite_AlphaThrumbo_Giver;
            rewardText = "OARK_ScienceShipLaunch_PryReward_Thrumbo".Translate();
        }

        PlanetTile tile = Tile;
        PlanetLayer tileLayer = tile.Layer;
        int nearSettleCount = Find.WorldObjects.Settlements.Where(s => !s.Faction.IsPlayerSafe() && s.Tile.Layer == tileLayer && Find.WorldGrid.ApproxDistanceInTiles(tile, s.Tile) <= 25f).Count();
        (Pawn maxSocialPawn, int maxSocialSkill) = OAFrame_PawnUtility.GetMaxSkillLevelPawn(associatedFixedCaravan.PawnsListForReading, SkillDefOf.Social);
        float chance = 0.05f + nearSettleCount * 0.025f + maxSocialSkill * 0.01f;

        sb.AppendLine();
        sb.AppendInNewLine("OARK_ScienceShipLaunch_PryRewardInfo".Translate(chance.ToStringPercent(), rewardText));
        sb.AppendLine();
        sb.AppendLine("OARK_ScienceShipLaunch_PryRewardChance_Base".Translate(0.05f.ToStringPercent()));
        sb.AppendLine("OARK_ScienceShipLaunch_PryRewardChance_Settlement".Translate(nearSettleCount, (nearSettleCount * 0.025f).ToStringPercent()));
        sb.AppendLine("OARK_ScienceShipLaunch_PryRewardChance_Skill".Translate(maxSocialSkill, (maxSocialSkill * 0.01f).ToStringPercent()));

        Dialog_NodeTreeWithFactionInfo nodeTree = OAFrame_DiaUtility.DefaultConfirmDiaNodeTreeWithFactionInfo(
            text: sb.ToString(),
            faction: Faction,
            acceptAction: delegate { PryResult(questScriptDef, chance); });

        Find.WindowStack.Add(nodeTree);
    }
    private void PryResult(QuestScriptDef questScriptDef, float chance)
    {
        nextCanInteractTick = Find.TickManager.TicksGame + 5000;

        bool questTriggered = false;
        if (Rand.Chance(chance))
        {
            Slate slate = new();
            slate.Set("points", Rand.Range(3000f, 5000f));
            slate.Set("discoveryMethod", "OARK_QuestDiscoveredFromFaction".Translate(Faction?.Name));
            questTriggered = OAFrame_QuestUtility.TryGenerateQuestAndMakeAvailable(out _, questScriptDef, slate, forced: true);
        }

        TaggedString letterText;
        if (questTriggered)
        {
            string rewardText = questScriptDef == OARK_RimWorldDefOf.Opportunity_AncientStructureGarrison ? "OARK_ScienceShipLaunch_PryReward_Weapon".Translate() : "OARK_ScienceShipLaunch_PryReward_Thrumbo".Translate();
            letterText = "OARK_ScienceShipLaunch_PrySuccess".Translate(rewardText);
        }
        else
        {
            letterText = "OARK_ScienceShipLaunch_PryNormal".Translate();
        }

        Find.LetterStack.ReceiveLetter(label: "OARK_ScienceShipLaunch_PryLabel".Translate(),
                                       text: letterText,
                                       textLetterDef: LetterDefOf.PositiveEvent,
                                       lookTargets: this,
                                       relatedFaction: Faction,
                                       quest: quest);
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
