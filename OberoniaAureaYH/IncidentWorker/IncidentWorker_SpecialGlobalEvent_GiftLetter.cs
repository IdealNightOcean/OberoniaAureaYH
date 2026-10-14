using OberoniaAurea_Frame.Utility;
using RimWorld;
using System;
using Verse;

namespace OberoniaAurea;

internal class IncidentWorker_SpecialGlobalEvent_GiftLetter : IncidentWorker
{
    protected override bool CanFireNowSub(IncidentParms parms)
    {
        if (!String.IsNullOrEmpty(parms.questTag) && SpecialGlobalEventManager.Instance.HasTriggeredSpecialEvents(parms.questTag))
            return false;

        return base.CanFireNowSub(parms);
    }

    protected static bool ResolveParams(IncidentParms parms)
    {
        if (!String.IsNullOrEmpty(parms.questTag) && SpecialGlobalEventManager.Instance.HasTriggeredSpecialEvents(parms.questTag))
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

        if (!String.IsNullOrEmpty(parms.customLetterLabel))
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

        if (!String.IsNullOrEmpty(parms.customLetterLabel))
        {
            Find.LetterStack.ReceiveLetter(
                label: parms.customLetterLabel,
                text: parms.customLetterText,
                textLetterDef: parms.customLetterDef,
                lookTargets: parms.gifts,
                relatedFaction: parms.faction);
        }

        if (!String.IsNullOrEmpty(parms.questTag))
            SpecialGlobalEventManager.Instance.MarkSpecialEventTriggered(parms.questTag);

        return true;
    }
}