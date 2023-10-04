using Godot;
using System;

public partial class Gluboid : CharacterBody2D
{
	private PlayerManager _playerManager;
	private Vector2 _playerPosition;
	private float _MaxDistanceFromPlayer;
	private float _MinHopDistance;
	private float _MaxHopDistance;
	private GluboidState _state;

	public float _distanceFromPlayer;
	// Get the gravity from the project settings to be synced with RigidBody nodes.
	public float gravity = ProjectSettings.GetSetting("physics/2d/default_gravity").AsSingle();

	public override void _Ready()
	{
		_playerManager = GetParent<PlayerManager>();
		_MaxDistanceFromPlayer = _playerManager.MaxDistanceFromPlayer;
		_MaxHopDistance = _playerManager.MaxHopDistance;
		_MinHopDistance = _playerManager.MinHopDistance;
	}
	public override void _PhysicsProcess(double delta)
	{
		Vector2 velocity = Velocity;
		SetDistanceFromPlayer(_playerPosition.X);
		// Add the gravity.
		if (!IsOnFloor())
		{
			velocity.Y += gravity * (float)delta;
			_state = GluboidState.Falling;
		}
		else
		{
			_state = GluboidState.Idle;
		}


		Velocity = velocity;
		MoveAndSlide();
	}

	public void SetPlayerPosition(Vector2 playerPosition)
	{
		_playerPosition = playerPosition;
	}
	public void SetDistanceFromPlayer(float playerX)
	{
		_distanceFromPlayer = GlobalPosition.X - playerX;
	}
}
