using OberoniaAurea_Frame;
using RimWorld;
using System.Collections.Generic;
using System.Text;
using Verse;

namespace OberoniaAurea;

public class MultiPartyTalksTopicEventPart_Things : MultiPartyTalksTopicEventPart
{
    public List<ThingMakeInfo> thingMakeInfoes;

    public override void ApplyEventOutcome(MultiPartyTalksTopicEventData eventData, StringBuilder resultBuilder)
    {
        if (thingMakeInfoes.NullOrEmpty() || eventData.TalksSite is null)
            return;

        List<Thing> things = new(thingMakeInfoes.Count);
        foreach (ThingMakeInfo makeInfo in thingMakeInfoes)
        {
            things.AddRange(OAFrame_ThingUtility.GenerateThingListSplitByStack(makeInfo));
        }

        MultiPartyTalksUtility.TryGiveThingsToPlayer(eventData, things);
        resultBuilder.AppendLine("OAFrame_GetThings".Translate(GenLabel.ThingsLabel(things).Named(KeyLibrary_NamedArgument.ThingsLabel)));
    }
}