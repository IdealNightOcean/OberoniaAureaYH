using OberoniaAurea_Frame;
using System;
using System.Collections.Generic;
using System.Linq;
using Verse;
using static OberoniaAurea.UnlockedFileUtility;

namespace OberoniaAurea;

public class ChoiceLetter_UnlockedFile : ChoiceLetter
{
    private DocumentType fileType;
    private Pawn pawn;

    private float detectionProp = 0.2f;

    public void InitLetter(Pawn pawn)
    {
        this.pawn = pawn;
        fileType = GetRandomDocumentType();
    }

    public override IEnumerable<DiaOption> Choices
    {
        get
        {
            if (ArchivedOnly || pawn is null)
            {
                yield return Option_Close;
                yield break;
            }

            yield return Option_Peep();
            yield return Option_Quiz;
            yield return Option_Ignore;
            yield return Option_Postpone;
        }
    }

    public DiaOption Option_Peep()
    {
        DiaOption diaOption = new("OARK_Peep".Translate())
        {
            action = delegate
            {
                ChoiceLetter_UnlockedFile_Peep letter = (ChoiceLetter_UnlockedFile_Peep)LetterMaker.MakeLetter(label: "OARK_LetterLabel_UnlockedFilePeep".Translate(),
                                                                                                               text: "OARK_Letter_UnlockedFilePeep".Translate(pawn),
                                                                                                               def: OARK_LetterDefOf.OARK_UnlockedFilePeepLetter,
                                                                                                               relatedFaction: ModUtility.OAFaction);

                letter.InitLetter(pawn, fileType, detectionProp);
                Find.LetterStack.ReceiveLetter(letter, delayTicks: 30000);
                Find.LetterStack.RemoveLetter(this);
            },
            resolveTree = true
        };
        return diaOption;
    }

    public DiaOption Option_Ignore => new("Ignore".Translate())
    {
        action = delegate
        {
            Find.LetterStack.RemoveLetter(this);
        },
        resolveTree = true
    };

    public DiaOption Option_Quiz => new("OARK_Quiz".Translate())
    {
        action = QuizResult,
        resolveTree = true
    };

    private void QuizResult()
    {
        if (Rand.Chance(0.1f) || fileType == DocumentType.LoveLetter || fileType == DocumentType.EncryptedFile)
        {
            OpenLetter();
            detectionProp = 0.5f;
            TaggedString diaText = fileType switch
            {
                DocumentType.LoveLetter => "OARK_UnlockedFile_LoveLetterRefuse".Translate(pawn),
                DocumentType.EncryptedFile => "OARK_UnlockedFile_EncryptedFileRefuse".Translate(pawn),
                _ => "OARK_UnlockedFile_NormalRefuse".Translate(pawn),
            };
            Find.WindowStack.Add(OAFrame_DiaUtility.DefaultConfirmDiaNodeTree(diaText));
        }
        else
        {
            Find.LetterStack.RemoveLetter(this);
            Find.WindowStack.Add(OAFrame_DiaUtility.ConfirmDiaNodeTree(text: "OARK_UnlockedFile_QuizAllow".Translate(),
                                                                       acceptText: "OARK_Check".Translate(),
                                                                       acceptAction: delegate { DocumentResult(fileType, pawn, isDelay: false); }));
        }
    }
    private static DocumentType GetRandomDocumentType()
    {
        List<DocumentType> documentTypes = Enum.GetValues(typeof(DocumentType)).Cast<DocumentType>().ToList();
        return documentTypes.RandomElementByWeight((t) => { return t == DocumentType.EncryptedFile ? 2 : 1; });
    }

    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_References.Look(ref pawn, "pawn");
        Scribe_Values.Look(ref fileType, "fileType", defaultValue: DocumentType.TechDraft);
        Scribe_Values.Look(ref detectionProp, "detectionProp", 0f);
    }
}