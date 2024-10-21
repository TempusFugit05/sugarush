using Godot;
using System;

public partial class Character : CharacterBody3D
{	

	/// <Summary>
	private void InitWeaponHandler()
	{
		WeaponNode = GetNode<Weapon>("PlayerCamera/Camera3D/Weapon");
	}

	/// <Summary>
	/// Handles inputs related to shooting
	/// </Summary>
	private void WeaponHandler ()
	{
		if(Input.IsActionPressed(KeyFire))
		{
			WeaponNode.Shoot();
		}
	}
}
