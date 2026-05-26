using OberoniaAurea_Frame;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;

namespace OberoniaAurea;

public class ExtraSiteGuest : IExposable
{
    public const int DefaultExpiredDays = 20;

    private WorldObject parent;
    public WorldObject Parent => parent;

    private Faction guestFaction;
    public Faction GuestFaction => guestFaction;

    private TraderKindDef traderKind;
    public TraderKindDef TraderKind => traderKind;

    private Faction nextGuestFaction;
    public Faction NextGuestFaction => nextGuestFaction;

    private TraderKindDef nextTraderKind;
    public TraderKindDef NextTraderKind => nextTraderKind;

    private int daysToExpired;
    public int DaysToExpired
    {
        get => daysToExpired;
        set => daysToExpired = value;
    }

    private bool firstInteracted = true;
    public bool FirstInteracted
    {
        get => firstInteracted;
        set => firstInteracted = value;
    }

    public bool IsActive => daysToExpired > 0 && guestFaction is not null;
    public bool IsTraderActive => daysToExpired > 0 && guestFaction is not null && traderKind is not null;

    private SiteTrader siteTrader;
    public SiteTrader SiteTrader => IsTraderActive ? (siteTrader ??= new SiteTrader(traderKind, parent, guestFaction)) : null;

    public ExtraSiteGuest() { }
    public ExtraSiteGuest(WorldObject parent)
    {
        this.parent = parent;
    }

    public void ExposeData()
    {
        Scribe_References.Look(ref parent, nameof(parent));

        Scribe_References.Look(ref guestFaction, nameof(guestFaction));
        Scribe_Defs.Look(ref traderKind, nameof(traderKind));

        Scribe_References.Look(ref nextGuestFaction, nameof(nextGuestFaction));
        Scribe_Defs.Look(ref nextTraderKind, nameof(nextTraderKind));

        Scribe_Values.Look(ref daysToExpired, nameof(daysToExpired), DefaultExpiredDays);

        Scribe_Values.Look(ref firstInteracted, nameof(firstInteracted), defaultValue: true);

        Scribe_Deep.Look(ref siteTrader, nameof(siteTrader));
    }

    public void ClearMeta()
    {
        siteTrader?.Destroy();
        siteTrader = null;
        guestFaction = null;
        traderKind = null;
        firstInteracted = true;
        daysToExpired = -1;
    }

    public void SetNextTraderMeta(Faction faction, TraderKindDef traderKind, int daysToSwitch = -1)
    {
        nextGuestFaction = faction;
        nextTraderKind = traderKind;
        if (daysToSwitch > 0)
        {
            daysToExpired = Mathf.Min(daysToExpired, daysToSwitch);
        }
    }

    public void RefreshTraderMeta()
    {
        ClearMeta();

        if (nextGuestFaction is not null)
        {
            guestFaction = nextGuestFaction;
            nextGuestFaction = null;
        }
        else
        {
            guestFaction = OAFrame_FactionUtility.RandomAvailableFactionOf(FactionValidationParams.DefaultFaction, f => !f.def.caravanTraderKinds.NullOrEmpty());
        }

        if (nextTraderKind is not null)
        {
            traderKind = nextTraderKind;
            nextTraderKind = null;
        }
        else
        {
            guestFaction.def.caravanTraderKinds.TryRandomElementByWeight(t => TraderKindCommonality(t, guestFaction), out traderKind);
            traderKind ??= guestFaction.def.caravanTraderKinds.RandomElement();
        }

        daysToExpired = OberoniaAureaSettings.ExtraSiteTraderExpiredDays;
    }

    public void RefreshTrader() => siteTrader?.RefreshThings();

    private static float TraderKindCommonality(TraderKindDef traderKind, Faction faction)
    {
        if (traderKind.faction is not null && faction.def != traderKind.faction)
            return 0f;

        if (ModsConfig.IdeologyActive && faction.ideos is not null && traderKind.category == "Slaver")
        {
            foreach (Ideo allIdeo in faction.ideos.AllIdeos)
            {
                if (!allIdeo.IdeoApprovesOfSlavery())
                    return 0f;
            }
        }

        return traderKind.CalculatedCommonality;
    }
}