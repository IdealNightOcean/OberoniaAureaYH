using OberoniaAurea_Frame;
using RimWorld;

namespace OberoniaAurea;

internal class MultiPartyTalksTopicEventWorker_RatkinOriginDiscussion : MultiPartyTalksTopicEventWorker_Direct
{
    public override bool CanTriggerEvent(MultiPartyTalksTopicEventData eventData)
    {
        int ratkinFactions = 0;
        foreach (Faction f in eventData.TopicFactions)
        {
            if (f.IsRatkinFaction())
            {
                ratkinFactions++;
                if (ratkinFactions >= 2)
                    return true;
            }
        }
        return false;
    }
}