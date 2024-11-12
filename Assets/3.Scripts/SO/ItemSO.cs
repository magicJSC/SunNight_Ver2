using System;
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

    public GameObject itemPrefab;
    public TileBase buildTile;

    [Header("Á¦·Ã")]
    public bool canSmelt;
    public ItemSO smeltItem;

    [Header("±Á±â")]
    public bool canBake;
    public ItemSO bakeItem;
    public float bakeTime;

    [Header("¸Ô±â")]
    public bool canEat;
    public float hungerAmount;
}
