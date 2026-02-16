using StardewModdingAPI;
using StardewModdingAPI.Events;
using MixTenMod.Config;
using MixTenMod.Content;
using MixTenMod.Features;
using MixTenMod.i18n;

namespace MixTenMod;

public class ModEntry : Mod
{
    private ModConfig Config = null!;
    private FastWateringFeature? _fastWateringFeature;
    private SpeedPotionFeature? _speedPotionFeature;
    private RL11Feature? _rl11Feature;
    private DehydratedAmmoFeature? _dehydratedAmmoFeature;
    private ModContent? _modContent;

    public override void Entry(IModHelper helper)
    {
        _ = new I18n(helper);

        // 初始化内容管理器
        _modContent = new ModContent(helper);
        helper.Events.GameLoop.GameLaunched += OnGameLaunched;

        Config = new ModConfig();
        Config.Load(helper, ModManifest);

        _dehydratedAmmoFeature = new DehydratedAmmoFeature(helper);
        _fastWateringFeature = new FastWateringFeature(helper, Config, _dehydratedAmmoFeature);
        _speedPotionFeature = new SpeedPotionFeature(helper);
        _rl11Feature = new RL11Feature(helper, _modContent);

        // 注册失水弹到RL-11
        _dehydratedAmmoFeature.SetRL11(_rl11Feature);
    }

    private void OnGameLaunched(object? sender, GameLaunchedEventArgs e)
    {
        _modContent?.LoadContent();
    }
}
