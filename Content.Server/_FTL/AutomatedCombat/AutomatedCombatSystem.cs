using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Numerics;
using Content.Server._FTL.ShipHealth;
using Content.Server._FTL.Weapons;
using Content.Server.Atmos.EntitySystems;
using Content.Server.Station.Components;
using Content.Shared.Atmos;
using Robust.Server.GameObjects;
using Robust.Shared.Map;
using Robust.Shared.Map.Components;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;

namespace Content.Server._FTL.AutomatedCombat;

/// <summary>
/// This handles ships automatically firing at the main ship.
/// </summary>
public sealed class AutomatedCombatSystem : EntitySystem
{
    [Dependency] private readonly WeaponTargetingSystem _weaponTargetingSystem = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly AtmosphereSystem _atmosphereSystem = default!;
    [Dependency] private readonly TransformSystem _transformSystem = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<AutomatedCombatComponent, MapInitEvent>(OnMapInit);
    }

    private void OnMapInit(EntityUid uid, AutomatedCombatComponent component, MapInitEvent args)
    {
        EnsureComp<ActiveAutomatedCombatComponent>(uid);
    }

    private bool TryFindRandomTile(EntityUid targetGrid, out Vector2i tile, out EntityCoordinates targetCoords)
    {
        tile = default;

        targetCoords = EntityCoordinates.Invalid;

        if (!TryComp<MapGridComponent>(targetGrid, out var gridComp))
            return false;

        var found = false;
        var (gridPos, _, gridMatrix) = _transformSystem.GetWorldPositionRotationMatrix(targetGrid);
        var gridBounds = gridMatrix.TransformBox(gridComp.LocalAABB);

        for (var i = 0; i < 10; i++)
        {
            var randomX = _random.Next((int) gridBounds.Left, (int) gridBounds.Right);
            var randomY = _random.Next((int) gridBounds.Bottom, (int) gridBounds.Top);

            tile = new Vector2i(randomX - (int) gridPos.X, randomY - (int) gridPos.Y);
            if (_atmosphereSystem.IsTileSpace(targetGrid, Transform(targetGrid).MapUid, tile,
                    mapGridComp: gridComp)
                || _atmosphereSystem.IsTileAirBlocked(targetGrid, tile, mapGridComp: gridComp))
            {
                continue;
            }

            found = true;
            targetCoords = gridComp.GridTileToLocal(tile);
            break;
        }

        return found;
    }

    private List<EntityUid> GetWeaponsOnGrid(EntityUid gridUid)
    {
        var weapons = new List<EntityUid>();
        var query = EntityQueryEnumerator<FTLWeaponComponent, TransformComponent>();
        while (query.MoveNext(out var entity, out var weapon, out var xform))
        {
            if (xform.GridUid == gridUid)
            {
                weapons.Add(entity);
            }
        }

        return weapons;
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<ActiveAutomatedCombatComponent, AutomatedCombatComponent>();
        while (query.MoveNext(out var entity, out var activeComponent, out var component))
        {
            activeComponent.TimeSinceLastAttack += frameTime;
            if (activeComponent.TimeSinceLastAttack < component.AttackRepetition)
                continue;

            var mainShips = EntityQuery<MainCharacterShipComponent>().ToList();

            if (mainShips.Count <= 0)
                break;

            var mainShip = _random.Pick(mainShips).Owner;

            var weapons = GetWeaponsOnGrid(entity);
            var weapon = _random.Pick(weapons);

            if (!TryComp<FTLWeaponComponent>(weapon, out var weaponComponent))
                return;

            var foundValidTile = false;
            EntityCoordinates? validTile = null;

            bool CorrectDistance(Vector2 position, float limit, out float distance)
            {
                distance = 0f;
                if (!component.LastFiredCoordinates.HasValue)
                    return true;
                distance = Vector2.Distance(component.LastFiredCoordinates.Value.Position, position);
                return distance >= limit;
            }

            // one fuck of a function
            // this whole section just makes sure that shots are properly spaced apart from each other
            var attempts = 0;
            while (!foundValidTile)
            {
                if (attempts >= 10)
                {
                    // stop crashin you cunt
                    foundValidTile = true;
                }

                if (TryFindRandomTile(mainShip, out _, out var coordinates))
                {
                    validTile = coordinates;
                }


                if (component.LastFiredCoordinates.HasValue)
                {
                    if (CorrectDistance(coordinates.Position, component.NoFireDistance, out _))
                    {
                        attempts++;
                        continue;
                    }
                    if (CorrectDistance(coordinates.Position, component.NoFireDistance, out var distance))
                    {
                        if (TryFindRandomTile(mainShip, out _, out var newCoordinates))
                        {
                            if (Vector2.Distance(component.LastFiredCoordinates.Value.Position,
                                    newCoordinates.Position) > distance)
                            {
                                validTile = newCoordinates;
                            }
                        }
                        foundValidTile = true;
                    }
                    if (Vector2.Distance(component.LastFiredCoordinates.Value.Position, coordinates.Position) > component.RerollFireDistance)
                    {
                        foundValidTile = true;
                    }
                }
                else
                {
                    // first time shooting
                    foundValidTile = true;
                }
            }

            if (!validTile.HasValue)
                return;

            activeComponent.TimeSinceLastAttack = 0;
            component.LastFiredCoordinates = validTile.Value;
            _weaponTargetingSystem.TryFireWeapon(weapon, weaponComponent, mainShip, validTile.Value, null);
        }
    }
}
