using System.Collections;
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

    //타워에 설치된 건축물 위치 데이터
    public static List<Vector3Int> buildData = new List<Vector3Int>();

    public static Tilemap walls;
    public static Tilemap matter;
    public static Tilemap building;
    public static Tilemap tower;

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

        if (walls.HasTile(pos))
            return false;
        else if (building.HasTile(new Vector3Int(pos.x - (int)towerPos.x, pos.y - (int)towerPos.y)))
        {
            ShowBuildUI(pos);
            return false; 
        }
        else if (tower.HasTile(new Vector3Int(pos.x - (int)towerPos.x, pos.y - (int)towerPos.y)))
            return false;
        else if (matter.HasTile(pos))
            return false;
        else if ((pos - new Vector3Int((int)towerPos.x, (int)towerPos.y)) == Vector3Int.zero)
            return false;

        return true;
    }

    //강화 할수 있는 UI 생성
    void ShowBuildUI(Vector3Int pos)
    {
        if (Managers.Game.mouse.CursorType == CursorType.Battle)
            return;
        Vector2 towerPos = Managers.Game.tower.transform.position;
        if (pos == new Vector3Int((int)towerPos.x, (int)towerPos.y))
            return;
        GameObject go = building.GetInstantiatedObject(new Vector3Int(pos.x - (int)towerPos.x, pos.y - (int)towerPos.y));
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