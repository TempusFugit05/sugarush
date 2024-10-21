using Godot;
using System;

public partial class Pickup : StaticBody3D
{
	public virtual void OnPickup()
	{
		QueueFree();
	}

	public virtual void  MarkAsPickedUp()
	{
		OnPickup();
	}
}
