using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Item : MonoBehaviour
{

    public Sprite itemIcon;
    public int id;
    public string objName;
    public Define.ItemType itemType;
    public TileBase tile;
}
