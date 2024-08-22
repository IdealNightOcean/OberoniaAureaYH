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
        SetVars(QuestGen.slate);
    }

    protected override bool TestRunInt(Slate slate)
    {
        return SetVars(slate);
    }

    protected bool SetVars(Slate slate)
    {
        Faction OAFaction = OberoniaAureaYHUtility.OAFaction;
        if (ValidFaction(OAFaction, slate))
        {
            slate.Set(storeAs.GetValue(slate), OberoniaAureaYHUtility.OAFaction);
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
