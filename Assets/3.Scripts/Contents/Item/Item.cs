using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Item : MonoBehaviour
{
    public Sprite itemIcon;
    public int id;
    public Define.ItemType itemType;
    public TileBase tile;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.layer == 6 && itemType != Define.ItemType.Building)
        {
            Managers.Inven.AddItem(id);
            Destroy(gameObject);
        }
    }


}
