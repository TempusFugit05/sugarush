using Godot;
using System;

public interface ICreature
{
	/// <Summary>
	///	Classes that Implement this method will do something special when they are shot/hurt in any way
	/// </Summary>
    void Hurt(float damage, Vector3 damagePosition = default, ulong colliderId = default);
	void Heal(float healAmount);
	void Kill();
}
