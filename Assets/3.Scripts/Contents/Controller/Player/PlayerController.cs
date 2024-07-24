using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;



public class PlayerController : CreatureController,IPlayer
{
    public Action escEvent;

    PlayerStat stat;
    bool init;
   

    [Header("Contents")]
    Vector2 dir;

    [HideInInspector]
    public GameObject toolParent;

    [HideInInspector]
    public List<GameObject> interactObjectList = new List<GameObject>();
    GameObject canInteractObj;

    bool isDie;

    public AssetReferenceGameObject statUIAsset;
    public AssetReferenceGameObject DieUIAsset;

    public AssetReferenceGameObject graveAsset;

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
        statUIAsset.InstantiateAsync();
        StartCoroutine(Move());
    }

    public void OnEnable()
    {
        if (!init)
            return;
        isDie = false;
        stat.Hp = stat.maxHP;

        StartCoroutine(Move());

        if(toolParent.transform.GetChild(0) != null)
         Destroy(toolParent.transform.GetChild(0).gameObject);
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
    }

    IEnumerator Move()
    {
        while (true)
        {
            rigid.velocity = dir * speed;
            yield return null;
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
        if (!canInteractObj)
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


    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.TryGetComponent<Item_Pick>(out var item))
        {
            if (Managers.Inven.AddItems(item.itemSo,item.Count))
                item.DestroyThis();
        }
    }

    void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.gameObject.GetComponent<ICaninteract>() != null)
        {
            SetInteractObj();
        }
    }

    public void GetDamage(int damage)
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
        graveAsset.InstantiateAsync().Completed += (go) => 
        {
            go.Result.transform.position = transform.position;
            gameObject.SetActive(false);
        };
        
        DieUIAsset.InstantiateAsync().Completed += (go) =>
        {
            if (!Managers.Game.isKeepingTower)
                go.Result.GetComponent<Animator>().Play("Die");
            else
                go.Result.GetComponent<Animator>().Play("GameOver");
        };
    }
}
