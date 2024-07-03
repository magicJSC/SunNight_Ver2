using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

[CreateAssetMenu(fileName ="ItemSO",menuName ="SO/Item")]
public class ItemSO : ScriptableObject
{
    public string idName;
    public string itemName;
    public string explain;
    public Sprite itemIcon;
    public Define.ItemType itemType;
    public int maxAmount;

    [Header("มฆทร")]
    public bool canSmelt;
    public ItemSO smelt;
    public TileBase tile;
}
