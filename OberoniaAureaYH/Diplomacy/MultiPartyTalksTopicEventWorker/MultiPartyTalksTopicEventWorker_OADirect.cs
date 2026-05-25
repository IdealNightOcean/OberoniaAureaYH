namespace OberoniaAurea;

public class MultiPartyTalksTopicEventWorker_OADirect : MultiPartyTalksTopicEventWorker_Direct
{
    public override bool CanTriggerEvent(MultiPartyTalksTopicEventData eventData)
    {
        return eventData.TopicFactions.Contains(ModUtility.OAFaction);
    }
}
