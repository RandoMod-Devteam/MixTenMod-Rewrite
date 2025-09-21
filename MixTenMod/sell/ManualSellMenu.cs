using MixTenMod.config;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Menus;

namespace MixTenMod.sell
{
    public class ManualSellMenu : BaseSellMenu
    {
        private readonly IncomeTracker _incomeTracker;
        private readonly IMonitor _monitor;
        private ModConfig _config;
        private static InventoryMenu _inventoryMenu = null!;
        private static Item _heldItem = null!;
        public ManualSellMenu(
            IMonitor monitor, 
            ModConfig config,
            IncomeTracker incomeTracker) 
            : base("手动出售物品",_inventoryMenu,_heldItem)
        {
            _monitor = monitor;
            _config = config;
            _incomeTracker = incomeTracker;
        }
        
        protected override void HandleLeftClick(int x, int y)
        {
            // 处理库存点击
            if (InventoryMenu.isWithinBounds(x, y))
            {
                int slot = InventoryMenu.getInventoryPositionOfClick(x, y);
                if (slot >= 0 && slot < Game1.player.Items.Count)
                {
                    Item item = Game1.player.Items[slot];
                    if (item != null)
                    {
                            HeldItem = item;
                            Game1.playSound("pickUpItem");
                            SellItem(item);
                            HeldItem = null!;
                    }
                }
            }
        }
        
        private void SellItem(Item item)
        {
            if (!item.canBeShipped()) return;
            
            int price = item.sellToStorePrice();
            
            // 从玩家库存移除物品
            item.Stack--;
            if (item.Stack <= 0)
            {
                Game1.player.Items[Game1.player.getIndexOfInventoryItem(item)] = null;
            }
            
            // 添加收入
            Game1.player.Money += price;
            _incomeTracker.AddToDailyIncome(price);
            
            // 播放音效
            Game1.playSound("coin");
            
            // 调试日志
            if (_config.DebugMode)
            {
                _monitor.Log($"手动出售: {item.Name} x1 获得 {price}金", LogLevel.Debug);
            }
        }
    }
}