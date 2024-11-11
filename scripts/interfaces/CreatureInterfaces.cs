using Godot;
using System;

public interface ICreature
{
	/// <Summary>
	///	Classes that Implement this method will do something special when they are shot/hurt in any way
	/// </Summary>
    void Hurt(float damage, Vector3 damagePosition = default);
    void Kill();
}
public interface ISoulful
{
	/// <Summary>
	///	Classes that Implement this method will do something special when they are shot/hurt in any way
	/// </Summary>
    void OnHurt(float damage, Vector3 damagePosition = default);
    void OnKill();
    CreatureSoul GetSoul();
}
