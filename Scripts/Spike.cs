using Godot;
using System;
using System.Diagnostics;

public partial class Spike : Area2D
{
	private Texture2D _onTexture;

	private Texture2D _offTexture;
	
	private AudioStreamPlayer2D StabbyOnSound;
	private AudioStreamPlayer2D StabbyOffSound;
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		_onTexture = ((Sprite2D)GetNode("Sprite2D")).Texture;
		_offTexture = GD.Load<Texture2D>("res://Assets/Art/Dynamics Art/SpikeOff.png");
		
		StabbyOnSound = GetNode<AudioStreamPlayer2D>("StabOn");
		StabbyOffSound = GetNode<AudioStreamPlayer2D>("StabOff");
	}

	public void TurnOffSpike()
	{
		Debug.WriteLine("turnSpikesOff");
		SetCollisionMaskValue(1,false);
		SetCollisionLayerValue(9, false);
		((Sprite2D)GetNode("Sprite2D")).Texture = _offTexture;
		StabbyOffSound.Play();
	}

	public void TurnOnSpike()
	{
		SetCollisionMaskValue(1,true);
		SetCollisionLayerValue(9, true);
		((Sprite2D)GetNode("Sprite2D")).Texture = _onTexture;
		StabbyOnSound.Play();
	}
}
