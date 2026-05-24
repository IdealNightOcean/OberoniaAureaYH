using System;
using System.Collections.Generic;
using Verse;

namespace OberoniaAurea;

public class MultiPartyTalksTopicEventDef : Def
{
    [Flags]
    public enum EventType
    {
        Normal = 1,
        Serious = 2,

        Succeeded = 4,
        Failed = 8,


        All = Normal | Serious | Succeeded | Failed
    }

    public Type workerClass = typeof(MultiPartyTalksTopicEventWorker);

    protected MultiPartyTalksTopicEventWorker worker;
    public MultiPartyTalksTopicEventWorker Worker => worker ??= MultiPartyTalksTopicEventWorker.GenerateEventWorker(this);

    [MustTranslate]
    public string confirmText = string.Empty;

    public EventType eventType = EventType.All;

    public List<MultiPartyTalksTopicEventPart> parts;
}
