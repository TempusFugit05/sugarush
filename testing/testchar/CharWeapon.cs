using Godot;
using System;

public partial class TestCharacter
{	

	/// <Summary>
	private void InitWeaponHandler()
	{
		WeaponNode = GetNodeOrNull<Weapon>("PlayerCamera/Weapon");
	    WeaponNode?.SetAttachmentMode(Weapon.AttachmentModeEnum.Player);
        WeaponNode.PlayerCameraNode = PlayerCamera;
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
