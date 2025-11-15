using StardewModdingAPI.Utilities;

namespace MixTenMod.config
{
    public class ModConfig
    {
        // 模组总开关
        public bool EnableMod { get; set; } = true;
        
        // 是否显示每日收入
        public bool ShowDailyIncome { get; set; } = true;
        
        // 是否显示物品价格
        public bool ShowPrices { get; set; } = true;
        
        // 调试模式开关
        public bool DebugMode { get; set; } = false;

        // 工具切换快捷键
        public KeybindList ToolSwitchKey { get; set; } = KeybindList.Parse("LeftControl+N");
        
        // 手动出售快捷键
        public KeybindList ManualSellKey { get; set; } = KeybindList.Parse("LeftControl+M");
    }
}