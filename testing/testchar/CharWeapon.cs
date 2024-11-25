using Godot;
using Helpers;

public partial class Character
{	

	public class WeaponHandler
	{
		Character P;
		Weapon WeaponNode;

		public void Init(Character character)
		{
			P = character;

			WeaponNode = P.GetNodeOrNull<Weapon>("PlayerCamera/Weapon");
			if (WeaponNode is not null)
			{
				WeaponNode.SetAttachmentMode(Weapon.AttachmentModeEnum.Player, P.OrganRids);
				WeaponNode.PlayerCameraNode = P.Camera;
			}
		}

		/// <Summary>
		/// Handles inputs related to shooting
		/// </Summary>
		public void Run()
		{
			
			if(Input.IsActionPressed("fire"))
			{
				WeaponNode?.Shoot();
			}
		}
	}
}
