using Godot;
using System;

public partial class Ui : Control
{
    [Export]
    Character PlayerNode;

    SugarLabel SugarLabelNode;
	SugarushStatus SugarushStatusNode;

	public override void _Ready()
	{
        SugarLabelNode = GetNode<SugarLabel>("SugarInfoControl/SugarLabel");
		SugarushStatusNode = GetNode<SugarushStatus>("SugarInfoControl/SugarushStatus");
        SugarLabelNode.Init(PlayerNode);
		SugarushStatusNode.Init(PlayerNode);
    }
}
