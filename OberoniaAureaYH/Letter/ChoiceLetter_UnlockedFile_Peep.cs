using RimWorld;
using System.Collections.Generic;
using Verse;
using static OberoniaAurea.UnlockedFileUtility;

namespace OberoniaAurea;

internal class ChoiceLetter_UnlockedFile_Peep : ChoiceLetter
{
    private DocumentType fileType;
    private Pawn pawn;
    private float detectionProp;

    public override IEnumerable<DiaOption> Choices
    {
        get
        {
            if (!ArchivedOnly && pawn is not null)
            {
                yield return Option_Check;
            }
            yield return Option_Close;
        }
    }

    private DiaOption Option_Check => new("OARK_Check".Translate())
    {
        action = delegate
        {
            if (Rand.Chance(detectionProp))
            {
                ModUtility.OAFaction?.TryAffectGoodwillWith(Faction.OfPlayer, -4);
                Messages.Message("OARK_UnlockedFile_Detection".Translate(pawn), MessageTypeDefOf.NegativeEvent);
            }
            DocumentResult(fileType, pawn, isDelay: false);
            Find.LetterStack.RemoveLetter(this);
        },
        resolveTree = true
    };

    public void InitLetter(Pawn pawn, DocumentType fileType, float detectionProp)
    {
        this.pawn = pawn;
        this.fileType = fileType;
        this.detectionProp = detectionProp;
    }

    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_References.Look(ref pawn, "pawn");
        Scribe_Values.Look(ref fileType, "fileType", defaultValue: DocumentType.TechDraft);
        Scribe_Values.Look(ref detectionProp, "detectionProp", 0f);
    }
}
