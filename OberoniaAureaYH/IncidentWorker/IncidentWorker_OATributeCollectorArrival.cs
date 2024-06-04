using RimWorld;
using System.Collections.Generic;
using Verse;

namespace OberoniaAurea;

//金鸢尾兰征募队到达事件
public class IncidentWorker_OACaravanArrivalTributeCollector : IncidentWorker_TraderCaravanArrival
{

    protected override bool TryResolveParmsGeneral(IncidentParms parms)
    {
        if (!base.TryResolveParmsGeneral(parms))
        {
            return false;
        }
        Faction OAFaction = OberoniaAureaYHUtility.OAFaction;
        if (OAFaction == null)
        {
            return false;
        }
        parms.faction = OAFaction;
        parms.traderKind = OberoniaAureaYHDefOf.OA_RK_Caravan_TributeCollector;
        return true;
    }

    protected override bool CanFireNowSub(IncidentParms parms)
    {

        if (!base.CanFireNowSub(parms))
        {
            return false;
        }
        Faction OAFaction = OberoniaAureaYHUtility.OAFaction;
        if (OAFaction == null || OAFaction.PlayerRelationKind == FactionRelationKind.Hostile)
        {
            return false;
        }
        return FactionCanBeGroupSource(OAFaction, (Map)parms.target);
    }

    protected override float TraderKindCommonality(TraderKindDef traderKind, Map map, Faction faction)
    {
        return traderKind.CalculatedCommonality;
    }

    protected override void SendLetter(IncidentParms parms, List<Pawn> pawns, TraderKindDef traderKind)
    {
        TaggedString letterLabel = "LetterLabelTributeCollectorArrival_OA".Translate().CapitalizeFirst();
        TaggedString letterText = "LetterTributeCollectorArrival_OA".Translate(parms.faction.NameColored).CapitalizeFirst();
        letterText += "\n\n" + "LetterCaravanArrivalCommonWarning".Translate();
        PawnRelationUtility.Notify_PawnsSeenByPlayer_Letter(pawns, ref letterLabel, ref letterText, "LetterRelatedPawnsNeutralGroup".Translate(Faction.OfPlayer.def.pawnsPlural), informEvenIfSeenBefore: true);
        SendStandardLetter(letterLabel, letterText, LetterDefOf.PositiveEvent, parms, pawns[0]);
    }
}
