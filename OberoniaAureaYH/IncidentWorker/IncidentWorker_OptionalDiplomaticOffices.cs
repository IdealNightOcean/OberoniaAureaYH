using OberoniaAurea_Frame;
using OberoniaAurea_Frame.Utility;
using RimWorld;
using Verse;

namespace OberoniaAurea;

//外交斡旋确认
public class IncidentWorker_OptionalDiplomaticOffices : OberoniaAurea_Frame.IncidentWorker_GiveQuest
{
    public string letterText = "OA_OptionalDiplomaticOffices".Translate(ModUtility.OAFaction.NameColored, ModUtility.OAFaction.leader);

    protected override bool TryExecuteWorker(IncidentParms parms)
    {
        ChoiceLetter_OptionalQuest coiceLetter_OptionalQuest = (ChoiceLetter_OptionalQuest)LetterMaker.MakeLetter(label: def.letterLabel,
                                                                                                                  text: letterText,
                                                                                                                  def: def.letterDef,
                                                                                                                  relatedFaction: ModUtility.OAFaction);
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
        Faction oaFaction = ModUtility.OAFaction;
        if (oaFaction is null)
        {
            return false;
        }
        if (oaFaction.PlayerRelationKind != FactionRelationKind.Ally)
        {
            return false;
        }


        Faction otherFaction = OAFrame_FactionUtility.RandomAvailableFactionOf(validationParams: FactionValidationParams.HostileNormalFaction,
                                                                               predicater: f => f != oaFaction && f.RelationKindWith(oaFaction) != FactionRelationKind.Hostile);

        return otherFaction is not null;
    }
}