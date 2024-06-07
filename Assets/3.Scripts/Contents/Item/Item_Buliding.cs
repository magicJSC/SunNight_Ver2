using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Item_Buliding : Item
{
    [HideInInspector]
    public Vector2 pos;

    private void Start()
    {
        itemType = Define.ItemType.Building;
        pos = transform.position - Managers.Game.tower.transform.position;
        Managers.Game.buildData.Add(pos, id);
    }

    public void DeleteBuilding()
    {
        Managers.Game.tower.build.SetTile(new Vector3Int((int)pos.x,(int)pos.y,0),null);
        Managers.Game.buildData.Remove(pos);
    }
}
