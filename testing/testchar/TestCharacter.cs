using Godot;
using Helpers;

public partial class TestCharacter : CreatureBase
{

    protected override void InitOrgan()
    {
        LinearDamp = 0;
        PlayerCamera = GetNode<Camera3D>("PlayerCamera");
        InitInteractionHandler();
        InitPickupSphere();
		InitWeaponHandler();
        HR.SetPlayerNode(this);
    }
    
	public new void Hurt(float Damage, Vector3 DamagePosition = default, ulong colliderId = default)
	{
		CurrentSugar -= Damage;
	}

	public new void Kill()
	{
        QueueFree();
    }

	public override void _Input(InputEvent CurrentInput)
	{
        if (CurrentInput is InputEventMouseMotion MouseMotion)
		{
			MouseMovement = MouseMotion.Relative;
        }
	}

	public override void _PhysicsProcess(double delta)
	{
		SugarHandler(delta);
		WeaponHandler();
        LookHandler(delta);
		MovementHandler(delta);
        InteractionHandler();
    }
}
