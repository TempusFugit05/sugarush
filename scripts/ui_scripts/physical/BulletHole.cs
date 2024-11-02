using System;
using Godot;
using Helpers;

public partial class BulletHole : Decal
{
    [Export]
    private float LifeTime = 8.0f;

    [Export]
    private float StartOpacity = 1.0f;

    [Export]
    private float EndOpacity = 0.0f;

    [Export]
    private float FadeStartTime = 5; // Time at which fading will start

    private float TimeToLive; // Remaining lifetime of the decal

    private Node3D ParentNode;

    private int PoolIndex; // The decal's index in the global decal pool

    Vector3 OriginNormal;
    Vector3 OriginalRotation;
    Tween FadeTween = null;

    /// <summary>
    /// Rotate the decal to project the image onto the surface that was hit
    /// </summary>
    /// <param name="Normal">The normal returned by the raycasting quary of a bullet</param> 
    private void RotateOnSurface(Vector3 normal)
    {
        if (normal.Y != Vector3.Up.Y && normal.Y != Vector3.Down.Y)
        { // LookAt() Fails if the rotation of the object is parallel to the axis around which we are rotating it
            LookAt(GlobalTransform.Origin - normal, Vector3.Up);
            RotateObjectLocal(Vector3.Right, 90);
        }
        RotateObjectLocal(Vector3.Up, (GD.Randf() - 0.5f) * 360); // Rotate the decal randomly on the surface
    }

    /// <summary>
    ///     Initializes parameters related to the decal sprite and rotates it onto the surface
    /// </summary>
    public void InitDecal(Vector3 surfaceNormal)
    {
        OriginNormal = surfaceNormal;
        OriginalRotation = GetParent<Node3D>().GlobalRotation;
        RotateOnSurface(surfaceNormal);
    }

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        TimeToLive = LifeTime;
        ParentNode = GetParent<Node3D>();
        GetTree().CreateTimer(LifeTime - FadeStartTime).Timeout += StartFade;
        HP.AddToDecalPool(this);
    }

	/// <summary>
    /// Start the fading animation after <c>FadeStartTime</c> lasting <c>LifeTime - FadeStartTime</c> seconds.
    /// Then destroys the node.
    /// </summary>
    private void StartFade()
    {
        // await ToSignal(GetTree().CreateTimer(FadeStartTime), SceneTreeTimer.SignalName.Timeout);
        FadeTween = CreateTween();
        FadeTween.TweenProperty(this, "modulate", new Color(0, 0, 0, 0), LifeTime - FadeStartTime);
        FadeTween.Finished += DeInitNode;
    }

    public void DeInitNode()
    {
        HP.RemoveFromDecalPool(this);
        QueueFree();
    }
}
