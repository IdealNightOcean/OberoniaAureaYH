using RimWorld;
using System.Collections.Generic;
using Verse;

namespace OberoniaAurea;

public class PortableCommsConsole : Apparel
{
    public override IEnumerable<Gizmo> GetWornGizmos()
    {
        foreach (Gizmo gizmo in base.GetWornGizmos())
        {
            yield return gizmo;
        }
        Faction oaFaction = OARatkin_MiscUtility.OAFaction;
        Command_Action command_Use = new()
        {
            defaultLabel = "OA_PortableCommsConsoleLable".Translate(),
            defaultDesc = "OA_PortableCommsConsoleDesc".Translate(Label),
            action = delegate
            {
                GizmoFloat_CommsConsoleList(Wearer, oaFaction);
            },
            icon = def.uiIcon,
            activateSound = SoundDefOf.Tick_Tiny
        };
        if (oaFaction is null)
        {
            command_Use.Disable("OA_RK_FactionNonexistent".Translate());
        }
        yield return command_Use;
    }

    protected void GizmoFloat_CommsConsoleList(Pawn pawn, Faction faction)
    {
        if (pawn is null)
        {
            return;
        }
        FloatMenuOption floatMenuOption = GetFailureReason(pawn);
        floatMenuOption ??= CommFloatMenuOption(pawn, faction);
        if (floatMenuOption is not null)
        {
            Find.WindowStack.Add(new FloatMenu([floatMenuOption]));
        }
    }
    public FloatMenuOption CommFloatMenuOption(Pawn negotiator, Faction faction)
    {
        string text = "CallOnRadio".Translate(faction.GetCallLabel());
        text = text + " (" + faction.PlayerRelationKind.GetLabelCap() + ", " + faction.GoodwillWith(Faction.OfPlayer).ToStringWithSign() + ")";
        Pawn leader = faction.leader;
        if (!LeaderIsAvailableToTalk(leader))
        {
            string text2 = ((leader is null) ? ((string)"LeaderUnavailableNoLeader".Translate()) : ((string)"LeaderUnavailable".Translate(leader.LabelShort, leader)));
            return new FloatMenuOption(text + " (" + text2 + ")", null, faction.def.FactionIcon, faction.Color);
        }
        return FloatMenuUtility.DecoratePrioritizedTask(new FloatMenuOption(text, delegate
        {
            TryOpenComms(negotiator, faction);
            PlayerKnowledgeDatabase.KnowledgeDemonstrated(ConceptDefOf.OpeningComms, KnowledgeAmount.Total);
        }, faction.def.FactionIcon, faction.Color, MenuOptionPriority.InitiateSocial), negotiator, this);

    }
    protected static void TryOpenComms(Pawn negotiator, Faction faction)
    {
        Dialog_Negotiation dialog_Negotiation = new(negotiator, faction, FactionDialogMaker.FactionDialogFor(negotiator, faction), radioMode: true)
        {
            soundAmbient = SoundDefOf.RadioComms_Ambience
        };
        Find.WindowStack.Add(dialog_Negotiation);
    }
    protected static bool LeaderIsAvailableToTalk(Pawn leader)
    {
        if (leader is null)
        {
            return false;
        }
        if (leader.Spawned && (leader.Downed || leader.IsPrisoner || !leader.Awake() || leader.InMentalState))
        {
            return false;
        }
        return true;
    }

    private FloatMenuOption GetFailureReason(Pawn pawn)
    {
        if (!pawn.health.capacities.CapableOf(PawnCapacityDefOf.Talking))
        {
            return new FloatMenuOption("CannotUseReason".Translate("IncapableOfCapacity".Translate(PawnCapacityDefOf.Talking.label, pawn.Named("PAWN"))), null);
        }
        return null;
    }

}
