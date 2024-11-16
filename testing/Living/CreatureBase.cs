using Godot;
using System.Collections.Generic;
using Helpers;

public partial class CreatureBase : Organ
{
    public struct CreatureSettingsStruct
    {
        public float GroundDetectionDistance = 100.0f;
        public CreatureSettingsStruct(){}
    }
    CreatureSettingsStruct CreatureSettings = new();

    public void NotifyKilled(Organ killedOrgan, OrganSettingStruct settings)
    {
        if (settings.Vital)
        {
            Kill();
            Destroy();
        }
    }

    protected float GetHeightAboveGround()
    {
        PhysicsRayQueryParameters3D Query = new()
        {
            From = GlobalPosition,
            To = -GlobalBasis.Y * CreatureSettings.GroundDetectionDistance,
            Exclude = OrganRids,
            CollideWithAreas = false,
            CollideWithBodies = true,
        };
        Godot.Collections.Dictionary RayDict = GetWorld3D().DirectSpaceState.IntersectRay(Query);
        float outDistance = float.NegativeInfinity;
        if (RayDict.Count != 0)
        {
            outDistance = ((Vector3)RayDict["position"]).DistanceTo(GlobalPosition) - (Hitbox.Size.Y / 2);
        }
        return outDistance;
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

}
