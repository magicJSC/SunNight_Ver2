using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;



public class Item_Buliding : Item,IGetDamage
{
    [HideInInspector]
    public Vector3Int pos;

    public GameObject buildUI;

    Stat stat;
    public SpriteRenderer[] bodySprites;

    LayerMask buildLayer;
    LayerMask inviLayer;
    private void Start()
    {
        Vector2 thisPos = transform.position - Managers.Game.tower.transform.position;
        pos = new Vector3Int((int)thisPos.x, (int)thisPos.y);
        buildUI = Util.FindChild(gameObject, "UI_Build",true);

        stat = GetComponent<Stat>();

        MapManager.buildData.Add(pos);

        inviLayer.value = 8;
        buildLayer.value = 9;
    }

    public void DeleteBuilding()
    {
        Managers.Game.tower.build.SetTile(pos,null);
        MapManager.buildData.Remove(pos);
    }

    public void GetDamge(float damage)
    {
        stat.Hp -= damage;
        if (stat.Hp <= 0)
           DeleteBuilding();
    }

    //설치 전 색깔로 변경
    public void ChangeColorBeforeIntall()
    {
        gameObject.layer = inviLayer;
        for(int i=0;i< bodySprites.Length;i++)
        {
            bodySprites[i].color = new Color(1,1,1,0.3f);
        }
    }

    //설치 후 색깔로 변경
    public void ChangeColorAfterIntall()
    {
        gameObject.layer = buildLayer;
        for (int i = 0; i < bodySprites.Length; i++)
        {
            bodySprites[i].color = new Color(1, 1, 1, 1);
        }
    }
}
