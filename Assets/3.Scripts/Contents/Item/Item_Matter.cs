using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Item_Matter : Item
{
    SpriteRenderer s;
    Sprite origin;
    Sprite take;


    private void Start()
    {
        s = GetComponent<SpriteRenderer>();
        Managers.Game.grid.matter.SetTile(new Vector3Int((int)transform.position.x, (int)transform.position.y), Managers.Game.grid.banTile);
        origin = s.sprite;
        take = itemIcon;
    }

    public void ChangeOrigin()
    {
        s.sprite = origin;
    }

    public void ChangeTake()
    {
        s.sprite = take;
    }

    public void DestroyThis()
    {
        Managers.Game.grid.matter.SetTile(new Vector3Int((int)transform.position.x, (int)transform.position.y), null);
        Destroy(gameObject);
    }
}
