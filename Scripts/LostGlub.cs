using Godot;
using System;
using GlubspaceJam.Scripts;

public partial class LostGlub : Area2D
{
	public override void _Ready()
	{
		var sprite = (Sprite2D)GetNode("GlubSprite");
		var numberOfSkins = Enum.GetNames(typeof(GluboidSkin)).Length;
		var skin = GluboidSkinController.GetInstance()
			.GetTexture((GluboidSkin)Rand.GetInstance().RandiRange(0, numberOfSkins-1));
		sprite.Texture = skin;
	}

	private void CollisionWithPlayer(Node2D body)
	{
		var sprite = (Sprite2D)GetNode("GlubSprite");
		var skin = sprite.Texture;
		body.GetParent<PlayerManager>().PickUpGluboid(GlobalPosition, skin);
		QueueFree();
	}
}
