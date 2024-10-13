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
    GameObject canFindEffect;

    bool canFind = true;
    bool cancel;
    Image amount;
    GameObject actGageUI;
    void Start()
    {
        canInteractSign = Util.FindChild(gameObject,"Sign", true);
        canFindEffect = Util.FindChild(gameObject, "CanFindEffect", true);
        canInteractSign.SetActive(false);
        actGageUI = Util.FindChild(gameObject, "UI_ActGage", true);
        amount = Util.FindChild<Image>(gameObject, "Amount", true);
        actGageUI.SetActive(false);

    }

    public void Interact()
    {
        if(canFind)
            StartCoroutine(FindItem());
    }

    IEnumerator FindItem()
    {
        float curTime = 0;
        cancel = false;
        actGageUI.SetActive(true);
        Managers.Game.isCantPlay = true;
        while (curTime < coolTime)
        {
            curTime += Time.deltaTime;
            amount.fillAmount = curTime / coolTime;
            if(cancel)
                yield break;
            yield return null;
        }
        Managers.Game.isCantPlay = false;
        actGageUI.SetActive(false);
        GetRandomItem();
    }

    void Cancel()
    {
        cancel = true;
        Managers.Game.isCantPlay = false;
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
        canFindEffect.SetActive(false);
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

    public void SetAction(PlayerInteract playerInteract)
    {
        playerInteract.interactAction += Interact;
        playerInteract.cancelAction += Cancel;
    }

    public void CancelAction(PlayerInteract playerInteract)
    {
        playerInteract.interactAction -= Interact;
        playerInteract.cancelAction -= Cancel;
    }
}
