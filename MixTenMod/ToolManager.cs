using MixTenMod.config;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Tools;

namespace MixTenMod;
public class ToolManager
{
    private readonly IMonitor _monitor;
    private readonly ModConfig _config;  // 改为public以便外部访问
    private readonly List<Type> _toolOrder = new List<Type>
    {
        typeof(Pickaxe),
        typeof(Axe),
        typeof(Hoe),
        typeof(WateringCan),
        typeof(FishingRod)
    };

    public ToolManager(IMonitor monitor, ModConfig config)
    {
        _monitor = monitor;
        _config = config;
    }

    public void HandleInput(ButtonPressedEventArgs e)
    {
        if (!_config.EnableMod) return;
        
        try
        {
            if (_config.ToolSwitchKey.JustPressed())
            {
                SwitchToNextTool();
            }
        }
        catch (Exception ex)
        {
            _monitor.Log($"处理工具切换输入时出错: {ex.Message}", LogLevel.Error);
        }
    }
    private void SwitchToNextTool()
    {
        try
        {
            // 获取当前工具类型索引
            int currentTypeIndex = -1;
            if (Game1.player.CurrentTool != null)
            {
                currentTypeIndex = _toolOrder.IndexOf(Game1.player.CurrentTool.GetType());
            }

            // 查找下一个工具类型
            int nextIndex = (currentTypeIndex + 1) % _toolOrder.Count;
            Type nextType = _toolOrder[nextIndex];

            // 在物品栏中查找该类型工具
            for (int i = 0; i < Game1.player.Items.Count; i++)
            {
                if (Game1.player.Items[i] is Tool tool && tool.GetType() == nextType)
                {
                    Game1.player.CurrentToolIndex = i;
                    Game1.playSound("toolSwap");
                    return;
                }
            }
        }
        catch (Exception ex)
        {
            _monitor.Log($"工具切换失败: {ex.Message}", LogLevel.Error);
        }
    }
}