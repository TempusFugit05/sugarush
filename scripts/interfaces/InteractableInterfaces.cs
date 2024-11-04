using Godot;

namespace Interactables
{
	public interface IShootable
	{
        void OnShot(Node3D shooter);
    }
    public interface IInteractable
    {
        void Interact(Node3D interacter);
    }
}
