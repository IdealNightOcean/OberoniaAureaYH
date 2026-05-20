using OberoniaAurea_Frame;
using RimWorld;
using RimWorld.Planet;
using Verse;

namespace OberoniaAurea;

public class ExtraSiteTrader : IExposable
{
    private WorldObject parent;
    public WorldObject Parent => parent;

    private Faction traderFaction;
    public Faction TraderFaction => traderFaction;

    private SiteTrader siteTrader;
    public SiteTrader SiteTrader => siteTrader;

    private int daysToExpired;
    public int DaysToExpired
    {
        get => daysToExpired;
        set => daysToExpired = value;
    }

    public ExtraSiteTrader() { }
    public ExtraSiteTrader(WorldObject parent)
    {
        this.parent = parent;
    }

    public static ExtraSiteTrader CreateExtraSiteTrader(WorldObject parent, Faction traderFaction = null, TraderKindDef traderKind = null, int daysToExpired = -2)
    {
        ExtraSiteTrader extraSiteTrader = new(parent);
        traderFaction ??= OAFrame_FactionUtility.RandomAvailableFactionOf(FactionValidationParams.DefaultFaction, f => !f.def.caravanTraderKinds.NullOrEmpty());
        if (traderKind is null)
        {
            traderFaction.def.caravanTraderKinds.TryRandomElementByWeight(t => TraderKindCommonality(t, traderFaction), out traderKind);
            traderKind ??= traderFaction.def.caravanTraderKinds.RandomElement();
        }

        extraSiteTrader.InitTrader(traderFaction, traderKind);
        return extraSiteTrader;
    }

    public void ExposeData()
    {
        Scribe_References.Look(ref parent, nameof(parent));
        Scribe_References.Look(ref traderFaction, nameof(traderFaction));
        Scribe_Values.Look(ref daysToExpired, nameof(daysToExpired));
        Scribe_Deep.Look(ref siteTrader, nameof(siteTrader));
    }

    public void InitTrader(Faction faction, TraderKindDef traderKind, int daysToExpired = -2)
    {
        Clear();

        traderFaction = faction;
        siteTrader = new SiteTrader(traderKind, parent, traderFaction);
        DaysToExpired = daysToExpired;
    }

    public void Clear()
    {
        traderFaction = null;
        siteTrader?.Destory();
        siteTrader = null;
        DaysToExpired = -1;
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