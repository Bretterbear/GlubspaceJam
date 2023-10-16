using Godot;
using System;

public partial class end_of_level : Area2D
{
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	public void EndOfLevel(Node2D body)
	{
		GetTree().ChangeSceneToFile("res://Scenes/Levels/Victory Scene.tscn");
	}
}
