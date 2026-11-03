using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class TrashPileController : MonoBehaviour,IInteractObject
{
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
    string explorationClaim;
    Image amount;
    GameObject actGageUI;
    void Start()
    {
        explorationClaim = FieldBalance.Configure(this);
        canInteractSign = Util.FindChild(gameObject,"Sign", true);
        canFindEffect = Util.FindChild(gameObject, "CanFindEffect", true);
        canInteractSign.SetActive(false);
        actGageUI = Util.FindChild(gameObject, "UI_ActGage", true);
        amount = Util.FindChild<Image>(gameObject, "Amount", true);
        actGageUI.SetActive(false);
        if (getItemList.Count == 0) { canFind = false; canFindEffect.SetActive(false); }

    }

    public void Interact()
    {
        RefreshExplorationClaim();
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
        if (explorationClaim != null && getItemList.Count > 0)
        {
            var reward = getItemList[0];
            int remaining = Mathf.Max(0, reward.count - FieldSession.State.Claimed(explorationClaim));
            int accepted = FieldInventory.Give(reward.item, remaining);
            FieldSession.State.AddClaim(explorationClaim, accepted);
            RefreshExplorationClaim();
            return;
        }
        int randomIndex = Random.Range(1, 101);
        float num = 0;
        for(int i = 0; i < getItemList.Count; i++)
        {
            if(num < randomIndex && randomIndex <= num + getItemList[i].probability)
            {
                Managers.Game.GetItem(getItemList[i].item, getItemList[i].count,transform.position);
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
        RefreshExplorationClaim();
        if (!canFind)
            return;

        canInteractSign.SetActive(true);
    }

    public void HideInteractSign()
    {
        canInteractSign.SetActive(false);
    }

    void RefreshExplorationClaim()
    {
        if (explorationClaim == null || getItemList.Count == 0) return;
        if (FieldSession.State.Claimed(explorationClaim) < getItemList[0].count) return;
        canFind = false;
        canFindEffect.SetActive(false);
        HideInteractSign();
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
