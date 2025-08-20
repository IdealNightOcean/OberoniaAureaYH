using RimWorld;
using UnityEngine;
using Verse;

namespace OberoniaAurea;

public class Dialog_SimpleNegotiation(Pawn negotiator, DiaNode startNode, bool radioMode) : Dialog_NodeTree(startNode, radioMode)
{
    private const float TitleHeight = 70f;
    private const float InfoHeight = 60f;

    protected Pawn negotiator = negotiator;

    public override Vector2 InitialSize => new(720f, 600f);

    public override void DoWindowContents(Rect inRect)
    {
        Widgets.BeginGroup(inRect);
        Rect rect = new(0f, 0f, inRect.width / 2f, TitleHeight);
        Rect rect2 = new(0f, rect.yMax, rect.width, InfoHeight);
        Text.Font = GameFont.Medium;
        Widgets.Label(rect, negotiator.LabelCap);
        Text.Anchor = TextAnchor.UpperLeft;
        Text.Font = GameFont.Small;
        GUI.color = new Color(1f, 1f, 1f, 0.7f);
        Widgets.Label(rect2, "SocialSkillIs".Translate(negotiator.skills.GetSkill(SkillDefOf.Social).Level));
        GUI.color = Color.white;
        Widgets.EndGroup();
        float num = 147f;
        Rect rect3 = new(0f, num, inRect.width, inRect.height - num);
        DrawNode(rect3);
    }
}