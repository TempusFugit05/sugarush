using Godot;
using System;

public partial class Ui : Control
{
    [Export]
    Character CharNode;

	public override void _Ready()
	{
        GetNode<InteractionLabel>("InteractionLabel")?.Init(CharNode);
        GetNode<SugarLabel>("SugarInfoControl/SugarLabel")?.Init(CharNode);
        GetNode<SugarushStatus>("SugarInfoControl/SugarushStatus")?.Init(CharNode);
    }
}
