using RimWorld;
using RimWorld.Planet;
using System.Collections.Generic;
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
        if (extraSiteGuest is null || extraSiteGuest.IsActive)
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
            defaultLabel = "OARK_CommandExtraSiteGuest".Translate(extraSiteGuest.GuestFaction.Named(KeyLibrary_NamedArgument.FACTION)),
            defaultDesc = "CommandTradeDesc".Translate(),
            icon = TradeCommandTex.Texture,
            action = () => InteractWithGuest(caravan)
        };
        yield return command_ExtraSiteTrader;
    }


    public void InteractWithGuest(Caravan caravan)
    {

    }

    public override void PostDestroy()
    {
        Deactive();
        base.PostDestroy();
    }
}