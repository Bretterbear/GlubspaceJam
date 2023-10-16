using Godot;
using System;
using System.Diagnostics;

public partial class Spike : Area2D
{
	private Texture2D _onTexture;

	private Texture2D _offTexture;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		_onTexture = ((Sprite2D)GetNode("Sprite2D")).Texture;
		_offTexture = GD.Load<Texture2D>("res://Assets/Art/Dynamics Art/SpikeOff.png");
	}

	public void TurnOffSpike()
	{
		Debug.WriteLine("turnSpikesOff");
		SetCollisionMaskValue(1,false);
		SetCollisionLayerValue(9, false);
		((Sprite2D)GetNode("Sprite2D")).Texture = _offTexture;
	}

	public void TurnOnSpike()
	{
		SetCollisionMaskValue(1,true);
		SetCollisionLayerValue(9, true);
		((Sprite2D)GetNode("Sprite2D")).Texture = _onTexture;
	}
}
