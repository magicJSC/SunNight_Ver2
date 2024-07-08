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
    public TileBase tile;

    [Header("Á¦·Ã")]
    public ItemSO smelt;

    [Header("±Á±â")]
    public ItemSO bakeItemSO;
    public float bakeTime;
}
