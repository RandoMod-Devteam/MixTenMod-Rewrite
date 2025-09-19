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
            

        // 创建 RequestCenter 并传入依赖项
        _requestCenter = new RequestCenter(helper, Monitor, config,incomeTracker, priceDisplayManager, toolManager);
        helper.Events.GameLoop.GameLaunched += _requestCenter.OnGameLaunched;
        helper.Events.GameLoop.DayStarted += _requestCenter.OnDayStarted;
        helper.Events.Input.ButtonPressed += _requestCenter.OnButtonPressed;
        helper.Events.Display.RenderingHud += _requestCenter.OnRenderingHud;
        helper.Events.Display.RenderedHud += _requestCenter.OnRenderedHud;
        helper.Events.Input.ButtonPressed += _requestCenter.OnButtonPressed;
        Console.WriteLine("MixTenMod is Write by ZZH!");
    }
}