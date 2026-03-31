using OberoniaAurea_Frame;
using RimWorld;
using Verse;

namespace OberoniaAurea;

internal class IncidentWorker_SpecialGlobalEvent_GiftLetter : IncidentWorker
{
    protected override bool CanFireNowSub(IncidentParms parms)
    {
        if (!string.IsNullOrEmpty(parms.questTag) && SpecialGlobalEventManager.Instance.HasTriggeredSpecialEvents(parms.questTag))
            return false;

        return base.CanFireNowSub(parms);
    }

    protected bool ResolveParams(IncidentParms parms)
    {
        if (!string.IsNullOrEmpty(parms.questTag) && SpecialGlobalEventManager.Instance.HasTriggeredSpecialEvents(parms.questTag))
            return false;

        Map map = (Map)parms.target ?? Find.AnyPlayerHomeMap;
        if (map is null)
            return false;

        parms.target = map;

        if (parms.gifts is null && !parms.spawnCenter.IsValid)
        {
            IntVec3 intVec = DropCellFinder.TradeDropSpot(map);
            if (!intVec.IsValid)
                return false;

            parms.spawnCenter = intVec;
        }

        if (!string.IsNullOrEmpty(parms.customLetterLabel))
        {
            parms.customLetterDef ??= LetterDefOf.NeutralEvent;
        }

        return true;
    }

    protected override bool TryExecuteWorker(IncidentParms parms)
    {
        if (!ResolveParams(parms))
            return false;

        Map map = (Map)parms.target;
        if (!parms.gifts.NullOrEmpty())
        {
            OAFrame_DropPodUtility.DefaultDropThing(parms.gifts, map, parms.faction, sendLetter: false);
        }

        if (!string.IsNullOrEmpty(parms.customLetterLabel))
        {
            Find.LetterStack.ReceiveLetter(
                label: parms.customLetterLabel,
                text: parms.customLetterText,
                textLetterDef: parms.customLetterDef,
                lookTargets: parms.gifts,
                relatedFaction: parms.faction);
        }

        if (!string.IsNullOrEmpty(parms.questTag))
            SpecialGlobalEventManager.Instance.MarkSpecialEventTriggered(parms.questTag);

        return true;
    }
}