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
	private float _snapDistance = 2000f;
	private GluboidState _state;
	private float _distanceFromPlayerX;

	public bool _isPlayer = false;
	// Get the gravity from the project settings to be synced with RigidBody nodes.
	private float _gravity = ProjectSettings.GetSetting("physics/2d/default_gravity").AsSingle();

	public void setup(Vector2 playerPosition)
	{
		GlobalPosition = playerPosition;
	}
	public override void _Ready()
	{
		AddToGroup("Gluboids");
		_state = GluboidState.Idle;
		_playerManager = GetParent<PlayerManager>();
	}
	public override void _PhysicsProcess(double delta)
	{
		//This branch handles the Five states for a normal gluboid while not being affected by player inputs
		//Idle indicates the gluboid is on the ground and within the maximum distance allowed from the player.
		//Falling indicates the Gluboid is above the ground and not moving in the horizontal direction.
		//Hopping indicates the Gluboid is moving towards the player through the air.
		//PreHopping accounts for getting the gluboid out into the air to start a Hop
		//IsPlayer represents that the Gluboid is the current visual representation of the Player.
		Debug.WriteLine(_state);
		if (_state != GluboidState.IsPlayer)
		{
			Debug.WriteLine(_distanceFromPlayerX);
			
			if (Math.Abs(_distanceFromPlayerX) > _snapDistance)
			{
				GlobalPosition = _playerPosition;
			}
			
			var velocity = Velocity;


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
				var playerDistance = Math.Abs(_distanceFromPlayerX);
				if (playerDistance > _maxDistanceFromPlayer && _state != GluboidState.PreHopping)
				{
					Debug.WriteLine("Start Hop");
					Hop();
				}
			}

			MoveAndSlide();
		}
		else
		{
			GlobalPosition = _playerPosition;
		}
	}

	/// <summary>
	///Updates the local variable _playerPosition to the current global position of the player
	/// </summary>
	/// <param name="playerPosition"></param>
	public void SetPlayerPosition(Vector2 playerPosition)
	{
		Debug.WriteLine("Distance invoked");
		_playerPosition = playerPosition;
		_distanceFromPlayerX = GlobalPosition.X - _playerPosition.X;
		
	}

	/// <summary>
	/// Does the math to randomly pick the strength of the hop to close the distance to the player and then sets initially Y velocity for the hop.
	/// </summary>
	public void Hop()
	{
		var velocity = Velocity;
		velocity.Y = -1 * _hopHeight;
		
		//Need to convert distance here to absolute value for calculation
		var playerDistance = Math.Abs(_distanceFromPlayerX);
		_hopPower = (float)GD.RandRange(playerDistance*(3f/4f), playerDistance+_maxHopPower);
		
		if (_distanceFromPlayerX > 0) //Hop to the Right
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

	public void MakePlayer()
	{
		_state = GluboidState.IsPlayer;
		_isPlayer = true;
	}

	public void MakeNotPlayer()
	{
		_state = GluboidState.Idle;
		_isPlayer = false;
	}
}
