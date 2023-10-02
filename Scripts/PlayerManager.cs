using Godot;
using System;
using System.Diagnostics;
using System.Globalization;

public partial class PlayerManager : Node2D
{
	private float MaxDistanceFromPlayer = 200f;
	private float MinHopDistance = 40f;
	private float MaxHopDistance = 300f;
	private Player _player;
	private Gluboid _gluboid;

	private PlayerState _playerState;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		_player = GetChild<Player>(0, false);
		_gluboid = GetChild<Gluboid>(0, false);
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		Debug.WriteLine("Player Location: (" + GetPlayerPosition().X.ToString() + "," + GetPlayerPosition().Y.ToString() + ")");
		Debug.WriteLine("Gluboid Location: (" + _gluboid.GlobalPosition.X.ToString() + "," + _gluboid.GlobalPosition.Y.ToString() + ")");
		
	}

	private Vector2 GetPlayerPosition()
	{
		return _player.GlobalPosition;
	}
}
