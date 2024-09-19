using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using static Define;

public struct Pos
{
    public Pos(int y, int x) { Y = y; X = x; }
    public int Y;
    public int X;
}

public class MapManager : MonoBehaviour
{
    public Grid CurrentGrid { get; private set; }


    public static List<Vector3Int> buildData = new List<Vector3Int>();

    public static Tilemap building;
    public static Tilemap cantBuild;
    public static Tilemap tower;

    public void Init()
    {
        buildData.Clear();
        GameObject go = Util.FindChild(gameObject, "CantBuild");
        cantBuild = go.GetComponent<Tilemap>();
    }

    public bool CheckCanUseTile(Vector3Int pos)
    {

        if (Managers.Game.tower == null)
            return false;
        Vector2 towerPos = Managers.Game.tower.transform.position;
        if (building.HasTile(new Vector3Int(pos.x - (int)towerPos.x, pos.y - (int)towerPos.y)))
            return false;
        else if (tower.HasTile(new Vector3Int(pos.x - (int)towerPos.x, pos.y - (int)towerPos.y)))
            return false;
        else if (cantBuild.HasTile(pos))
            return false;
        else if ((pos - new Vector3Int((int)towerPos.x, (int)towerPos.y)) == Vector3Int.zero)
            return false;

        return true;
    }

    public void ShowBuildUI(Vector3Int pos)
    {
        if (Managers.Game.mouse.CursorType == CursorType.Battle)
            return;
        Vector2 towerPos = Managers.Game.tower.transform.position;
        if (pos == new Vector3Int((int)towerPos.x, (int)towerPos.y))
            return;
        GameObject go = building.GetInstantiatedObject(new Vector3Int(pos.x, pos.y));
        go.GetComponent<Item_Buliding>().buildUI.SetActive(true);
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