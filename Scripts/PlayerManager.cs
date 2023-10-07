using Godot;
using System;
using System.Diagnostics;
using System.Globalization;

public partial class PlayerManager : Node2D
{
	private Player _player;
	private Gluboid _gluboid;

	private PlayerState _playerState;
	///<summary>
	///Grabs a reference to the player scene and the Gluboid, will change to list of gluboids.
	/// Also does first time randomizer setup.
	/// </summary>
	public override void _Ready()
	{
		_player = GetChild<Player>(0, false);
		_gluboid = GetChild<Gluboid>(1, false);
		GD.Randomize(); //Move to a more global place
	}

	/// <summary>
	/// Currently only passing Player position to the Gluboid, in future will add gluboid array management functions into here.
	/// </summary>
	/// <param name="delta"></param>
	public override void _PhysicsProcess(double delta)
	{
		_gluboid.SetPlayerPosition(GetPlayerPosition());
	}

	private Vector2 GetPlayerPosition()
	{
		return _player.GlobalPosition;
	}
}
