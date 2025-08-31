using RimWorld;
using RimWorld.Planet;
using System.Collections.Generic;
using System.Text;
using Verse;
using Verse.AI;

namespace OberoniaAurea;

public class CompProperties_Hackable : CompProperties
{
    public JobDef hackJob;
    public float maxHackPoint = 600;
    public int intellectualSkillPrerequisite = 0;
    public bool destoryOnHackComplete = false;

    public CompProperties_Hackable()
    {
        compClass = typeof(CompHackable);
    }
}

public class CompHackable : ThingComp
{
    public CompProperties_Hackable Props => (CompProperties_Hackable)props;

    protected List<string> MapParentQuestTag => parent.MapHeld?.Parent.questTags;
    protected MapParent MapParent => parent.MapHeld?.Parent;
    protected virtual bool SendMapParentQuestTagSignal => false;

    protected JobDef HackJob => Props.hackJob ?? OARK_ModDefOf.OARK_Job_Hack;

    protected bool isHacked;
    protected bool isHackable = true;
    [Unsaved] protected bool isHackForceEnded;

    public bool IsHacked => isHacked;
    public bool IsHackable => isHackable;

    protected float hackPoint;
    protected virtual float MaxHackPoint => Props.maxHackPoint;

    public float ProgressPercent => hackPoint / MaxHackPoint;

    public virtual void DoHack(float points, Pawn hackPawn)
    {
        if (!isHackable)
        {
            return;
        }

        hackPoint += points;

        if (hackPoint >= MaxHackPoint)
        {
            hackPoint = MaxHackPoint;
            FinishHack(hackPawn);
        }
    }

    public virtual void FinishHack(Pawn hackPawn)
    {
        isHacked = true;
        isHackable = false;

        QuestUtility.SendQuestTargetSignals(parent.questTags, "HackCompleted", parent.Named("SUBJECT"), hackPawn.Named("HACKER"));
        if (SendMapParentQuestTagSignal)
        {
            QuestUtility.SendQuestTargetSignals(MapParentQuestTag, "HackCompleted", parent.Named("SUBJECT"), hackPawn.Named("HACKER"));
        }
        if (Props.destoryOnHackComplete)
        {
            parent.Kill();
        }
    }

    public virtual void ForceEndHack()
    {
        isHackable = false;
        isHackForceEnded = true;
        QuestUtility.SendQuestTargetSignals(parent.questTags, "HackForceEnded", parent.Named("SUBJECT"));
        if (SendMapParentQuestTagSignal)
        {
            QuestUtility.SendQuestTargetSignals(MapParentQuestTag, "HackForceEnded", parent.Named("SUBJECT"));
        }
    }

    public override void PostDestroy(DestroyMode mode, Map previousMap)
    {
        if (!isHacked && !isHackForceEnded)
        {
            ForceEndHack();
        }
        base.PostDestroy(mode, previousMap);
    }

    public override IEnumerable<FloatMenuOption> CompFloatMenuOptions(Pawn selPawn)
    {
        foreach (FloatMenuOption menuOption in base.CompFloatMenuOptions(selPawn))
        {
            yield return menuOption;
        }
        AcceptanceReport acceptanceReport = CanHackNow(selPawn);
        if (acceptanceReport)
        {
            yield return new FloatMenuOption("Hack".Translate(parent.Label), delegate
            {
                selPawn.jobs.TryTakeOrderedJob(JobMaker.MakeJob(HackJob, parent), JobTag.Misc);
            });
        }
        else if (!acceptanceReport.Reason.NullOrEmpty())
        {
            yield return new FloatMenuOption("CannotHack".Translate(parent.Label) + ": " + acceptanceReport.Reason, null);
        }
        else
        {
            yield return new FloatMenuOption("CannotHack".Translate(parent.Label), null);
        }
    }

    public override IEnumerable<Gizmo> CompGetGizmosExtra()
    {
        if (DebugSettings.ShowDevGizmos)
        {
            yield return new Command_Action()
            {
                defaultLabel = "DEV: Hack",
                action = delegate { DoHack(MaxHackPoint, null); }
            };
        }
    }

    protected virtual AcceptanceReport CanHackNow(Pawn pawn)
    {
        if (!isHackable)
        {
            return "OARK_NotHackableNow".Translate();
        }
        if (!HackUtility.IsCapableOfHacking(pawn))
        {
            return "IncapableOfHacking".Translate();
        }
        if (!pawn.CanReach(parent, PathEndMode.ClosestTouch, Danger.Deadly))
        {
            return "NoPath".Translate().CapitalizeFirst();
        }
        if (Props.intellectualSkillPrerequisite > 0 && pawn.skills.GetSkill(SkillDefOf.Intellectual).Level < Props.intellectualSkillPrerequisite)
        {
            return "SkillTooLow".Translate(SkillDefOf.Intellectual.label, pawn.skills.GetSkill(SkillDefOf.Intellectual).Level, Props.intellectualSkillPrerequisite);
        }
        return true;
    }

    public override string CompInspectStringExtra()
    {
        return CompInspectStringExtraBuilder().ToString();
    }

    protected virtual StringBuilder CompInspectStringExtraBuilder()
    {
        StringBuilder sb = new();
        if (!isHackable)
        {
            sb.AppendInNewLine("OARK_NotHackableNow".Translate());
        }
        sb.AppendInNewLine("HackProgress".Translate());
        sb.Append(": ");
        sb.Append(hackPoint.ToString("F2"));
        sb.Append(" / ");
        sb.Append(MaxHackPoint);
        if (isHacked)
        {
            sb.AppendInNewLine("Hacked".Translate());
        }
        else if (Props.intellectualSkillPrerequisite > 0)
        {
            sb.AppendInNewLine("IntellectualSkillPrerequisite".Translate());
            sb.Append(": ");
            sb.Append(Props.intellectualSkillPrerequisite);
        }

        return sb;
    }

    public override void PostExposeData()
    {
        base.PostExposeData();
        Scribe_Values.Look(ref isHacked, "isHacked", defaultValue: false);
        Scribe_Values.Look(ref isHackable, "isHackable", defaultValue: true);
        Scribe_Values.Look(ref hackPoint, "hackPoint", 0f);
    }
}
