using Godot;
using System;
using System.Transactions;

/// <summary>
/// We're going to use this later to make the camera have whatever scheme we want
/// </summary>
public partial class CameraFollowControl : Camera2D
{    
    private readonly Vector2 SCREEN_SIZE = new Vector2(1150, 600);
    private Vector2 cur_screen = new Vector2(0, 0);
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
    {
        TopLevel = true;
        GlobalPosition = ((Node2D)GetParent()).GlobalPosition;
        UpdateScreen(cur_screen);
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
        Vector2 parent_screen = (((Node2D)GetParent()).GlobalPosition / SCREEN_SIZE).Floor();
        if (!parent_screen.IsEqualApprox(cur_screen))
        {
            UpdateScreen(parent_screen);
        }
	}

    private void UpdateScreen(Vector2 new_screen)
    {
        cur_screen = new_screen;
        GlobalPosition = cur_screen * SCREEN_SIZE + SCREEN_SIZE * 0.5f;
    }
}
