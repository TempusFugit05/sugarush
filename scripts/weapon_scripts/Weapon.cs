using Godot;
using System;
using System.Threading.Tasks;

public partial class Weapon : Node3D
{
    [Export]
    private PackedScene DecalScene;

	[Export]
	private float Range = 75.0f;
	
	[Export]
	private float FireRate = 0.25f; // Time between each bullet fired
	
	[Export]
	private float Damage = 25.0f;

	[Export]
	private bool AttachedToCamera = true;

	[Export]
	private Camera3D CameraNode = null;

	private bool CanShoot = true;


	public override void _Ready()
	{
		InitShootingHandler();
	}

	/// <Summary>
	/// Apply damage to the object that was hit
	/// </Summary>
	private void ApplyDamage(Godot.Collections.Dictionary RayDict)
	{
		if(RayDict is not null)
		{
			Node HitObject = (Node)RayDict["collider"];
			if(HitObject is IHurtable hurtable)
			{
				hurtable.Hurt(Damage, (Vector3)RayDict["position"]); // Hurtable is a special method for objects which are damage-able
			}
		}
	}

	
	private async Task SetCooldown()
	{
		CanShoot = false; // Disable shooting
		await ToSignal(GetTree().CreateTimer(FireRate), SceneTreeTimer.SignalName.Timeout); // Set timer for next shot
		CanShoot = true; // Enable shooting
	}

	/// <Summary>
	/// Attempt to shoot. The weapon will only fire if its firing cooldown has expired.
	/// </Summary>
	/// <returns>
	/// <c>true</c> if weapon was successfully fired; <c>false</c> if the cooldown has not expired yet.
	/// </returns>
	public bool Shoot()
	{
		if(CanShoot)
		{
			if(AttachedToCamera && CameraNode is not null)
			{
				ShootFromCamera();
			}			

			else
			{
				ShootFromWeapon();
			}

            _ = SetCooldown();

			return true;
		}

		return false;
	}
}
