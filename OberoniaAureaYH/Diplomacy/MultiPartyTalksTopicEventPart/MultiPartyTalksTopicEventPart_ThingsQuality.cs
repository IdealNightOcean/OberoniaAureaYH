using OberoniaAurea_Frame;
using RimWorld;
using System.Collections.Generic;
using System.Text;
using Verse;

namespace OberoniaAurea;

using static MultiPartyTalksTopicEventWorker;

public class MultiPartyTalksTopicEventPart_ThingsQuality : MultiPartyTalksTopicEventPart
{
    public List<ThingDefCountClass> thingDefCountClasses;
    public QualityCategory quality = QualityCategory.Normal;

    public override void ApplyEventOutcome(EventData eventData, StringBuilder resultBuilder)
    {
        if (thingDefCountClasses.NullOrEmpty() || eventData.TalksSite?.AssociatedFixedCaravan is null)
            return;

        List<Thing> things = new(thingDefCountClasses.Count);
        foreach (ThingDefCountClass thingDefCount in thingDefCountClasses)
        {
            Thing t = ThingMaker.MakeThing(thingDefCount.thingDef);
            t.stackCount = thingDefCount.count;
            CompQuality compQuality = t.TryGetComp<CompQuality>();
            if (compQuality is not null)
            {
                compQuality.SetQuality(quality, ArtGenerationContext.Outsider);
            }
            things.Add(t);
        }
        OAFrame_FixedCaravanUtility.GiveThings(eventData.TalksSite.AssociatedFixedCaravan, things);
        resultBuilder.AppendLine("OARK_MultiPartyTalksTopicEventPart_ThingsQuality".Translate(
            quality.Named("Quality"),
            GenLabel.ThingsLabel(things).Named("ThingsLabel")));
    }
}