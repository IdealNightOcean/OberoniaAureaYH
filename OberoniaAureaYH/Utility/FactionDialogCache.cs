using OberoniaAurea_Frame;
using RimWorld;
using Verse;

namespace OberoniaAurea;

public class FactionDialogCache
{
    public Pawn Negotiator { get; }
    public Faction Faction { get; }
    public Map Map { get; }

    public FactionRelationKind PlayerRelationKind => Faction.PlayerRelationKind;
    public float TotalAllianceDuration { get; } = -1f;

    public int AmountSendableSilver { get; set; } = -1;

    public FactionDialogCache(Pawn negotiator, Faction faction)
    {
        Negotiator = negotiator;
        Faction = faction;
        Map = negotiator.MapHeld;

        AmountSendableSilver = OAFrame_MapUtility.AmountSendableSilver(this.Map);
        TotalAllianceDuration = OAInteractHandler.Instance.AllianceDuration();
    }
}