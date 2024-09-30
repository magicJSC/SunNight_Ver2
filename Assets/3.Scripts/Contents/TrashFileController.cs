using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class TrashFileController : MonoBehaviour,IInteractObject
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


    bool canFind = true;

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
        if(canFind)
            StartCoroutine(FindItem());
    }

    IEnumerator FindItem()
    {
        float curTime = 0;
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
        actGageUI.SetActive(false);
    }

    void GetRandomItem()
    {
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
    }
}
