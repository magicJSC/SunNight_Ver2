using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
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

    static List<Vector3Int> directList = new List<Vector3Int>();

    //타워에 설치된 건축물 위치 데이터
    public static List<Vector3Int> buildData = new List<Vector3Int>();

    public static Tilemap matter;
    public static Tilemap building;
    public static Tilemap cantBuild;
    public static Tilemap tower;

    public void Init()
    {
        buildData.Clear();
        GameObject go = Util.FindChild(gameObject, "Matter");
        matter = go.GetComponent<Tilemap>();
        go = Util.FindChild(gameObject, "CantBuild");
        cantBuild = go.GetComponent<Tilemap>();
        directList.Add(Vector3Int.right);
        directList.Add(Vector3Int.up);
        directList.Add(Vector3Int.down);
        directList.Add(Vector3Int.left);
    }

    public bool CheckCanUseTile(Vector3Int pos)
    {
        Vector2 towerPos = Managers.Game.tower.transform.position;
        if (building.HasTile(new Vector3Int(pos.x - (int)towerPos.x, pos.y - (int)towerPos.y)))
            return false;
        else if (tower.HasTile(new Vector3Int(pos.x - (int)towerPos.x, pos.y - (int)towerPos.y)))
            return false;
        else if (matter.HasTile(pos))
            return false;
        else if (cantBuild.HasTile(pos))
            return false;
        else if ((pos - new Vector3Int((int)towerPos.x, (int)towerPos.y)) == Vector3Int.zero)
            return false;

        return true;
    }

    //강화 할수 있는 UI 생성
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

    public void SpawnItem(ItemSO itemSO, int count, Vector3Int pos)
    {
        pos = FindSpawnSpot(pos, itemSO);
        GameObject go = matter.GetInstantiatedObject(pos);
        if (go == null)
        {
            matter.SetTile(pos, itemSO.tile);
            matter.GetInstantiatedObject(pos).GetComponent<Item_Pick>().Count = count;
        }
        else
        {
            Item_Pick item = matter.GetInstantiatedObject(pos).GetComponent<Item_Pick>();
            item.Count += count;
            if (item.Count > item.itemSo.maxAmount)
            {
                SpawnItem(itemSO, item.Count - item.itemSo.maxAmount, pos);
                item.Count = item.itemSo.maxAmount;
            }
        }
    }

    Vector3Int FindSpawnSpot(Vector3Int pos, ItemSO itemSO)
    {
        Vector3Int nextPos = pos;
        while (true)
        {
            pos = nextPos;
            for (int i = 0; i < directList.Count; i++)
            {
                if (!CheckCanUseTile(directList[i] + pos))
                {
                    if (matter.HasTile(pos + directList[i]))
                    {
                        if (itemSO == matter.GetInstantiatedObject(pos + directList[i]).GetComponent<Item>().itemSo)
                        {
                            return pos + directList[i];
                        }
                    }

                    if (cantBuild.HasTile(pos))
                        continue;
                    nextPos = pos + directList[i];
                }
                else
                    return pos + directList[i];
            }
        }
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