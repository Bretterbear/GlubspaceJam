using Godot;
using System;
using GlubspaceJam.Scripts;

public partial class Lock : Sprite2D, IDynamicReceiver
{
	private Texture2D _offTexture;

	private Texture2D _onTexture;

	private bool _powered = false;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		_offTexture = ((Sprite2D)GetNode("Sprite2D")).Texture;
		_onTexture = GD.Load<Texture2D>("res://Assets/Art/Placeholder Art/LockOn.png");
	}
	
	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	public void ProvidePower()
	{
		_powered = true;
		((Sprite2D)GetNode("Sprite2D")).Texture = _onTexture;
	}

	public void StopPower()
	{
		_powered = false;
		((Sprite2D)GetNode("Sprite2D")).Texture = _offTexture;
	}

	public bool IsOn()
	{
		return _powered;
	}
}
