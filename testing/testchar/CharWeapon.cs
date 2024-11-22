using Godot;
using Helpers;

public partial class TestCharacter
{	

	/// <Summary>
	private void InitWeaponHandler()
	{
		WeaponNode = GetNodeOrNull<Weapon>("PlayerCamera/Weapon");
		if (WeaponNode is not null)
		{
			WeaponNode.SetAttachmentMode(Weapon.AttachmentModeEnum.Player, OrganRids);
			WeaponNode.PlayerCameraNode = PlayerCamera;
		}
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
