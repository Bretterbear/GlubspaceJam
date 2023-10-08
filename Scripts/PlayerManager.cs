using Godot;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using Range = System.Range;

public partial class PlayerManager : Node2D
{
	private Player _player;

	public int _numberOfGlubs;

	//private Gluboid _gluboid;
	private string _gluboidSceneLocation = "res://Scenes/Actors/gluboid.tscn";
	private PackedScene _gluboidScene;
	private List<Gluboid> _gluboidPack;

	private PlayerState _playerState;
	private Gluboid _playerGlub;

	///<summary>
	///Grabs a reference to the player scene and the Gluboid, will change to list of gluboids.
	/// Also does first time randomizer setup.
	/// </summary>
	public override void _Ready()
	{
		_gluboidScene = (PackedScene)ResourceLoader.Load(_gluboidSceneLocation);
		_gluboidPack = new List<Gluboid>();
		_player = GetChild<Player>(0, false);
		PickUpGluboid();
		PickUpGluboid();
		PickPlayerGlub();
		//_gluboid = GetChild<Gluboid>(1, false);
		GD.Randomize(); //Move to a more global place
	}

	/// <summary>
	/// Currently only passing Player position to the Gluboid, in future will add gluboid array management functions into here.
	/// </summary>
	/// <param name="delta"></param>
	public override void _PhysicsProcess(double delta)
	{
		GetTree().CallGroup("Gluboids", "SetPlayerPosition", GetPlayerPosition());
	}

	private Vector2 GetPlayerPosition()
	{
		return _player.GlobalPosition;
	}

	public void PickUpGluboid()
	{
		Gluboid gluboid = (Gluboid)_gluboidScene.Instantiate();
		gluboid.setup(GetPlayerPosition());
		AddChild(gluboid);
		_gluboidPack.Add(gluboid);
		_numberOfGlubs++;
	}

	public void DropGluboid(Gluboid glub)
	{
		_gluboidPack.RemoveAt(_gluboidPack.IndexOf(glub));
		RemoveChild(glub);
		glub.Free();
		_numberOfGlubs--;
	}

	public void PickPlayerGlub()
	{
		int glubIndex = (int)GD.Randi() % _gluboidPack.Count;
		_playerGlub = _gluboidPack[glubIndex];
		_playerGlub .MakePlayer();
	}
}
	
