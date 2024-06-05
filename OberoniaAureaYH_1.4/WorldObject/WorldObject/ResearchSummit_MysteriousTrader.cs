using RimWorld;
using RimWorld.Planet;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace OberoniaAurea;
public class ResearchSummit_MysteriousTrader : WorldObject_InteractiveBase
{
    protected static readonly int NeedSilver = 1500;
    public override void Notify_CaravanArrived(Caravan caravan)
    {
        Pawn pawn = BestCaravanPawnUtility.FindBestNegotiator(caravan);
        if (pawn == null)
        {
            Messages.Message("OA_MessageNoTrader".Translate(), caravan, MessageTypeDefOf.NegativeEvent, historical: false);
            return;
        }

        Dialog_SimpleNegotiation dialog_Negotiation = new(pawn, TraderNode(caravan, this), radioMode: true)
        {
            soundAmbient = SoundDefOf.RadioComms_Ambience
        };
        Find.WindowStack.Add(dialog_Negotiation);
    }
    public override IEnumerable<FloatMenuOption> GetFloatMenuOptions(Caravan caravan)
    {
        foreach (FloatMenuOption floatMenuOption in base.GetFloatMenuOptions(caravan))
        {
            yield return floatMenuOption;
        }
        foreach (FloatMenuOption floatMenuOption2 in CaravanArrivalAction_VisitInteractiveObject.GetFloatMenuOptions(caravan, this))
        {
            yield return floatMenuOption2;
        }
    }
    protected static DiaNode TraderNode(Caravan caravan, WorldObject worldObject)
    {
        DiaNode root = new("OA_ResearchSummit_MysteriousTrader".Translate());
        TaggedString taggedString = "OA_RSMysteriousTrader_BuyTech".Translate();
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
                buyTech.Disable("NeedSilverLaunchable".Translate(NeedSilver));
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
        ResearchManager researchManager = Find.ResearchManager;
        foreach (ResearchProjectDef projectDef in availableResearch)
        {
            researchManager.FinishProject(projectDef, doCompletionDialog: true);
        }
        int remaining = NeedSilver;
        List<Thing> list = CaravanInventoryUtility.TakeThings(caravan, delegate (Thing thing)
        {
            if (thing.def != RimWorld.ThingDefOf.Silver)
            {
                return 0;
            }
            int num = Mathf.Min(remaining, thing.stackCount);
            remaining -= num;
            return num;
        });
        for (int i = 0; i < list.Count; i++)
        {
            list[i].Destroy();
        }
    }
    private static bool ValidResearch(ResearchProjectDef projectDef)
    {
        if (projectDef == null)
        {
            return false;
        }
        if (projectDef.baseCost <= 0 || projectDef.IsFinished)
        {
            return false;
        }
        return true;
    }
}
