using Content.Server.GameTicking;
using Content.Shared.Antag;
using Content.Shared.Body.Components;
using Robust.Shared.Configuration;
using Content.Shared.CCVar;

namespace Content.Server.Antag;

/// <summary>
/// This handles revealing antag icons on roundend
/// </summary>
public sealed class RevealAntagIconsOnRoundEndSystem : EntitySystem
{
    [Dependency] private readonly IConfigurationManager _configurationManager = default!;

    /// <inheritdoc/>
    public override void Initialize()
    {
        SubscribeLocalEvent<GameRunLevelChangedEvent>(OnGameRunLevelChangedEvent);
    }

    private void OnGameRunLevelChangedEvent(GameRunLevelChangedEvent ev)
    {
        if (!_configurationManager.GetCVar(CCVars.RevealAntagIconsOnRoundEnd))
            return; // Don't reveal antag icons

        var query = EntityQueryEnumerator<BodyComponent>();
        while (query.MoveNext(out var entity, out var bobby))
        {
            EnsureComp<ShowAntagIconsComponent>(entity);
        }
    }
}
