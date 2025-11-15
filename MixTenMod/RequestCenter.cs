using GenericModConfigMenu;
using MixTenMod.config;
using MixTenMod.sell;
using StardewModdingAPI;
using StardewModdingAPI.Events;

namespace MixTenMod;

public class RequestCenter
{
    private readonly IModHelper _helper;
    private readonly IMonitor _monitor;
    private readonly ModConfig _config;
    private readonly ManualSellingManager _sellingManager;
    private readonly PriceDisplayManager _priceDisplayManager;
    private readonly IncomeTracker _incomeTracker;
    private readonly ToolManager _toolManager;
        private IGenericModConfigMenuApi? _api;
        readonly IManifest _manifest;
        public RequestCenter(
            IModHelper helper, 
            IMonitor monitor, 
            ModConfig config, 
            IncomeTracker incomeTracker, 
            PriceDisplayManager priceDisplayManager, 
            ToolManager toolManager, 
            IManifest manifest
            )
        {
            _helper = helper;
            _monitor = monitor;
            _config = config;
            _incomeTracker = incomeTracker;
            _priceDisplayManager = priceDisplayManager;
            _toolManager = toolManager;
            // 初始化 ManualSellingManager (需补全其实现)
            _sellingManager = new ManualSellingManager(helper, monitor, config, _incomeTracker);
            _sellingManager.SetIncomeTracker(_incomeTracker);
            _manifest = manifest;
        }
    public void OnGameLaunched(object? sender, GameLaunchedEventArgs e)
    { 
        // 获取GenericModConfigMenu API
        _api = _helper.ModRegistry.GetApi<IGenericModConfigMenuApi>("spacechase0.GenericModConfigMenu");
        ModConfigCenter configCentre = new ModConfigCenter(_helper, _config, _api, _manifest); 
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