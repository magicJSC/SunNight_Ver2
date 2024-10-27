using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;



public class Item_Buliding : Item,IBuilding,IMonsterTarget
{
    [HideInInspector]
    public Vector3Int pos;

    [HideInInspector]
    public GameObject buildUI;
    GameObject buildEffect;

    public SpriteRenderer[] bodySprites;

    Stat stat;

    Transform towerTransform;

    BoxCollider2D boxCollider;
    private void Start()
    {
        pos = Managers.Game.tower.build.WorldToCell(transform.position);
        buildUI = Util.FindChild(gameObject, "UI_Build",true);
        buildEffect = Util.FindChild(gameObject, "BuildEffect", true);


        stat = GetComponent<Stat>();
        boxCollider = GetComponent<BoxCollider2D>();

        towerTransform = Managers.Game.tower.transform;
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
        Managers.Map.SetCanBuildTile();
    }

    public void Die()
    {
        DeleteBuilding();
    }

    //설치 전 색깔로 변경
    public void ChangeBeforeIntall()
    {
        buildEffect.SetActive(false);
        boxCollider.enabled = false;
        for(int i=0;i< bodySprites.Length;i++)
        {
            bodySprites[i].color = new Color(1,1,1,0.5f);
        }
    }

   
    public bool ChangeAfterIntall()
    {
        if (MapManager.cantBuild.HasTile(MapManager.building.WorldToCell(new Vector3(transform.position.x + towerTransform.position.x,transform.position.y + towerTransform.position.y))))
        {
            Item_Buliding building = GetComponentInParent<Item_Buliding>();
            Managers.Inven.AddItems(building.itemSo, 1);
            building.DeleteBuilding();
            return false;
        }

        buildEffect.SetActive(true);
        boxCollider.enabled = true;
        for (int i = 0; i < bodySprites.Length; i++)
        {
            bodySprites[i].color = new Color(1, 1, 1, 1);
        }
        return true;
    }

    public void GetDamage(int damage)
    {
       stat.GetDamage(damage);
    }
}
