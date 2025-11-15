using MixTenMod.config;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;

namespace MixTenMod.sell
{
    public class ManualSellingManager
    {
        private readonly IModHelper _helper;
        private readonly IMonitor _monitor;
        private readonly ModConfig _config;
        private IncomeTracker _incomeTracker;
        
        private bool _menuOpen;
        
        public ManualSellingManager(IModHelper helper, IMonitor monitor, ModConfig config,IncomeTracker incomeTracker)
        {
            _helper = helper;
            _monitor = monitor;
            _config = config;
            _incomeTracker = incomeTracker;
        }
        
        public void SetIncomeTracker(IncomeTracker incomeTracker)
        {
            _incomeTracker = incomeTracker;
        }
        
        public void HandleInput(ButtonPressedEventArgs e)
        {
            _monitor.Log("ManualSellingManager.HandleInput called", LogLevel.Debug);
            _monitor.Log($"EnableMod: {_config.EnableMod}, Context.IsPlayerFree: {Context.IsPlayerFree}", LogLevel.Debug);
            if (!_config.EnableMod || !Context.IsPlayerFree)
                return;
            
            _monitor.Log($"ManualSellKey.JustPressed(): {_config.ManualSellKey.JustPressed()}", LogLevel.Debug);
            // 检查手动出售快捷键
            if (_config.ManualSellKey.JustPressed())
            {
                OpenSellMenu();
            }
        }
        
        private void OpenSellMenu()
        {
            if (_menuOpen) return;
            try
            {
                _monitor.Log("Opening sell menu", LogLevel.Debug);
                var sellMenu = new ManualSellMenu(_monitor, _incomeTracker, _config);
                Game1.activeClickableMenu = sellMenu;
                _menuOpen = true;
                Game1.playSound("bigSelect");
                
                // 注册关闭事件
                _helper.Events.Display.MenuChanged += OnMenuChanged;
                _monitor.Log("Sell menu opened successfully", LogLevel.Debug);
            }
            catch (Exception ex)
            {
                _monitor.Log($"打开出售菜单失败: {ex.Message}", LogLevel.Error);
                _monitor.Log(ex.StackTrace, LogLevel.Error);
            }
        }
        
        private void OnMenuChanged(object? sender, MenuChangedEventArgs e)
        {
            var oldMenuName = e.OldMenu?.GetType().Name ?? "null";
            var newMenuName = e.NewMenu?.GetType().Name ?? "null";
            _monitor.Log($"MenuChanged: OldMenu={oldMenuName}, NewMenu={newMenuName}", LogLevel.Info);
            if (e.NewMenu == null && _menuOpen)
            {
                _monitor.Log("Closing sell menu", LogLevel.Info);
                _menuOpen = false;
                _helper.Events.Display.MenuChanged -= OnMenuChanged;
                _monitor.Log("Sell menu closed", LogLevel.Info);
            }
        }
    }
}