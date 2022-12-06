using Robust.Shared.Prototypes;

namespace Content.Server.Dynamic;

[Prototype("dynamicPrices")]
public sealed class DynamicPriceListPrototype : IPrototype
{
    [IdDataField] public string ID { get; } = default!;
    [DataField("prices")] public Dictionary<string, int> Prices = new();
}
