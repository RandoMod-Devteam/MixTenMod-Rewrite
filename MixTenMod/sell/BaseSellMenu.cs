using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.Menus;

namespace MixTenMod.sell
{
    public abstract class BaseSellMenu : IClickableMenu
    {
        private const int ItemSize = 64;
        private const int Spacing = 16;
        private InventoryMenu _inventoryMenu;
        
        protected InventoryMenu InventoryMenu;
        protected Item HeldItem;
        private readonly string _title;
        private readonly InventoryMenu.highlightThisItem _highlightMethod = null!;
        
        protected BaseSellMenu(string title)
        {
            _title = title;
            int xPosition = (Game1.viewport.Width - width) / 2;
            int yPosition = (Game1.viewport.Height - height) / 2;
            
            initialize(xPosition, yPosition, width, height);
            
            SetupInventory();
        }
        
        private void SetupInventory()
        {
            InventoryMenu = new InventoryMenu(
                xPositionOnScreen + Spacing,
                yPositionOnScreen + 160,
                false,
                Game1.player.Items,
                _highlightMethod,
                -1,
                ItemSize
            );
        }
        
        public override void draw(SpriteBatch b)
        {
            // 绘制半透明背景
            b.Draw(Game1.fadeToBlackRect, new Rectangle(0, 0, Game1.viewport.Width, Game1.viewport.Height), Color.Black * 0.5f);
            
            // 绘制菜单框
            drawTextureBox(b, xPositionOnScreen, yPositionOnScreen, width, height, Color.White);
            
            // 绘制标题
            string title = _title;
            Vector2 titleSize = Game1.dialogueFont.MeasureString(title);
            Utility.drawTextWithShadow(b, title, Game1.dialogueFont, 
                new Vector2(xPositionOnScreen + (width - titleSize.X) / 2, yPositionOnScreen + Spacing), 
                Color.Gold);
            
            // 绘制库存
            InventoryMenu.draw(b);
            var itemSize2 = ItemSize / 2;
            // 绘制手持物品
            HeldItem.drawInMenu(b, 
                new Vector2(Game1.getMouseX() - itemSize2, Game1.getMouseY() - itemSize2), 
                1f);
            
            // 绘制帮助文本
            string helpText = "左键点击物品出售 | 右键取消";
            Utility.drawTextWithShadow(b, helpText, Game1.smallFont, 
                new Vector2(xPositionOnScreen + Spacing, yPositionOnScreen + height - 40), 
                Color.LightGray);
            
            // 绘制鼠标
            drawMouse(b);
        }
        
        public override void receiveLeftClick(int x, int y, bool playSound = true)
        {
            base.receiveLeftClick(x, y, playSound);
            HandleLeftClick(x, y);
        }
        
        public override void receiveRightClick(int x, int y, bool playSound = true)
        {
            base.receiveRightClick(x, y, playSound);
            HeldItem = null!;
        }
        
        protected abstract void HandleLeftClick(int x, int y);
    }
}