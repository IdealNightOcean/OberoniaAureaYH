using RimWorld;
using RimWorld.Planet;
using System.Collections.Generic;
using System.Text;
using Verse;
using Verse.AI;

namespace OberoniaAurea;

public class CompProperties_Hackable : CompProperties
{
    public float maxHackPoint = 600;
    public int intellectualSkillPrerequisite = 0;
    public bool destoryOnHackComplete = false;
}

public abstract class CompHackable : ThingComp
{
    public CompProperties_Hackable Props => (CompProperties_Hackable)props;

    protected List<string> WorldObjectQuestTag => parent.MapHeld?.Parent.questTags;
    protected MapParent MapParent => parent.MapHeld?.Parent;

    protected bool isHacked;
    protected bool isHackable = true;

    public bool IsHacked => isHacked;
    public bool IsHackable => isHackable;

    protected float hackPoint;

    public float ProgressPercent => hackPoint / Props.maxHackPoint;

    public virtual void DoHack(float points)
    {
        if (!isHackable)
        {
            return;
        }

        hackPoint += points;

        if (hackPoint >= Props.maxHackPoint)
        {
            FinishHack();
        }
    }

    public virtual void FinishHack()
    {
        isHackable = false;
        QuestUtility.SendQuestTargetSignals(parent.questTags, "HackCompleted", this.Named("SUBJECT"));
        if (Props.destoryOnHackComplete)
        {
            parent.Kill();
        }
    }

    public virtual void EndHack()
    {
        isHacked = true;
        isHackable = false;
        QuestUtility.SendQuestTargetSignals(WorldObjectQuestTag, "HackEnded", MapParent.Named("SUBJECT"));
    }

    public override void PostDeSpawn(Map map, DestroyMode mode = DestroyMode.Vanish)
    {
        if (!isHacked)
        {
            QuestUtility.SendQuestTargetSignals(map.Parent.questTags, "DeSpawnedBeforeHacked", this.Named("SUBJECT"));
        }
        base.PostDeSpawn(map, mode);
    }

    public override void PostDestroy(DestroyMode mode, Map previousMap)
    {
        if (!isHacked)
        {
            QuestUtility.SendQuestTargetSignals(previousMap.Parent.questTags, "DestroyedBeforeHacked", this.Named("SUBJECT"));
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
                selPawn.jobs.TryTakeOrderedJob(JobMaker.MakeJob(OARK_ModDefOf.OARK_Job_Hack, parent), JobTag.Misc);
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

    public AcceptanceReport CanHackNow(Pawn pawn)
    {
        if (!isHackable)
        {
            return false;
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
        sb.Append(hackPoint);
        sb.Append(" / ");
        sb.Append(Props.maxHackPoint);
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
