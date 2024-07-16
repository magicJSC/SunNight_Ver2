using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;



public class PlayerController : CreatureController,IPlayer
{
    public Action escEvent;

    PlayerStat stat;
    bool init;
   

    [Header("Contents")]
    Vector2 dir;
    Vector2 mousePos;

    [HideInInspector]
    public GameObject toolParent;

    List<GameObject> matters = new List<GameObject>();
    public List<GameObject> interactObjectList = new List<GameObject>();
    GameObject canInteractObj;

    bool isDie;


    Rigidbody2D rigid;
    Animator anim;
    SpriteRenderer sprite;

    public void Init()
    {
        if (init)
            return;

        init = true;
        toolParent = Util.FindChild(gameObject, "Tool");
        rigid = GetComponent<Rigidbody2D>();
        Camera.main.GetComponent<CameraController>().target = transform;
        anim = GetComponent<Animator>();
        sprite = GetComponent<SpriteRenderer>();
        stat = GetComponent<PlayerStat>();
        Instantiate(Resources.Load<GameObject>("UI/UI_PlayerStat"));
    }

    public void OnEnable()
    {
        if (!init)
            return;
        isDie = false;
        stat.Hp = stat.maxHP;
    }

    void OnMove(InputValue value)
    {
        dir = value.Get<Vector2>();

        if (dir != Vector2.zero)
        {
            anim.Play("Move");
            if(dir.x != 0)
                sprite.flipX = dir.x > 0;
        }
        else
            anim.Play("Idle");

        rigid.velocity = new Vector3(dir.x, dir.y, 0) * speed;
    }

    void OnPick()
    {
        if (Time.timeScale == 0)
            return;
        for (int i = 0; i < matters.Count; i++)
        {
            if (Managers.Inven.AddOneItem(matters[i].GetComponent<Item_Matter>().itemSo.idName))
                matters[i].GetComponent<Item_Matter>().DestroyThis();
        }
    }

    void OnShowInventory()
    {
        if (Time.timeScale == 0)
            return;
        Managers.Inven.inventoryUI.gameObject.SetActive(!Managers.Inven.inventoryUI.gameObject.activeSelf);
    }

    void OnInteract()
    {
        if (Time.timeScale == 0)
            return;
        canInteractObj.GetComponent<ICaninteract>().Interact();
    }

    public void SetInteractObj()
    {
        canInteractObj = null;
        for(int i = 0; i < interactObjectList.Count; i++)
        {
            if (canInteractObj == null)
                canInteractObj = interactObjectList[i];
            else if (Vector2.Distance(canInteractObj.transform.position,transform.position) > Vector2.Distance(interactObjectList[i].transform.position, transform.position))
                canInteractObj = interactObjectList[i];

            interactObjectList[i].GetComponent<ICaninteract>().canInteractSign.SetActive(false);
        }
        if(canInteractObj)
            canInteractObj.GetComponent<ICaninteract>().canInteractSign.SetActive(true);
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

    void OnESC()
    {
        escEvent?.Invoke();
    }


    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.TryGetComponent<Item_Matter>(out var item))
        {
            matters.Add(collision.gameObject);
            item.ChangeTake();

            //자동 줍기
            //if (Managers.Inven.AddOneItem(item.itemSo.idName))
            //    item.DestroyThis();
        }
    }

    void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.gameObject.GetComponent<ICaninteract>() != null)
        {
            SetInteractObj();
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.TryGetComponent<Item_Matter>(out var item))
        {
            matters.Remove(collision.gameObject);
            item.ChangeOrigin();
        }
    }

    public void GetDamage(float damage)
    {
        if (isDie)
            return;

        stat.Hp -= damage;
        if (stat.Hp <= 0)
            Die();
    }

    public void Die()
    {
        isDie = true;
        GameObject go = Instantiate(Resources.Load<GameObject>("Prefabs/GraveStone"));
        go.transform.position = transform.position;
        gameObject.SetActive(false);
        Instantiate(Resources.Load<GameObject>("UI/UI_Die"));
    }
}
