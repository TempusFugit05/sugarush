using Godot;
using System;

public partial class Character
{
	private Area3D PickupSphereNode;

	/// <Summary>
	/// Initialize the character's area responsible for pickup detection
	/// </Summary>
	private void InitPickupSphere()
	{
		PickupSphereNode = GetNode<Area3D>("PickupSphere");
		PickupSphereNode.AreaEntered += OnPickupDetected;
	} 

	/// <Summary>
	/// Signal to detect pickups entering the player's pickup sphere
	/// </Summary>
	private void OnPickupDetected(Node3D detectedObj)
	{
		Node obj = (Node)detectedObj.GetParent(); // Get pickup object (since we are only detecting the pickup area of the pickup object)
		
		if (obj is IPickable pickupObj)
		{
			pickupObj.Pickup(); // Mark object as picked up to be handled on its iteration

			if(pickupObj is Food food)
			{
				sugarHandler.ConsumeSugar(food.GetSugar()); // Consume sugar if object is food
			}
		}
	}
}