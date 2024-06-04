using RimWorld;
using Verse;

namespace OberoniaAurea_Hyacinth;

public class StatPart_RatkinHarmTurret : StatPart
{
    public float factor = 0.05f;

    public bool applyToNegativeValues;

    private bool applied;

    public override void TransformValue(StatRequest req, ref float val)
    {
        if (!req.HasThing)
        {
            return;
        }
        Thing thing = req.Thing;
        Map map = thing.Map;
        if (map == null || !thing.Position.IsValid || !thing.Spawned)
        {
            return;
        }
        if (val > 0f || applyToNegativeValues)
        {
            applied = Current.Game.GetComponent<GameComponent_Hyacinth>()?.RatkinHarmTurretCheck(map, thing) ?? false;
        }
        else
        {
            applied = false;
        }
        val *= applied ? factor : 1f;
    }

    public override string ExplanationPart(StatRequest req)
    {
        if (!req.HasThing)
        {
            return null;
        }
        if (!applyToNegativeValues && req.Thing.GetStatValue(parentStat) <= 0f)
        {
            return null;
        }
        if (applied)
        {
            string text = "电磁干扰: x" + (factor).ToStringPercent();
            float num = 9999999f;
            if (num < 999999f)
            {
                text += "\n    (" + "StatsReport_MaxGain".Translate() + ": " + num.ToStringByStyle(parentStat.ToStringStyleUnfinalized, parentStat.toStringNumberSense) + ")";
            }
            return text;
        }
        return null;
    }
}
