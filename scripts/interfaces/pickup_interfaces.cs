using Godot;
using System;
using Helpers;

public interface IPickable
{
    // /// <summary>
    // ///     Every class that inherits the IPickable must call this signal upon entering the tree.
    // ///     This ensures that it will be added to the exclusion lists for weapon targets.
    // /// </summary>
    // [Signal]
    // public delegate void PickupCreatedEventHandler();

	/// <summary>
    /// 	Classes that implament this method are able to be picked up
    /// </summary>
    void Pickup();
}
