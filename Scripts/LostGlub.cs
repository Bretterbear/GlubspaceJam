using Godot;
using System;
using GlubspaceJam.Scripts;

public partial class LostGlub : CharacterBody2D
{
	public override void _Ready()
	{
		var sprite = (Sprite2D)GetNode("GlubSprite");
		var numberOfSkins = Enum.GetNames(typeof(GluboidSkin)).Length;
		var skin = GluboidSkinController.GetInstance()
			.GetTexture((GluboidSkin)Rand.GetInstance().RandiRange(0, numberOfSkins-1));
		sprite.Texture = skin;
	}
}
