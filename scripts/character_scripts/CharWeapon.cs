using Godot;
using System;

public partial class Character : CharacterBody3D
{	

	/// <Summary>
	private void InitWeaponHandler()
	{
		WeaponNode = GetNodeOrNull<Weapon>("PlayerCamera/Weapon");
	}

	/// <Summary>
	/// Handles inputs related to shooting
	/// </Summary>
	private void WeaponHandler ()
	{
		if(Input.IsActionPressed(InputMap[InputMapEnum.ActionFire]))
		{
			WeaponNode?.Shoot();
		}
	}
}
