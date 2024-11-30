using Helpers;
using Godot;
using System.Collections.Generic;
using System.Linq;

/// <summary>
///     Organs are the building blocks of creatures in the game. 
///     They can be used to define weak spots.
/// </summary>
public partial class Organ : RigidBody3D, ICreature
{
    public enum DestructModeEnum
    {
        Gibs,
        Delete,
        Nothing,
        Default = Nothing
    }

    public struct OrganColliders
	{
        public Godot.Collections.Array<CollisionShape3D> Colliders;
        public Node Owner;

		public OrganColliders (Godot.Collections.Array<CollisionShape3D> colliders, Node owner)
		{
            Colliders = colliders;
            Owner = owner;
        }
    }

    protected struct OrganStateStruct
    {
        public bool Alive = true;
        public bool Destroyed = false;
        public OrganStateStruct(){}
    }

    public struct OrganSettingStruct
    {
        public bool Vital = false;
        public bool UpdateColliders = false;

        public float OriginalMass = 0.01f;
        public float MaxHealth = 100.0f;
        public float MaxDestructionHealth = 500.0f;
        public float Shielding = 50.0f;
        public float GibsFadeStart = 10.0f;
        public float GibsFadeTime = 5.0f;

        public DestructModeEnum DestructMode = DestructModeEnum.Gibs;

        public OrganSettingStruct(){}
    }

    public float Health { get; protected set; }
    public float DestructionHealth { get; protected set; }

    protected OrganSettingStruct OrganSettings = new();
    protected OrganStateStruct OrganState = new();
    protected Aabb Hitbox;

    public Organ OrganBase;
    protected LinkedList<OrganColliders> GuestColliders = new();
    protected Godot.Collections.Array<CollisionShape3D> OwnColliders;
    protected Node3D TransformReference;
    protected Godot.Collections.Array<Rid> OrganRids = new();

    public virtual void UseOrgan(){}
    public virtual void OnAddedToCreature(){}
    protected virtual void OnKill(){}
    protected virtual void OnHurt(){}
    protected virtual void OnDelete(){}
    protected virtual void OnDestroy(){}
    protected virtual void InitOrgan(){}
    protected virtual void OrganPhysicsProcess(double delta){}

    public bool IsAlive()
    {
        return OrganState.Alive;
    }

    /// <summary>
    ///     Take the colliders of all child organs.
    /// </summary>
    private void AmalgamateOrgans()
	{
        foreach (MeshInstance3D mesh in HR.GetChildrenOfType<MeshInstance3D>(this, true))
        {
            Hitbox = Hitbox.Merge(mesh.GetAabb());
        }
        OrganRids.Add(GetRid());

        foreach (RigidBody3D node in HR.GetChildrenOfType<RigidBody3D>(this, true))
		{
            OrganRids.Add(node.GetRid());
            if (node is Organ organ)
            {
                organ.TakeColliders(this);
                organ.OnAddedToCreature();
                organ.OrganBase = this; // Update new parent
            }
        } // Take the colliders of all child organs
	}

    /// <summary>
    ///     Apply fade out animation to whole branch of organs.
    ///     This is used for gibs.
    /// </summary>
    private void FadeBranch()
    {
        StandardMaterial3D transperancyMaterial = (StandardMaterial3D)ResourceLoader.Load("res://assets/materials/TrasparancyMaterial.tres").Duplicate();
        transperancyMaterial.Transparency = BaseMaterial3D.TransparencyEnum.AlphaDepthPrePass;
        Color newColor = transperancyMaterial.AlbedoColor;
        newColor.A = 0f;
        CreateTween().TweenProperty(transperancyMaterial, "albedo_color", newColor, OrganSettings.GibsFadeTime).Finished += QueueFree;

        foreach (MeshInstance3D mesh in HR.GetChildrenOfType<MeshInstance3D>(this, true))
        {
            mesh.SetSurfaceOverrideMaterial(mesh.GetSurfaceOverrideMaterialCount() - 1, transperancyMaterial);
        }
    }

    /// <summary>
    /// 	Attempt to return collider to original organ.
    /// </summary>
    /// <param name="collider">Collider to return</param>
    /// <param name="returnTo">Original owner</param>
    public bool ReturnCollider(Node returnTo)
	{
        LinkedListNode<OrganColliders> currentItem = GuestColliders.First;
		while (currentItem != null)
		{
            OrganColliders stored = currentItem.Value;
            if (stored.Owner == returnTo)
			{
				foreach (CollisionShape3D collider in stored.Colliders)
				{
                    collider.Reparent(returnTo);
                }

                if (returnTo is RigidBody3D body)
                {
                    Mass -= body.Mass;
                } // Update own mass

                GuestColliders.Remove(currentItem);
                return true;
            } // If colliders were found in collider list, return them to the original owner

            currentItem = currentItem.Next;
        } // Iterate through collider list until owner is found

		GD.PushError("Organ " + returnTo.Name + " requested collider but it was not found");
		return false;
	}

    /// <summary>
    ///     Combine colliders of an organ with this one.
    /// </summary>
    /// <param name="colliders">The colliders of the organ.</param>
	public void MergeColliders(OrganColliders colliders)
	{
        if (colliders.Owner is RigidBody3D body)
        {
            Mass += body.Mass;
        } // Update mass of this organ
        GuestColliders.AddLast(colliders);
        foreach (CollisionShape3D shape in colliders.Colliders)
        {
            shape.Reparent(this);
        }
	}

    /// <summary>
    ///     Destroy organ, turning it to gibs or deleting it.
    /// </summary>
    protected void Destroy()
    {
        if (!OrganState.Destroyed)
        {
            OrganState.Destroyed = true;
            OnDestroy();

            switch (OrganSettings.DestructMode)
            {
                case DestructModeEnum.Gibs:
                    CallDeferred("TurnToGibs");
                    break;
                case DestructModeEnum.Delete:
                    QueueFree();
                    break;
            }
        }
    }

    private void KillOrgan(bool recursive = false)
    {
        OrganState.Alive = false;

        if (recursive)
        {
            foreach (Organ organ in HR.GetChildrenOfType<Organ>(this, true))
            {
                organ.KillOrgan();
            }
        }
        OnKill();

        // if (OrganBase is CreatureBase creature)
        // {
        //     creature.NotifyKilled(this, OrganSettings);
        // }
    }

    /// <summary>
    ///     Kill organ.
    ///     Part of ICreature.
    /// </summary>
    public virtual void Kill()
    {
        if (OrganState.Alive)
        {
            KillOrgan(recursive: true);
        }
    }

    public virtual void Heal(float amount)
    {
        if (Health < OrganSettings.MaxHealth)
        {
            Health += amount;
            if (Health > OrganSettings.MaxHealth)
            {
                Health = OrganSettings.MaxHealth;
            }
        }
    }

    /// <summary>
    ///     Apply damage to organ.
    ///     Part of ICreature.
    /// </summary>
    /// <param name="damage">Amount of damage to apply.</param>
    /// <param name="damagePosition">Where damage was applied.</param>
    public void ApplyDamage(float damage, Vector3 damagePosition = default, bool createIndicator = true)
    {
        if (OrganState.Alive)
        {
            if (createIndicator)
            {
                HP.CreateDamageIndicator(damage, (damagePosition == default) ? GlobalPosition : damagePosition);
            }

            // if (OrganBase is CreatureBase creature)
            // {
            //     creature.ApplyDamage(damage * (OrganSettings.Shielding / 100), createIndicator: false);
            // }

            Health -= damage;
            if (Health <= 0)
            {
                Health = 0;
                Kill();
            }
            else
            {
                OnHurt();
            }
        } // Apply organ damage.

        if (!OrganState.Destroyed)
        {
            DestructionHealth -= damage;

            if (DestructionHealth <= 0)
            {
                Destroy();
            }
        } // Apply destruction damage.
    }

    /// <summary>
    ///     Hurt organ.
    ///     Part of ICreature.
    /// </summary>
    public void Hurt(float damage, Vector3 damagePosition = default, ulong colliderId = default)
    {
        if (colliderId == default)
        {
            ApplyDamage(damage, damagePosition);
            return;
        } // If this specific organ was damaged, apply damage

        foreach (OrganColliders colliderList in GuestColliders)
		{
			foreach (CollisionShape3D shape in colliderList.Colliders)
			{
				if (colliderId == shape.GetInstanceId() && colliderList.Owner is Organ organ)
				{
					organ.Hurt(damage, damagePosition);
					return;
				}
			} // Find the organ of which the collider was damaged.
		}
    }

	/// <summary>
    /// 	Turn node to gibs by taking back its collider and unfreezing itself.
    /// </summary>
	protected void TurnToGibs()
	{
        // if (this is not CreatureBase && OrganBase is not null)
        // {
        //     OrganBase?.ReturnCollider(this); // If organ has its own colliders
        //     AddCollisionExceptionWith(OrganBase);
        //     Freeze = false;
        //     AmalgamateOrgans();
        //     OrganBase = null;
        //     Reparent(GetTree().Root.GetNode("Main"));
        // }
        GetTree().CreateTimer(OrganSettings.GibsFadeStart).Timeout += FadeBranch;
    }

    /// <summary>
    ///     Take colliders from this organ.
    ///     Used to create a single entity from multiple organs.
    /// </summary>
    /// <param name="newParent">Organ node that takes the colliders</param>
    public void TakeColliders(Organ newParent)
    {
        OrganBase?.ReturnCollider(this);
        OwnColliders = HR.GetChildrenOfType<CollisionShape3D>(this);
        OrganBase = newParent;
        OrganBase.MergeColliders(new(OwnColliders, this));
    }

    /// <summary>
    ///     Get the transform 
    /// </summary>
    /// <param name="node"></param>
    /// <returns></returns>
    protected Node3D GetNodeTransformReference(Node3D node)
    {
        Godot.Collections.Array<MeshInstance3D> meshes = HR.GetChildrenOfType<MeshInstance3D>(node);

        if (meshes.Count != 0)
        {
            return meshes[0];
        }
        return this;
    }

    /// <summary>
    ///     Apply settings for physical objects.
    ///     Called on deffered to prevent unexpected behavior.
    /// </summary>
    private void DefferedInit()
    {                
        FreezeMode = FreezeModeEnum.Static;
        if (OrganBase is not null)
        {
            Freeze = true;
        }
    }

    /// <summary>
    ///     Initialize organ.
    ///     Must be called for every organ in _Ready function.
    /// </summary>
    protected void Init()
    {
        OrganRids.Add(GetRid());
        OwnColliders = HR.GetChildrenOfType<CollisionShape3D>(this);

        // if (this is CreatureBase)
        // {
        //     AmalgamateOrgans();
        // }
        
        TransformReference = GetNodeTransformReference(this);
        
        /*Initialize parameters*/
        OrganSettings.OriginalMass = Mass;
        Health = OrganSettings.MaxHealth;
        DestructionHealth = OrganSettings.MaxDestructionHealth;

        CallDeferred("DefferedInit");

        InitOrgan();
    }

    private void DeleteCollidersOf(Node node)
    {
        LinkedListNode<OrganColliders> currentNode = GuestColliders.First;
        while (currentNode is not null)
        {
            if (currentNode.Value.Owner == node)
            {
                foreach (CollisionShape3D collider in currentNode.Value.Colliders)
                {
                    collider.QueueFree();
                }
                GuestColliders.Remove(currentNode);
            }
            currentNode = currentNode.Next;
        }
    }

    public override void _ExitTree()
    {
        // OnDelete();
        // OrganBase?.DeleteCollidersOf(this);
    }

    public override void _Ready()
    {
        Init();
    }
    
    protected void UpdateCollidersOf(Transform3D referenceTransform, Godot.Collections.Array<CollisionShape3D> Colliders)
    {
        if (OrganBase != null && Colliders is not null)
        {
            foreach (CollisionShape3D collider in Colliders)
            {
                collider.GlobalTransform = referenceTransform;
            }
        }
    }

    public override void _PhysicsProcess(double delta)
    {
        OrganPhysicsProcess(delta);
        if (TransformReference is not null)
        {
            CallDeferred("UpdateCollidersOf", TransformReference.GlobalTransform, OwnColliders);
        }
    }
}