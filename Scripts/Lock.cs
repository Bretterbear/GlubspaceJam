using Godot;
using System;
using GlubspaceJam.Scripts;

public partial class Lock : Sprite2D, IDynamicReceiver
{
	private Texture2D _offTexture;

	private Texture2D _onTexture;

	private bool _powered = false;

	private bool _inverted;

	//private IDynamicReceiver _dynamicReceiverImplementation;
	
	private AudioStreamPlayer2D LockSound;


	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		_offTexture = ((Sprite2D)GetNode("Sprite2D")).Texture;
		_onTexture = GD.Load<Texture2D>("res://Assets/Art/Placeholder Art/LockOn.png");

		if(!(GetParent() is IDynamicReceiver))
		{
			DynamicsSetup();
		}
		LockSound = GetNode<AudioStreamPlayer2D>("Lock");

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

	private void ToggleLock()
	{
		if (!_inverted && _powered || _inverted && !_powered)
		{
			_powered = true;
			((Sprite2D)GetNode("Sprite2D")).Texture = _onTexture;
			LockSound.Play();
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
}
