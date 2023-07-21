using Robust.Shared.Map;

namespace Content.Server._FTL.AutomatedCombat;

/// <summary>
/// This is used for...
/// </summary>
[RegisterComponent]
public sealed class AutomatedCombatComponent : Component
{
    /// <summary>
    /// How long does it take to fire a weapon?
    /// </summary>
    [DataField("attackRepetition")] [ViewVariables(VVAccess.ReadWrite)]
    public float AttackRepetition = 15f;

    /// <summary>
    /// The distance between the old shot and the new shot must not be lower than the no fire distance, otherwise, it will drop the current shot and randomize again.
    /// </summary>
    [DataField("noFireRadius")] public float NoFireDistance = 5f;

    /// <summary>
    /// The distance between the old shot and the new shot must not be lower than the reroll fire distance, otherwise, it will randomize again. If the new coordinate has a further distance than our current position, it will use that coordinate, otherwise it will use the current one.
    /// </summary>
    [DataField("rerollFireRadius")] public float RerollFireDistance = 10f;

    public EntityCoordinates? LastFiredCoordinates = null;
}
