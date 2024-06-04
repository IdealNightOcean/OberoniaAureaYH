using RimWorld;
using Verse;

namespace OberoniaAurea;
public class IncidentWorker_OptionalDiplomaticOffices : IncidentWorker_GiveQuest
{
    public string letterText = "OA_OptionalDiplomaticOffices".Translate(OberoniaAureaYHUtility.OAFaction.NameColored, OberoniaAureaYHUtility.OAFaction.leader);

    protected override bool TryExecuteWorker(IncidentParms parms)
    {
        ChoiceLetter_OptionalQuest coiceLetter_OptionalQuest = (ChoiceLetter_OptionalQuest)LetterMaker.MakeLetter(def.letterLabel, letterText, def.letterDef, OberoniaAureaYHUtility.OAFaction);
        coiceLetter_OptionalQuest.questScriptDef = def.questScriptDef;
        coiceLetter_OptionalQuest.StartTimeout(30000);
        Find.LetterStack.ReceiveLetter(coiceLetter_OptionalQuest);
        return true;
    }
    protected override bool CanFireNowSub(IncidentParms parms)
    {
        if (!base.CanFireNowSub(parms))
        {
            return false;
        }
        Faction OAFaction = OberoniaAureaYHUtility.OAFaction;
        if (OAFaction == null)
        {
            return false;
        }
        if (OAFaction.PlayerRelationKind != FactionRelationKind.Ally)
        {
            return false;
        }
        foreach (Faction f in Find.FactionManager.AllFactionsVisible)
        {
            if (f != OAFaction && f.PlayerRelationKind == FactionRelationKind.Hostile && f.RelationKindWith(OAFaction) != FactionRelationKind.Hostile)
            {
                return true;
            }
        }
        return false;
    }
}