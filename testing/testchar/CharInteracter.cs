using Godot;
using System;

public partial class TestCharacter
{
    private RayCast3D InteractRay;

	private void InitInteractionHandler()
	{
        InteractRay = new()
        {
            TargetPosition = Vector3.Forward * 10,
            DebugShapeThickness = 0
        };
        
        PlayerCamera.AddChild(InteractRay);

        InteractRay.GlobalPosition = PlayerCamera.GlobalPosition;
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
