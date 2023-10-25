using Godot;
using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using GlubspaceJam.Scripts;

public partial class Gluboid : CharacterBody2D
{
	private PlayerManager _playerManager;
	private Vector2 _playerPosition;
	private float _maxDistanceFromPlayer = 50f;
	private float _maxHopPower = 150f;
	private float _hopPower;
	private float _hopHeight = 100f;
	private float _snapDistance = 2000f;
	private GluboidState _state = GluboidState.Idle;
	private float _distanceFromPlayerX;
	private Vector2 _extendPosition;
	private float _extendDistance = 64;
	private float _extendSpeed = 50;
	private int _index;
	private float _groupSpeed = 50;
	private const float _hopRatio = 5f / 6f;
	private bool _wasFloored = true;
	private AnimatedSprite2D _sprite;
	
		// ----------- Audio Components -----------//
	private AudioStreamPlayer2D FoleyLandSound;
	private AudioStreamPlayer2D NagSound;
	private AudioStreamPlayer2D GlubHopSound;
	private AudioStreamPlayer2D InteractJoinSound;
	private AudioStreamPlayer2D GlubGatherSound;

	public bool _isPlayer = false;
	// Get the gravity from the project settings to be synced with RigidBody nodes.
	private float _gravity = ProjectSettings.GetSetting("physics/2d/default_gravity").AsSingle();

	public void setup(Vector2 position, int index, AnimatedSprite2D skin)
	{
		GlobalPosition = position;
		_index = index;
		_sprite = skin;
		AddChild(skin);
	}

	public override void _Ready()
	{
		AddToGroup("Gluboids");
		_playerManager = GetParent<PlayerManager>();
		
		FoleyLandSound = GetNode<AudioStreamPlayer2D>("FoleyLand");
		NagSound = GetNode<AudioStreamPlayer2D>("Nag");
		GlubHopSound = GetNode<AudioStreamPlayer2D>("GlubHop");
	}
	public override void _PhysicsProcess(double delta)
	{
		//This branch handles the Five states for a normal gluboid while not being affected by player inputs
		//Idle indicates the gluboid is on the ground and within the maximum distance allowed from the player.
		//Falling indicates the Gluboid is above the ground and not moving in the horizontal direction.
		//Hopping indicates the Gluboid is moving towards the player through the air.
		//PreHopping accounts for getting the gluboid out into the air to start a Hop
		//IsPlayer represents that the Gluboid is the current visual representation of the Player.
		//Grouping is the state before extending.
		//Debug.WriteLine(_state);
		//Debug.WriteLine("Player X:" + _playerPosition.X + " Player Y:" + _playerPosition.Y);
		//Debug.WriteLine("Glub X:" + GlobalPosition.X + " Glub Y:" + GlobalPosition.Y);
		
		if (_state == GluboidState.IsPlayer)
		{
			var velocity = Velocity;
			var position = GlobalPosition;
			position.X = _playerPosition.X;
			if (position.Y < _playerPosition.Y)
			{
				velocity.Y += _gravity * (float)delta;
			}
			else if(position.Y == _playerPosition.Y)
			{
				velocity.Y -= _hopHeight;
			}
			else
			{
				position.Y = _playerPosition.Y;
				velocity.Y = 0;
			}

			Velocity = velocity;
			GlobalPosition = position;
			//Velocity = new Vector2(0, 0);
			//GlobalPosition = _playerPosition;
			MoveAndSlide();
		}
		else if (_state == GluboidState.Extending)
		{
			if (GlobalPosition != _extendPosition)
			{
				var direction = GlobalPosition.DirectionTo(_extendPosition);

				Velocity = direction * _extendSpeed;
				MoveAndSlide();
			}
		}
		else if (_state == GluboidState.Retracting)
		{
			if (GlobalPosition != _playerPosition)
			{
				var direction = GlobalPosition.DirectionTo(_playerPosition);

				Velocity = direction * _extendSpeed;
				MoveAndSlide();
			}
			else
			{
				_state = GluboidState.Grouping;
			}
		}
		else if (_state != GluboidState.IsPlayer && _state != GluboidState.Grouping)
		{
			//Debug.WriteLine(_distanceFromPlayerX);
			
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
					GlubHopSound.Play();
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
					//Debug.WriteLine("Reset to Idle");
					//Debug.WriteLine(_state);
					FoleyLandSound.Play();
				}

				//Checking to see if Gluboid needs to Hop
				var playerDistance = Math.Abs(_distanceFromPlayerX);
				if (playerDistance > _maxDistanceFromPlayer && _state != GluboidState.PreHopping)
				{
					//Debug.WriteLine("Start Hop");
					Hop();
				}
				// FoleyLandSound.Play(); PLAYS SOUND WHEN STOPPING FALLING
			}

			MoveAndSlide();
		}
		else
		{
			if (GlobalPosition != _playerPosition)
			{
				var direction = GlobalPosition.DirectionTo(_playerPosition);
				Velocity = direction * _groupSpeed;
				MoveAndSlide();
			}
			else
			{
				Visible = false;
			}
		}
	}

	/// <summary>
	///Updates the local variable _playerPosition to the current global position of the player
	/// </summary>
	/// <param name="playerPosition"></param>
	public void SetPlayerPosition(Vector2 playerPosition)
	{
		//Debug.WriteLine("Distance invoked");
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
		_hopPower = (float)GD.RandRange(playerDistance*(_hopRatio), playerDistance+_maxHopPower);
		
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
		SetCollisionMaskValue(9,true);
		_state = GluboidState.IsPlayer;
		_isPlayer = true;
		Debug.WriteLine("Player Called");
		Debug.WriteLine(_state);
	}

	public void MakeNotPlayer()
	{
		_state = GluboidState.Idle;
		_isPlayer = false;
	}
	
	public void Extend(Direction direction, int blocks, float time)
	{
		SetCollisionMaskValue(9,false);
		
		switch (direction)
		{
			case (Direction.North):
				_extendPosition.X = 0;
				_extendPosition.Y = -1;
				break;
			case (Direction.NorthEast):
				_extendPosition.X = 1;
				_extendPosition.Y = -1;
				break;
			case (Direction.East):
				_extendPosition.X = 1;
				_extendPosition.Y = 0;
				break;
			case (Direction.SouthEast):
				_extendPosition.X = 1;
				_extendPosition.Y = 1;
				break;
			case (Direction.South):
				_extendPosition.X = 0;
				_extendPosition.Y = 1;
				break;
			case(Direction.SouthWest):
				_extendPosition.X = -1;
				_extendPosition.Y = 1;
				break;
			case(Direction.West):
				_extendPosition.X = -1;
				_extendPosition.Y = 0;
				break;
			case(Direction.NorthWest):
				_extendPosition.X = -1;
				_extendPosition.Y = -1;
				break;
		}

		_extendPosition.X = _playerPosition.X + (_extendPosition.X * _extendDistance * blocks);
		_extendPosition.Y = _playerPosition.Y + (_extendPosition.Y * _extendDistance * blocks);

		_extendSpeed = GlobalPosition.DistanceTo(_extendPosition) / time;

		_state = GluboidState.Extending;
	}

	public void Retract(float time)
	{
		
		SetCollisionMaskValue(9,false);
		
		if (_state != GluboidState.IsPlayer)
		{
			_extendSpeed = GlobalPosition.DistanceTo(_playerPosition) / time;
			_state = GluboidState.Retracting;
		}
	}
	public void UpdateIndex(int index)
	{
		_index = index;
	}

	public void ReturnToIdle()
	{
		SetCollisionMaskValue(9,true);
		
		if (_state != GluboidState.IsPlayer)
		{
			_state = GluboidState.Idle;
			Visible = true;
		}
	}

	public void GroupToPlayer()
	{
		SetCollisionMaskValue(9,false);
		if(_state != GluboidState.IsPlayer)
			_state = GluboidState.Grouping;
	}
}
