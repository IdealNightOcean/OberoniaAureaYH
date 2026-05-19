using UnityEngine;
using Verse;

namespace OberoniaAurea;

public class OberoniaAureaMod : Mod
{
    public static OberoniaAureaSettings Settings;

    public OberoniaAureaMod(ModContentPack content) : base(content)
    {
        Settings = GetSettings<OberoniaAureaSettings>();
    }

    public override void DoSettingsWindowContents(Rect inRect)
    {
        Settings.DoSettingsWindowContents(inRect);
    }

    public override string SettingsCategory()
    {
        return "Mod.OberoniaAurea".Translate();
    }
}

public class OberoniaAureaSettings : ModSettings
{
    private Vector2 scrollPosition;
    private float viewRectHeight;

    public static bool BirthdayWishesShowLetter = true;

    public void DoSettingsWindowContents(Rect inRect)
    {
        Rect outRect = new(inRect.x, inRect.y, inRect.width * 0.6f, inRect.height);
        outRect = outRect.CenteredOnXIn(inRect);
        float viewRectX = outRect.x + 8f;
        Rect viewRect = new(viewRectX, outRect.y, outRect.width - 16f, viewRectHeight);
        Widgets.BeginScrollView(outRect, ref scrollPosition, viewRect);
        Listing_Standard listing_Rect = new()
        {
            ColumnWidth = viewRect.width,
            maxOneColumn = true
        };
        listing_Rect.Begin(viewRect);

        listing_Rect.CheckboxLabeled($"OARK_Setting_{nameof(BirthdayWishesShowLetter)}".Translate(), ref BirthdayWishesShowLetter);


        listing_Rect.End();
        if (Event.current.type == EventType.Layout)
        {
            viewRectHeight = listing_Rect.MaxColumnHeightSeen + 50f;
        }
        Widgets.EndScrollView();

    }

    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_Values.Look(ref BirthdayWishesShowLetter, nameof(BirthdayWishesShowLetter), defaultValue: true);
    }
}

