using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Item_Buliding : Item,IGetDamage
{
    [Header("Building")]
    public int level;

    [HideInInspector]
    public Vector2 pos;

    public GameObject buildUI;

    private void Start()
    {
        pos = transform.position - Managers.Game.tower.transform.position;
        buildUI = Util.FindChild(gameObject, "UI_Build",true);
        Managers.Game.buildData.Add(pos, itemSo.idName);
    }

    public void DeleteBuilding()
    {
        Managers.Game.tower.build.SetTile(new Vector3Int((int)pos.x,(int)pos.y,0),null);
        Managers.Game.buildData.Remove(pos);
    }
}
