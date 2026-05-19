using OberoniaAurea_Frame;
using RimWorld;
using RimWorld.Planet;
using System.Collections.Generic;
using Verse;

namespace OberoniaAurea;

//大地图事件点：外交斡旋
[StaticConstructorOnStartup]
public class WorldObject_DiplomaticOffices : WorldObject_InteractiveBase
{
    private Faction ParticipantFaction => participantFactions.FirstOrFallback(null);

    /// <summary>
    /// 大成功所需要的谈判能力
    /// </summary>
    private const float TriumphNeedAbility = 2.2f;

    public override void Notify_CaravanArrived(Caravan caravan)
    {
        if (Faction is null || ParticipantFaction is null)
        {
            QuestUtility.SendQuestTargetSignals(questTags, "Resolved", this.Named(KeyLibrary_NamedArgument.SUBJECT));
            Destroy();
            return;
        }

        Pawn pawn = BestCaravanPawnUtility.FindBestDiplomat(caravan);
        if (pawn is null)
        {
            Messages.Message("OAFrame_MessageNoDiplomat".Translate(), caravan, MessageTypeDefOf.NegativeEvent, historical: false);
            return;
        }

        float negotiationAbility = pawn.GetStatValue(StatDefOf.NegotiationAbility);
        if (negotiationAbility < TriumphNeedAbility)
        {
            Outcome_Success(caravan);
        }
        else
        {
            Outcome_Triumph(caravan);
        }
        pawn.skills?.Learn(SkillDefOf.Social, 6000f, direct: true);
        QuestUtility.SendQuestTargetSignals(questTags, "Resolved", this.Named(KeyLibrary_NamedArgument.SUBJECT));
        Destroy();
    }

    /// <summary>
    /// 结果 - 谈判成功
    /// </summary>
    private void Outcome_Success(Caravan caravan)
    {
        FactionRelationKind playerRelationKind = ParticipantFaction.PlayerRelationKind;
        Faction.OfPlayer.TryAffectGoodwillWith(ParticipantFaction, 60, canSendMessage: false, canSendHostilityLetter: false, HistoryEventDefOf.PeaceTalksSuccess);
        Find.LetterStack.ReceiveLetter("OA_LetterLabelDiplomaticOffices_Success".Translate(), GetLetterText("OA_LetterDiplomaticOffices_Success".Translate(Faction.NameColored, ParticipantFaction.NameColored, 60), caravan, playerRelationKind), LetterDefOf.PositiveEvent, caravan, Faction);
    }

    /// <summary>
    /// 结果 - 谈判大成功
    /// </summary>
    private void Outcome_Triumph(Caravan caravan)
    {
        FactionRelationKind playerRelationKind = ParticipantFaction.PlayerRelationKind;
        Faction.OfPlayer.TryAffectGoodwillWith(ParticipantFaction, 100, canSendMessage: false, canSendHostilityLetter: false, HistoryEventDefOf.PeaceTalksTriumph);
        ThingSetMakerParams parms = default;
        parms.makingFaction = ParticipantFaction;
        parms.techLevel = ParticipantFaction.def.techLevel;
        parms.maxTotalMass = 20f;
        parms.totalMarketValueRange = new FloatRange(500f, 1200f);
        parms.tile = Tile;
        List<Thing> list = ThingSetMakerDefOf.Reward_ItemsStandard.root.Generate(parms);
        for (int i = 0; i < list.Count; i++)
        {
            caravan.AddPawnOrItem(list[i], addCarriedPawnToWorldPawnsIfAny: true);
        }
        Find.LetterStack.ReceiveLetter("OA_LetterLabelDiplomaticOffices_Triumph".Translate(), GetLetterText("OA_LetterDiplomaticOffices_Triumph".Translate(Faction.NameColored, ParticipantFaction.NameColored, 100, GenLabel.ThingsLabel(list)), caravan, playerRelationKind), LetterDefOf.PositiveEvent, caravan, Faction);
    }

    private string GetLetterText(string baseText, Caravan caravan, FactionRelationKind previousRelationKind)
    {
        TaggedString text = baseText;
        Pawn pawn = BestCaravanPawnUtility.FindBestDiplomat(caravan);
        if (pawn is not null)
        {
            text += "\n\n" + "PeaceTalksSocialXPGain".Translate(pawn.LabelShort, 6000f.ToString("F0"), pawn.Named(KeyLibrary_NamedArgument.PAWN));
        }
        ParticipantFaction.TryAppendRelationKindChangedInfo(ref text, previousRelationKind, ParticipantFaction.PlayerRelationKind);
        return text;
    }
}