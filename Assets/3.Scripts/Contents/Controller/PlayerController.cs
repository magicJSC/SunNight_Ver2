using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public interface IGetDamage
{

}

public class PlayerController : CreatureController,IGetDamage
{
     

    Rigidbody2D rigid;
    Animator anim;
    SpriteRenderer sprite;


    [Header("Contents")]
    Vector2 dir;
    bool canGetTower = false;

    [HideInInspector]
    public GameObject toolParent;

    List<GameObject> matters = new List<GameObject>();

    public Action interact;

    public void Init()
    {
        toolParent = Util.FindChild(gameObject, "Tool");
        rigid = GetComponent<Rigidbody2D>();
        Camera.main.GetComponent<CameraController>().target = transform;
        anim = GetComponent<Animator>();
        sprite = GetComponent<SpriteRenderer>();
        Instantiate(Resources.Load<GameObject>("UI/UI_PlayerStat"));
    }

    void OnMove(InputValue value)
    {
        dir = value.Get<Vector2>();

        if (dir.x != 0 || dir.y != 0)
        {
            if (dir.x != 0)
                sprite.flipX = dir.x < 0;
            if (dir.y > 0)
                anim.Play("UpWalk");
            else
                anim.Play("DownWalk");
        }
        else
            anim.SetTrigger("Stop");
        rigid.velocity = new Vector3(dir.x, dir.y, 0) * speed;
    }

    void OnPick()
    {
        for (int i = 0; i < matters.Count; i++)
        {
            if (Managers.Inven.AddOneItem(matters[i].GetComponent<Item_Matter>().itemSo.idName))
                matters[i].GetComponent<Item_Matter>().DestroyThis();
        }
    }

    void OnShowInventory()
    {
        Managers.Inven.inventoryUI.gameObject.SetActive(!Managers.Inven.inventoryUI.gameObject.activeSelf);
        Managers.Game.isHandleUI = false;
    }

    void OnGetTower()
    {
        interact?.Invoke();
        if (!canGetTower || Managers.Game.isKeepingTower)
            return;

        Managers.Game.isKeepingTower = true;
        Managers.Inven.hotBarUI.CheckChoice();
        Managers.Inven.hotBarUI.towerSlot.ShowTowerIcon();
        Managers.Game.tower.transform.SetParent(Managers.Game.build.transform);
        Managers.Game.tower.transform.position = Managers.Game.build.transform.position;
    }

    void OnBuild()
    {
        if (Managers.Game.isHandleUI)
            return;

        if (Managers.Inven.choicingTower)
            Managers.Game.build.BuildTower();
        else
            Managers.Game.build.BuildItem(); 
    }

    //void OnInterface()
    //{
    //    interact.Invoke();
    //}


    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.GetComponent<Item_Matter>())
        {
            matters.Add(collision.gameObject);
            collision.gameObject.GetComponent<Item_Matter>().ChangeTake();
        }
        else if (collision.gameObject.GetComponent<TowerController>())
        {
            canGetTower = true;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.GetComponent<Item_Matter>())
        {
            matters.Remove(collision.gameObject);
            collision.gameObject.GetComponent<Item_Matter>().ChangeOrigin();
        }
        else if (collision.gameObject.GetComponent<TowerController>())
        {
            canGetTower = false;
        }
    }
}
