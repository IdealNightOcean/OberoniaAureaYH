using OberoniaAurea_Frame;
using RimWorld;
using System;
using Verse;

namespace OberoniaAurea;

public class GameComponent_OberoniaAurea : GameComponent
{
    public static GameComponent_OberoniaAurea Instance { get; private set; }

    private OAInteractHandler interactHandler;

    protected int ticksRemaining;
    public int initAllianceTick = -1; //本次结盟的起始时刻
    public float oldAllianceDuration; //记录旧结盟时间
    public int assistPoints;

    public bool newYearEventTriggeredOnce;

    public GameComponent_OberoniaAurea(Game game) => Instance = this;

    public override void StartedNewGame()
    {
        GameStart();
    }

    public override void LoadedGame()
    {
        GameStart();
    }

    public override void GameComponentTick()
    {
        if (--ticksRemaining < 0)
        {
            ticksRemaining = 60000;
            interactHandler.TickDay();
        }
    }

    private void GameStart()
    {
        interactHandler ??= new OAInteractHandler();
        ModUtility.Notify_GameStart();
        TryAddNewYearEnent();
    }

    private void TryAddNewYearEnent()
    {
        if (!newYearEventTriggeredOnce)
        {
            DateTime date = DateTime.Now;
            if (date.Month == 1 && date.Day <= 3)
            {
                Map map = Find.AnyPlayerHomeMap;
                if (map is not null && ModUtility.OAFaction is not null)
                {
                    if (ModUtility.OAFaction.PlayerRelationKind == FactionRelationKind.Ally)
                    {
                        IncidentParms parms = new()
                        {
                            target = map,
                            faction = ModUtility.OAFaction
                        };
                        OAFrame_MiscUtility.AddNewQueuedIncident(OARK_ModDefOf.OARatkin_NewYearEvent, 2500, parms);
                    }
                }
            }
        }
    }

    public override void ExposeData()
    {
        base.ExposeData();

        Scribe_Values.Look(ref ticksRemaining, "ticksRemaining", -1);
        Scribe_Deep.Look(ref interactHandler, "interactHandler");

        Scribe_Values.Look(ref oldAllianceDuration, "oldAllianceDuration", 0f);
        Scribe_Values.Look(ref initAllianceTick, "initAllianceTick", -1);
        Scribe_Values.Look(ref assistPoints, "assistPoints", 0);

        Scribe_Values.Look(ref newYearEventTriggeredOnce, "newYearEventTriggeredOnce", defaultValue: false);
    }
}
