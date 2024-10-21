using Godot;
using GlobalEnums;

public partial class DamageIndicator : Label3D
{	
	[Export]
	private float LifeTime = 1f; // Lifespan of the indicator after which it will be deleted
	
	[Export]
	private float StartScale = 0.15f;

	[Export]
	private float EndScale = 0; // This is the size of the indicator on the viewport and will remain constant despite distance

	[Export]
	private float Speed = 0.2f;

	[Export]
	private Color IndicatorColor = new(0.929f, 0.169f, 0.169f, 1);

	[Export]
	private Color OutlineColor = new(0.0f, 0.0f, 0.0f, 1);

	[Export]
	private float MaxDistanceFromOrigin = 0.75f; // This is used to slow down the indicator as it moves away from its spawnpoint

	[Export]
	private float MinDistanceToOrigin = 0.1f; // This distance will be used in the first iteration as the distance from the origin will be 0

    Camera3D PlayerCamera;

	Vector3 PositionOrigin; // The position of the label spwan point

    private Vector3 RandSpeedFactor; // Random factor to scale speed

    private bool IsInitialized = false;

    public DamageIndicator(float damage)
	{
		InitRandomSpeeds();
		Modulate = IndicatorColor;
		Text = damage.ToString();
	}

	/// <summary>
    /// 	Create a vector of 3 random values between -1 and 1 to act as random modifiers for the indicator's speed.
    /// </summary>
	private void InitRandomSpeeds()
	{
		RandSpeedFactor = new((GD.Randf() * 2) - 1 , (GD.Randf() * 2) - 1 , (GD.Randf() * 2) - 1 ); // Create 3 random values between -1 and 1
        RandSpeedFactor /= Mathf.Abs(RandSpeedFactor.X) + Mathf.Abs(RandSpeedFactor.Y) + Mathf.Abs(RandSpeedFactor.Z); // Divide all of them by the sum of absolute values; The sum of absolute values will be exactly 1
    }

	/// <summary>
    /// 	Add random movement to all directions, slowing down as the label gets further from the origin
    /// </summary>
	private void ApplyAnimation(double delta)
	{
		Vector3 NewPos = GlobalPosition;
        NewPos += RandSpeedFactor * (Speed / Mathf.Clamp(PositionOrigin.DistanceTo(GlobalPosition) / MaxDistanceFromOrigin, MinDistanceToOrigin, MaxDistanceFromOrigin)) * (float)delta;
        GlobalPosition = NewPos;
	}

	/// <summary>
    /// 	Creates the tweens for the fading and scaling animations.
    /// </summary>
    /// <returns>The tween to which the QueueFree callback should be attached</returns>
	private Tween InitAnimations()
	{
        Tween FadeTween;
		Tween ScaleTween;
        /*Fade label and then destroy at the end*/
        FadeTween = GetTree().CreateTween();
        FadeTween.TweenProperty(this, "modulate", new Color(IndicatorColor.R, IndicatorColor.G, IndicatorColor.B, 0), LifeTime);
		FadeTween.Parallel().TweenProperty(this, "outline_modulate", new Color(OutlineColor.R, OutlineColor.G, OutlineColor.B, 0), LifeTime);

		/*Scaling animation to make the label shrink*/
        ScaleTween = GetTree().CreateTween();
        ScaleTween.SetTrans(Tween.TransitionType.Spring);
        ScaleTween.TweenProperty(this, "scale", new Vector3(EndScale, EndScale, EndScale), LifeTime);

        return FadeTween;
    }

	/// <summary>
    /// 	Initializes the origin position for the random speed calculations.
    /// 	After initialization, removes itself from the signal.
    /// </summary>
	private void InitNode()
	{
		PositionOrigin = GlobalPosition; // Get current (initialized) position
        MaxDistanceFromOrigin *= PositionOrigin.DistanceTo(PlayerCamera.GlobalPosition); // Scale the max distance by the distance from the camera to prevent indicators looking too close at a distance
        InitAnimations().Finished += QueueFree; // Attach signal to destroy node at the end of fading animation
        IsInitialized = true;

        GetTree().ProcessFrame -= InitNode; // Remove signal since it's only needed once
    }

	public override void _Ready()
	{
		SetLayerMask((uint)RenderingLayersEnum.UiElemetns); // Set layer mask to not be affected by decals
        FixedSize = true;
        Scale = new(StartScale, StartScale, StartScale);
		Billboard = BaseMaterial3D.BillboardModeEnum.Enabled; // This makes the indicator always face the camera
		NoDepthTest = true; // This will cause the label to be drawn in front of other objects

		PlayerCamera = GetViewport().GetCamera3D();
		MaxDistanceFromOrigin += PositionOrigin.DistanceTo(GlobalPosition) * (Mathf.Pi / 180.0f) * (PlayerCamera.Fov / 2) * 2; // Scale distance from origin by distance from camera to prevend idicators bunching up when far away from enemy
		
        GetTree().ProcessFrame += InitNode;
    }

    public override void _Process(double delta)
    {
        if (IsInitialized)
        {
			ApplyAnimation(delta);
        }
    }
}
