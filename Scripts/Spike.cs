using Godot;
using System;

public partial class Spike : StaticBody2D
{
	private Texture2D _onTexture;

	private Texture2D _offTexture;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		_onTexture = ((Sprite2D)GetNode("Sprite2D")).Texture;
		_offTexture = GD.Load<Texture2D>("res://Assets/Art/Placeholder Art/LockOn.png");
	}

	public void TurnOffSpikes()
	{
		SetCollisionMaskValue(1,false);
		SetCollisionLayerValue(9, false);
		((Sprite2D)GetNode("Sprite2D")).Texture = _offTexture;
	}
}
