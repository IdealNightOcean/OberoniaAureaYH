using OberoniaAurea_Frame;
using RimWorld;
using RimWorld.Planet;
using System.Collections.Generic;
using Verse;

namespace OberoniaAurea;

public class ChoiceLetter_ExtraSiteGuest : ChoiceLetter
{
    private WorldObjectComp_ExtraGuest GuestComp { get; set; }
    private ExtraSiteGuest ExtraSiteGuest { get; set; }
    private Caravan Caravan { get; set; }

    public override IEnumerable<DiaOption> Choices
    {
        get
        {
            if (ExtraSiteGuest is null || !ExtraSiteGuest.IsActive)
            {
                yield return Option_Close;
                yield break;
            }

            if (ExtraSiteGuest.IsTraderActive && ExtraSiteGuest.SiteTrader is not null)
            {
                yield return Option_Trade();
            }
            yield return Option_Close;
        }

    }

    public void InitExtraSiteGuestLetter(WorldObjectComp_ExtraGuest guestComp, Caravan caravan)
    {
        this.GuestComp = guestComp;
        this.Caravan = caravan;
        InitExtraSiteGuestTest();
    }

    private DiaOption Option_Trade()
    {
        SiteTrader siteTrader = ExtraSiteGuest.SiteTrader;
        Pawn playerNegotiator = BestCaravanPawnUtility.FindBestNegotiator(Caravan, siteTrader.Faction, siteTrader.TraderKind);
        DiaOption option = new("OARK_ExtraSiteGuest_Trade".Translate(siteTrader.TraderKind.Named(KeyLibrary_NamedArgument.DEF)))
        {
            action = delegate
            {
                Find.WindowStack.Add(new Dialog_Trade(playerNegotiator, siteTrader));
                Find.LetterStack.RemoveLetter(this);
            },
            resolveTree = true
        };
        if (playerNegotiator is null || playerNegotiator.skills.GetSkill(SkillDefOf.Social).TotallyDisabled)
        {
            option.Disable("");
        }

        return option;
    }


    private string InitExtraSiteGuestTest()
    {
        if (GuestComp.ExtraSiteGuest is null || !GuestComp.ExtraSiteGuest.IsActive)
            return string.Empty;

        Faction guestFaction = GuestComp.ExtraSiteGuest.GuestFaction;
        switch (guestFaction.PlayerRelationKind)
        {
            case FactionRelationKind.Ally:
                {

                    break;
                }
            case FactionRelationKind.Neutral:
                {

                    break;
                }
            case FactionRelationKind.Hostile:
                {
                    break;
                }
            default: break;
        }

        throw new System.NotImplementedException();

    }



}