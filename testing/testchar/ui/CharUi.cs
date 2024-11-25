using Godot;

public partial class CharUi : Control
{
	Character P;
	SugarLabel sugarLabel;
	SugarushStatus sugarushStatus;
	InteractionLabel interactionLabel;


	public void init(Character character)
	{
		P = character;
		
		interactionLabel = GetNode<InteractionLabel>("InteractionLabel");
		sugarushStatus = GetNode<SugarushStatus>("SugarInfoControl/SugarushStatus");
		sugarLabel = GetNode<SugarLabel>("SugarInfoControl/SugarLabel");

		interactionLabel.Init(P);
		sugarLabel.Init(P);
		sugarushStatus.Init(P);
	} 

	public void Run()
	{
		sugarushStatus.Run();
		sugarLabel.Run();
	}
}