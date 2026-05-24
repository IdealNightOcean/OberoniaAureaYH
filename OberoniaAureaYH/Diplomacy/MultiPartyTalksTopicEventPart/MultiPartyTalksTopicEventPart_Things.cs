using OberoniaAurea_Frame;
using RimWorld;
using System.Collections.Generic;
using System.Text;
using Verse;

namespace OberoniaAurea;

using static MultiPartyTalksTopicEventWorker;

public class MultiPartyTalksTopicEventPart_Things : MultiPartyTalksTopicEventPart
{
    public List<ThingDefCountClass> thingDefCountClasses;

    public override void ApplyEventOutcome(EventData eventData, StringBuilder resultBuilder)
    {
        if (thingDefCountClasses.NullOrEmpty() || eventData.TalksSite?.AssociatedFixedCaravan is null)
            return;

        List<Thing> things = new(thingDefCountClasses.Count);
        foreach (ThingDefCountClass thingDefCount in thingDefCountClasses)
        {
            Thing t = ThingMaker.MakeThing(thingDefCount.thingDef);
            t.stackCount = thingDefCount.count;
            things.Add(t);
        }
        OAFrame_FixedCaravanUtility.GiveThings(eventData.TalksSite.AssociatedFixedCaravan, things);
        resultBuilder.AppendLine("OARK_MultiPartyTalksTopicEventPart_Things".Translate(GenLabel.ThingsLabel(things).Named("ThingsLabel")));
    }
}