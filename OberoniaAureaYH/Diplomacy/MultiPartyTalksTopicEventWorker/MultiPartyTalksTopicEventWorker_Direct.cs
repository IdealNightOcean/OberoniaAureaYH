using OberoniaAurea_Frame;
using System.Text;
using Verse;

namespace OberoniaAurea;

public class MultiPartyTalksTopicEventWorker_Direct : MultiPartyTalksTopicEventWorker
{
    public override bool TryTriggerEvent(MultiPartyTalksTopicEventData eventData)
    {
        StringBuilder rootTextBuilder = new(128);
        rootTextBuilder.AppendLine(ResolveEventRootNodeText(eventData));

        rootTextBuilder.AppendLine();
        rootTextBuilder.AppendLine();
        ApplyEventOutcome(eventData, rootTextBuilder);

        DiaNode rootNode = new(rootTextBuilder.ToTaggedString());

        rootNode.AddDefaultConfirmOption();

        Find.WindowStack.Add(new Dialog_NodeTree(rootNode));
        return true;
    }

    protected virtual void ApplyEventOutcome(MultiPartyTalksTopicEventData eventData, StringBuilder resultBuilder)
    {
        if (!Def.parts.NullOrEmpty())
        {
            resultBuilder.AppendLine();
            foreach (MultiPartyTalksTopicEventPart part in Def.parts)
            {
                part.ApplyEventOutcome(eventData, resultBuilder);
            }
        }

        Find.WindowStack.Add(OAFrame_DiaUtility.ConfirmDiaNodeTree(resultBuilder.ToTaggedString()));
    }
}