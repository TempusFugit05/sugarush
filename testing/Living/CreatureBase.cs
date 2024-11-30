using System;
using System.Collections.Generic;
using System.ComponentModel;
using Godot;
using Helpers;

public partial class CreatureBase : RigidBody3D, ICreature
{
    public struct CreatureSettingsStruct
    {
        public float GroundDetectionDistance = 100.0f;
        public float GroundDetectionSafeMargin = 0.35f;

        public bool RecursiveHitbox = false;
        public bool GroundDetectFromColliders = true;
        public bool GroundDetectFromRecursive = false;

        public float GroundSnapStart = 0.25f;
        public float GroundSnapEnd = 0.15f;
        public float GroundSnapMargin = 0.1f;
        public Godot.Collections.Array<Rid> ExcludeRids = new();
        public CreatureSettingsStruct(){}
    }

    public class CreatureState
    {
        public bool IsAlive = true;
        public float Health = 100.0f;
        public float MaxHealth = 100.0f;
        public float Armor = 0.0f;
        public CreatureState(){}
    }

    protected CreatureSettingsStruct CreatureSettings = new();
    protected CreatureState State = new();
    protected Aabb Hitbox = new();
    protected Aabb GroundDetectRec = new(Vector3.Zero, 0.01f, 0.01f, 0.01f);

    private ShapeCast3D GroundDetectArea;
    private Timer GroundSnapTimer;

    protected virtual void InitCreature(){}

    private GenericCache Cache = new();

    public float GetHealth()
    {
        return State.Health;
    }

    public float GetMaxHealth()
    {
        return State.MaxHealth;
    }

    public virtual void Kill()
    {
        QueueFree();
    }

    public virtual void Hurt(float damage, Vector3 damagePosition, ulong colliderId)
    {
        State.Health -= damage;
        if (State.Health <= 0)
        {
            Kill();
        }
    }

    public virtual void Heal(float amount)
    {
        if (State.Health < State.MaxHealth)
        {
            State.Health += amount;
            if (State.Health > State.MaxHealth)
            {
                State.Health = State.MaxHealth;
            }
        }
    }

    // protected void SnapToFloor()
    // {
    //     if (CreatureSettings.GroundSnapping)
    //     {
    //         if (outDistance <= CreatureSettings.GroundSnapStart - HM.Epsilon &&
    //             outDistance >= CreatureSettings.GroundSnapEnd + HM.Epsilon
    //             && LinearVelocity.LengthSquared() <= 0.75 * 0.75)
    //         {
    //             Vector3 dir = (closestCollision - GlobalPosition).Normalized();
    //             float targetDistance = CreatureSettings.GroundSnapMargin + (Hitbox.Size.Y / 2);

    //             GlobalPosition = closestCollision - (dir * targetDistance);
    //             GD.Print(outDistance);
    //             GroundSnapTimer.Start(0.5f);
    //         }
    //     }
    // }

    /// <summary>
    ///     Get the height of a creature above the ground, while taking the hitbox into account.
    /// </summary>
    /// <param name="downDir">The direction in relation to which the distance will be calculated. Default is the normalized gravity vector.</param>
    /// <returns>The distance of the creature from the ground or negative infinity when nothing is detected.</returns>
    protected float GetGroundDistance()
    {
        const string funcName = nameof(GetGroundDistance);

        if (Cache.IsSameIteration(funcName))
        {
            return (float)Cache.GetCachedItem(funcName);
        }

        float outDistance = float.NegativeInfinity;

        if (GroundDetectArea.IsColliding())
        {
            for (int i = 0; i < GroundDetectArea.GetCollisionCount(); i++)
            {
                Vector3 collisionPoint = GroundDetectArea.GetCollisionPoint(i);
                float distance = collisionPoint.DistanceSquaredTo(GlobalPosition);

                if (distance > outDistance)
                {
                    outDistance = distance;
                }
            }

            outDistance = Mathf.Sqrt(outDistance) - (Hitbox.Size.Y / 2);

        }
        Cache.UpdateCache(funcName, outDistance);
        return outDistance;
    }

    /// <summary>
    ///     Check if the creature is on ground or not.
    /// </summary>
    /// <param name="downDir">The direction in relation to which the distance will be calculated. Default is the normalized gravity vector.</param>
    /// <returns>True if creature is on ground, false if not.</returns>
    protected bool IsOnGround()
    {
        const string funcName = nameof(IsOnGround);

        if (Cache.IsSameIteration(funcName))
        {
            return (bool)Cache.GetCachedItem(funcName);
        }

        bool onGround;
        float height = GetGroundDistance();

        if (height != float.NegativeInfinity && Mathf.Abs(height) <= CreatureSettings.GroundDetectionSafeMargin)
        {
            onGround =  true;
        }
        else
        {
            onGround = false;
        }

        if (OS.IsDebugBuild())
        {
            if (onGround)
            {
                GroundDetectArea.DebugShapeCustomColor = new(0, 255, 0);
            }
            else
            {
                GroundDetectArea.DebugShapeCustomColor = new(255, 0, 0);
            }
        }

        Cache.UpdateCache(funcName, onGround);
        return onGround;
    }

    protected bool IsLookingAtTarget(Node3D lookingNode, Node3D target)
    {
        if (lookingNode.GlobalBasis.Z.AngleTo(lookingNode.GlobalPosition - target.GlobalPosition) <= Mathf.DegToRad(5))
        {
            return true;
        }
        return false;
    }

    // protected bool IsPlayerVisible()
    // {
    //     if (HR.GetPlayerNode() is not null)
    //     {
    //         PhysicsRayQueryParameters3D Query = new()
    //         {
    //             From = GlobalPosition,
    //             To = HR.GetPlayerNode().GlobalPosition,
    //             Exclude = OrganRids,
    //             CollideWithAreas = false,
    //             CollideWithBodies = true,
    //         };

    //         Godot.Collections.Dictionary RayDict = GetWorld3D().DirectSpaceState.IntersectRay(Query);
    //         if (RayDict.Count != 0)
    //         {
    //             if ((Node)RayDict["collider"] is Character)
    //             {
    //                 return true;
    //             }
    //         }
    //     }
        
    //     return false;
    // }

    private void GetHitbox()
    {
        if (CreatureSettings.RecursiveHitbox)
        {
            foreach (MeshInstance3D mesh in HR.GetChildrenOfType<MeshInstance3D>(this, true))
            {
                Hitbox = Hitbox.Merge(mesh.GetAabb());
            } // Create hitbox form all meshes in node
        }
        else
        {
            MeshInstance3D mesh = GetNodeOrNull<MeshInstance3D>("MeshInstance3D");
            if (mesh == null)
            {
                CreatureSettings.RecursiveHitbox = true;
                GD.PushWarning("Could not construct hitbox from child mesh. Attempting recursive...");
                GetHitbox();
            }
            Hitbox = mesh.GetAabb();
        }
    }

    private BoxShape3D GetGroundDetectRec()
    {
        Godot.Collections.Array<CollisionShape3D> colliders;
        
        if (CreatureSettings.GroundDetectFromRecursive)
        {
            colliders = HR.GetChildrenOfType<CollisionShape3D>(this, true);
        }
        else
        {
            colliders = HR.GetChildrenOfType<CollisionShape3D>(this, false);
        }

        Aabb colliderBoundss = colliders[0].Shape.GetDebugMesh().GetAabb() * colliders[0].GlobalTransform.AffineInverse();
        colliderBoundss.Size *= colliders[0].GlobalBasis.Scale;
        GroundDetectRec = colliderBoundss;

        foreach (var collider in colliders)
        {
            if (CreatureSettings.GroundDetectFromColliders)
            {
                Aabb colliderBounds = collider.Shape.GetDebugMesh().GetAabb();
                colliderBounds.Size *= collider.GlobalBasis.Scale;
                GroundDetectRec.Merge(colliderBounds);
            }

            Rid colliderRid = collider.Shape.GetRid();
            CreatureSettings.ExcludeRids.Add(colliderRid);
            GroundDetectArea.AddExceptionRid(colliderRid);
        }
        
        if (!CreatureSettings.GroundDetectFromColliders)
        {
            GroundDetectRec = Hitbox;
        }

        BoxShape3D detectorShape = new();
        detectorShape.Size = new Vector3(GroundDetectRec.Size.X, 0.01f, GroundDetectRec.Size.Z);

        return detectorShape;
    }

    private void SetupColliders()
    {
        GroundDetectArea = new()
        {
            Enabled = true,
            CollideWithBodies = true,
            CollideWithAreas = false,
            ExcludeParent = true,
            TargetPosition = -GlobalBasis.Y.Normalized() * (CreatureSettings.GroundDetectionDistance + (GroundDetectRec.Size.Y / 2)),
        };

        GroundDetectArea.Shape = GetGroundDetectRec();

        GetHitbox();
        AddChild(GroundDetectArea);

        GroundDetectArea.GlobalPosition = GlobalPosition - ((GroundDetectArea.TargetPosition - GlobalPosition).Normalized() * 0.01f);
    }

    public override void _Ready()
    {
        InitCreature(); // User defined initializer
        SetupColliders();
    }
}
