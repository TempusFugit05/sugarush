using Helpers;
using Godot;


/// <summary>
///     Organs are the building blocks of creatures in the game. 
///     They can be used to define weak spots.
/// </summary>
public partial class Organ : RigidBody3D, ICreature
{
    public float Health = 100.0f;
    public bool IsActive = true;
    public bool IsVital = false;
    public bool OnHurtAfterDeath = false;
    protected bool IsNested = false;
    protected bool ColliderReparented = false;

    [Export]
    protected bool GibsOnDeath = true;
    protected Godot.Collections.Array<CollisionShape3D> OwnColliders;
    protected Godot.Collections.Array<Godot.Collections.Array<CollisionShape3D>> GuestColliders = new();

    [Signal]
    public delegate void OrganDestroyedEventHandler(Organ destroyedOrgan);

    public virtual void UseOrgan(){}
    protected virtual void OnKill(){}
    protected virtual void OnHurt(){}

    /// <summary>
    ///     Emits signal to notify a connected soul that this organ was destroyed.
    /// </summary>
    public virtual void Kill()
    {
        IsActive = false;
        if (true)
		{
            CallDeferred("TurnToGibs");
        }

        EmitSignal(SignalName.OrganDestroyed, this);
        OnKill();
    }
    protected void ApplyDamage(float damage, Vector3 damagePosition = default)
    {
        DamageIndicator Indicator = new(damage); // Create a damage indicator 
        GetTree().Root.AddChild(Indicator); // Add it to the scene
        Indicator.GlobalPosition = (damagePosition == default) ? GlobalPosition : damagePosition; // Set position of indicator to a specific position on body (ie bullethole) or object position for non specific damage soruce (ie fall damage)
        Health -= damage;

        if (Health <= 0)
        {
            Kill();
        }
        else
        {
            OnHurt();
        }
    }

    public virtual void Hurt(float damage, Vector3 damagePosition = default, ulong colliderId = default)
    {
        // if (!IsActive && OnHurtAfterDeath)
        // {
        //     OnHurt();
        //     return;
        // }

        if (IsActive && (colliderId == default || OwnColliders is null))
        {
            ApplyDamage(damage, damagePosition);
        } // Organ will hurt itself if no additional info was provided

        else
        {
            if (IsActive)
            {
                foreach (CollisionShape3D shape in OwnColliders)
                {
                    if (colliderId == shape.GetInstanceId())
                    {
                        ApplyDamage(damage, damagePosition);
                        return;
                    }
                }
            } // If the weapon is still active, check if it was hurt

            foreach (Node node in GetChildren())
            {
                if (node is Organ organ)
                {
                    organ.Hurt(damage, damagePosition, colliderId);
                }
            } // Otherwise, propogate the function the tree to find the hurt organ
        }
    }

	protected bool CombineColliders(Godot.Collections.Array<CollisionShape3D> colliders)
	{
		if (IsNested)
		{
           return GetParent<Organ>().CombineColliders(colliders);
        }
		else
		{
            GuestColliders.Add(colliders);
            foreach (CollisionShape3D shape in colliders)
            {
                shape.Reparent(this);
            }
            return true;
        }
	}

	/// <summary>
    /// 	Attempt to return collider to original organ.
    /// </summary>
    /// <param name="collider">Collider to return</param>
    /// <param name="returnTo">Original owner</param>
	protected bool ReturnCollider(Godot.Collections.Array<CollisionShape3D> colliders, Node returnTo)
	{
		if (IsNested)
		{
            return GetParent<Organ>().ReturnCollider(colliders, returnTo);
        } // Propogate request up the tree if organ is nested
		else
		{
			int colliderIndex = GuestColliders.IndexOf(colliders);
			if (colliderIndex != -1)
			{
				GuestColliders.RemoveAt(colliderIndex);
                ((RigidBody3D)returnTo).AddCollisionExceptionWith(this);

                foreach (CollisionShape3D shape in colliders)
                {
                    shape.Reparent(returnTo);
                }
                
                return true;
            } // If collider found in collider list, return it to the original owner

			else
			{
				GD.PushError("Organ " + returnTo.Name + " requested collider but it was not found");
                return false;
            }
		}
    }


    /// <summary>
    /// 	Attempt to reparent collider to parent organ or take collider back from parent.
    /// </summary>
    /// <param name="ToParent">If true, attempt to give collider to top organ, else attempt to get collider back</param>
    protected bool AttemptColliderReparent(bool ToParent, Godot.Collections.Array<CollisionShape3D> customColliders = default, Node customNode = default)
	{
		if (ToParent)
		{
            if (GetParent().IsNodeReady())
			{
				ColliderReparented = true;
                if (GetParent() is Organ organParent)
                {
				    return organParent.CombineColliders((customColliders == default) ? OwnColliders : customColliders);
                }
                else
                {
                    CombineColliders((customColliders == default) ? OwnColliders : customColliders);
                }
			}
            else
            {
                GD.PushWarning("Parent organ is not ready for reparent yet.");
            }
        }
        else
		{
            if (IsNested)
            {
                return GetParent<Organ>().ReturnCollider((customColliders == default) ? OwnColliders : customColliders,
                                                        (customNode == default) ? this : customNode);
            }

            else if (customColliders != default && customNode != default)
            {
                return ReturnCollider(customColliders, customNode);
            }
            GD.PushError("Organ " + Name + " tried to get colliders from parent but it is unnested.");
        }

        return false;
    }

	/// <summary>
    /// 	Turn node to gibs by taking back its collider and unfreezing itself.
    /// </summary>
	protected void TurnToGibs()
	{
		if (GetParent() is Organ)
		{
            if (AttemptColliderReparent(false))
            {
                Reparent(GetTree().Root.GetNode("Main"));
                Freeze = false;
                IsNested = false;
            }
            else
            {
                GD.PushError("Reparent of organ failed!");
            }
		}
    }

    protected void Init()
    {
		if (GetParent() is Organ)
		{
            IsNested = true;
            FreezeMode = FreezeModeEnum.Static;
			Freeze = true;
            AttemptColliderReparent(true);
        }

        else
		{
            Freeze = false;
		} 

    }

    public override void _Ready()
    {
        OwnColliders = HR.GetChildrenOfType<CollisionShape3D>(this);
        CallDeferred("Init");
    }

    protected void DefaultPhysicsProcess()
    {
        if (OwnColliders is not null)
        {
            foreach (CollisionShape3D collider in OwnColliders)
            {
                collider.GlobalTransform = GlobalTransform;
            }
        }
    }

    public override void _PhysicsProcess(double delta)
    {
        DefaultPhysicsProcess();
        // if (IsNested && !ColliderReparented)
        // {
        //     CallDeferred("AttemptColliderReparent", true); // If the organ is nested, attempt to give it this organ's collider 
        // }
    }
}