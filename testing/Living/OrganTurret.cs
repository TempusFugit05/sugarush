using Godot;
using Helpers;

public partial class OrganTurret : CreatureBase
{
    private OrganArm WeaponHolder;
    private Organ Body;
    float HoverForce = 50;
    float HoverHeight = 3;
    Vector3 Force;

    protected override void InitOrgan()
    {
        WeaponHolder = GetNode<OrganArm>("WeaponHolder");
        Body = GetNode<Organ>("Body");
        OrganSettings.MaxHealth = 500;
    }
    
	private void RotateTowardsPlayer(double delta)
	{       
        Quaternion targetRotation;
        Quaternion FinalRotation;

        Quaternion = new Quaternion(0, 0, 0, 1); // Maintain rotation of base node

        if (Body.IsAlive())
        {
            /* Rotate body */
            targetRotation = HM.LookingAtAxis(Body, HR.GetPlayerNode(), GlobalBasis.Y); // Get rotation to look at player on Y axis
            FinalRotation = HM.RotateTowards(Body.Quaternion, targetRotation, Mathf.Tau / 4 * (float)delta); // Rotate to target
            Body.Quaternion = HM.ValidateQuaternion(FinalRotation); // Apply rotation
        }

        if (WeaponHolder.IsAlive())
        {
            /* Rotate weapon */
            Quaternion xComponent = HM.ProjectQuaternion(WeaponHolder.Quaternion, GlobalBasis.X); // Get current rotation on x axis
            targetRotation = HM.LookingAtAxis(WeaponHolder, HR.GetPlayerNode(), GlobalBasis.X); // Get rotation to look at player on x axis
            FinalRotation = HM.RotateTowards(xComponent, targetRotation, Mathf.Tau / 4 * (float)delta); // Rotate to target
            WeaponHolder.Quaternion = HM.ProjectQuaternion(Body.Quaternion, GlobalBasis.X).Inverse() * Body.Quaternion * FinalRotation; // Rotate with body and override rotation on x axis
        }
    }

    private void Hover()
    {
        float distance = GetHeightAboveGround();
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

    protected override void OrganPhysicsProcess(double delta)
    {
        Force = Vector3.Zero;
        if (IsPlayerVisible() && OrganState.Alive)
        {
            WeaponHolder.UseOrgan();
            RotateTowardsPlayer(delta);
            Hover();
        }
        ApplyCentralForce(Force);
    }
}
