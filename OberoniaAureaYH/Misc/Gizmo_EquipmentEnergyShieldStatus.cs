using RimWorld;
using UnityEngine;
using Verse;

namespace OberoniaAurea;

[StaticConstructorOnStartup]
public class Gizmo_EquipmentEnergyShieldStatus : Gizmo
{
    public CompEquipmentShield shieldComp;

    private static readonly Texture2D FullShieldBarTex = SolidColorMaterials.NewSolidColorTexture(new Color(0.2f, 0.2f, 0.24f));

    private static readonly Texture2D EmptyShieldBarTex = SolidColorMaterials.NewSolidColorTexture(Color.clear);

    public Gizmo_EquipmentEnergyShieldStatus()
    {
        Order = -100f;
    }

    public override float GetWidth(float maxWidth)
    {
        return 140f;
    }

    public override GizmoResult GizmoOnGUI(Vector2 topLeft, float maxWidth, GizmoRenderParms parms)
    {
        Rect rect = new(topLeft.x, topLeft.y, GetWidth(maxWidth), 75f);
        Rect innerRect = rect.ContractedBy(6f);
        Widgets.DrawWindowBackground(rect);
        Rect titleRect = innerRect;
        titleRect.height = rect.height / 2f;
        Text.Font = GameFont.Tiny;
        Widgets.Label(titleRect, shieldComp.parent.LabelCap);
        Rect barRect = innerRect;
        barRect.yMin = innerRect.y + innerRect.height / 2f;
        float fillPercent = shieldComp.Energy / Mathf.Max(1f, shieldComp.parent.GetStatValue(StatDefOf.EnergyShieldEnergyMax));
        Widgets.FillableBar(barRect, fillPercent, FullShieldBarTex, EmptyShieldBarTex, doBorder: false);
        Text.Font = GameFont.Small;
        Text.Anchor = TextAnchor.MiddleCenter;
        Widgets.Label(barRect, (shieldComp.Energy * 100f).ToString("F0") + " / " + (shieldComp.parent.GetStatValue(StatDefOf.EnergyShieldEnergyMax) * 100f).ToString("F0"));
        Text.Anchor = TextAnchor.UpperLeft;
        TooltipHandler.TipRegion(innerRect, "ShieldPersonalTip".Translate());
        return new GizmoResult(GizmoState.Clear);
    }
}