using System.Collections.Generic;
using System.Diagnostics;
using Godot;

namespace GlubspaceJam.Scripts;

public class GluboidSkinController
{
    private static GluboidSkinController _instance;
    private List<string> _skinFilePath;
    private List<PackedScene> _skins;
    
    public GluboidSkinController()
    {
        _skinFilePath = new List<string>(){"res://Scenes/Actors/Animated Sprites/anime_glub_sprite_blue.tscn"};

        _skins = new List<PackedScene>();
        for (int i = 0; i < _skinFilePath.Count; i++)
        {
            _skins.Add(GD.Load<PackedScene>(_skinFilePath[i]));
        }
    }

    public static GluboidSkinController GetInstance()
    {
        if (_instance == null)
        {
            _instance = new GluboidSkinController();
        }

        return _instance;
    }
    public AnimeGlubSprite GetTexture(GluboidSkin skin)
    {
        return (AnimeGlubSprite)_skins[(int)skin].Instantiate();
    }
}