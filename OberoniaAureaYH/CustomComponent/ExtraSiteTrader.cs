using OberoniaAurea_Frame;
using RimWorld;
using RimWorld.Planet;
using Verse;

namespace OberoniaAurea;

public class ExtraSiteTrader : IExposable
{
    public const int DefaultExpiredDays = 20;

    private WorldObject parent;
    public WorldObject Parent => parent;

    private int expiredDays = DefaultExpiredDays;
    public int ExpiredDays => expiredDays;

    private Faction fixedTraderFaction;
    public Faction FixedTraderFaction => fixedTraderFaction;

    private TraderKindDef fixedTraderKind;
    public TraderKindDef FixedTraderKind => fixedTraderKind;


    private Faction traderFaction;
    public Faction TraderFaction => traderFaction;

    private TraderKindDef traderKind;
    public TraderKindDef TraderKind => traderKind;

    private int daysToExpired;
    public int DaysToExpired
    {
        get => daysToExpired;
        set => daysToExpired = value;
    }
    public bool IsActive => daysToExpired > 0 && traderFaction is not null && traderKind is not null;

    private SiteTrader siteTrader;
    public SiteTrader SiteTrader => IsActive ? (siteTrader ??= new SiteTrader(traderKind, parent, traderFaction)) : null;

    public ExtraSiteTrader() { }
    public ExtraSiteTrader(WorldObject parent, int expiredDays = DefaultExpiredDays, Faction fixedFaction = null, TraderKindDef fixedKind = null)
    {
        this.parent = parent;
        this.expiredDays = expiredDays;
        this.fixedTraderFaction = fixedFaction;
        this.fixedTraderKind = fixedKind;
    }

    public void ExposeData()
    {
        Scribe_References.Look(ref parent, nameof(parent));

        Scribe_References.Look(ref fixedTraderFaction, nameof(fixedTraderFaction));
        Scribe_Defs.Look(ref fixedTraderKind, nameof(fixedTraderKind));
        Scribe_References.Look(ref traderFaction, nameof(traderFaction));
        Scribe_Defs.Look(ref traderKind, nameof(traderKind));

        Scribe_Values.Look(ref expiredDays, nameof(expiredDays), DefaultExpiredDays);
        Scribe_Values.Look(ref daysToExpired, nameof(daysToExpired), DefaultExpiredDays);

        Scribe_Deep.Look(ref siteTrader, nameof(siteTrader));
    }

    public void ClearTraderMeta()
    {
        siteTrader?.Destroy();
        siteTrader = null;
        traderFaction = null;
        traderKind = null;
        daysToExpired = -1;
    }

    public void RefreshTraderMeta()
    {
        ClearTraderMeta();

        if (fixedTraderFaction is not null)
        {
            traderFaction = fixedTraderFaction;
        }
        else
        {
            traderFaction = OAFrame_FactionUtility.RandomAvailableFactionOf(FactionValidationParams.DefaultFaction, f => !f.def.caravanTraderKinds.NullOrEmpty());
        }

        if (fixedTraderKind is not null)
        {
            traderKind = fixedTraderKind;
        }
        else
        {
            traderFaction.def.caravanTraderKinds.TryRandomElementByWeight(t => TraderKindCommonality(t, traderFaction), out traderKind);
            traderKind ??= traderFaction.def.caravanTraderKinds.RandomElement();
        }

        daysToExpired = expiredDays;
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