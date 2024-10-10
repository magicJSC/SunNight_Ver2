using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;



public class Item_Buliding : Item,IBuilding,IGetDamage,IMonsterTarget
{
    [HideInInspector]
    public Vector3Int pos;

    [HideInInspector]
    public GameObject buildUI;
    GameObject buildEffect;

    Stat stat;
    public SpriteRenderer[] bodySprites;

    private void Start()
    {
        pos = Managers.Game.tower.build.WorldToCell(transform.position);
        buildUI = Util.FindChild(gameObject, "UI_Build",true);
        buildEffect = Util.FindChild(gameObject, "BuildEffect", true);

        stat = GetComponent<Stat>();

        MapManager.buildData.Add(pos);

    }

    private void OnDisable()
    {
        buildUI.SetActive(false);
    }

    public void DeleteBuilding()
    {
        Managers.Game.tower.build.SetTile(pos,null);
        MapManager.buildData.Remove(pos);
    }

    public void GetDamage(int damage)
    {
        stat.Hp -= damage;
        if (stat.Hp <= 0)
           DeleteBuilding();
    }

    //설치 전 색깔로 변경
    public void ChangeColorBeforeIntall()
    {
        buildEffect.SetActive(false);
        for(int i=0;i< bodySprites.Length;i++)
        {
            bodySprites[i].color = new Color(1,1,1,0.3f);
        }
    }

    public void CantInstallColor()
    {
        buildEffect.SetActive(false);
        for (int i = 0; i < bodySprites.Length; i++)
        {
            bodySprites[i].color = new Color(1, 0.5f, 0.5f, 0.3f);
        }
    }

    //설치 후 색깔로 변경
    public void ChangeColorAfterIntall()
    {
        if (MapManager.cantBuild.HasTile(new Vector3Int((int)(transform.position.x), (int)(transform.position.y), 0)))
        {
            Item_Buliding building = GetComponentInParent<Item_Buliding>();
            Managers.Inven.AddItems(building.itemSo, 1);
            building.DeleteBuilding();
            return;
        }

        buildEffect.SetActive(true);
        for (int i = 0; i < bodySprites.Length; i++)
        {
            bodySprites[i].color = new Color(1, 1, 1, 1);
        }
    }
}
