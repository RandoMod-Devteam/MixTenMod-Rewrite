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
            if (!_config.EnableMod || !Context.IsPlayerFree)
                return;
            
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
                
                var sellMenu = new ManualSellMenu(_monitor, _config, _incomeTracker);
                Game1.activeClickableMenu = sellMenu;
                _menuOpen = true;
                
                // 注册关闭事件
                _helper.Events.Display.MenuChanged += OnMenuChanged;
            }
            catch (Exception ex)
            {
                _monitor.Log($"打开出售菜单失败: {ex.Message}", LogLevel.Error);
            }
        }
        
        private void OnMenuChanged(object sender, MenuChangedEventArgs e)
        {
            if (e.NewMenu == null && _menuOpen)
            {
                _menuOpen = false;
            }
        }
    }
}