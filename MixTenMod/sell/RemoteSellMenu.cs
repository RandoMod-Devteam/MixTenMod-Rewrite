using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MixTenMod.config;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Menus;

namespace MixTenMod.sell;

public class RemoteSellMenu : IClickableMenu
{
    private readonly IModHelper _helper;
    private readonly IMonitor _monitor;
    private readonly ModConfig _config;
    private readonly IncomeTracker _incomeTracker;

    private List<Item> playerItems;
    private List<ClickableComponent> itemComponents;
    private ClickableTextureComponent sellButton;

    private int totalValue = 0;

    public RemoteSellMenu(IModHelper helper, IMonitor monitor, ModConfig config, IncomeTracker incomeTracker)
    {
        _helper = helper;
        _monitor = monitor;
        _config = config;
        _incomeTracker = incomeTracker;

        // 设置菜单尺寸和位置
        width = 800;
        height = 600;
        xPositionOnScreen = (Game1.uiViewport.Width - width) / 2;
        yPositionOnScreen = (Game1.uiViewport.Height - height) / 2;

        // 获取玩家物品
        playerItems = Game1.player.Items.ToList();
        itemComponents = new List<ClickableComponent>();

        // 创建出售按钮
        sellButton = new ClickableTextureComponent(
            new Rectangle(xPositionOnScreen + width - 100, yPositionOnScreen + height - 60, 64, 64),
            Game1.mouseCursors,
            new Rectangle(128, 256, 64, 64),
            1f
        );

        // 计算总价值
        CalculateTotalValue();
    }

    private void CalculateTotalValue()
    {
        totalValue = 0;
        foreach (Item item in playerItems)
        {
            if (item != null && item.canBeShipped())
            {
                totalValue += item.sellToStorePrice() * item.Stack;
            }
        }
    }

    public override void draw(SpriteBatch b)
    {
        try
        {
            // 绘制背景
            drawTextureBox(b, xPositionOnScreen, yPositionOnScreen, width, height, Color.White);

            // 绘制标题
            string title = "远程出售";
            Vector2 titleSize = Game1.dialogueFont.MeasureString(title);
            b.DrawString(Game1.dialogueFont, title,
                new Vector2(xPositionOnScreen + (width - titleSize.X) / 2, yPositionOnScreen + 20), Game1.textColor);

            // 绘制总价值
            string valueText = $"总价值: {totalValue} 金";
            b.DrawString(Game1.smallFont, valueText, new Vector2(xPositionOnScreen + 20, yPositionOnScreen + 60),
                Game1.textColor);

            // 绘制物品列表
            int yOffset = 100;
            for (int i = 0; i < Math.Min(playerItems.Count, 24); i++) // 只显示前24个物品
            {
                int devision6 = i / 6;
                    // 绘制物品
                    playerItems[i].drawInMenu(b,
                        new Vector2(xPositionOnScreen + 20 + (i % 6) * 64, yPositionOnScreen + yOffset + (devision6) * 64),
                        1f);

                    // 绘制物品数量和价格
                    if (playerItems[i].Stack > 1)
                    {
                        b.DrawString(Game1.tinyFont, playerItems[i].Stack.ToString(),
                            new Vector2(xPositionOnScreen + 20 + (i % 6) * 64 + 4,
                                yPositionOnScreen + yOffset + (devision6) * 64 + 32), Color.White);
                    }

                    if (playerItems[i].canBeShipped())
                    {
                        int price = playerItems[i].sellToStorePrice();
                        b.DrawString(Game1.tinyFont, $"{price}金",
                            new Vector2(xPositionOnScreen + 20 + (i % 6) * 64,
                                yPositionOnScreen + yOffset + (devision6) * 64 + 44), Color.Gold);
                        
                }
            }

            // 绘制出售按钮
            sellButton.draw(b);
            b.DrawString(Game1.smallFont, "出售全部", new Vector2(sellButton.bounds.X - 50, sellButton.bounds.Y + 20),
                Game1.textColor);

            // 绘制鼠标
            drawMouse(b);
        }
        catch (Exception ex)
        {
            _monitor.Log($"绘制远程出售菜单时出错: {ex.Message}", LogLevel.Error);
        }
    }

    public override void receiveLeftClick(int x, int y, bool playSound = true)
    {
        try
        {
            // 检查是否点击了出售按钮
            if (sellButton.containsPoint(x, y))
            {
                SellAllItems();
                exitThisMenu();
                return;
            }

            base.receiveLeftClick(x, y, playSound);
        }
        catch (Exception ex)
        {
            _monitor.Log($"处理远程出售菜单点击时出错: {ex.Message}", LogLevel.Error);
        }
    }

    private void SellAllItems()
    {
        try
        {
            // 出售所有可出售的物品
            int soldValue = 0;
            List<Item> itemsToRemove = new List<Item>();

            foreach (Item item in playerItems)
            {
                if (item != null && item.canBeShipped())
                {
                    soldValue += item.sellToStorePrice() * item.Stack;
                    itemsToRemove.Add(item);
                }
            }

            // 移除已出售的物品
            foreach (Item item in itemsToRemove)
            {
                Game1.player.removeItemFromInventory(item);
            }
            _incomeTracker.AddToDailyIncome(soldValue);

            Game1.playSound("coin");
            _monitor.Log($"远程出售完成，获得 {soldValue} 金", LogLevel.Info);
        }
        catch (Exception ex)
        {
            _monitor.Log($"远程出售物品时出错: {ex.Message}", LogLevel.Error);
        }
    }
}