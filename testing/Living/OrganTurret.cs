using Godot;
using Helpers;

public partial class OrganTurret : CreatureBase
{
    private OrganArm WeaponHolder;
    private Organ Body;
    float HoverForce = 50;
    float HoverHeight = 3;
    Vector3 Force;

    // protected override void InitCreature()
    // {
    //     Body = GetNode<Organ>("Body");
    //     WeaponHolder = GetNode<OrganArm>("WeaponHolder");
    //     OrganSettings.MaxHealth = 500;
    // }
    
	private void RotateTowardsPlayer(double delta)
	{       
        Quaternion targetRotation;
        Quaternion FinalRotation;

        // Quaternion = HM.RotateTowards(Quaternion, Quaternion.Identity, Mathf.Tau / 4 * (float)delta); // Maintain rotation of base node

        static Quaternion rotateTo(Vector3 from, Vector3 to)
        {
            Vector3 midWay = (from + to).Normalized();

            return new Quaternion
            (
                w: from.Dot(midWay),
                x: from.Y * midWay.Z - from.Z * midWay.Y,
                y: from.Z * midWay.X - from.X * midWay.Z,
                z: from.X * midWay.Y - from.Y * midWay.X
            );
        }
        
        static Vector3 projectOntoPlane(Vector3 planeVec1, Vector3 planeVec2, Vector3 toProject)
        {
            Vector3 planeNormal = planeVec1.Cross(planeVec2); // Normal created by the two plane vectors
            Vector3 projection = toProject.Project(planeNormal);
            return toProject - projection;
        }

        Vector3 targetRot = WeaponHolder.GlobalTransform.LookingAt(HR.GetPlayerNode().GlobalPosition, -GlobalBasis.Y).Basis.Z; // Forward vector that looks at player
        Vector3 upDown = projectOntoPlane(Body.Basis.Z, Body.Basis.Y, targetRot); // Project that onto the forward-down plane of main body
        WeaponHolder.LookAt(-upDown*10000000);
        GetNode<CsgMesh3D>("foo").LookAt(upDown*100000);

        // Quaternion finalRot = new (WeaponHolder.Basis.Z, upDown); // Rotate from current forward to new forward
        // WeaponHolder.Quaternion *= finalRot.Normalized();
    }

    private void Hover()
    {
        float distance = GetGroundDistance();
        Vector3 hoverForce = -GetGravity().Normalized() * HoverForce;
        if (distance > 0)
        {
            if (distance < HoverHeight)
            {
                float distanceFactor = Mathf.Clamp(HoverHeight - distance, 0.0f, 0.5f);
                Force += hoverForce + (hoverForce * distanceFactor);
            }
        }
    }

    // protected override void OrganPhysicsProcess(double delta)
    // {
    //     Force = Vector3.Zero;
    //     if (IsPlayerVisible() && OrganState.Alive)
    //     {
    //         RotateTowardsPlayer(delta);
    //     }
    //     Hover();
    //     ApplyCentralForce(Force);
    // }
}
