using LudeonTK;
using OberoniaAurea_Frame;
using RimWorld;
using RimWorld.Planet;
using System.Collections.Generic;
using System.Text;
using Verse;

namespace OberoniaAurea;

//大地图事件点：外交斡旋
[StaticConstructorOnStartup]
public class WorldObject_DiplomaticOffices : WorldObject_WithMutiFactions
{
    private Faction ParticipantFaction => participantFactions.FirstOrFallback(null);

    private const float TriumphNeedAbility = 2.2f; //大成功所需要的谈判能力

    public override void Notify_CaravanArrived(Caravan caravan) //判定谈判结果
    {
        if (base.Faction is null || ParticipantFaction is null)
        {
            QuestUtility.SendQuestTargetSignals(questTags, "Resolved", this.Named("SUBJECT"));
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
        pawn.skills.Learn(SkillDefOf.Social, 6000f, direct: true);
        QuestUtility.SendQuestTargetSignals(questTags, "Resolved", this.Named("SUBJECT"));
        Destroy();
    }

    private void Outcome_Success(Caravan caravan) //成功谈判的结果
    {
        FactionRelationKind playerRelationKind = ParticipantFaction.PlayerRelationKind;
        Faction.OfPlayer.TryAffectGoodwillWith(ParticipantFaction, 60, canSendMessage: false, canSendHostilityLetter: false, HistoryEventDefOf.PeaceTalksSuccess);
        Find.LetterStack.ReceiveLetter("OA_LetterLabelDiplomaticOffices_Success".Translate(), GetLetterText("OA_LetterDiplomaticOffices_Success".Translate(base.Faction.NameColored, ParticipantFaction.NameColored, 60), caravan, playerRelationKind), LetterDefOf.PositiveEvent, caravan, base.Faction);
    }

    private void Outcome_Triumph(Caravan caravan) //大成功谈判的结果
    {
        FactionRelationKind playerRelationKind = ParticipantFaction.PlayerRelationKind;
        Faction.OfPlayer.TryAffectGoodwillWith(ParticipantFaction, 100, canSendMessage: false, canSendHostilityLetter: false, HistoryEventDefOf.PeaceTalksTriumph);
        ThingSetMakerParams parms = default;
        parms.makingFaction = ParticipantFaction;
        parms.techLevel = ParticipantFaction.def.techLevel;
        parms.maxTotalMass = 20f;
        parms.totalMarketValueRange = new FloatRange(500f, 1200f);
        parms.tile = base.Tile;
        List<Thing> list = ThingSetMakerDefOf.Reward_ItemsStandard.root.Generate(parms);
        for (int i = 0; i < list.Count; i++)
        {
            caravan.AddPawnOrItem(list[i], addCarriedPawnToWorldPawnsIfAny: true);
        }
        Find.LetterStack.ReceiveLetter("OA_LetterLabelDiplomaticOffices_Triumph".Translate(), GetLetterText("OA_LetterDiplomaticOffices_Triumph".Translate(base.Faction.NameColored, ParticipantFaction.NameColored, 100, GenLabel.ThingsLabel(list)), caravan, playerRelationKind), LetterDefOf.PositiveEvent, caravan, base.Faction);
    }

    private string GetLetterText(string baseText, Caravan caravan, FactionRelationKind previousRelationKind)
    {
        TaggedString text = baseText;
        Pawn pawn = BestCaravanPawnUtility.FindBestDiplomat(caravan);
        if (pawn is not null)
        {
            text += "\n\n" + "PeaceTalksSocialXPGain".Translate(pawn.LabelShort, 6000f.ToString("F0"), pawn.Named("PAWN"));
        }
        ParticipantFaction.TryAppendRelationKindChangedInfo(ref text, previousRelationKind, ParticipantFaction.PlayerRelationKind);
        return text;
    }

    [DebugOutput("Incidents", false)]
    private static void DiplomaticOfficesChances()
    {
        StringBuilder stringBuilder = new();
        AppendDebugChances(stringBuilder, 0f);
        AppendDebugChances(stringBuilder, 1f);
        AppendDebugChances(stringBuilder, 2.2f);
        AppendDebugChances(stringBuilder, 2.5f);
        Log.Message(stringBuilder.ToString());
    }

    private static void AppendDebugChances(StringBuilder sb, float negotiationAbility)
    {
        if (sb.Length > 0)
        {
            sb.AppendLine();
        }
        if (negotiationAbility < TriumphNeedAbility)
        {
            sb.AppendLine("Success: " + 1f.ToStringPercent());
            sb.AppendLine("Triumph: " + 0f.ToStringPercent());
        }
        else
        {
            sb.AppendLine("Success: " + 0f.ToStringPercent());
            sb.AppendLine("Triumph: " + 1f.ToStringPercent());
        }
    }
}
