using Godot;
using System;
using System.Diagnostics;

public partial class Gluboid : CharacterBody2D
{
	private PlayerManager _playerManager;
	private Vector2 _playerPosition;
	private float _maxDistanceFromPlayer = 100f;
	private float _maxHopPower = 75f;
	private float _hopPower;
	private float _hopHeight = 500f;
	private GluboidState _state;
	private float _distanceFromPlayer;
	// Get the gravity from the project settings to be synced with RigidBody nodes.
	private float _gravity = ProjectSettings.GetSetting("physics/2d/default_gravity").AsSingle();

	public override void _Ready()
	{
		_state = GluboidState.Idle;
		_playerManager = GetParent<PlayerManager>();
	}
	public override void _PhysicsProcess(double delta)
	{
		var velocity = Velocity;
		SetDistanceFromPlayer(_playerPosition.X);
		
		//This branch handles the Four states for a normal gluboid while not being affected by player inputs
		//Idle indicates the gluboid is on the ground and within the maximum distance allowed from the player.
		//Falling indicates the Gluboid is above the ground and not moving in the horizontal direction.
		//Hopping indicates the Gluboid is moving towards the player through the air.
		//PreHopping accounts for getting the gluboid out into the air to start a Hop
		if (!IsOnFloor()) //Falling, PreHopping, or Hopping
		{
			velocity.Y += _gravity * (float)delta;
			if (_state == GluboidState.PreHopping)
			{
				_state = GluboidState.Hopping;
			}
			
			if (_state == GluboidState.Hopping)
			{
				velocity.X = _hopPower;
			}
			else
			{
				_state = GluboidState.Falling;
			}
			
			Velocity = velocity;
		}
		else if (_state == GluboidState.PreHopping) //PreHopping and still on the ground
		{
			velocity.X = _hopPower;
			Velocity = velocity;
		}
		else //Hopping and on the ground or Idle
		{
			//End of a Hop
			if (_state == GluboidState.Hopping) 
			{
				velocity.X = 0;
				_state = GluboidState.Idle;
				Velocity = velocity;
				Debug.WriteLine("Reset to Idle");
				Debug.WriteLine(_state);
			}

			//Checking to see if Gluboid needs to Hop
			var playerDistance = Math.Abs(_distanceFromPlayer);
			if (playerDistance > _maxDistanceFromPlayer && _state != GluboidState.PreHopping)
			{
				Debug.WriteLine("Start Hop");
				Hop();
			}
		}
		
		MoveAndSlide();
	}

	/// <summary>
	///Updates the local variable _playerPosition to the current global position of the player
	/// </summary>
	/// <param name="playerPosition"></param>
	public void SetPlayerPosition(Vector2 playerPosition)
	{
		_playerPosition = playerPosition;
	}
	
	/// <summary>
	///Calculates the Distance between the global position fo the playe rand this gluboid.
	/// </summary>
	/// <param name="playerX"></param>
	public void SetDistanceFromPlayer(float playerX)
	{
		_distanceFromPlayer = GlobalPosition.X - playerX;
	}

	/// <summary>
	/// Does the math to randomly pick the strength of the hop to close the distance to the player and then sets initially Y velocity for the hop.
	/// </summary>
	public void Hop()
	{
		var velocity = Velocity;
		velocity.Y = -1 * _hopHeight;
		
		//Need to convert distance here to absolute value for calculation
		var playerDistance = Math.Abs(_distanceFromPlayer);
		_hopPower = (float)GD.RandRange(playerDistance*(3f/4f), playerDistance+_maxHopPower);
		
		if (_distanceFromPlayer > 0) //Hop to the Right
		{
			_hopPower = -1 * _hopPower;
		}
		else //Hop to the left
		{
			_hopPower = 1 * _hopPower;
		}
		
		_state = GluboidState.PreHopping;
		velocity.X = _hopPower;
		Velocity = velocity;
	}
}
