using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public struct Pos
{
    public Pos(int y, int x) { Y = y; X = x; }
    public int Y;
    public int X;
}

public class MapManager : MonoBehaviour 
{
    public Grid CurrentGrid { get; private set; }

    public Tilemap walls;
    public Tilemap matter;
    public Tilemap building;
    public TileBase banTile;

    public void Init()
    {
        GameObject go = Util.FindChild(gameObject, "Wall");
        walls = go.GetComponent<Tilemap>();
        go = Util.FindChild(gameObject, "Matter");
        matter = go.GetComponent<Tilemap>();
    }

    public bool CheckCanBuild(Vector3Int pos)
    {
        Vector2 towerPos = Managers.Game.tower.transform.position;

        if(walls.HasTile(pos))
            return false;
        else if(building.HasTile(new Vector3Int(pos.x - (int)towerPos.x,pos.y - (int)towerPos.y)))
            return false;
        else if (matter.HasTile(pos))
            return false;
        else if((pos - new Vector3Int((int)towerPos.x,(int)towerPos.y)) == Vector3Int.zero)
            return false;

        return true;
    }

    public void LoadMap(int mapId)
    {
        DestroyMap();

        string mapName = "Map_" + mapId.ToString("000");

    }

    public void DestroyMap()
    {
        GameObject map = GameObject.Find("Map");
        if (map != null)
            GameObject.Destroy(map);
    }
}