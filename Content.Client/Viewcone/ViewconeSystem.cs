using Robust.Client.GameObjects;

namespace Content.Client.Viewcone;

public sealed class ViewconeSystem : EntitySystem
{
    private EntityUid _coneEntity;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<PlayerAttachedEvent>(OnPlayerAttached);
    }

    private void OnPlayerAttached(PlayerAttachedEvent ev)
    {
        var xform = Transform(ev.Entity);
        if (_coneEntity != EntityUid.Invalid)
            Del(_coneEntity);

        _coneEntity = Spawn("WallViewcone", xform.Coordinates);

        var coneXform = Transform(_coneEntity);
        coneXform.AttachParent(ev.Entity);
        coneXform.LocalRotation = Angle.Zero;
        coneXform.Coordinates = new(ev.Entity, 0f, 1.1f);
    }
}
