using OberoniaAurea_Frame;
using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using Verse;
using static RimWorld.ResearchManager;

namespace OberoniaAurea;
public class ResearchSummit_MysteriousTrader : WorldObject_InteractiveBase
{
    protected const int NeedSilver = 1500;
    public override void Notify_CaravanArrived(Caravan caravan)
    {
        Pawn pawn = BestCaravanPawnUtility.FindBestNegotiator(caravan);
        if (pawn is null)
        {
            Messages.Message("OAFrame_MessageNoTrader".Translate(), caravan, MessageTypeDefOf.NegativeEvent, historical: false);
            return;
        }

        Dialog_SimpleNegotiation dialog_Negotiation = new(pawn, TraderNode(caravan, this), radioMode: true)
        {
            soundAmbient = SoundDefOf.RadioComms_Ambience
        };
        Find.WindowStack.Add(dialog_Negotiation);
    }
    protected static DiaNode TraderNode(Caravan caravan, WorldObject worldObject)
    {
        DiaNode root = new("OA_ResearchSummit_MysteriousTrader".Translate());
        TaggedString taggedString = "OA_RSMysteriousTrader_BuyTech".Translate(NeedSilver);
        DiaOption buyTech = new(taggedString);
        List<ResearchProjectDef> availableResearch = DefDatabase<ResearchProjectDef>.AllDefsListForReading.Where(ValidResearch).InRandomOrder().Take(2).ToList();
        if (availableResearch.Any())
        {
            if (CaravanInventoryUtility.HasThings(caravan, ThingDefOf.Silver, NeedSilver))
            {
                buyTech.action = delegate
                {
                    BuyTech(caravan, availableResearch);
                    worldObject.Destroy();
                };
                TaggedString buyString = "OA_RSMysteriousTrader_ConfirmBuy".Translate();
                foreach (ResearchProjectDef projectDef in availableResearch)
                {
                    buyString += "\n" + projectDef.label;
                }
                buyTech.link = new DiaNode(buyString)
                {
                    options =
                    {
                        new DiaOption("OK".Translate())
                        {
                            resolveTree = true
                        }
                    }
                };
            }
            else
            {
                buyTech.Disable("OA_RSMysteriousTrader_NeedSilverCaravan".Translate(NeedSilver));
            }
        }
        else
        {
            buyTech.Disable("OA_RSMysteriousTrader_NoAvailableResearch".Translate());
        }
        DiaOption postpone = new("PostponeLetter".Translate())
        {
            resolveTree = true,
        };
        DiaOption reject = new("RejectLetter".Translate())
        {
            action = worldObject.Destroy,
            resolveTree = true,
        };
        root.options.Add(buyTech);
        root.options.Add(reject);
        root.options.Add(postpone);
        return root;
    }
    protected static void BuyTech(Caravan caravan, List<ResearchProjectDef> availableResearch)
    {
        foreach (ResearchProjectDef projectDef in availableResearch)
        {
            FinishProjectOnly(projectDef, doCompletionDialog: true);
        }
        OAFrame_CaravanUtility.RemoveThingsOfDef(caravan, ThingDefOf.Silver, NeedSilver);
    }
    private static bool ValidResearch(ResearchProjectDef projectDef)
    {
        if (projectDef is null)
        {
            return false;
        }
        if (projectDef.baseCost <= 0 || projectDef.IsFinished || projectDef.IsHidden)
        {
            return false;
        }
        return true;
    }
    private static void FinishProjectOnly(ResearchProjectDef proj, bool doCompletionDialog = false, Pawn researcher = null, bool doCompletionLetter = true)
    {
        ResearchManager researchManager = Find.ResearchManager;
        int num = researchManager.GetTechprints(proj);
        if (num < proj.TechprintCount)
        {
            researchManager.AddTechprints(proj, proj.TechprintCount - num);
        }

        if (proj.RequiredAnalyzedThingCount > 0)
        {
            for (int j = 0; j < proj.requiredAnalyzed.Count; j++)
            {
                CompProperties_CompAnalyzableUnlockResearch compProperties = proj.requiredAnalyzed[j].GetCompProperties<CompProperties_CompAnalyzableUnlockResearch>();
                Find.AnalysisManager.ForceCompleteAnalysisProgress(compProperties.analysisID);
            }
        }

        if (proj.baseCost > 0f)
        {
            Dictionary<ResearchProjectDef, float> progress = ResearchUtility.GetProjectProgress();
            progress[proj] = proj.baseCost;
            ResearchUtility.SetProjectProgress(progress);
        }
        else if (ModsConfig.AnomalyActive && proj.knowledgeCost > 0f)
        {
            Dictionary<ResearchProjectDef, float> anomalyKnowledge = ResearchUtility.GetAnomalyKnowledges();
            anomalyKnowledge?.SetOrAdd(proj, proj.knowledgeCost);
            ResearchUtility.SetAnomalyKnowledges(anomalyKnowledge);
            Find.SignalManager.SendSignal(new Signal("ThingStudied", global: true));
        }

        if (researcher is not null)
        {
            TaleRecorder.RecordTale(TaleDefOf.FinishedResearchProject, researcher, proj);
        }

        researchManager.ReapplyAllMods();
        if (proj.recalculatePower)
        {
            try
            {
                foreach (Map map in Find.Maps)
                {
                    foreach (Thing item in map.listerThings.ThingsInGroup(ThingRequestGroup.PowerTrader))
                    {
                        CompPowerTrader compPowerTrader;
                        if ((compPowerTrader = item.TryGetComp<CompPowerTrader>()) is not null)
                        {
                            compPowerTrader.SetUpPowerVars();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex.ToString());
            }
        }

        if (doCompletionDialog)
        {
            DiaNode diaNode = new((string)("ResearchFinished".Translate(proj.LabelCap) + "\n\n" + proj.description));
            diaNode.options.Add(DiaOption.DefaultOK);
            DiaOption diaOption = new("ResearchScreen".Translate())
            {
                resolveTree = true,
                action = delegate
                {
                    Find.MainTabsRoot.SetCurrentTab(MainButtonDefOf.Research);
                    if (MainButtonDefOf.Research.TabWindow is MainTabWindow_Research mainTabWindow_Research && proj.tab is not null)
                    {
                        mainTabWindow_Research.CurTab = proj.tab;
                    }
                }
            };
            diaNode.options.Add(diaOption);
            Find.WindowStack.Add(new Dialog_NodeTree(diaNode, delayInteractivity: true));
        }

        if (doCompletionLetter && !proj.discoveredLetterTitle.NullOrEmpty() && Find.Storyteller.difficulty.AllowedBy(proj.discoveredLetterDisabledWhen))
        {
            Find.LetterStack.ReceiveLetter(proj.discoveredLetterTitle, proj.discoveredLetterText, LetterDefOf.NeutralEvent);
        }

        if (proj.teachConcept is not null)
        {
            LessonAutoActivator.TeachOpportunity(proj.teachConcept, OpportunityType.Important);
        }

        if (researchManager.GetProject() == proj)
        {
            researchManager.SetCurrentProject(null);
        }
        else if (ModsConfig.AnomalyActive && proj.knowledgeCategory is not null)
        {
            foreach (KnowledgeCategoryProject currentAnomalyKnowledgeProject in researchManager.CurrentAnomalyKnowledgeProjects)
            {
                if (currentAnomalyKnowledgeProject.project == proj)
                {
                    currentAnomalyKnowledgeProject.project = null;
                    break;
                }
            }
        }

        foreach (Def unlockedDef in proj.UnlockedDefs)
        {
            if (unlockedDef is ThingDef thingDef)
            {
                thingDef.Notify_UnlockedByResearch();
            }
        }

        Find.SignalManager.SendSignal(new Signal("ResearchCompleted", global: true));
    }
}
