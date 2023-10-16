using Godot;
using System;
using GlubspaceJam.Scripts;

public partial class Lock : Sprite2D, IDynamicReceiver
{
	private Texture2D _offTexture;

	private Texture2D _onTexture;

	private bool _powered = false;

	private bool _inverted;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		_offTexture = ((Sprite2D)GetNode("Sprite2D")).Texture;
		_onTexture = GD.Load<Texture2D>("res://Assets/Art/Placeholder Art/LockOn.png");
	}
	

	public void ProvidePower()
	{
		_powered = true;
		ToggleLock();
	}

	public void StopPower()
	{
		_powered = false;
		ToggleLock();
	}

	private void ToggleLock()
	{
		if (!_inverted && _powered || _inverted && !_powered)
		{
			_powered = true;
			((Sprite2D)GetNode("Sprite2D")).Texture = _onTexture;
		}
		else
		{
			_powered = false;
			((Sprite2D)GetNode("Sprite2D")).Texture = _offTexture;
		}
	}
	public bool IsOn()
	{
		return _powered;
	}

	public void Inverted()
	{
		_inverted = true;
	}
}
