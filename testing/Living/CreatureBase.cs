using System.Linq;
using Godot;
using Helpers;

public partial class CreatureBase : Organ
{
    public struct CreatureSettingsStruct
    {
        public float GroundDetectionDistance = 100.0f;
        public float GroundDetectionSafeMargin = 0.25f;
        public CreatureSettingsStruct(){}
    }
    protected CreatureSettingsStruct CreatureSettings = new();
    private ShapeCast3D GroundDetectArea;

    public void NotifyKilled(Organ killedOrgan, OrganSettingStruct settings)
    {
        if (settings.Vital)
        {
            Kill();
            Destroy();
        }
    }

    /// <summary>
    ///     Get the height of a creature above the ground, while taking the hitbox into account.
    /// </summary>
    /// <param name="downDir">The direction in relation to which the distance will be calculated. Default is the normalized gravity vector.</param>
    /// <returns>The distance of the creature from the ground or negative infinity when nothing is detected.</returns>
    protected float GetGroundDistance()
    {
        float outDistance = float.NegativeInfinity;
        Node3D closestCollider;

        if (GroundDetectArea.IsColliding())
        {
            for (int i = 0; i < GroundDetectArea.GetCollisionCount(); i++)
            {
                float distance = GroundDetectArea.GetCollisionPoint(i).DistanceSquaredTo(GlobalPosition);

                if (distance > outDistance)
                {
                    outDistance = distance;
                }
            }

            outDistance = Mathf.Sqrt(outDistance) - (Hitbox.Size.Y / 2);
        }
        return outDistance;
    }

    /// <summary>
    ///     Check if the creature is on ground or not.
    /// </summary>
    /// <param name="downDir">The direction in relation to which the distance will be calculated. Default is the normalized gravity vector.</param>
    /// <returns>True if creature is on ground, false if not.</returns>
    protected bool IsOnGround()
    {
        float height = GetGroundDistance();
        if (height != float.NegativeInfinity)
        {
            if (Mathf.Abs(height) <= CreatureSettings.GroundDetectionSafeMargin)
            {
                return true;
            }
        }

        return false;
    }

    protected bool IsLookingAtTarget(Node3D lookingNode, Node3D target)
    {
        if (lookingNode.GlobalBasis.Z.AngleTo(lookingNode.GlobalPosition - target.GlobalPosition) <= Mathf.DegToRad(5))
        {
            return true;
        }
        return false;
    }

    protected bool IsPlayerVisible()
    {
        if (HR.GetPlayerNode() is not null)
        {
            PhysicsRayQueryParameters3D Query = new()
            {
                From = GlobalPosition,
                To = HR.GetPlayerNode().GlobalPosition,
                Exclude = OrganRids,
                CollideWithAreas = false,
                CollideWithBodies = true,
            };

            Godot.Collections.Dictionary RayDict = GetWorld3D().DirectSpaceState.IntersectRay(Query);
            if (RayDict.Count != 0)
            {
                if ((Node)RayDict["collider"] is Character)
                {
                    return true;
                }
            }
        }
        
        return false;
    }

    public override void _Ready()
    {
        Init();
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

        GroundDetectArea.GlobalPosition = GlobalPosition;
        GroundDetectArea.GlobalRotation = GlobalRotation;
    }

}
