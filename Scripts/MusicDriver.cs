using Godot;
using System;

public partial class MusicDriver : Node2D
{
	private AudioStreamPlayer Muzak;

	public void OnPause()
	{
		Muzak.Bus = "Pause Music";
	}

	public void OnResume()
	{
		Muzak.Bus = "Music";
	}


}
