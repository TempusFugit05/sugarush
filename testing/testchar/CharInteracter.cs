using Godot;
using System;

public partial class Character
{
    public class InteractionHandler
    {
        private RayCast3D InteractRay;
	    public IInteractable InteractingWith { get; private set; }
        private float PickupRange = 10f;
        Character P;

        public void init(Character character)
        {
            P = character;
            InteractRay = new()
            {
                TargetPosition = -P.Camera.GlobalBasis.Z.Normalized() * PickupRange,
                DebugShapeThickness = 0
            };
            
            P.Camera.AddChild(InteractRay);

            InteractRay.GlobalPosition = P.Camera.GlobalPosition;
        }

        public void Run()
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
}
