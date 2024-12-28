using RimWorld;
using Verse;

namespace OberoniaAurea;

//外交斡旋确认
public class IncidentWorker_OptionalDiplomaticOffices : IncidentWorker_GiveQuest
{
    public string letterText = "OA_OptionalDiplomaticOffices".Translate(OARatkin_MiscUtility.OAFaction.NameColored, OARatkin_MiscUtility.OAFaction.leader);

    protected override bool TryExecuteWorker(IncidentParms parms)
    {
        ChoiceLetter_OptionalQuest coiceLetter_OptionalQuest = (ChoiceLetter_OptionalQuest)LetterMaker.MakeLetter(def.letterLabel, letterText, def.letterDef, OARatkin_MiscUtility.OAFaction);
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
        Faction oaFaction = OARatkin_MiscUtility.OAFaction;
        if (oaFaction == null)
        {
            return false;
        }
        if (oaFaction.PlayerRelationKind != FactionRelationKind.Ally)
        {
            return false;
        }
        foreach (Faction f in Find.FactionManager.AllFactionsVisible)
        {
            if (f != oaFaction && f.PlayerRelationKind == FactionRelationKind.Hostile && f.RelationKindWith(oaFaction) != FactionRelationKind.Hostile)
            {
                return true;
            }
        }
        return false;
    }
}