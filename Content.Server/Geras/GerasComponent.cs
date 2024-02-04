using Content.Shared.Actions;
using Robust.Shared.Prototypes;

namespace Content.Server.Geras;

/// <summary>
/// This is used for...
/// </summary>
[RegisterComponent]
public sealed partial class GerasComponent : Component
{
    [DataField] public ProtoId<EntityPrototype> GerasPolymorphId = "SlimeMorphGeras";

    [DataField] public ProtoId<EntityPrototype> GerasAction = "ActionMorphGeras";

    public EntityUid? GerasActionEntity;
}
