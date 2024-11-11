using Godot;
using Helpers;

public partial class Character : CharacterBody3D, ICreature
{
    public override void _Ready()
	{
        PlayerCamera = GetNode<Camera3D>("PlayerCamera");
        InitPickupSphere();
		InitWeaponHandler();
        HR.SetPlayerNode(this);
    }
	public void Hurt(float Damage, Vector3 DamagePosition = default)
	{
		CurrentSugar -= Damage;
	}

	public void Kill()
	{
        QueueFree();
    }

	public override void _Input(InputEvent CurrentInput)
	{
        if (CurrentInput is InputEventMouseMotion MouseMotion)
		{
			MouseMovement = MouseMotion.Relative;
            LookHandler();
        }
	}

	public override void _PhysicsProcess(double delta)
	{
		SugarHandler(delta);
		WeaponHandler();
		MovementHandler(delta);
	}
}
