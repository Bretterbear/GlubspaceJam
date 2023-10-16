using Godot;
using System.Collections.Generic;
using System.Diagnostics;
using GlubspaceJam.Scripts;

public partial class PlayerManager : Node2D
{
	private Player _player;
	
	public int _numberOfGlubs;
	
	private string _gluboidSceneLocation = "res://Scenes/Actors/gluboid.tscn";
	private PackedScene _gluboidScene;
	private List<Gluboid> _gluboidPack;
	private Gluboid _endOfChain;
	private PlayerState _playerState;


	///<summary>
	///Grabs a reference to the player scene and the Gluboid, will change to list of gluboids.
	/// Also does first time randomizer setup.
	/// </summary>
	public override void _Ready()
	{
		_gluboidScene = (PackedScene)ResourceLoader.Load(_gluboidSceneLocation);
		_gluboidPack = new List<Gluboid>();
		_player = GetChild<Player>(0);
		PickUpGluboid(_player.GlobalPosition, GluboidSkinController.GetInstance().GetTexture(0)); 
		PickUpGluboid(_player.GlobalPosition, GluboidSkinController.GetInstance().GetTexture(0));
		ShuffleGlubs();
	}

	/// <summary>
	/// Currently only passing Player position to the Gluboid, in future will add gluboid array management functions into here.
	/// </summary>
	/// <param name="delta"></param>
	public override void _PhysicsProcess(double delta)
	{
		UpdatePlayerPosition();
	}

	private void UpdatePlayerPosition()
	{
		GetTree().CallGroup("Gluboids", "SetPlayerPosition", GetPlayerPosition());
	}
	private Vector2 GetPlayerPosition()
	{
		return new Vector2(_player.GlobalPosition.X,_player.GlobalPosition.Y);
	}

	
	public void PickUpGluboid(Vector2 position, Texture2D skin)
	{
		Gluboid gluboid = (Gluboid)_gluboidScene.Instantiate();
		_gluboidPack.Add(gluboid);
		Debug.WriteLine(_gluboidPack.Count);
		gluboid.setup(GetPlayerPosition(), _gluboidPack.IndexOf(gluboid), skin);
		CallDeferred("add_child", gluboid);
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
	/// <param name="timeToComplete"></param>
	public void ExtendGlubChain(int blockDistance, Direction direction, float timeToComplete)
	{
		//turn off collision for gluboids during extension
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

		for (int i = 0; i < goal; i++)
		{
			_gluboidPack[i].Visible = true;
			_gluboidPack[i].Extend(direction, i, timeToComplete);
		}

		_endOfChain = _gluboidPack[blockDistance - 1];
	}
	private void ShuffleGlubs()
	{
		_gluboidPack[0].MakeNotPlayer();
		
		var n = _gluboidPack.Count;  
		while (n > 1) {  
			n--;
			var k = Rand.GetInstance().RandiRange(0,n);
			(_gluboidPack[k], _gluboidPack[n]) = (_gluboidPack[n], _gluboidPack[k]);
		}

		for (int i = 0; i < _numberOfGlubs; i++)
		{
			_gluboidPack[i].UpdateIndex(i);
			_gluboidPack[i].ZIndex = _numberOfGlubs - i;
		}
		_gluboidPack[0].MakePlayer();
		_gluboidPack[0].Visible = true;
	}

	/// <summary>
	/// Releases the glubs from either grouping or extended
	/// </summary>
	public void ReleaseChain()
	{
		foreach (Gluboid glub in _gluboidPack)
		{
			glub.ReturnToIdle();
			glub.Visible = true;
		}
	}

	public void GroupGlubs(float timeToComplete){
		UpdatePlayerPosition();
		foreach (Gluboid glub in _gluboidPack)
		{
			glub.GroupToPlayer();
		}
	}

	public void RetractChain(float timeToComplete, bool toGoal)
	{
		if (toGoal)
		{
			(_gluboidPack[0], _gluboidPack[_gluboidPack.IndexOf(_endOfChain)]) = (_gluboidPack[_gluboidPack.IndexOf(_endOfChain)], _gluboidPack[0]);
			_gluboidPack[0].MakePlayer();
		}
		
		foreach (var glub in _gluboidPack)
		{
			glub.Retract(timeToComplete);
		}
	}

}
	
