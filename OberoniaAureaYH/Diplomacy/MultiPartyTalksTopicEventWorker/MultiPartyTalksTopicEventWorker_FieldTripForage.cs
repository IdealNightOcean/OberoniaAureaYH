using Verse;

namespace OberoniaAurea;

public class MultiPartyTalksTopicEventWorker_FieldTripForage : MultiPartyTalksTopicEventWorker_Direct
{
    public override bool CanTriggerEvent(MultiPartyTalksTopicEventData eventData)
    {
        if (eventData.TalksSite is null) return false;

        return Find.WorldGrid[eventData.TalksSite.Tile].PrimaryBiome?.foragedFood is not null;
    }
}