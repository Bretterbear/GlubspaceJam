using Godot;
using System;
using System.Diagnostics;

public partial class Gluboid : CharacterBody2D
{
	private PlayerManager _playerManager;
	private Vector2 _playerPosition;
	private float _MaxDistanceFromPlayer;
	private float _MinHopPower;
	private float _MaxHopPower;
	private float _HopPower;
	private float _HopHeight = 500f;
	private GluboidState _state;

	public float _distanceFromPlayer;
	// Get the gravity from the project settings to be synced with RigidBody nodes.
	public float gravity = ProjectSettings.GetSetting("physics/2d/default_gravity").AsSingle();

	public override void _Ready()
	{
		_state = GluboidState.Idle;
		_playerManager = GetParent<PlayerManager>();
		_MaxDistanceFromPlayer = _playerManager.MaxDistanceFromPlayer;
		_MaxHopPower = _playerManager.MaxHopDistance;
		_MinHopPower = _playerManager.MinHopDistance;
		
	}
	public override void _PhysicsProcess(double delta)
	{
		Vector2 velocity = Velocity;
		SetDistanceFromPlayer(_playerPosition.X);
		// Add the gravity.
		if (!IsOnFloor())
		{
			velocity.Y += gravity * (float)delta;
			if (_state == GluboidState.PreHopping)
			{
				_state = GluboidState.Hopping;
			}
			
			if (_state == GluboidState.Hopping)
			{
				velocity.X = _HopPower;
			}
			else
			{
				_state = GluboidState.Falling;
			}
			
			Velocity = velocity;
		}
		else if (_state == GluboidState.PreHopping)
		{
			velocity.X = _HopPower;
			Velocity = velocity;
		}
		else
		{
			if (_state == GluboidState.Hopping)
			{
				velocity.X = 0;
				_state = GluboidState.Idle;
				Velocity = velocity;
				Debug.WriteLine("Reset to Idle");
				Debug.WriteLine(_state);
			}

			var playerDistance = Math.Abs(_distanceFromPlayer);
			if (playerDistance > _MaxDistanceFromPlayer && _state != GluboidState.PreHopping)
			{
				Debug.WriteLine("Start Hop");
				Hop();
			}
		}
		
		MoveAndSlide();
	}

	public void SetPlayerPosition(Vector2 playerPosition)
	{
		_playerPosition = playerPosition;
	}
	public void SetDistanceFromPlayer(float playerX)
	{
		_distanceFromPlayer = GlobalPosition.X - playerX;
		Debug.WriteLine(_distanceFromPlayer.ToString());
	}

	public void Hop()
	{
		var velocity = Velocity;
		velocity.Y = -1 * _HopHeight;
		Debug.WriteLine(velocity.Y);
		var playerDistance = Math.Abs(_distanceFromPlayer);
		_HopPower = (float)GD.RandRange(playerDistance*(3f/4f), playerDistance+_MaxHopPower);
		
		Debug.WriteLine(_distanceFromPlayer);
		if (_distanceFromPlayer > 0)
		{
			_HopPower = -1 * _HopPower;
			Debug.WriteLine("Positive Hop");
		}
		else
		{
			_HopPower = 1 * _HopPower;
			Debug.WriteLine("Negative Hop");
		}

		_state = GluboidState.PreHopping;
		velocity.X = _HopPower;
		Velocity = velocity;
	}
}
