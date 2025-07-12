using RimWorld;
using System.Text;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace OberoniaAurea;

[StaticConstructorOnStartup]
public class Gizmo_CircuitRegulator : Gizmo_Slider
{
    private readonly ThingWithComps thing;

    private static readonly Texture2D IconTex = ContentFinder<Texture2D>.Get("UI/Icons/SuppressionToggle");

    private readonly CompCircuitRegulator comp;

    protected override string Title => "OA_CircuitRegulatorGizmo".Translate();

    protected override float ValuePercent => comp.CurCircuitStability;

    protected override bool IsDraggable => true;

    protected override FloatRange DragRange => new(0f, 1f);

    protected override string HighlightTag => "OA_CircuitRegulatorGizmo";

    protected override float Target
    {
        get
        {
            return comp.TargetCircuitStability;
        }
        set
        {
            comp.TargetCircuitStability = value;
        }
    }

    public Gizmo_CircuitRegulator(ThingWithComps thing)
    {
        this.thing = thing;
        comp = thing.GetComp<CompCircuitRegulator>();
    }

    protected override string GetTooltip()
    {
        StringBuilder stringBuilder = new();
        stringBuilder.AppendLine("OA_CircuitRegulatorRepairmentTooltipTitle".Translate().CapitalizeFirst().Colorize(ColoredText.TipSectionTitleColor));
        stringBuilder.AppendLine();
        stringBuilder.Append("OA_CircuitRegulatorRepairmentTooltipDesc".Translate(thing.LabelNoParenthesis, comp.TargetCircuitStability.ToStringPercent("0").Colorize(ColoredText.TipSectionTitleColor).Named("LEVEL")).Resolve());
        if (!comp.repairmentEnabled)
        {
            stringBuilder.Append("\n\n" + "OA_CircuitRegulatorRepairmentTooltipDisabled".Translate(thing.LabelNoParenthesis).CapitalizeFirst().Colorize(ColoredText.FactionColor_Hostile));
        }
        return stringBuilder.ToString();
    }

    protected override void DrawHeader(Rect headerRect, ref bool mouseOverElement)
    {
        headerRect.xMax -= 24f;

        Rect rect = new(headerRect.xMax, headerRect.y, 24f, 24f);
        GUI.DrawTexture(rect, IconTex);
        GUI.DrawTexture(new Rect(rect.center.x, rect.y, rect.width / 2f, rect.height / 2f), comp.repairmentEnabled ? Widgets.CheckboxOnTex : Widgets.CheckboxOffTex);
        if (Widgets.ButtonInvisible(rect))
        {
            comp.repairmentEnabled = !comp.repairmentEnabled;
            if (comp.repairmentEnabled)
            {
                SoundDefOf.Tick_High.PlayOneShotOnCamera();
            }
            else
            {
                SoundDefOf.Tick_Low.PlayOneShotOnCamera();
            }
        }
        if (Mouse.IsOver(rect))
        {
            Widgets.DrawHighlight(rect);
            TooltipHandler.TipRegion(rect, GetTooltipDesc, 828267373);
            mouseOverElement = true;
        }

        base.DrawHeader(headerRect, ref mouseOverElement);
    }

    private string GetTooltipDesc()
    {
        string arg = (comp.repairmentEnabled ? "On" : "Off").Translate().ToString().UncapitalizeFirst();
        string arg2 = comp.TargetCircuitStability.ToStringPercent("0").Colorize(ColoredText.TipSectionTitleColor);
        return "OA_CircuitRegulatorRepairmentToggleTooltipDesc".Translate(thing.LabelNoParenthesis, arg2.Named("LEVEL"), arg.Named("ONOFF")).Resolve();
    }
}
