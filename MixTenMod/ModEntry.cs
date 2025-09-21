using MixTenMod.config;
using StardewModdingAPI;
namespace MixTenMod;
public class ModEntry : Mod
{
    private RequestCenter _requestCenter = null!;
    public override void Entry(IModHelper helper)
    {
        ModConfig config = helper.ReadConfig<ModConfig>();
        // 初始化管理器实例
        var incomeTracker = new IncomeTracker(Monitor, config);
        var priceDisplayManager = new PriceDisplayManager(Monitor, config);
        var toolManager = new ToolManager(Monitor, config);
        IManifest manifest = null!;

        // 创建 RequestCenter 并传入依赖项
        _requestCenter = new RequestCenter(helper, Monitor, config,incomeTracker, priceDisplayManager, toolManager,manifest); 
        Helper.Events.GameLoop.GameLaunched += _requestCenter.OnGameLaunched;
        Helper.Events.GameLoop.DayStarted += _requestCenter.OnDayStarted;
        Helper.Events.Input.ButtonPressed += _requestCenter.OnButtonPressed;
        Helper.Events.Display.RenderingHud += _requestCenter.OnRenderingHud;
        Helper.Events.Display.RenderedHud += _requestCenter.OnRenderedHud;
        Console.WriteLine("MixTenMod is Write by ZZH!");
    }
}