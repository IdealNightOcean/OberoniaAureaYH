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
        Faction faction = OberoniaAureaYHUtility.OAFaction;
        if (ValidFaction(faction, slate))
        {
            slate.Set(storeAs.GetValue(slate), faction);
            if (!faction.Hidden)
            {
                QuestPart_InvolvedFactions questPart_InvolvedFactions = new();
                questPart_InvolvedFactions.factions.Add(faction);
                QuestGen.quest.AddPart(questPart_InvolvedFactions);
            }
        }
    }

    protected override bool TestRunInt(Slate slate)
    {
        Faction faction = OberoniaAureaYHUtility.OAFaction;
        if (ValidFaction(faction, slate))
        {
            slate.Set(storeAs.GetValue(slate), faction);
            return true;
        }
        else
        {
            return false;
        }
    }
    protected bool ValidFaction(Faction faction, Slate slate)
    {
        if (faction == null)
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
