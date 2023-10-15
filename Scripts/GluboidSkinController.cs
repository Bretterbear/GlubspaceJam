using System.Collections.Generic;
using System.Diagnostics;
using Godot;

namespace GlubspaceJam.Scripts;

public class GluboidSkinController
{
    private static GluboidSkinController _instance;
    private List<string> _skinFilePath;
    private List<Texture2D> _skins;
    
    public GluboidSkinController()
    {
        _skinFilePath = new List<string>(){"res://Assets/Art/Char_GlubPrinceps-01.png"};

        _skins = new List<Texture2D>();
        for (int i = 0; i < _skinFilePath.Count; i++)
        {
            _skins.Add(GD.Load<Texture2D>(_skinFilePath[i]));
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
    public Texture2D GetTexture(GluboidSkin skin)
    {
        Debug.WriteLine(skin);
        Debug.WriteLine((int)skin);
        return _skins[(int)skin];
    }
}