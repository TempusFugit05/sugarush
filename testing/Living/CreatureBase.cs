using Godot;
using System.Collections.Generic;
using Helpers;

public partial class CreatureBase : RigidBody3D, ICreature
{
	public struct OrganCollider
	{
        public Godot.Collections.Array<CollisionShape3D> Colliders;
        public Node Owner;

		public OrganCollider (Godot.Collections.Array<CollisionShape3D> colliders, Node owner)
		{
            Colliders = colliders;
            Owner = owner;
        }
    }

    protected LinkedList<OrganCollider> GuestColliders = new();

	public void CombineColliders(OrganCollider colliders)
	{
        GuestColliders.AddLast(colliders);
        foreach (CollisionShape3D shape in colliders.Colliders)
        {
            shape.Reparent(this);
        }
	}

    public void Kill()
	{
        return;
    }

    public void Hurt(float damage, Vector3 damagePosition = default, ulong colliderId = default)
    {
		foreach (OrganCollider colliderList in GuestColliders)
		{
			foreach (CollisionShape3D shape in colliderList.Colliders)
			{
				if (colliderId == shape.GetInstanceId() && colliderList.Owner is Organ organ)
				{
					organ.ApplyDamage(damage, damagePosition);
					return;
				}
			}
		}
    }

	/// <summary>
    /// 	Attempt to return collider to original organ.
    /// </summary>
    /// <param name="collider">Collider to return</param>
    /// <param name="returnTo">Original owner</param>
	public bool ReturnCollider(Node returnTo)
	{
        LinkedListNode<OrganCollider> currentCollider = GuestColliders.First;
		while (currentCollider != null)
		{
            OrganCollider keptColliders = currentCollider.Value;
            if (keptColliders.Owner == returnTo)
			{
				foreach (CollisionShape3D collider in keptColliders.Colliders)
				{
                    collider.Reparent(returnTo);
                }
                return true;
            } // If colliders were found in collider list, return them to the original owner
            currentCollider = currentCollider.Next;
        }

		GD.PushError("Organ " + returnTo.Name + " requested collider but it was not found");
		return false;
	}

	private void AmalgamateOrgans()
	{
		foreach (Organ organ in HR.GetChildrenOfType<Organ>(this, true))
		{
            organ.TakeColliders(this);
            organ.OnAddedToCreature();
        }
	}

    public override void _Ready()
    {
        AmalgamateOrgans();
    }

}
