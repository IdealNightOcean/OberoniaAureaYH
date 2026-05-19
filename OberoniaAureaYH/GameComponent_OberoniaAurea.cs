using OberoniaAurea_Frame;
using RimWorld;
using UnityEngine;
using Verse;

namespace OberoniaAurea;

public class GameComponent_OberoniaAurea : GameComponent
{
    private readonly int interactHashOffset;
    public static GameComponent_OberoniaAurea Instance { get; private set; }

    private CooldownRecordManager cooldownManager;
    public CooldownRecordManager CooldownManager => cooldownManager;

    private OAInteractHandler interactHandler;
    private ScienceDepartmentInteractHandler sdInteractHandler;
    private SpecialGlobalEventManager specialGlobalEventManager;

    public GameComponent_OberoniaAurea(Game game)
    {
        if (Instance != this)
        {
            Log.Message($"[OARK] {nameof(GameComponent_OberoniaAurea)} 实例已正确切换。".Colorize(Color.cyan));
        }
        Instance = this;
        interactHashOffset = Rand.Range(0, int.MaxValue).HashOffset();
    }

    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_Deep.Look(ref cooldownManager, nameof(cooldownManager));

        Scribe_Deep.Look(ref interactHandler, nameof(interactHandler));
        Scribe_Deep.Look(ref sdInteractHandler, nameof(sdInteractHandler));
        Scribe_Deep.Look(ref specialGlobalEventManager, nameof(specialGlobalEventManager));
    }

    public override void StartedNewGame() => GameStart();
    public override void LoadedGame() => GameStart();

    public override void GameComponentTick()
    {
        if (ModUtility.IsHashIntervalTick(interactHashOffset, 60000))
        {
            interactHandler.TickDay();
            if (ModsConfig.OdysseyActive)
            {
                sdInteractHandler.TickDay();
            }
        }

        specialGlobalEventManager.Tick();
    }

    private void GameStart()
    {
        EnsureComponentsInit();

        ModUtility.Notify_GameStart();
        specialGlobalEventManager.Notify_GameStart();

        if (!ModsConfig.IsActive("OARK.OberoniaAurea.Framework") && !ModsConfig.IsActive("OARK.OberoniaAurea.Framework_Steam"))
        {
            Find.LetterStack.ReceiveLetter(
                label: "OARO_LetterLabel_FrameMiss".Translate(),
                text: "OARO_LetterText_FrameMiss".Translate(),
                textLetterDef: LetterDefOf.NegativeEvent,
                delayTicks: 300);
        }
    }

    private void EnsureComponentsInit()
    {
        cooldownManager ??= new();
        try
        {
            interactHandler ??= new OAInteractHandler();
        }
        catch (System.Exception ex)
        {
            Log.Error($"[OARK] 在 {nameof(GameComponent_OberoniaAurea)}.{nameof(EnsureComponentsInit)} 中初始化 {nameof(OAInteractHandler)} 时发生异常\n{ex}");
            OAInteractHandler.ClearStaticCache();
            interactHandler = new OAInteractHandler();
        }

        try
        {
            specialGlobalEventManager ??= new SpecialGlobalEventManager();
        }
        catch (System.Exception ex)
        {
            Log.Error($"[OARK] 在 {nameof(GameComponent_OberoniaAurea)}.{nameof(EnsureComponentsInit)} 中初始化 {nameof(SpecialGlobalEventManager)} 时发生异常：\n{ex}");
            SpecialGlobalEventManager.ClearStaticCache();
            specialGlobalEventManager = new SpecialGlobalEventManager();
        }

        if (ModsConfig.OdysseyActive)
        {
            try
            {
                sdInteractHandler ??= new ScienceDepartmentInteractHandler();
            }
            catch (System.Exception ex)
            {
                Log.Error($"[OARK] 在 {nameof(GameComponent_OberoniaAurea)}.{nameof(GameStart)} 中初始化 {nameof(ScienceDepartmentInteractHandler)} 时发生异常：\n{ex}");
                ScienceDepartmentInteractHandler.ClearStaticCache();
                sdInteractHandler = new ScienceDepartmentInteractHandler();
            }
        }
    }
}