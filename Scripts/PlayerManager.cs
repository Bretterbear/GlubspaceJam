using Godot;
using System;
using System.Diagnostics;
using System.Globalization;

public partial class PlayerManager : Node2D
{
	public float MaxDistanceFromPlayer = 200f;
	public float MinHopDistance = 40f;
	public float MaxHopDistance = 300f;
	private Player _player;
	private Gluboid _gluboid;

	private PlayerState _playerState;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		_player = GetChild<Player>(0, false);
		_gluboid = GetChild<Gluboid>(1, false);
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		Debug.WriteLine("Player Location: (" + GetPlayerPosition().X + "," + GetPlayerPosition().Y + ")");
		Debug.WriteLine("Gluboid Location: (" + GetGlubiodPosition(_gluboid).X + "," + GetGlubiodPosition(_gluboid).Y + ")");
		
	}

	public override void _PhysicsProcess(double delta)
	{
		_gluboid.SetPlayerPosition(_player.GlobalPosition);
	}

	private Vector2 GetPlayerPosition()
	{
		return _player.GlobalPosition;
	}

	private Vector2 GetGlubiodPosition(Gluboid gluboid)
	{
		return gluboid.GlobalPosition;
	}
}
