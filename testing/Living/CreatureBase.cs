using System;
using System.Collections.Generic;
using Godot;
using Helpers;

public partial class CreatureBase : RigidBody3D, ICreature
{
    public struct CreatureSettingsStruct
    {
        public float GroundDetectionDistance = 100.0f;
        public float GroundDetectionSafeMargin = 0.35f;
        public bool GroundSnapping = true;
        public float GroundSnapStart = 0.25f;
        public float GroundSnapEnd = 0.15f;
        public float GroundSnapMargin = 0.1f;

        public CreatureSettingsStruct(){}
    }

    protected CreatureSettingsStruct CreatureSettings = new();
    private ShapeCast3D GroundDetectArea;
    private Timer GroundSnapTimer;

    public float Health {get; private set;} = 100.0f;
    protected Aabb Hitbox = new();

    protected virtual void InitCreature(){}

    private GenericCache Cache = new();

    public virtual void Kill()
    {
        QueueFree();
    }

    public virtual void Hurt(float damage, Vector3 damagePosition, ulong colliderId)
    {
        Health -= damage;
        if (Health <= 0)
        {
            Kill();
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

    private void GetHitBox()
    {
        foreach (MeshInstance3D mesh in HR.GetChildrenOfType<MeshInstance3D>(this, true))
        {
            Hitbox = Hitbox.Merge(mesh.GetAabb());
        }
    }

    public override void _Ready()
    {
        GetHitBox();

        GroundSnapTimer = new();
        AddChild(GroundSnapTimer);
        GroundSnapTimer.OneShot = true;

        BoxShape3D detectorShape = new();
        detectorShape.Size = new Vector3(Hitbox.Size.X, 0.01f, Hitbox.Size.Z);
        
        GroundDetectArea = new()
        {
            Enabled = true,
            CollideWithBodies = true,
            CollideWithAreas = false,
            ExcludeParent = true,
            Shape = detectorShape,
            TargetPosition = -GlobalBasis.Y.Normalized() * (CreatureSettings.GroundDetectionDistance + (Hitbox.Size.Y / 2)),
        };

        AddChild(GroundDetectArea);

        GroundDetectArea.GlobalPosition = GlobalPosition - ((GroundDetectArea.TargetPosition - GlobalPosition).Normalized() * 0.01f);
        GroundDetectArea.GlobalRotation = GlobalRotation;

        InitCreature(); // User defined initializer
    }

}
