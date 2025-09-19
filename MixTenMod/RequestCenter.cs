using GenericModConfigMenu;
using MixTenMod.config;
using MixTenMod.sell;
using StardewModdingAPI;
using StardewModdingAPI.Events;

namespace MixTenMod;

public class RequestCenter
{
    private static readonly IModHelper Helper = null!;
    private static readonly ModConfig StaticConfig = new ModConfig();
    private readonly ManualSellingManager _sellingManager;
    private readonly PriceDisplayManager _priceDisplayManager;
    private readonly IncomeTracker _incomeTracker;
    private readonly ToolManager _toolManager;
    private static readonly IGenericModConfigMenuApi Api = null!;
    public RequestCenter(
        IModHelper helper, 
        IMonitor monitor, 
        ModConfig config,
        IncomeTracker incomeTracker,
        PriceDisplayManager priceDisplayManager,
        ToolManager toolManager
        )
    {
        _incomeTracker = incomeTracker;
        _priceDisplayManager = priceDisplayManager;
        _toolManager = toolManager;
        // 初始化 ManualSellingManager (需补全其实现)
        _sellingManager = new ManualSellingManager(helper, monitor, config,_incomeTracker);
        _sellingManager.SetIncomeTracker(_incomeTracker);
    }
    public void OnGameLaunched(object? sender, GameLaunchedEventArgs e)
    {
        ModConfigCenter configCentre = new ModConfigCenter(Helper,StaticConfig,Api); 
        configCentre.SetupConfigMenu();
    }

    public void OnDayStarted(object? sender, DayStartedEventArgs e)
    {
        _incomeTracker.ResetDailyIncome();
    }

    public void OnButtonPressed(object? sender, ButtonPressedEventArgs e)
    {
        if (!Context.IsWorldReady) return;
        
        _toolManager.HandleInput(e);
        _sellingManager.HandleInput(e);
    }

    public void OnRenderingHud(object? sender, RenderingHudEventArgs e)
    {
        _priceDisplayManager.Update();
    }

    public void OnRenderedHud(object? sender, RenderedHudEventArgs e)
    {
        _incomeTracker.AfterHudRender();
        _priceDisplayManager.AfterHudRender();
    }
}