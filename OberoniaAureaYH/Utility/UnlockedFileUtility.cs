using OberoniaAurea_Frame;
using RimWorld;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace OberoniaAurea;

public static class UnlockedFileUtility
{
    public enum DocumentType
    {
        TechDesign,
        TechDraft,
        LoveLetter,
        Dinner,
        DeletedFile,
        Manuscript,
        EncryptedFile
    }

    public static void DocumentResult(DocumentType type, Pawn pawn, bool isDelay)
    {
        switch (type)
        {
            case DocumentType.TechDesign:
                Result_TechDesign(pawn, isDelay);
                return;
            case DocumentType.TechDraft:
                Result_TechDraft(pawn, isDelay);
                return;
            case DocumentType.LoveLetter:
                Result_LoveLetter(pawn, isDelay);
                return;
            case DocumentType.Dinner:
                Result_Dinner(pawn, isDelay);
                return;
            case DocumentType.DeletedFile:
                Result_DeletedFile(pawn, isDelay);
                return;
            case DocumentType.Manuscript:
                Result_Manuscript(pawn, isDelay);
                return;
            case DocumentType.EncryptedFile:
                Result_EncryptedFile(pawn);
                return;
            default: return;
        }
    }

    private static void Result_TechDesign(Pawn pawn, bool isDelay)
    {
        StringBuilder sb = new(isDelay ? "OARK_UnlockedFile_TechDesignDelay".Translate()
                                       : "OARK_UnlockedFile_TechDesign".Translate());

        pawn.skills?.Learn(SkillDefOf.Intellectual, 2000f);
        sb.AppendLine();
        sb.AppendInNewLine("OAFrame_PawnGainSkillXp".Translate(pawn, SkillDefOf.Intellectual.LabelCap, 2000));

        ResearchProjectDef projectDef = ModUtility.GetSignalResearchableProject(maxPotentialCount: 10);
        if (projectDef is not null)
        {
            Find.ResearchManager.FinishProject(projectDef, doCompletionDialog: false, researcher: pawn, doCompletionLetter: true);
            sb.AppendLine();
            sb.AppendInNewLine("OAFrame_ResearchFinished".Translate());
            sb.AppendInNewLine(projectDef.LabelCap);
        }
        Log.Message(sb.ToString());
        Find.WindowStack.Add(OAFrame_DiaUtility.DefaultConfirmDiaNodeTree(sb.ToString()));
    }

    private static void Result_TechDraft(Pawn pawn, bool isDelay)
    {
        StringBuilder sb = new(isDelay ? "OARK_UnlockedFile_TechDraftDelay".Translate()
                                       : "OARK_UnlockedFile_TechDraft".Translate());

        pawn.skills?.Learn(SkillDefOf.Intellectual, 6000f);
        sb.AppendLine();
        sb.AppendInNewLine("OAFrame_PawnGainSkillXp".Translate(pawn, SkillDefOf.Intellectual.LabelCap, 6000));

        int skillLevel = pawn.skills?.GetSkill(SkillDefOf.Intellectual)?.GetLevel() ?? 0;
        if (skillLevel > 0)
        {
            List<ResearchProjectDef> researchProjects = DefDatabase<ResearchProjectDef>.AllDefsListForReading.Where(r => !r.IsFinished && !r.IsHidden).Take(10).TakeRandomDistinct(4);
            if (!researchProjects.NullOrEmpty())
            {
                float progress = skillLevel * 150f;
                sb.AppendLine();
                sb.AppendInNewLine("OAFrame_ResearchProgressAdded".Translate(progress.ToString("F0")));
                foreach (ResearchProjectDef projectDef in researchProjects)
                {
                    Find.ResearchManager.AddProgress(projectDef, progress);
                    sb.AppendInNewLine(projectDef.LabelCap);
                }
            }
        }
        Find.WindowStack.Add(OAFrame_DiaUtility.DefaultConfirmDiaNodeTree(sb.ToString()));
    }

    private static void Result_LoveLetter(Pawn pawn, bool isDelay)
    {
        pawn.needs?.mood?.thoughts?.memories?.TryGainMemory(OARK_ThoughtDefOf.OARK_UnlockedFile_LoveLetter);
        Find.WindowStack.Add(OAFrame_DiaUtility.DefaultConfirmDiaNodeTree(isDelay ? "OARK_UnlockedFile_LoveLetterDelay".Translate(pawn)
                                                                                  : "OARK_UnlockedFile_LoveLetter".Translate(pawn)));
    }

    private static void Result_Dinner(Pawn pawn, bool isDelay)
    {
        StringBuilder sb = new();
        if (Rand.Bool)
        {
            sb.Append(isDelay ? "OARK_UnlockedFile_DinnerSuccessDelay".Translate(pawn)
                              : "OARK_UnlockedFile_DinnerSuccess".Translate(pawn));

            pawn.skills?.Learn(SkillDefOf.Cooking, 300f);
            sb.AppendLine();
            sb.AppendInNewLine("OAFrame_PawnGainSkillXp".Translate(pawn, SkillDefOf.Cooking.LabelCap, 300));
        }
        else
        {
            sb.Append(isDelay ? "OARK_UnlockedFile_DinnerFailDelay".Translate(pawn)
                              : "OARK_UnlockedFile_DinnerFail".Translate(pawn));

            pawn.skills?.Learn(SkillDefOf.Cooking, -300f);
            sb.AppendLine();
            sb.AppendInNewLine("OAFrame_PawnGainSkillXp".Translate(pawn, SkillDefOf.Cooking.LabelCap, -300));
        }
        Find.WindowStack.Add(OAFrame_DiaUtility.DefaultConfirmDiaNodeTree(sb.ToString()));
    }

    private static void Result_DeletedFile(Pawn pawn, bool isDelay)
    {
        pawn.needs?.mood?.thoughts?.memories?.TryGainMemory(OARK_ThoughtDefOf.OARK_UnlockedFile_DeletedFile);

        Find.WindowStack.Add(OAFrame_DiaUtility.DefaultConfirmDiaNodeTree(isDelay ? "OARK_UnlockedFile_DeletedFileDelay".Translate(pawn)
                                                                                  : "OARK_UnlockedFile_DeletedFile".Translate(pawn)));
    }

    private static void Result_Manuscript(Pawn pawn, bool isDelay)
    {

    }

    private static void Result_EncryptedFile(Pawn pawn)
    {
        Find.WindowStack.Add(OAFrame_DiaUtility.DefaultConfirmDiaNodeTree("OARK_UnlockedFile_EncryptedFile".Translate(pawn)));
        Map map = Find.AnyPlayerHomeMap;
        if (map is null)
        {
            Log.Error("OARK_UnlockedFile_EncryptedFile: No player home map found.");
            return;
        }

        Thing encryptedFile = ThingMaker.MakeThing(OARK_ThingDefOf.OARK_UnlockedFile_EncryptedFile);
        encryptedFile.TryGetComp<CompUseEffect_UnlockedFile_EncryptedFile>()?.InitEncryptedFile(pawn);
        IntVec3 dropCell = DropCellFinder.TradeDropSpot(map);
        DropPodUtility.DropThingsNear(dropCell, map, [encryptedFile], forbid: false, allowFogged: false, faction: ModUtility.OAFaction);
    }
}