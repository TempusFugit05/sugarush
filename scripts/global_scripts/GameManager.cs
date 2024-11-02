using Godot;
using System;
using Helpers;

public partial class GameManager : Node
{

	private const string KeyExitGame = "exit";

	private void InitHelper()
	{
		HP.HelperNode Helper = new();
        GetTree().Root.AddChild(Helper);
	}

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		// Engine.MaxFps = (int)DisplayServer.ScreenGetRefreshRate();
		Engine.MaxFps = -1;
		DisplayServer.WindowSetMode(DisplayServer.WindowMode.Fullscreen);
		Input.MouseMode = Input.MouseModeEnum.Captured; // Disable cursor
        CallDeferred("InitHelper");
    }

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		// Quit game on button press
		if (Input.IsActionJustPressed(KeyExitGame))
		{
			GetTree().Root.PropagateNotification((int)NotificationWMCloseRequest);
			GetTree().Quit();
		}
	}
}
