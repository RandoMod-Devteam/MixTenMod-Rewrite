using Microsoft.Xna.Framework;
using MixTenMod.config;
using StardewModdingAPI;
using StardewValley;

namespace MixTenMod;
public class IncomeTracker
{
    private readonly IModHelper _helper;
    private readonly IMonitor _monitor;
    private readonly ModConfig _config;
    public int DailyIncome => _dailyIncome;

    private int _dailyIncome;

    public IncomeTracker(IModHelper helper, IMonitor monitor, ModConfig config)
    {
        _helper = helper;
        _monitor = monitor;
        _config = config;
    }

    public void ResetDailyIncome()
    {
        _dailyIncome = 0;
        if (_config.DebugMode)
        {
            _monitor.Log("每日收入已重置", LogLevel.Debug);
        }
    }
    
    public void AddToDailyIncome(int amount)
    {
        _dailyIncome += amount;
        if (_config.DebugMode)
        {
            _monitor.Log($"增加收入: {amount}金, 总计: {_dailyIncome}金", LogLevel.Debug);
        }
    }
    
    public void AfterHudRender()
    {
        if (!_config.EnableMod || !_config.ShowDailyIncome || !Context.IsWorldReady) 
            return;
            
        try
        {
            // 添加背景框增强可读性
            string incomeText = $"今日收入: {_dailyIncome}金";
            Vector2 textSize = Game1.dialogueFont.MeasureString(incomeText);
            
            // 绘制背景
            Game1.spriteBatch.Draw(
                Game1.staminaRect,
                new Rectangle(15, 15, (int)textSize.X + 20, (int)textSize.Y + 10),
                new Color(0, 0, 0, 150)
            );
            
            // 绘制文本
            Game1.spriteBatch.DrawString(
                Game1.dialogueFont,
                incomeText,
                new Vector2(20, 20),
                Color.Gold
            );
        }
        catch (Exception ex)
        {
            _monitor.Log($"绘制收入时出错: {ex.Message}", LogLevel.Debug);
        }
    }
}