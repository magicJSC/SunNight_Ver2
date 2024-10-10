using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class TrashPileController : MonoBehaviour,IInteractObject
{
    public static Action interactEvent;

    [Serializable]
    public struct GetItem
    {
        public ItemSO item;
        public int count;
        public float probability;
    }

    public List<GetItem> getItemList = new List<GetItem>();

    [SerializeField] float coolTime;

    public GameObject canInteractSign { get; private set; }


    bool canFind = true;
    bool isFinding;
    Image amount;
    GameObject actGageUI;

    Rigidbody2D playerRIgid;

    void Start()
    {
        canInteractSign = Util.FindChild(gameObject,"Sign", true);
        canInteractSign.SetActive(false);
        actGageUI = Util.FindChild(gameObject, "UI_ActGage", true);
        amount = Util.FindChild<Image>(gameObject, "Amount", true);
        actGageUI.SetActive(false);
        playerRIgid = Managers.Game.player.GetComponent<Rigidbody2D>();

    }

    public void Interact()
    {
        if(canFind && !isFinding)
            StartCoroutine(FindItem());
    }

    IEnumerator FindItem()
    {
        float curTime = 0;
        isFinding = true;
        actGageUI.SetActive(true);
        while (curTime < coolTime)
        {
            curTime += Time.deltaTime;
            amount.fillAmount = curTime / coolTime;
            if (playerRIgid.velocity != Vector2.zero)
            {
                Cancel();
                yield break;
            }
            yield return null;
        }
        actGageUI.SetActive(false);
        GetRandomItem();
    }

    void Cancel()
    {
        isFinding = false;
        actGageUI.SetActive(false);
    }

    void GetRandomItem()
    {
        interactEvent?.Invoke();
        int randomIndex = Random.Range(1, 101);
        float num = 0;
        for(int i = 0; i < getItemList.Count; i++)
        {
            if(num < randomIndex && randomIndex <= num + getItemList[i].probability)
            {
                Managers.Game.SpawnItem(getItemList[i].item, getItemList[i].count,transform.position);
                break;
            }
            num += getItemList[i].probability;
        }
        canFind = false;
        HideInteractSign();
    }

    public void ShowInteractSign()
    {
        if (!canFind)
            return;

        canInteractSign.SetActive(true);
    }

    public void HideInteractSign()
    {
        canInteractSign.SetActive(false);
    }
}
