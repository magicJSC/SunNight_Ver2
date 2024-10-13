using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;



public class PlayerController : CreatureController, IPlayer, IBuffReciever, IMonsterTarget
{
    public static Action tutorial1Event;
    public Action escEvent;

    PlayerStat stat;
    bool init;


    [Header("Contents")]
    Vector2 dir;

    [HideInInspector]
    public GameObject toolParent;

   

    public static bool isDie;

    public AssetReferenceGameObject statUIAsset;
    public AssetReferenceGameObject DieUIAsset;
    public AssetReferenceGameObject miniMapAsset;
    public AssetReferenceGameObject gameMenuUIAsset;

    public AssetReferenceGameObject graveAsset;

    Rigidbody2D rigid;
    Animator anim;
    SpriteRenderer sprite;

    public List<BaseBuffGiver> buffList { get; private set; }

    public void Init()
    {
        if (init)
            return;

        init = true;
        Camera.main.transform.parent = transform;
        Camera.main.transform.position = Camera.main.transform.parent.position + new Vector3(0, 0, -10);
        toolParent = Util.FindChild(gameObject, "Tool");
        rigid = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        sprite = GetComponent<SpriteRenderer>();
        stat = GetComponent<PlayerStat>();
        statUIAsset.LoadAssetAsync().Completed += (obj) =>
        {
            Managers.UI.ShowUI<GameObject>(obj.Result, transform);
        };
        gameMenuUIAsset.LoadAssetAsync().Completed += (obj) =>
        {
            GameObject go = Managers.UI.ShowUI(obj.Result);
            GetComponent<PopUICloser>().gameMenuUI = go;
        };
        buffList = new List<BaseBuffGiver>();
        miniMapAsset.InstantiateAsync();

        StartCoroutine(Move());
    }

    public void OnEnable()
    {
        if (!init)
            return;
        if (!Managers.Game.completeTutorial)
        {
            SceneManager.LoadScene("TutorialScene");
            return;
        }
        StartCoroutine(Move());
        isDie = false;
        stat.Hp = stat.maxHP;
        Camera.main.transform.parent = transform;
        Camera.main.transform.position = Camera.main.transform.parent.position + new Vector3(0, 0, -10);
        dir = Vector2.zero;
        StartCoroutine(UpdateBuff());
        if (toolParent.transform.GetChild(0) != null)
            Destroy(toolParent.transform.GetChild(0).gameObject);
    }


    void OnMove(InputValue value)
    {
        if (Managers.Game.isCantPlay)
        {
            anim.Play("Idle");
            dir = Vector2.zero;
            return;
        }
         dir = value.Get<Vector2>();

        if (dir != Vector2.zero)
        {
            tutorial1Event?.Invoke();

            anim.Play("Move");
            if (dir.x != 0)
                sprite.flipX = dir.x > 0;
        }
        else
            anim.Play("Idle");
    }

    IEnumerator Move()
    {
        while (true)
        {
            if (!Managers.Game.isCantPlay)
                rigid.velocity = dir * stat.Speed;
            else
                rigid.velocity = Vector2.zero;
            yield return null;
        }
    }

    void OnShowInventory()
    {
        if (Managers.Game.isCantPlay)
            return;
            if (Time.timeScale == 0)
            return;
        Managers.Inven.inventoryUI.gameObject.SetActive(!Managers.Inven.inventoryUI.gameObject.activeSelf);
    }

   

    

    public void BuildAction()
    {
        if (Managers.Game.isCantPlay)
            return;
        if (Managers.Game.isHandleUI)
            return;

        if(Managers.Game.build != null)
        Managers.Game.build.BuildItem();
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.TryGetComponent<Item_Pick>(out var item))
        {
            if (Managers.Inven.AddItems(item.itemSo, item.Count))
            {
                item.DestroyThis();
            }
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
        Camera.main.transform.parent = null;
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

   

    public IEnumerator UpdateBuff()
    {
        while (true)
        {
            yield return null;
            if (buffList.Count == 0)
                continue;

            for (int i = 0; i < buffList.Count; i++)
            {
                buffList[i].CoolTime -= Time.deltaTime;
            }
        }
    }

    public void GetBuff(BaseBuffGiver buff)
    {
        buffList.Add(buff);
        for(int i = 0;i < buff.buffSO.buffList.Count; i++)
        {
            switch (buff.buffSO.buffList[i].buffType)
            {
                case Define.BuffType.SpeedUp:
                    stat.Speed *= buff.buffSO.buffList[i].amount;
                    break;
                case Define.BuffType.SpeedDown:
                    stat.Speed /= buff.buffSO.buffList[i].amount;
                    break;
            }
        }
    }
    
    public void StopBuff(BaseBuffGiver buff)
    {
        for (int i = 0; i < buffList.Count; i++)
        {
            switch (buff.buffSO.buffList[i].buffType)
            {
                case Define.BuffType.SpeedUp:
                    stat.Speed /= buff.buffSO.buffList[i].amount;
                    break;
                case Define.BuffType.SpeedDown:
                    stat.Speed *= buff.buffSO.buffList[i].amount;
                    break;
            }
        }
        buffList.Remove(buff);
        Destroy(buff.gameObject);
    }
}
