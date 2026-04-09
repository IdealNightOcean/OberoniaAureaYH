using OberoniaAurea_Frame;
using RimWorld;
using RimWorld.QuestGen;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace OberoniaAurea;

public class IncidentWorker_BirthdayWishes : IncidentWorker
{
    protected override bool CanFireNowSub(IncidentParms parms)
    {
        if (!ModUtility.IsOAFactionAlly())
        {
            return false;
        }
        Map map = (Map)parms.target;
        map ??= Find.RandomPlayerHomeMap;
        if (map is null)
        {
            return false;
        }

        parms.target = map;
        return base.CanFireNowSub(parms);
    }

    private bool ResolveParms(IncidentParms parms, out List<Pawn> birthdayPawns)
    {
        birthdayPawns = null;
        if (!ModUtility.IsOAFactionAlly())
        {
            return false;
        }
        parms.faction = ModUtility.OAFaction;

        Map map = (Map)parms.target;
        map ??= Find.RandomPlayerHomeMap;
        if (map is null)
        {
            return false;
        }
        parms.target = map;

        birthdayPawns = map.mapPawns.FreeColonists.Where(p => p.IsColonistOnFirstBirthdayPerYear()).ToList();
        if (birthdayPawns.Count == 0)
        {
            return false;
        }

        return true;
    }

    protected override bool TryExecuteWorker(IncidentParms parms)
    {
        if (!ResolveParms(parms, out List<Pawn> birthdayPawns))
        {
            return false;
        }

        BirthdayWishes(parms, birthdayPawns);

        if (ModsConfig.OdysseyActive)
        {
            TryBirthdayVisit(parms, birthdayPawns);
        }

        return true;
    }

    private void BirthdayWishes(IncidentParms parms, List<Pawn> allBirthdayPawns)
    {
        List<Pawn> birthdayPawns = allBirthdayPawns.Where(p => p.IsOberoniaAureaPawn()).ToList();
        if (birthdayPawns.NullOrEmpty())
            return;

        Map map = (Map)parms.target;
        int birthdayPawnsCount = birthdayPawns.Count;
        IntVec3 intVec = DropCellFinder.TradeDropSpot(map);

        if (!intVec.IsValid)
        {
            return;
        }

        List<Thing> gifts = OAFrame_MiscUtility.TryGenerateThing(OARK_ThingDefOf.Oberonia_Aurea_Chanwu_AB, birthdayPawnsCount * 5);
        gifts.AddRange(OAFrame_MiscUtility.TryGenerateThing(OARK_ThingDefOf.OA_RK_New_Hat_A, birthdayPawnsCount));
        OAFrame_DropPodUtility.DefaultDropThing(gifts, map, parms.faction, sendLetter: false);

        if (OberoniaAureaSettings.BirthdayWishesShowLetter)
        {
            Find.LetterStack.ReceiveLetter(
                label: "OARK_LetterLabel_OABirthdayWishes".Translate(GenLabel.ThingsLabel(birthdayPawns).Named("PawnsInfo")),
                text: "OARK_LetterText_OABirthdayWishes".Translate(GenLabel.ThingsLabel(birthdayPawns).Named("PawnsInfo"), parms.faction.Named("Faction")),
                textLetterDef: LetterDefOf.PositiveEvent,
                lookTargets: parms.gifts,
                relatedFaction: parms.faction);
        }
        else
        {
            Messages.Message(
                text: "OARK_LetterLabel_OABirthdayWishes".Translate(GenLabel.ThingsLabel(birthdayPawns).Named("PawnsInfo")),
                lookTargets: birthdayPawns,
                def: MessageTypeDefOf.PositiveEvent);
        }
    }

    private void TryBirthdayVisit(IncidentParms parms, List<Pawn> birthdayPawns)
    {
        /*
        if (ScienceDepartmentInteractHandler.Instance.PlayerTechPoints < 5000)
        {
            return;
        }
        */
        if (ModUtility.CooldownManager.IsInCooldown("SDBirthdayVisit"))
        {
            return;
        }

        if (ScienceDepartmentInteractHandler.Instance.GravTechPointEMBlockDays > 0)
        {
            Find.LetterStack.ReceiveLetter(
                label: "OARK_LetterLabel_OABirthdayVisit".Translate(GenLabel.ThingsLabel(birthdayPawns).Named("PawnsInfo")),
                text: "OARK_LetterText_OABirthdayVisit".Translate(GenLabel.ThingsLabel(birthdayPawns).Named("PawnsInfo"), parms.faction.Named("Faction")),
                textLetterDef: LetterDefOf.PositiveEvent);

            ModUtility.CooldownManager.RegisterRecord("SDBirthdayVisit", cdTicks: 10 * 60000);
        }
        else
        {
            Map map = (Map)parms.target;
            Slate slate = new();
            slate.Set("map", map);
            slate.Set("faction", parms.faction);
            slate.Set("birthdayPawns", birthdayPawns);

            if (OAFrame_QuestUtility.TryGenerateQuestAndMakeAvailable(out _, OARK_QuestScriptDefOf.OARK_SDBirthdayVisit, slate))
            {
                Log.Message("Generated birthday visit quest.");
                ModUtility.CooldownManager.RegisterRecord("SDBirthdayVisit", cdTicks: 35 * 60000);
            }
            else
            {
                Log.Message("Failed to generate birthday visit quest.");
                ModUtility.CooldownManager.RegisterRecord("SDBirthdayVisit", cdTicks: 5 * 60000);
            }
        }
    }

}
