using RimWorld;
using RimWorld.QuestGen;
using Verse;

namespace OberoniaAurea;

//QuestNode：获取金鼠鼠派系
public class QuestNode_GetOberoniaAureaFaction : QuestNode
{
    [NoTranslate]
    public SlateRef<string> storeAs;

    public SlateRef<bool> allowAlly = true;
    public SlateRef<bool> allowNeutral = true;
    public SlateRef<bool> allowHostile;

    protected override void RunInt()
    {
        Slate slate = QuestGen.slate;
        Faction oaFaction = ModUtility.OAFaction;
        if (ValidFaction(oaFaction, slate))
        {
            slate.Set(storeAs.GetValue(slate), oaFaction);
            if (!oaFaction.Hidden)
            {
                QuestPart_InvolvedFactions questPart_InvolvedFactions = new();
                questPart_InvolvedFactions.factions.Add(oaFaction);
                QuestGen.quest.AddPart(questPart_InvolvedFactions);
            }
        }
    }

    protected override bool TestRunInt(Slate slate)
    {
        Faction oaFaction = ModUtility.OAFaction;
        if (ValidFaction(oaFaction, slate))
        {
            slate.Set(storeAs.GetValue(slate), oaFaction);
            return true;
        }
        else
        {
            return false;
        }
    }
    protected bool ValidFaction(Faction faction, Slate slate)
    {
        if (faction is null)
        {
            return false;
        }
        FactionRelationKind playerRelationKind = faction.PlayerRelationKind;
        return playerRelationKind switch
        {
            FactionRelationKind.Ally => allowAlly.GetValue(slate),
            FactionRelationKind.Neutral => allowNeutral.GetValue(slate),
            FactionRelationKind.Hostile => allowHostile.GetValue(slate),
            _ => false,
        };
    }
}
