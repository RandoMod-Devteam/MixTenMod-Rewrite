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
        private const int Width = 800;
        private const int Height = 600;

        protected Item? _heldItem;
        private readonly string _title;
        protected readonly InventoryMenu _inventoryMenu;
        
        public BaseSellMenu(string title)
            : base(
                (Game1.viewport.Width - Width) / 2,
                (Game1.viewport.Height - Height) / 2,
                Width,
                Height
            )
        {
            _title = title;
            _heldItem = null;
            
            _inventoryMenu = new InventoryMenu(
                xPositionOnScreen + Spacing,
                yPositionOnScreen + 160,
                true,
                Game1.player.Items
            );
        }
        
        public override void draw(SpriteBatch b)
        {
            // 调用基类draw方法
            base.draw(b);
            
            // 绘制半透明背景
            b.Draw(Game1.fadeToBlackRect, new Rectangle(0, 0, Game1.viewport.Width, Game1.viewport.Height), Color.Black * 0.5f);
            
            // 绘制菜单框
            drawTextureBox(b, xPositionOnScreen, yPositionOnScreen, Width, Height, Color.White);
            
            // 绘制标题
            string title = _title;
            Vector2 titleSize = Game1.dialogueFont.MeasureString(title);
            Utility.drawTextWithShadow(b, title, Game1.dialogueFont, 
                new Vector2(xPositionOnScreen + (Width - titleSize.X) / 2, yPositionOnScreen + Spacing), 
                Color.Gold);
            
            // 绘制库存
            _inventoryMenu.draw(b);
            var itemSize2 = ItemSize / 2;
            // 绘制手持物品
            if (_heldItem != null)
            {
                _heldItem.drawInMenu(b, 
                    new Vector2(Game1.getMouseX() - itemSize2, Game1.getMouseY() - itemSize2), 
                    1f);
            }
            
            // 绘制帮助文本
            string helpText = "左键点击物品出售 | 右键取消";
            Utility.drawTextWithShadow(b, helpText, Game1.smallFont, 
                new Vector2(xPositionOnScreen + Spacing, yPositionOnScreen + Height - 40), 
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
            _heldItem = null!;
        }
        
        protected abstract void HandleLeftClick(int x, int y);
    }
}