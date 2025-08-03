using OberoniaAurea_Frame;
using RimWorld;
using System.Collections.Generic;
using System.Text;
using Verse;

namespace OberoniaAurea;

public class ScienceShipLaunchSite : WorldObject_InteractWithFixedCarvanBase
{
    private bool shipLaunched;
    private string shipName;
    public override int TicksNeeded => 12500;

    public override void PostAdd()
    {
        base.PostAdd();
        shipName = ScienceShipRecord.GenerateShipName();
    }

    public void SetShipName(string shipName)
    {
        this.shipName = shipName;
    }

    public override string GetInspectString()
    {
        StringBuilder sb = new(base.GetInspectString());
        sb.AppendInNewLine("OARK_ScienceShipName".Translate());
        sb.Append(": ");
        sb.Append(shipName);

        sb.AppendInNewLine("OARK_ScienceShipLaunchSite_TimeLeft".Translate(ticksRemaining.ToStringTicksToPeriod()));

        return sb.ToString();
    }

    protected override void FinishWork()
    {
        int maxLevel = 0;

        SkillRecord skillRecord;
        int skillLevel;
        Pawn maxSkillPawn = null;
        foreach (Pawn pawn in associatedFixedCaravan.PawnsListForReading)
        {
            skillRecord = pawn.skills?.GetSkill(SkillDefOf.Intellectual);
            skillLevel = skillRecord is null ? 0 : skillRecord.GetLevel();
            if (skillLevel > maxLevel)
            {
                maxLevel = skillLevel;
                maxSkillPawn = pawn;
            }

            if (pawn.needs.mood is not null)
            {
                Thought_Memory thought = ThoughtMaker.MakeThought(OARK_ThoughtDefOf.OARK_Thought_ScienceShipLaunch, skillLevel < 10 ? 0 : 1);
                pawn.needs.mood.thoughts.memories.TryGainMemory(thought);
            }
        }

        maxSkillPawn ??= associatedFixedCaravan.PawnsListForReading.RandomElement();

        List<Thing> rewards = OAFrame_MiscUtility.TryGenerateThing(OARK_ThingDefOf.Oberonia_Aurea_Chanwu_AB, associatedFixedCaravan.PawnsListForReading.Count * 3);
        OAFrame_FixedCaravanUtility.GiveThings(associatedFixedCaravan, rewards);

        LaunchShip();

        Find.WindowStack.Add(OAFrame_DiaUtility.DefaultConfirmDiaNodeTree(GetShipLaunchText(maxSkillPawn, maxLevel)));

        Destroy();
    }

    protected override void InterruptWork()
    {
        LaunchShip();
        Find.WindowStack.Add(OAFrame_DiaUtility.DefaultConfirmDiaNodeTree("OARK_ScienceShipLaunch_Interrupt".Translate()));
        Destroy();
    }

    private void LaunchShip()
    {
        shipLaunched = true;
        ScienceShipRecord scienceShip = ScienceShipRecord.GenerateShip(shipName);
        ScienceDepartmentInteractHandler.Instance.ScienceShipRecord = scienceShip;
        IncidentParms parms = new()
        {
            target = Find.World,
            faction = ModUtility.OAFaction
        };
        OAFrame_MiscUtility.AddNewQueuedIncident(OARK_IncidentDefOf.OARK_ScienceShipRecycle, Rand.RangeInclusive(20, 40) * 60000, parms);
    }

    public override void Destroy()
    {
        if (!shipLaunched)
        {
            LaunchShip();
        }
        base.Destroy();
    }

    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_Values.Look(ref shipLaunched, "shipLaunched", defaultValue: false);
        Scribe_Values.Look(ref shipName, "shipName");
    }

    private static string GetShipLaunchText(Pawn pawn, int skillLevel)
    {
        StringBuilder sb = new("OARK_ScienceShipLaunchFinished".Translate(3000, 3));

        ScienceShipRecord shipRecord = ScienceDepartmentInteractHandler.Instance.ScienceShipRecord.Value;

        if (skillLevel >= 15)
        {
            sb.AppendLine();
            sb.AppendInNewLine("OARK_ScienceShipEnvir".Translate(pawn));
            sb.AppendInNewLine(ScienceShipRecord.GetShipEnvirText(shipRecord.EnvironmentAffect).Translate());

            if (skillLevel >= 18)
            {
                sb.AppendLine();
                sb.AppendInNewLine(ScienceShipRecord.GetShipLaunchText(shipRecord.TypeOfShip).Translate());
            }
        }

        return sb.ToString();
    }
}
