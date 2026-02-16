using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using StardewModdingAPI;
using StardewModdingAPI.Events;

namespace MixTenMod.i18n
{
    public class I18n
    {
        private static I18n? _instance;
        public static I18n Instance => _instance ?? throw new InvalidOperationException("I18n not initialized");

        private readonly Dictionary<string, string> _translations = new();
        private readonly IModHelper _helper;
        private string _currentLang = string.Empty;

        public I18n(IModHelper helper)
        {
            _helper = helper;
            _instance = this;

            // 延迟加载翻译，等到游戏启动完成
            helper.Events.GameLoop.GameLaunched += OnGameLaunched;
        }

        private void OnGameLaunched(object? sender, GameLaunchedEventArgs e)
        {
            LoadTranslations();
        }

        private void LoadTranslations()
        {
            try
            {
                string lang = _helper.Translation.LocaleEnum.ToString().ToLower();
                
                // 只有当语言发生变化时才重新加载
                if (lang == _currentLang)
                    return;
                
                _currentLang = lang;
                _translations.Clear();

                string i18nPath = Path.Combine(_helper.DirectoryPath, "i18n");

                string targetFile = lang switch
                {
                    "zh" or "zh_cn" => "zh.json",
                    _ => "default.json"
                };

                string filePath = Path.Combine(i18nPath, targetFile);
                if (!File.Exists(filePath))
                {
                    filePath = Path.Combine(i18nPath, "default.json");
                }

                if (File.Exists(filePath))
                {
                    string json = File.ReadAllText(filePath);
                    var dict = JsonSerializer.Deserialize<Dictionary<string, string>>(json);
                    if (dict != null)
                    {
                        foreach (var kvp in dict)
                        {
                            _translations[kvp.Key] = kvp.Value;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // 如果加载失败，使用空翻译
                _helper.ModRegistry.GetType().Assembly.GetType("StardewModdingAPI.Framework.SCore")?.GetMethod("Log")?.Invoke(null, new object[] { $"MixTenMod: Failed to load translations: {ex.Message}", LogLevel.Warn });
            }
        }

        public string Get(string key, Dictionary<string, string>? tokens = null)
        {
            // 每次获取翻译时检查语言是否变化
            LoadTranslations();

            if (_translations.TryGetValue(key, out string? value))
            {
                if (tokens != null)
                {
                    foreach (var token in tokens)
                    {
                        value = value.Replace($"{{{{{token.Key}}}}}", token.Value);
                    }
                }
                return value;
            }
            return key;
        }

        public static string Tr(string key, Dictionary<string, string>? tokens = null)
        {
            return Instance.Get(key, tokens);
        }
    }
}
