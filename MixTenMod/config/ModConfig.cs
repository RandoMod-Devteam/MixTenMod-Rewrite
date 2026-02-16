using GenericModConfigMenu;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using System.IO;
using MixTenMod.i18n;

namespace MixTenMod.Config;

/// <summary>
/// 模组配置类
/// </summary>
public class ModConfig
{
    private IModHelper? _helper;
    private IManifest? _manifest;

    /// <summary>
    /// 启用快速浇水功能
    /// </summary>
    public bool EnableFastWatering { get; set; } = true;

    /// <summary>
    /// 浇水范围半径（1 = 3x3区域）
    /// </summary>
    public int WateringRange { get; set; } = 1;

    /// <summary>
    /// 重置配置为默认值
    /// </summary>
    public void Reset()
    {
        EnableFastWatering = true;
        WateringRange = 1;
        Write();
    }

    /// <summary>
    /// 加载配置并设置 GMCM 菜单
    /// </summary>
    /// <param name="helper">SMAPI 助手实例</param>
    /// <param name="manifest">模组清单</param>
    public void Load(IModHelper helper, IManifest manifest)
    {
        _helper = helper;
        _manifest = manifest;
        
        // 读取配置文件
        LoadFromFile();

        // 延迟设置 GMCM 菜单，等到游戏启动完成
        helper.Events.GameLoop.GameLaunched += OnGameLaunched;
    }

    /// <summary>
    /// 游戏启动完成后设置 GMCM
    /// </summary>
    private void OnGameLaunched(object? sender, GameLaunchedEventArgs e)
    {
        if (_helper is null || _manifest is null) return;
        
        SetupGenericModConfigMenu(_helper, _manifest);
    }

    /// <summary>
    /// 从配置文件读取配置
    /// </summary>
    private void LoadFromFile()
    {
        string configPath = GetConfigPath();
        
        if (!File.Exists(configPath))
        {
            // 如果文件不存在，使用默认配置并创建文件
            Write();
            return;
        }

        try
        {
            var lines = File.ReadAllLines(configPath);
            
            foreach (var line in lines)
            {
                var parts = line.Split('=');
                if (parts.Length != 2)
                    continue;

                var key = parts[0].Trim();
                var value = parts[1].Trim();

                switch (key)
                {
                    case nameof(EnableFastWatering):
                        if (bool.TryParse(value, out var enableFastWatering))
                            EnableFastWatering = enableFastWatering;
                        break;

                    case nameof(WateringRange):
                        if (int.TryParse(value, out var wateringRange))
                            WateringRange = Math.Clamp(wateringRange, 1, 3); // 确保范围在 1-3 之间
                        break;
                }
            }
        }
        catch
        {
            // 如果读取失败，使用默认配置
            Reset();
        }
    }

    /// <summary>
    /// 将配置写入文件
    /// </summary>
    public void Write()
    {
        if (_helper is null)
            return;

        string configPath = GetConfigPath();
        
        try
        {
            var lines = new[]
            {
                $"{nameof(EnableFastWatering)}={EnableFastWatering}",
                $"{nameof(WateringRange)}={WateringRange}"
            };
            
            File.WriteAllLines(configPath, lines);
        }
        catch
        {
            // 忽略写入错误
        }
    }

    /// <summary>
    /// 获取配置文件路径
    /// </summary>
    /// <returns>配置文件的完整路径</returns>
    private string GetConfigPath()
    {
        if (_helper is null)
            throw new System.InvalidOperationException("Helper not initialized");
        
        return Path.Combine(_helper.DirectoryPath, "config.txt");
    }

    /// <summary>
    /// 设置通用模组配置菜单
    /// </summary>
    /// <param name="helper">SMAPI 助手实例</param>
    /// <param name="manifest">模组清单</param>
    private void SetupGenericModConfigMenu(IModHelper helper, IManifest manifest)
    {
        try
        {
            // 获取 GMCM API（使用 DLL 中的实际接口）
            var api = helper.ModRegistry.GetApi<IGenericModConfigMenuApi>("spacechase0.GenericModConfigMenu");
            if (api is null)
                return;

            // 注册模组
            api.Register(
                mod: manifest,
                reset: Reset,
                save: Write
            );

            // 添加布尔选项
            api.AddBoolOption(
                mod: manifest,
                name: () => I18n.Tr("config.fast-watering.title"),
                tooltip: () => I18n.Tr("config.fast-watering.desc"),
                getValue: () => EnableFastWatering,
                setValue: value => EnableFastWatering = value
            );

            // 添加数字选项
            api.AddNumberOption(
                mod: manifest,
                name: () => I18n.Tr("config.watering-range.title"),
                tooltip: () => I18n.Tr("config.watering-range.desc"),
                getValue: () => WateringRange,
                setValue: value => WateringRange = Math.Clamp(value, 1, 3),
                min: 1,
                max: 3
            );
        }
        catch
        {
            // 如果 GMCM 调用失败，忽略错误
        }
    }
}
