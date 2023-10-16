using Godot;
using System;
using System.Diagnostics;
using GlubspaceJam.Scripts;


public partial class VicotoryPiont : Area2D
{
	
	
	//Audio Stuff
	
	private AudioStreamPlayer2D VictoryBell;
	
	
	
	public override void _Ready()
	{
		
		
		
		//Audio
		
		VictoryBell = GetNode<AudioStreamPlayer2D>("VictoryBell");
	}
	
	//VictoryBell.Play(); 
}
