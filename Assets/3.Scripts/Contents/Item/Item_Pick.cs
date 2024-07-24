using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Item_Pick : Item,IPickable
{
    [HideInInspector]
    public Text countText;

    [HideInInspector]
    public int Count { get { return count; } set { count = value; SetCountText(); } }

    int count = 1;

    private void Start()
    {
        GetCountText();
        countText.text = $"{count}";
        SetCountText();
    }

    public void DestroyThis()
    {
        MapManager.matter.SetTile(new Vector3Int((int)transform.position.x, (int)transform.position.y), null);
    }

    public void Pick()
    {
        Managers.Inven.AddItems(itemSo, Count);
        DestroyThis();
    }

    void SetCountText()
    {
        if (countText == null)
            GetCountText();

        if(count != 1)
            countText.text = $"{count}";
        else
            countText.text = $"";
    }

    void GetCountText()
    {
        countText = Util.FindChild(gameObject, "Count", true).GetComponent<Text>();
    }
}
