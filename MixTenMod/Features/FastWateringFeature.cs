using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.TerrainFeatures;
using StardewValley.Tools;
using Microsoft.Xna.Framework;
using MixTenMod.Config;

namespace MixTenMod.Features;

public class FastWateringFeature
{
    private readonly IModHelper Helper;
    private readonly ModConfig Config;
    private readonly DehydratedAmmoFeature? _dehydratedAmmoFeature;

    // 静态属性，用于检查是否应该翻倍浇水时间
    public static bool ShouldDoubleWateringTime { get; set; } = false;

    public FastWateringFeature(IModHelper helper, ModConfig config, DehydratedAmmoFeature? dehydratedAmmoFeature = null)
    {
        Helper = helper;
        Config = config;
        _dehydratedAmmoFeature = dehydratedAmmoFeature;

        helper.Events.Input.ButtonPressed += OnButtonPressed;
        helper.Events.GameLoop.UpdateTicked += OnUpdateTicked;
    }

    /// <summary>
    /// 更新逻辑 - 检查失水弹状态
    /// </summary>
    private void OnUpdateTicked(object? sender, UpdateTickedEventArgs e)
    {
        // 更新静态属性，让其他系统可以检查
        if (_dehydratedAmmoFeature != null)
        {
            ShouldDoubleWateringTime = _dehydratedAmmoFeature.ShouldDoubleWateringTime();
        }
    }

    private void OnButtonPressed(object? sender, ButtonPressedEventArgs e)
    {
        if (!Config.EnableFastWatering)
            return;

        if (!e.Button.IsActionButton())
            return;

        if (Game1.player.CurrentTool is not WateringCan)
            return;

        Vector2 tile = e.Cursor.Tile;
        WaterSurroundingTiles(tile);
    }

    private void WaterSurroundingTiles(Vector2 centerTile)
    {
        int range = Config.WateringRange;
        var location = Game1.player.currentLocation;

        for (int x = -range; x <= range; x++)
        {
            for (int y = -range; y <= range; y++)
            {
                Vector2 tile = new(centerTile.X + x, centerTile.Y + y);
                
                if (location.terrainFeatures.TryGetValue(tile, out var feature) && feature is HoeDirt dirt)
                {
                    if (dirt.state.Value != HoeDirt.watered)
                    {
                        dirt.state.Value = HoeDirt.watered;
                        
                        // 播放浇水音效
                        if (x == 0 && y == 0)
                        {
                            Game1.playSound("wateringCan");
                        }
                    }
                }
            }
        }
    }
}
