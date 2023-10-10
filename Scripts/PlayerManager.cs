using Godot;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using GlubspaceJam.Scripts;
using Range = System.Range;

public partial class PlayerManager : Node2D
{
	private Player _player;

	public int _numberOfGlubs;
	
	private string _gluboidSceneLocation = "res://Scenes/Actors/gluboid.tscn";
	private PackedScene _gluboidScene;
	private List<Gluboid> _gluboidPack;

	private PlayerState _playerState;

	///<summary>
	///Grabs a reference to the player scene and the Gluboid, will change to list of gluboids.
	/// Also does first time randomizer setup.
	/// </summary>
	public override void _Ready()
	{
		GD.Randomize();
		_gluboidScene = (PackedScene)ResourceLoader.Load(_gluboidSceneLocation);
		_gluboidPack = new List<Gluboid>();
		_player = GetChild<Player>(0, false);
		PickUpGluboid();
		PickUpGluboid();
		ShuffleGlubs();
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
		_gluboidPack.Add(gluboid);
		gluboid.setup(GetPlayerPosition(), _gluboidPack.IndexOf(gluboid));
		AddChild(gluboid);
		_numberOfGlubs++;
	}

	public void DropGluboid(Gluboid glub)
	{
		_gluboidPack.RemoveAt(_gluboidPack.IndexOf(glub));
		RemoveChild(glub);
		glub.Free();
		_numberOfGlubs--;
	}

	/// <summary>
	/// Sets specific glub as the player glub, not ready for use yet.
	/// </summary>
	/// <param name="glub"></param>
	public void PickPlayerGlub(Gluboid glub)
	{
		if (_gluboidPack.Contains(glub))
		{
			_gluboidPack[0].MakeNotPlayer();
			(_gluboidPack[_gluboidPack.IndexOf(glub)], _gluboidPack[0]) =
				(_gluboidPack[0], _gluboidPack[_gluboidPack.IndexOf(glub)]);
			_gluboidPack[0].MakePlayer();
			
			foreach(Gluboid g in _gluboidPack)
			{
				g.UpdateIndex(_gluboidPack.IndexOf(g));
			}
		}
	}

	/// <summary>
	/// Logic for setting the glubs in the correct position for chain extension.
	/// </summary>
	/// <param name="blockDistance"></param>
	/// <param name="direction"></param>
	public void ExtendGlubChain(int blockDistance, Direction direction)
	{
		ShuffleGlubs();
		int goal;
		if (blockDistance < _numberOfGlubs - 1)
		{
			goal = blockDistance;
		}
		else
		{
			goal = _numberOfGlubs-1;
		}

		for (int i = 1; i < goal; i++)
		{
			_gluboidPack[i].Extend(direction, i);
		}
	}
	private void ShuffleGlubs()
	{
		
		_gluboidPack[0].MakeNotPlayer();
		
		var n = _gluboidPack.Count;  
		while (n > 1) {  
			n--;  
			var k = (int)GD.Randi() % (n+1);  
			(_gluboidPack[k], _gluboidPack[n]) = (_gluboidPack[n], _gluboidPack[k]);
		}

		foreach(Gluboid glub in _gluboidPack)
		{
			glub.UpdateIndex(_gluboidPack.IndexOf(glub));
		}
		
		
		_gluboidPack[0].MakePlayer();
	}

	/// <summary>
	/// Releases the glubs from either grouping or extended
	/// </summary>
	public void ReleaseChain()
	{
		foreach (Gluboid glub in _gluboidPack)
		{
			glub.ReturnToIdle();
		}
	}

	public void GroupGlubs(){
		foreach (Gluboid glub in _gluboidPack)
		{
			glub.GroupToPlayer();
		}
	}

}
	
