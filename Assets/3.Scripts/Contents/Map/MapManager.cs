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
    public static Tilemap canBuild;
    public static Tilemap tower;

    public Vector3Int startPos;
    public Vector3Int sizeVector;
    public TileBase canBuildTile;

    public void Init()
    {
        buildData.Clear();
        GameObject go = Util.FindChild(gameObject, "CantBuild");
        cantBuild = go.GetComponent<Tilemap>();
        Managers.Map.sizeVector = sizeVector;
        Managers.Map.startPos = startPos;
        Managers.Map.canBuildTile = canBuildTile;
    }

    public bool CheckCanUseTile(Vector3Int pos)
    {
        if (Managers.Game.tower == null)
            return false;
        return canBuild.HasTile(pos);
    }

    public void SetCanBuildTile()
    {
        Vector3 towerPos = Managers.Game.tower.transform.position;
        for (int i = 0; i < sizeVector.x; i++)
        {
            for (int j = 0; j < sizeVector.y; j++) {
                Vector3Int pos = new Vector3Int(i, -j) + startPos;

                if (building.HasTile(pos))
                    canBuild.SetTile(pos, null);
                else if (tower.HasTile(pos))
                    canBuild.SetTile(pos, null);
                else if (cantBuild.HasTile(pos + new Vector3Int((int)towerPos.x, (int)towerPos.y, 0)))
                    canBuild.SetTile(pos, null);
                else
                    canBuild.SetTile(pos, canBuildTile);
            }
        }

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

    public bool CheckCanBuild(Vector3Int startPos, Vector3 size,bool build = true)
    {
        Transform towerTransform = Managers.Game.tower.transform;
        Vector3Int towerPos = new Vector3Int((int)towerTransform.position.x,(int)towerTransform.position.y);
        for(int i = 0; i < size.x; i++)
        {
            for(int j = 0; j < size.y;j++)
            {
                if(cantBuild.HasTile(startPos+towerPos + new Vector3Int(i,-j)))
                    return false;
                if(build)
                    if (tower.HasTile(new Vector3Int(i, -j) + startPos))
                        return false;
            }
        }

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