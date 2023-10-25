using Godot;
using System;
using GlubspaceJam.Scripts;

public partial class AnimeGlubSprite : AnimatedSprite2D
{
	private const float _scale = 0.517f;

	public void _Ready()
	{
		Idle();
	}
	public void Hook(Direction direction)
	{
		
		switch (direction)
		{
			case Direction.North:
				GlobalScale = new Vector2(_scale, -_scale);
				Play("Hook S_N");
				break;
			case Direction.NorthEast:
				Play("Hook NE_SW");
				break;
			case Direction.East:
				Play("Hook E_W");
				break;
			case Direction.SouthEast:
				Play("Hook SE_NW");
				break;
			case Direction.South:
				Play("Hook S_N");
				break;
			case Direction.SouthWest:
				GlobalScale = new Vector2(-_scale, -_scale);
				Play("Hook NE_SW");
				break;
			case Direction.West:
				GlobalScale = new Vector2(-_scale, _scale);
				Play("Hook E_W");
				break;
			case Direction.NorthWest:
				GlobalScale = new Vector2(-_scale, -_scale);
				Play("Hook SE_NW");
				break;
		}
		
	}

	public void Idle()
	{
		GlobalScale = new Vector2(GlobalScale.X, _scale);
		Play("Idle");
	}

	public void Walking(Direction direction)
	{
		switch (direction)
		{
			case Direction.East:
				Play("Walking_E");
				break;
			case Direction.West:
				GlobalScale = new Vector2(-_scale, _scale);
				Play("Walking_E");
				break;
		}
	}
	
}
