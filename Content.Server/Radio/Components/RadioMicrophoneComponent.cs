using Content.Server.Radio.EntitySystems;
using Content.Shared.Radio;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype;

namespace Content.Server.Radio.Components;

/// <summary>
///     Listens for local chat messages and relays them to some radio frequency
/// </summary>
[RegisterComponent]
[Access(typeof(RadioDeviceSystem))]
public sealed class RadioMicrophoneComponent : Component
{
    [DataField("broadcastChannel", customTypeSerializer: typeof(PrototypeIdSerializer<RadioChannelPrototype>))]
    public string BroadcastChannel = "Common";

    [DataField("listenRange")]
    [ViewVariables(VVAccess.ReadWrite)]
    public int ListenRange  = 4;

    [DataField("enabled")]
    [ViewVariables(VVAccess.ReadWrite)]
    public bool Enabled = false;
}
