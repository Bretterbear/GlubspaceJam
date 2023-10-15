using Godot;

/// <summary>
/// As of now only responsible for calling up the pause menu on escape, attach to scene root node
/// </summary>
public partial class GameManager : Node2D
{
	// -------- Reference Variable Declarations  -------- //
	private PauseMenu pauseReference;					// Reference to the pause menu subscene

	// ------------- Constants Declarations ------------- //
	private string pathPauseMenu = "Player/Camera2D/Level_MenuPause";	// Path to the folder containing all levels

	/// <summary>
	/// Links manager to the pause menu on scene loading
	/// </summary>
	public override void _Ready()
	{
		// Make a link to the pause menu & spit a message if we can't get that link
		pauseReference = GetNode<PauseMenu>(pathPauseMenu);
		if (pauseReference == null)
		{
			GD.Print("Error(GameManager) - can't find pause menu node on path " +  pathPauseMenu);
		}

		// Make sure our pause menu isn't visible in playspace on scene load
		pauseReference.Visible = false;
	}

	/// <summary>
	/// Input handling for pause menu calls
	/// </summary>
	public override void _UnhandledInput(InputEvent @event)
	{
		if (@event.IsActionPressed("ui_cancel"))
		{
			// Used to gobble up the input & avoid double-calling immediately inside the pause menu to disengage
			GetViewport().SetInputAsHandled();

			// Pause & call up the menu
			GetTree().Paused = true;
			pauseReference.ShowMenu();
		}
	}
}
