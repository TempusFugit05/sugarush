using Godot;

public partial class Turret : Enemy
{
	// Called when the node enters the scene tree for the first time.
	Weapon TurretWeapon;
    Node3D PlayerNode;
    Node3D WeaponHolder;
    Node3D Body;

    [Export]
	private float RotationSpeed = 1.0f; // Speed of rotation in radians

	public override void _Ready()
	{
        Body = GetNode<Node3D>("Body");
        WeaponHolder = Body.GetNode<Node3D>("WeaponHolder");
		TurretWeapon = WeaponHolder.GetNode<Weapon>("TurretWeapon");
		PlayerNode = GetTree().Root.GetNode<Node3D>("Node3D/Character");
        InitNode();
	}

    private bool IsPlayerVisible()
    {
        PhysicsRayQueryParameters3D Quary = new()
        {
            From = GlobalPosition,
            To = PlayerNode.GlobalPosition,
            Exclude = new Godot.Collections.Array<Rid> { GetRid() },
            CollideWithAreas = false,
			CollideWithBodies = true,
        };
        Godot.Collections.Dictionary RayDict = GetWorld3D().DirectSpaceState.IntersectRay(Quary);
        if ((Node)RayDict["collider"] is Character)
        {
            return true;
        }
        
        return false;

    }

	private bool RotateTowardsPlayerY(double delta)
	{
        if (PlayerNode is not null)
        {
            Vector3 Difference = PlayerNode.GlobalPosition - GlobalPosition;
            float Angle = Mathf.PosMod(Mathf.Atan2(Difference.X, Difference.Z) - Body.GlobalRotation.Y + Mathf.Pi, Mathf.Tau);
            if (Angle >= 0.01f)
            {
                Body.RotateY(RotationSpeed * (float)delta * ((Angle > Mathf.Tau / 2) ? -1 : 1));
                return true;
            }
        }
        return false;
    }

	private bool RotateTowardsPlayerZ(double delta)
	{
        if (PlayerNode is not null)
        {
            Vector3 Difference = PlayerNode.GlobalPosition - GlobalPosition;
            float Angle = Mathf.PosMod(Mathf.Atan2(Difference.Z, Difference.X) - WeaponHolder.GlobalRotation.Z + Mathf.Pi, Mathf.Tau);
            if (Angle >= 0.01f)
            {
                WeaponHolder.GlobalRotate(Vector3.Right, RotationSpeed * (float)delta * ((Angle > Mathf.Tau / 2) ? -1 : 1));
                return true;
            }
        }
        return false;
    }

	public override void _PhysicsProcess(double delta)
	{
        if (PlayerNode is not null)
        {
            if (IsPlayerVisible())
            {
                if(!RotateTowardsPlayerY(delta) && TurretWeapon.ReadyToShoot())
                {
                    TurretWeapon.Shoot();
                }
            }
        }
    }
}
