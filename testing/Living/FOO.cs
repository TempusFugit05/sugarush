using Godot;

public partial class foo : Organ
{
	protected override void OnHurt()
	{
        QueueFree();
    }
    public override void _Ready()
    {
        Init();
    }
}
