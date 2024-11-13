using Helpers;
using Godot;


/// <summary>
///     Organs are the building blocks of creatures in the game. 
///     They can be used to define weak spots.
/// </summary>
public partial class Organ : RigidBody3D
{
    public float Health = 100.0f;
    public bool IsActive = true;
    public bool IsDestroyed = false;
    public bool IsVital = false;
    public bool OnHurtAfterDeath = false;
    protected bool ColliderReparented = false;
    public CreatureBase ColliderBank;

    [Export]
    protected bool GibsOnDeath = true;
    protected Godot.Collections.Array<CollisionShape3D> OwnColliders;

    [Signal]
    public delegate void OrganDestroyedEventHandler(Organ destroyedOrgan);

    public virtual void UseOrgan(){}
    public virtual void OnAddedToCreature(){}
    protected virtual void OnKill(){}
    protected virtual void OnHurt(){}

    /// <summary>
    ///     Emits signal to notify a connected soul that this organ was destroyed.
    /// </summary>
    public virtual void Kill()
    {
        OnKill();
        IsActive = false;
        IsDestroyed = true;
        if (GibsOnDeath)
		{
            CallDeferred("TurnToGibs");
        }
        
        foreach (Organ organ in HR.GetChildrenOfType<Organ>(this, true))
        {
            organ.Kill();
        }

        EmitSignal(SignalName.OrganDestroyed, this);
    }
    public void ApplyDamage(float damage, Vector3 damagePosition = default)
    {
        if (IsActive)
        {
            DamageIndicator Indicator = new(damage); // Create a damage indicator 
            GetTree().Root.AddChild(Indicator); // Add it to the scene
            Indicator.GlobalPosition = (damagePosition == default) ? GlobalPosition : damagePosition; // Set position of indicator to a specific position on body (ie bullethole) or object position for non specific damage soruce (ie fall damage)
        }
        if (!IsDestroyed)
        {
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
    }

    private void RestartCollision()
    {
        HR.ClearCollisionExceptions(this);
    }

	/// <summary>
    /// 	Turn node to gibs by taking back its collider and unfreezing itself.
    /// </summary>
	protected void TurnToGibs()
	{
        if (ColliderBank.ReturnCollider(this))
        {
            Reparent(GetTree().Root.GetNode("Main"));
            Freeze = false;
            // GetTree().CreateTimer(2f).Timeout += RestartCollision;
        }
        else
        {
            GD.PushError("Reparent of organ failed!");
        }
    }

    public void TakeColliders(CreatureBase parentBase)
    {
        ColliderBank = parentBase;
        ColliderBank.CombineColliders(new(OwnColliders, this));
        AddCollisionExceptionWith(ColliderBank);
        ColliderReparented = true;
    }

    private void DefferedInit()
    {                
        FreezeMode = FreezeModeEnum.Static;
        Freeze = true;
    }

    protected void Init()
    {
        OwnColliders = HR.GetChildrenOfType<CollisionShape3D>(this);
        CallDeferred("DefferedInit");
    }

    public override void _Ready()
    {
        Init();
    }

    protected void DefaultPhysicsProcess()
    {
        if (OwnColliders is not null)
        {
                MeshInstance3D mesh = GetNodeOrNull<MeshInstance3D>("MeshInstance3D");
                if (mesh is not null)
                {
                    foreach (CollisionShape3D collider in OwnColliders)
                    {
                        collider.GlobalTransform = mesh.GlobalTransform;
                    }
                }

        }
    }

    public override void _PhysicsProcess(double delta)
    {
        DefaultPhysicsProcess();
    }
}