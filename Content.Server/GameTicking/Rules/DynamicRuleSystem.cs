using System.Linq;
using Content.Server.Dynamic;
using Content.Server.GameTicking.Presets;
using Content.Server.GameTicking.Rules.Configurations;
using Content.Shared.Random;
using Content.Shared.Random.Helpers;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;

namespace Content.Server.GameTicking.Rules;

/// <summary>
/// A ghetto version of /tg/'s dynamic.
/// </summary>
public sealed class DynamicRuleSystem : GameRuleSystem
{
    [Dependency] private readonly IPrototypeManager _prototypeManager = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly GameTicker _ticker = default!;

    public override string Prototype => "Dynamic";
    private int _threatLevel;

    public override void Started()
    {
        Logger.Info("Dynamic is running");

        _threatLevel = _random.Next(1, 100);
        Logger.InfoS("gamepreset", $"Threat level: {_threatLevel}.");
        PickRules();
    }

    public override void Ended()
    {
        // noop
        // Preset should already handle it.
        return;
    }

    private void PickRules()
    {
        Logger.InfoS("gamepreset", $"Now buying presets with a threat level of {_threatLevel}.");
        var pricesPrototype = _prototypeManager.Index<DynamicPriceListPrototype>("Default");
        var pricesKeys = new List<string>(pricesPrototype.Prices.Keys);
        var presets = Array.Empty<string>();

        while (pricesKeys.Count > 0)
        {
            var preset = _random.PickAndTake(pricesKeys);
            var price = pricesPrototype.Prices[preset];
            if (price >= _threatLevel)
            {
                presets.Append(preset);
                _threatLevel -= price;
                Logger.InfoS("gamepreset", $"Spent {price} on {preset}.");
            }
        }

        foreach (var preset in presets)
        {
            var rules = _prototypeManager.Index<GamePresetPrototype>(preset).Rules;
            Logger.InfoS("gamepreset", $"Running preset: {preset}.");
            foreach (var rule in rules)
            {
                _ticker.StartGameRule(_prototypeManager.Index<GameRulePrototype>(rule));
            }
        }
    }
}
