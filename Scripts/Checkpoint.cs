using Godot;
using System;
using System.Diagnostics;

public partial class Checkpoint : Area2D
{
	// Called when the node enters the scene tree for the first time.
	private bool _reached;

	public void PlayerReached(Node2D body)
	{
		var checkpoint = GlobalPosition;
		if (!_reached)
		{
			GameManager.GetGameManager().SetCheckPoint(checkpoint);
			Debug.WriteLine("Tada!");
		}

		_reached = true;
	}
	
}
