using System;
using System.Collections.Generic;
using Verse;

namespace OberoniaAurea;

public class MultiPartyTalksTopicEventDef : Def
{
    [Flags]
    public enum EventType
    {
        None = 0,

        Normal = 1,
        Serious = 2,
        EasterEgg = 4,

        NormalAndSerious = Normal | Serious,
        All = Normal | Serious | EasterEgg,
    }

    public Type workerClass = typeof(MultiPartyTalksTopicEventWorker);

    protected MultiPartyTalksTopicEventWorker worker;
    public MultiPartyTalksTopicEventWorker Worker => worker ??= MultiPartyTalksTopicEventWorker.GenerateEventWorker(this);

    public EventType eventType = EventType.NormalAndSerious;

    public bool allowSucceedTopic = true;
    public bool allowFailedTopic = true;

    public List<MultiPartyTalksTopicEventPart> parts;
}
