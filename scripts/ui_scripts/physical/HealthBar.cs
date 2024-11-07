using Godot;
public partial class HealthBar : Sprite3D
{
	Label HealthText;
    ProgressBar HealthProgressBar;
    const string UiElementsPath = "ViewPort/Control/";

    public override void _Ready()
	{
 		HealthText = GetNode<Label>(UiElementsPath + "Label");
        HealthProgressBar = GetNode<ProgressBar>(UiElementsPath + "ProgressBar");
    }

    public override void _PhysicsProcess(double delta)
    {
        GlobalRotation = Vector3.Zero;
    }
    
	/// <summary>
    /// 	Update the healthbar to display current hp
    /// </summary>
    /// <param name="CurrentHealth">Current health of the target</param>
    /// <param name="MaxHealth">Maximum health of target</param>
	public void SetHealthPoint(float CurrentHealth, float MaxHealth)
	{
		HealthProgressBar.Value = (CurrentHealth / MaxHealth) * 100;
		if (CurrentHealth ==  float.PositiveInfinity)
        {
            HealthText.Text = "IMMORTAL!";
        }
        else if (CurrentHealth ==  float.NegativeInfinity)
        {
            HealthText.Text = "UNDEAD!";
        }
        else
        {
            HealthText.Text = Mathf.RoundToInt(CurrentHealth).ToString();	
        }
	}
}
