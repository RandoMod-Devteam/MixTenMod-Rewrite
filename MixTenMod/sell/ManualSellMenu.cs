using MixTenMod.config;
using StardewModdingAPI;
using StardewValley;

namespace MixTenMod.sell
{
    public class ManualSellMenu : BaseSellMenu
    {
        private readonly ModConfig _config;
        private readonly IMonitor _monitor;
        private readonly IncomeTracker _incomeTracker;
        
        public ManualSellMenu(
            IMonitor monitor, 
            ModConfig config,
            IncomeTracker incomeTracker) 
            : base("手动出售物品")
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
                        // 第一次点击：拿起物品
                        if (HeldItem == null!)
                        {
                            HeldItem = item;
                            Game1.playSound("pickUpItem");
                        }
                        // 第二次点击：出售物品
                        else
                        {
                            SellItem(item);
                            HeldItem = null!;
                        }
                    }
                }
            }
        }
        
        private void SellItem(Item item)
        {
            if (item == null! || !item.canBeShipped()) return;
            
            int price = item.sellToStorePrice();
            
            // 从玩家库存移除物品
            item.Stack--;
            if (item.Stack <= 0)
            {
                Game1.player.removeItemFromInventory(item);
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