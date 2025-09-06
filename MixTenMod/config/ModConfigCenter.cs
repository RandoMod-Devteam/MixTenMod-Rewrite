using GenericModConfigMenu;
using StardewModdingAPI;

namespace MixTenMod.config
{
    public class ModConfigCenter
    {
        private readonly IModHelper _helper;
        private ModConfig _config;
        private readonly IGenericModConfigMenuApi _api;
        readonly IManifest _manifest = null!;

        public ModConfigCenter(IModHelper helper, ModConfig config, IGenericModConfigMenuApi api)
        {
            _helper = helper;
            _config = config;
            _api = api;
        }

        public void SetupConfigMenu()
        {        
            // 注册配置菜单
            _api.Register(
                mod: _manifest,
                reset: () => _config = new ModConfig(),
                save: () => _helper.WriteConfig(_config)
            );

            // 添加配置选项
            _api.AddBoolOption(
                mod: _manifest,
                name: () => "启用模组",
                tooltip: () => "总开关",
                getValue: () => _config.EnableMod,
                setValue: value => _config.EnableMod = value
            );

            _api.AddBoolOption(
                mod: _manifest,
                name: () => "显示每日收入",
                tooltip: () => "在屏幕左上角显示当天收入",
                getValue: () => _config.ShowDailyIncome,
                setValue: value => _config.ShowDailyIncome = value
            );

            _api.AddBoolOption(
                mod: _manifest,
                name: () => "显示物品价格",
                tooltip: () => "鼠标悬停时显示物品售价",
                getValue: () => _config.ShowPrices,
                setValue: value => _config.ShowPrices = value
            );

            _api.AddKeybindList(
                mod: _manifest,
                name: () => "工具切换键",
                tooltip: () => "循环切换工具",
                getValue: () => _config.ToolSwitchKey,
                setValue: value => _config.ToolSwitchKey = value
            );

            // 添加其他配置项...
        }
    }
}