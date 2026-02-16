using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;
using System;
using System.Collections.Generic;
using System.IO;

namespace MixTenMod.Content
{
    /// <summary>
    /// 模组内容管理器 - 加载自定义贴图和音效
    /// </summary>
    public class ModContent
    {
        private readonly IModHelper Helper;
        private readonly Dictionary<string, Texture2D> _textures = new();
        private readonly Dictionary<string, SoundEffect> _soundEffects = new();

        public ModContent(IModHelper helper)
        {
            Helper = helper;
        }

        /// <summary>
        /// 加载所有内容
        /// </summary>
        public void LoadContent()
        {
            LoadTextures();
            LoadAudio();
        }

        /// <summary>
        /// 加载贴图
        /// </summary>
        private void LoadTextures()
        {
            string imagesPath = Path.Combine(Helper.DirectoryPath, "assets", "images");
            if (!Directory.Exists(imagesPath))
                return;

            // 加载RL-11枪械贴图
            LoadTexture("RL11", Path.Combine(imagesPath, "rl11.png"));
            
            // 加载失水弹贴图
            LoadTexture("DehydratedAmmo", Path.Combine(imagesPath, "dehydrated_ammo.png"));
            
            // 加载子弹贴图
            LoadTexture("Bullet", Path.Combine(imagesPath, "bullet.png"));
        }

        /// <summary>
        /// 加载单个贴图
        /// </summary>
        private void LoadTexture(string key, string path)
        {
            try
            {
                if (File.Exists(path))
                {
                    using var stream = File.OpenRead(path);
                    var texture = Texture2D.FromStream(Game1.graphics.GraphicsDevice, stream);
                    _textures[key] = texture;
                }
            }
            catch
            {
                // 忽略加载错误
            }
        }

        /// <summary>
        /// 加载音效
        /// </summary>
        private void LoadAudio()
        {
            string audioPath = Path.Combine(Helper.DirectoryPath, "assets", "audio");
            if (!Directory.Exists(audioPath))
                return;

            // 加载射击音效
            LoadSoundEffect("RifleShot", Path.Combine(audioPath, "rifle_shot.wav"));
            
            // 加载装填开始音效
            LoadSoundEffect("ReloadStart", Path.Combine(audioPath, "reload_start.wav"));
            
            // 加载装填完成音效
            LoadSoundEffect("ReloadEnd", Path.Combine(audioPath, "reload_end.wav"));
        }

        /// <summary>
        /// 加载单个音效
        /// </summary>
        private void LoadSoundEffect(string key, string path)
        {
            try
            {
                if (File.Exists(path))
                {
                    using var stream = File.OpenRead(path);
                    var soundEffect = SoundEffect.FromStream(stream);
                    _soundEffects[key] = soundEffect;
                }
            }
            catch
            {
                // 忽略加载错误
            }
        }

        /// <summary>
        /// 获取贴图
        /// </summary>
        public Texture2D? GetTexture(string key)
        {
            return _textures.TryGetValue(key, out var texture) ? texture : null;
        }

        /// <summary>
        /// 播放音效
        /// </summary>
        public void PlaySound(string key)
        {
            if (_soundEffects.TryGetValue(key, out var soundEffect))
            {
                soundEffect.Play();
            }
            else
            {
                // 如果自定义音效不存在，使用游戏内置音效
                PlayDefaultSound(key);
            }
        }

        /// <summary>
        /// 播放默认音效
        /// </summary>
        private void PlayDefaultSound(string key)
        {
            string soundId = key switch
            {
                "RifleShot" => "slingshot",
                "ReloadStart" => "shwip",
                "ReloadEnd" => "coin",
                _ => "coin"
            };
            Game1.playSound(soundId);
        }

        /// <summary>
        /// 检查是否有自定义音效
        /// </summary>
        public bool HasCustomSound(string key)
        {
            return _soundEffects.ContainsKey(key);
        }
    }
}
