using Godot;
using System;
using System.Diagnostics;
using GlubspaceJam.Scripts;

public partial class LostGlub : Area2D
{
	private AnimeGlubSprite _sprite;
	public override void _Ready()
	{
		var numberOfSkins = Enum.GetNames(typeof(GluboidSkin)).Length;
		var selection = (GluboidSkin)Rand.GetInstance().RandiRange(0, numberOfSkins - 1);
		Debug.WriteLine(selection);
		var skin = GluboidSkinController.GetInstance()
			.GetTexture(selection);
		Debug.WriteLine(skin);
		_sprite = skin;
		AddChild(skin);
		_sprite.Idle();
	}

	private void CollisionWithPlayer(Node2D body)
	{
		RemoveChild(_sprite);
		body.GetParent<PlayerManager>().PickUpGluboid(GlobalPosition, _sprite);
		QueueFree();
		
	}
}
