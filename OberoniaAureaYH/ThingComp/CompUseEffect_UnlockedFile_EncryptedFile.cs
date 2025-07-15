using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using Verse;
using static OberoniaAurea.UnlockedFileUtility;

namespace OberoniaAurea;

public class CompUseEffect_UnlockedFile_EncryptedFile : CompUseEffect
{
    private Pawn pawn;
    private DocumentType fileType;
    private int expiredTick;

    public void InitEncryptedFile(Pawn pawn)
    {
        this.pawn = pawn;
        fileType = GetRandomDocumentType();
        expiredTick = Find.TickManager.TicksGame + (30 * 60000);
    }

    public override void DoEffect(Pawn usedBy)
    {
        base.DoEffect(usedBy);
        DocumentResult(fileType, pawn, isDelay: true);
    }

    public override AcceptanceReport CanBeUsedBy(Pawn p)
    {
        if (p != pawn)
        {
            return "OARK_UnlockedFile_NotLendPawn".Translate(pawn);
        }
        if (Find.TickManager.TicksGame > expiredTick)
        {
            return "OARK_UnlockedFile_Expired".Translate();
        }
        return true;
    }

    public override void PostExposeData()
    {
        base.PostExposeData();
        Scribe_References.Look(ref pawn, "pawn");
        Scribe_Values.Look(ref fileType, "fileType", DocumentType.TechDesign);
        Scribe_Values.Look(ref expiredTick, "expiredTick", 0);
    }

    private static DocumentType GetRandomDocumentType()
    {
        List<DocumentType> documentTypes = Enum.GetValues(typeof(DocumentType)).Cast<DocumentType>().ToList();
        documentTypes.Remove(DocumentType.EncryptedFile);
        return documentTypes.RandomElement();
    }
}
