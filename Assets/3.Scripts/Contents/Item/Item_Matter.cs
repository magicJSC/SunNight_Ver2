using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Item_Matter : Item
{
    SpriteRenderer spriteRenderer;
    Sprite origin;
    Sprite pickTarget;


    private void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        origin = spriteRenderer.sprite;
        pickTarget = itemSo.itemIcon;
    }

    public void ChangeOrigin()
    {
        spriteRenderer.sprite = origin;
    }

    public void ChangeTake()
    {
        spriteRenderer.sprite = pickTarget;
    }

    public void DestroyThis()
    {
        MapManager.matter.SetTile(new Vector3Int((int)transform.position.x, (int)transform.position.y), null);
    }
}
