using StardewModdingAPI;
using StardewValley;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MixTenMod.API;
using MixTenMod.i18n;
using MixTenMod.Features;

namespace MixTenMod.UI;

/// <summary>
/// 枪械GUI - 在左下角显示弹药和能量信息
/// </summary>
public class GunGUI
{
    private readonly IModHelper Helper;
    private readonly IGun Gun;
    private readonly RL11Feature? _rl11;

    // GUI位置和尺寸
    private const int GuiWidth = 120;
    private const int GuiHeight = 80;
    private const int Padding = 10;

    // 颜色
    private static readonly Color BackgroundColor = new(0, 0, 0, 180);
    private static readonly Color BorderColor = new(100, 100, 100, 255);
    private static readonly Color AmmoColor = new(255, 200, 50, 255);
    private static readonly Color EnergyColor = new(50, 200, 255, 255);
    private static readonly Color LowEnergyColor = new(255, 100, 100, 255);
    private static readonly Color ReloadingColor = new(255, 100, 100, 255);
    private static readonly Color TextColor = Color.White;

    public GunGUI(IModHelper helper, IGun gun)
    {
        Helper = helper;
        Gun = gun;
        _rl11 = gun as RL11Feature;
    }

    /// <summary>
    /// 绘制GUI
    /// </summary>
    public void Draw(SpriteBatch spriteBatch)
    {
        if (Game1.activeClickableMenu != null) return;

        // 计算位置（左下角）
        int x = Padding;
        int y = Game1.viewport.Height - GuiHeight - Padding;

        // 绘制背景
        DrawBackground(spriteBatch, x, y);

        // 绘制弹药信息
        DrawAmmoInfo(spriteBatch, x, y);

        // 绘制能量条
        if (_rl11 != null)
        {
            DrawEnergyBar(spriteBatch, x, y);
        }

        // 绘制装填进度
        if (Gun.IsReloading)
        {
            DrawReloadingIndicator(spriteBatch, x, y);
        }
    }

    /// <summary>
    /// 绘制背景
    /// </summary>
    private void DrawBackground(SpriteBatch spriteBatch, int x, int y)
    {
        // 背景矩形
        Rectangle bgRect = new(x, y, GuiWidth, GuiHeight);
        spriteBatch.Draw(
            Game1.fadeToBlackRect,
            bgRect,
            BackgroundColor
        );

        // 边框
        DrawBorder(spriteBatch, bgRect, BorderColor);
    }

    /// <summary>
    /// 绘制边框
    /// </summary>
    private void DrawBorder(SpriteBatch spriteBatch, Rectangle rect, Color color)
    {
        // 上边框
        spriteBatch.Draw(
            Game1.fadeToBlackRect,
            new Rectangle(rect.X, rect.Y, rect.Width, 2),
            color
        );

        // 下边框
        spriteBatch.Draw(
            Game1.fadeToBlackRect,
            new Rectangle(rect.X, rect.Y + rect.Height - 2, rect.Width, 2),
            color
        );

        // 左边框
        spriteBatch.Draw(
            Game1.fadeToBlackRect,
            new Rectangle(rect.X, rect.Y, 2, rect.Height),
            color
        );

        // 右边框
        spriteBatch.Draw(
            Game1.fadeToBlackRect,
            new Rectangle(rect.X + rect.Width - 2, rect.Y, 2, rect.Height),
            color
        );
    }

    /// <summary>
    /// 绘制弹药信息
    /// </summary>
    private void DrawAmmoInfo(SpriteBatch spriteBatch, int x, int y)
    {
        string ammoText = $"{Gun.CurrentAmmo}/{Gun.MagazineCapacity}";

        // 计算文本位置（居中）
        Vector2 textSize = Game1.smallFont.MeasureString(ammoText);
        Vector2 textPos = new(
            x + (GuiWidth - textSize.X) / 2,
            y + 25
        );

        // 绘制弹药数量
        Color ammoDisplayColor = Gun.IsReloading ? ReloadingColor : AmmoColor;
        spriteBatch.DrawString(
            Game1.smallFont,
            ammoText,
            textPos,
            ammoDisplayColor
        );

        // 绘制枪械名称
        string gunName = I18n.Tr("gui.gun.name");
        Vector2 nameSize = Game1.tinyFont.MeasureString(gunName);
        Vector2 namePos = new(
            x + (GuiWidth - nameSize.X) / 2,
            y + 5
        );

        spriteBatch.DrawString(
            Game1.tinyFont,
            gunName,
            namePos,
            TextColor
        );

        // 绘制弹药类型（如果有）
        if (Gun.CurrentAmmoType != null)
        {
            string ammoType = Gun.CurrentAmmoType.AmmoName;
            Vector2 typeSize = Game1.tinyFont.MeasureString(ammoType);
            Vector2 typePos = new(
                x + (GuiWidth - typeSize.X) / 2,
                y + GuiHeight - typeSize.Y - 5
            );

            spriteBatch.DrawString(
                Game1.tinyFont,
                ammoType,
                typePos,
                GetAmmoTypeColor(Gun.CurrentAmmoType.SpecialEffect)
            );
        }
    }

    /// <summary>
    /// 绘制能量条
    /// </summary>
    private void DrawEnergyBar(SpriteBatch spriteBatch, int x, int y)
    {
        if (_rl11 == null) return;

        int barWidth = GuiWidth - 20;
        int barHeight = 6;
        int barX = x + 10;
        int barY = y + 45;

        // 计算能量百分比
        float energyPercent = (float)_rl11.CurrentEnergy / _rl11.MaxEnergy;
        int fillWidth = (int)(barWidth * energyPercent);

        // 绘制背景条
        spriteBatch.Draw(
            Game1.fadeToBlackRect,
            new Rectangle(barX, barY, barWidth, barHeight),
            new Color(50, 50, 50, 200)
        );

        // 绘制能量填充
        Color energyFillColor = energyPercent > 0.3f ? EnergyColor : LowEnergyColor;
        if (fillWidth > 0)
        {
            spriteBatch.Draw(
                Game1.fadeToBlackRect,
                new Rectangle(barX, barY, fillWidth, barHeight),
                energyFillColor
            );
        }

        // 绘制能量数值
        string energyText = $"{_rl11.CurrentEnergy}/{_rl11.MaxEnergy}";
        Vector2 textSize = Game1.tinyFont.MeasureString(energyText);
        Vector2 textPos = new(
            barX + (barWidth - textSize.X) / 2,
            barY - textSize.Y - 2
        );

        spriteBatch.DrawString(
            Game1.tinyFont,
            energyText,
            textPos,
            energyFillColor
        );
    }

    /// <summary>
    /// 绘制装填指示器
    /// </summary>
    private void DrawReloadingIndicator(SpriteBatch spriteBatch, int x, int y)
    {
        string reloadText = I18n.Tr("gui.gun.reloading");
        Vector2 textSize = Game1.smallFont.MeasureString(reloadText);

        // 绘制在GUI上方
        Vector2 textPos = new(
            x + (GuiWidth - textSize.X) / 2,
            y - textSize.Y - 5
        );

        // 绘制文字阴影
        spriteBatch.DrawString(
            Game1.smallFont,
            reloadText,
            textPos + new Vector2(1, 1),
            Color.Black
        );

        spriteBatch.DrawString(
            Game1.smallFont,
            reloadText,
            textPos,
            ReloadingColor
        );
    }

    /// <summary>
    /// 获取弹药类型颜色
    /// </summary>
    private Color GetAmmoTypeColor(AmmoSpecialEffect effect)
    {
        return effect switch
        {
            AmmoSpecialEffect.Fire => Color.OrangeRed,
            AmmoSpecialEffect.Ice => Color.Cyan,
            AmmoSpecialEffect.Electric => Color.Yellow,
            AmmoSpecialEffect.Poison => Color.Green,
            AmmoSpecialEffect.Explosive => Color.Red,
            AmmoSpecialEffect.Pierce => Color.Gray,
            _ => TextColor
        };
    }
}
