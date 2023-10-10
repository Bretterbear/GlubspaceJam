using Godot;

/// <summary>
/// As of now only handles pausing the game, returning to main menu and resuming \n
/// NOTE - maybe add a restart level option here, might be nice for puzzly confusion
/// </summary>
public partial class PauseMenu : Control
{
    // ------------- Constants Declarations ------------- //
    private string pathMainMenu = "res://Scenes/Levels_NonIndexable/Level_Menu-Main.tscn";	// Path to the folder containing all levels

    /// <summary>
    /// Double checks the game is paused and makes the pausemenu visible \n
    /// Called from GameManager (which should be attached to active scene's root node)
    /// </summary>
    public void ShowMenu()
    {
        // Make sure menu is visible - game should already be paused, but it can't hurt to double check
        Visible = true;
        GetTree().Paused = true; // Pause the game

        // May need to add code to set mouse mode depending on mouse vis in game
    }

    /// <summary>
    /// Simple resume button call - also accessed via "escape" in while in pause menu
    /// </summary>
    private void _OnButtonResumePressed()
    {
        // Hide pause menu & turn off pause
        Visible = false; 
        GetTree().Paused = false;

        // May need to add mousemode hiding into here depending on mouse vis in game
    }

    private void _OnButtonMainMenuPressed()
    {
        // Try initiating a scene load - catch the return message (should be 'Error.Ok')
        Error loadStatus = GetTree().ChangeSceneToFile(pathMainMenu);

        // Toss a message if we're not set up to load nicely
        switch (loadStatus)
        {
            case Error.Ok:
                break;
            case Error.CantOpen:
                GD.Print("Error - scene " + pathMainMenu + " can't be loaded into a packedScene");
                break;
            case Error.CantCreate:
                GD.Print("Error - scene " + pathMainMenu + " can't be instantiated properly");
                break;
            default:
                break;
        }

        // Unpause the game before jumping to the main menu (bugfix)
        GetTree().Paused = false;
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        if (@event.IsActionPressed("ui_cancel"))
        {
            // Used to gobble up the input & avoid double-calling immediately inside the pause menu to disengage
            GetViewport().SetInputAsHandled();

            // Call our standard resume as if we'd pressed the button
            _OnButtonResumePressed();
        }
    }
}
