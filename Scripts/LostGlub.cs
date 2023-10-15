using Godot;
using System;
using System.Diagnostics;
using GlubspaceJam.Scripts;

public partial class LostGlub : Area2D
{
	public override void _Ready()
	{
		var numberOfSkins = Enum.GetNames(typeof(GluboidSkin)).Length;
		var selection = (GluboidSkin)Rand.GetInstance().RandiRange(0, numberOfSkins - 1);
		Debug.WriteLine(selection);
		var skin = GluboidSkinController.GetInstance()
			.GetTexture(selection);
		Debug.WriteLine(skin);
		((Sprite2D)GetNode("Sprite2D")).Texture = skin;
	}

	private void CollisionWithPlayer(Node2D body)
	{
		var sprite = (Sprite2D)GetNode("Sprite2D");
		var skin = sprite.Texture;
		body.GetParent<PlayerManager>().PickUpGluboid(GlobalPosition, skin);
		QueueFree();
	}
}
