using Godot;
using System;


/// <summary>
/// Class currently unused, will fix if I have the time.
/// </summary>
public partial class BarrierDetector : Area2D
{
    public void OnBodyShapeEntered(Rid bodyRid, Node2D body, int bodyShapeIndex, int LocalShapeIndex )
    {
        //GD.Print("ENTERED ");// + bodyRid);
        //GD.Print("Body RID: " + bodyRid);
        //GD.Print("Node2D  : " + body);
        //GD.Print("shapeIn : " + bodyShapeIndex);
        //GD.Print("LocalSh : " + LocalShapeIndex);

        //TileMap maparoo = (TileMap)body;
        // Vector2I tileCoords = maparoo.GetCoordsForBodyRid(bodyRid);
        //TileData rumples = maparoo.GetCellTileData(0, tileCoords);

        //this.SetBlockSignals(true);
        //rumples.Modulate = new Color(0, 0, 1, .5f);
        //this.SetBlockSignals(false);
    }
    public void OnBodyShapeExited(Rid bodyRid, Node2D body, int bodyShapeIndex, int LocalShapeIndex)
    {
        //GD.Print("EXITED  ");// + bodyRid);
        //GD.Print("Body RID: " + bodyRid);
        //GD.Print("Node2D  : " + body);
        //GD.Print("shapeIn : " + bodyShapeIndex);
        //GD.Print("LocalSh : " + LocalShapeIndex);

        // TileMap maparoo = (TileMap)body;
        //  Vector2I tileCoords = maparoo.GetCoordsForBodyRid(bodyRid);
        //  TileData rumples = maparoo.GetCellTileData(0, tileCoords);

        // rumples.Modulate = new Color(1, 1, 0, 1f);
    }

}
