using RimWorld;
using Verse;

namespace OberoniaAurea;

public class IncidentWorker_ScienceShip_TravelRKReward : IncidentWorker_DropThingsOnMap
{
    protected override bool ResolveFaction(IncidentParms parms)
    {
        if (OARK_ModDefOf.Rakinia_TravelRatkin is not null)
        {
            parms.faction = Find.FactionManager.FirstFactionOfDef(OARK_ModDefOf.Rakinia_TravelRatkin);
        }

        return parms.faction is not null;
    }

    protected override bool ResolveDropthings(IncidentParms parms)
    {
        float rewardValue = Rand.RangeInclusive(1500, 3500);
        Reward_Items reward = new();
        RewardsGeneratorParams rewardParms = new()
        {
            thingRewardItemsOnly = true,
            rewardValue = rewardValue,
            giverFaction = parms.faction,

        };
        reward.InitFromValue(rewardValue, rewardParms, out _);
        parms.gifts ??= [];
        parms.gifts.AddRange(reward.ItemsListForReading);
        return !parms.gifts.NullOrEmpty();
    }

    protected override void PostThingDroped(IncidentParms parms)
    {
        Find.LetterStack.ReceiveLetter(label: "OARK_ScienceShip_TravelRKRewardLabel".Translate(),
                                       text: "OARK_ScienceShip_TravelRKReward".Translate(),
                                       textLetterDef: LetterDefOf.PositiveEvent,
                                       lookTargets: new LookTargets(parms.spawnCenter, (Map)parms.target),
                                       relatedFaction: parms.faction);
    }
}
