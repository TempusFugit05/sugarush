using Godot;
using System;

public partial class TestCharacter
{
	private void InitInteractionHandler()
	{
        InteractRay = PlayerCamera.GetNode<RayCast3D>("InteractRay");
        InteractRay.AddException(this);
        InteractRay.GlobalPosition = PlayerCamera.GlobalPosition;
        InteractRay.TargetPosition = Vector3.Forward * 10;
        InteractRay.DebugShapeThickness = 0;
    }

	private void InteractionHandler()
	{
		if (InteractRay.IsColliding())
		{
            if (InteractRay.GetCollider() is IInteractable interactable)
			{
                InteractingWith = interactable;
                return;
            }
		}
        InteractingWith = null;
	}
}
