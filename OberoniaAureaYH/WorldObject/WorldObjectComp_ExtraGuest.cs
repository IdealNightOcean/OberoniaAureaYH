using OberoniaAurea_Frame.Utility;
using RimWorld;
using RimWorld.Planet;
using System.Collections.Generic;
using System.Text;
using Verse;

namespace OberoniaAurea;

public class WorldObjectCompProperties_ExtraGuest : WorldObjectCompProperties
{
    public WorldObjectCompProperties_ExtraGuest() => compClass = typeof(WorldObjectComp_ExtraGuest);
}

public class WorldObjectComp_ExtraGuest : WorldObjectComp
{
    private static readonly CachedTexture TradeCommandTex = new("UI/Commands/Trade");

    private ExtraSiteGuest extraSiteGuest;
    public ExtraSiteGuest ExtraSiteGuest => extraSiteGuest;

    public override void PostExposeData()
    {
        base.PostExposeData();
        Scribe_Deep.Look(ref extraSiteGuest, nameof(extraSiteGuest));
    }

    public override string CompInspectStringExtra()
    {
        if (extraSiteGuest is null || !extraSiteGuest.IsActive)
            return null;

        return "OARK_Inspect_ExtraSiteGuestFaction".Translate(extraSiteGuest.GuestFaction.Named(KeyLibrary_NamedArgument.FACTION));
    }

    public static void Notify_GameStart()
    {
        Faction oaFaction = ModUtility.OAFaction;
        foreach (Settlement settlement in Find.WorldObjects.Settlements)
        {
            if (settlement.Destroyed || settlement.Faction != oaFaction)
                continue;

            WorldObjectComp_ExtraGuest extraSiteTraderComp = settlement.GetComponent<WorldObjectComp_ExtraGuest>();
            if (extraSiteTraderComp is not null && extraSiteTraderComp.extraSiteGuest is null)
            {
                extraSiteTraderComp.Active();
                extraSiteTraderComp.extraSiteGuest?.RefreshTraderMeta();
            }
        }
    }

    public override void CompTickInterval(int delta)
    {
        if (parent.IsHashIntervalTick(60000) && extraSiteGuest is not null)
        {
            if ((--extraSiteGuest.DaysToExpired) <= 0)
            {
                extraSiteGuest.RefreshTraderMeta();
            }
        }
    }

    public void Active() => extraSiteGuest ??= new(parent);

    public void Deactive()
    {
        extraSiteGuest?.ClearMeta();
        extraSiteGuest = null;
    }

    public override IEnumerable<Gizmo> GetCaravanGizmos(Caravan caravan)
    {
        foreach (Gizmo gizmo in base.GetCaravanGizmos(caravan))
            yield return gizmo;

        if (extraSiteGuest is null || !extraSiteGuest.IsActive)
            yield break;

        Command_Action command_ExtraSiteTrader = new()
        {
            defaultLabel = "OARK_VisitExtraGuestInfo".Translate(extraSiteGuest.GuestFaction.Named(KeyLibrary_NamedArgument.FACTION)),
            defaultDesc = "CommandTradeDesc".Translate(),
            icon = TradeCommandTex.Texture,
            action = () => InteractWithGuest(caravan)
        };
        yield return command_ExtraSiteTrader;
    }


    public void InteractWithGuest(Caravan caravan)
    {
        StringBuilder nodeTextBuilder = new(64);
        NamedArgument factionNamedArgument = extraSiteGuest.GuestFaction.Named(KeyLibrary_NamedArgument.FACTION);
        switch (extraSiteGuest.GuestFaction.PlayerRelationKind)
        {
            case FactionRelationKind.Ally:
                {
                    nodeTextBuilder.AppendLine("OARK_InteractWithGuest_AllyNode".Translate(factionNamedArgument));

                    if (extraSiteGuest.FirstInteracted)
                    {
                        nodeTextBuilder.AppendLine("OARK_InteractWithGuest_AllyNode_First".Translate(factionNamedArgument));

                        int count = Rand.Range(5, 25);
                        List<Thing> gifts = OAFrame_ThingUtility.GenerateThingListSplitByStack(OARK_ThingDefOf.OARK_IntelligenceDraft, count);
                        OAFrame_CaravanUtility.GiveThings(caravan, gifts);

                        nodeTextBuilder.AppendLine();
                        nodeTextBuilder.AppendLine("OAFrame_GetThings".Translate(GenLabel.ThingsLabel(gifts).Named(KeyLibrary_NamedArgument.ThingsLabel)));

                        extraSiteGuest.FirstInteracted = false;
                    }

                    break;
                }
            case FactionRelationKind.Hostile:
                {
                    nodeTextBuilder.AppendLine("OARK_InteractWithGuest_HostileNode".Translate(factionNamedArgument));
                    break;
                }
            default:
                {
                    nodeTextBuilder.AppendLine("OARK_InteractWithGuest_NeutralNode".Translate(factionNamedArgument));
                    break;
                }
        }
        DiaNode diaNode = new(TaggedString.Empty);
        if (extraSiteGuest.IsTraderActive && extraSiteGuest.SiteTrader is not null)
        {
            nodeTextBuilder.AppendLine();
            nodeTextBuilder.AppendLine("OARK_InteractWithGuest_TradeText".Translate(extraSiteGuest.TraderKind.Named(KeyLibrary_NamedArgument.DEF)));

            DiaOption tradeOpt = new("OARK_InteractWithGuest_TradeOpt".Translate(extraSiteGuest.TraderKind.Named(KeyLibrary_NamedArgument.DEF)))
            {
                action = delegate
                {
                    if (extraSiteGuest.SiteTrader is not null)
                    {
                        Pawn playerNegotiator = BestCaravanPawnUtility.FindBestNegotiator(caravan, extraSiteGuest.GuestFaction, extraSiteGuest.TraderKind);
                        Find.WindowStack.Add(new Dialog_Trade(playerNegotiator, extraSiteGuest.SiteTrader));
                    }
                },
                resolveTree = true
            };

            diaNode.options.Add(tradeOpt);
        }

        diaNode.AddDefaultCloseOption();

        diaNode.text = nodeTextBuilder.ToTaggedString();
        Find.WindowStack.Add(new Dialog_NodeTreeWithFactionInfo(diaNode, extraSiteGuest.GuestFaction));
    }

    public override void PostDestroy()
    {
        Deactive();
        base.PostDestroy();
    }
}