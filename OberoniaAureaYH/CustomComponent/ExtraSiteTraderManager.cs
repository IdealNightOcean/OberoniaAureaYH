using OberoniaAurea_Frame;
using RimWorld.Planet;
using System.Collections.Generic;
using Verse;

namespace OberoniaAurea;

public class ExtraSiteTraderManager : IExposable
{
    public static ExtraSiteTraderManager Instance { get; private set; }

    private List<ExtraSiteTrader> extraSiteTraders = [];
    public IReadOnlyList<ExtraSiteTrader> ExtraSiteTraders => extraSiteTraders;

    private Dictionary<WorldObject, ExtraSiteTrader> extraSiteTradersDict = [];

    public ExtraSiteTraderManager()
    {
        OAFrame_MiscUtility.ValidateSingleton(Instance, nameof(Instance));
        Instance = this;
    }
    public static void ClearStaticCache() => Instance = null;

    public void ExposeData()
    {
        Scribe_Collections.Look(ref extraSiteTraders, nameof(extraSiteTraders), LookMode.Deep);
        if (Scribe.mode == LoadSaveMode.PostLoadInit)
        {
            extraSiteTraders.RemoveAll(t => t.Parent is null || t.SiteTrader is null || t.DaysToExpired <= 0);

            extraSiteTradersDict = new(extraSiteTraders.Count);
            foreach (ExtraSiteTrader extraSiteTrader in extraSiteTraders)
            {
                extraSiteTradersDict[extraSiteTrader.Parent] = extraSiteTrader;
            }
        }
    }

    public void ExtraSiteTradersTickDay()
    {
        for (int i = extraSiteTraders.Count - 1; i >= 0; i--)
        {
            ExtraSiteTrader extraSiteTrader = extraSiteTraders[i];
            if (extraSiteTrader is null || extraSiteTrader.SiteTrader is null || (--extraSiteTrader.DaysToExpired) <= 0)
            {
                DeregisterExtraSiteTraderDirect(extraSiteTrader);
            }
        }
    }

    public bool HasExtraSiteTrader(WorldObject worldObject) => extraSiteTradersDict.ContainsKey(worldObject);

    public bool TryGetExtraSiteTrader(WorldObject worldObject, out ExtraSiteTrader extraSiteTrader) => extraSiteTradersDict.TryGetValue(worldObject, out extraSiteTrader);

    public bool RegisterExtraSiteTrader(ExtraSiteTrader extraSiteTrader, bool replaceCur = false)
    {
        if (extraSiteTradersDict.TryGetValue(extraSiteTrader.Parent, out ExtraSiteTrader existingTrader))
        {
            if (!replaceCur)
                return false;

            DeregisterExtraSiteTraderDirect(existingTrader);
        }

        extraSiteTradersDict[extraSiteTrader.Parent] = extraSiteTrader;
        return true;
    }

    public bool DeregisterExtraSiteTrader(WorldObject worldObject)
    {
        if (!extraSiteTradersDict.TryGetValue(worldObject, out ExtraSiteTrader extraSiteTrader))
            return false;

        DeregisterExtraSiteTraderDirect(extraSiteTrader);
        return true;
    }

    private void DeregisterExtraSiteTraderDirect(ExtraSiteTrader extraSiteTrader)
    {
        extraSiteTraders.Remove(extraSiteTrader);
        extraSiteTradersDict.Remove(extraSiteTrader.Parent);
        extraSiteTrader?.Clear();
    }
}