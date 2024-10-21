using Godot;
using System;

public interface IHurtable
{
	/// <Summary>
	///	Classes that Implement this method will do something special when they are shot/hurt in any way
	/// </Summary>
    void Hurt(float damage, Vector3 DamagePosition = default);
}
