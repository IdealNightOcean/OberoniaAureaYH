using RimWorld;
using Verse;

namespace OberoniaAurea;

public class FactionDialogCache
{
    public readonly bool IsAlly;
    public readonly float AllianceDuration = -1f;
    public readonly Pawn Negotiator;
    public readonly Faction Faction;
    public readonly Map Map;

    public FactionDialogCache(Pawn negotiator, Faction faction)
    {
        IsAlly = ModUtility.OAFaction?.PlayerRelationKind == FactionRelationKind.Ally;
        AllianceDuration = OAInteractHandler.Instance.AllianceDuration();
        Negotiator = negotiator;
        Faction = faction;
        Map = negotiator.MapHeld;
    }
}