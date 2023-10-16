using Godot;
using System;

public partial class CreditsScroll : Control
{
	private float _scrollSpeed = 40f;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		var position = GlobalPosition;
		position.Y -= _scrollSpeed * (float)delta;
		GlobalPosition = position;
	}
}
