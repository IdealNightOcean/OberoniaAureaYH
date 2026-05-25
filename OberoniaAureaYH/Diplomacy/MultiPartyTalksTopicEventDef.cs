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

        Succeeded = 8,
        Failed = 16,

        AllCommon = Normal | Serious | Succeeded | Failed,
    }

    public Type workerClass = typeof(MultiPartyTalksTopicEventWorker);

    protected MultiPartyTalksTopicEventWorker worker;
    public MultiPartyTalksTopicEventWorker Worker => worker ??= MultiPartyTalksTopicEventWorker.GenerateEventWorker(this);

    [MustTranslate]
    public string confirmText = string.Empty;

    public EventType eventType = EventType.AllCommon;

    public List<MultiPartyTalksTopicEventPart> parts;
}
