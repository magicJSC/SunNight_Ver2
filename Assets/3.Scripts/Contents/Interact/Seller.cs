using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;

public class Seller : MonoBehaviour, IInteractObject
{
    public GameObject canInteractSign { get; private set; }

    public Action<int> timerEvent;
    public Action buyEvent;

    int Time { get { return time; } set { time = value; timerEvent?.Invoke(time); } }

    GameObject IInteractObject.canInteractSign => throw new NotImplementedException();

    int time;

    GameObject storeUI;

    public AssetReferenceGameObject coinSlotAsset;
    public AssetReferenceGameObject itemSlotAsset;

    GameObject coinSlot;
    GameObject itemSlot;

    GameObject content;

    public enum ExchangeType
    {
        Coin,
        Item
    }

    [Serializable]
    public struct SellItemInfo
    {
        public ItemSO sellItem;
        public int sellItemCount;
        public ItemSO needItem;
        public int needItemCount;
        public int exchangeCount;
        public ExchangeType exchangeType;
    }

    public List<SellItemInfo> sellInfoList = new List<SellItemInfo>();

    public void Start()
    {
        time = 300;

        storeUI = Util.FindChild(gameObject, "UI_Store", true);

        canInteractSign = Util.FindChild(gameObject, "Sign");
        canInteractSign.SetActive(false);

        content = Util.FindChild(gameObject, "Content", true);

        coinSlotAsset.LoadAssetAsync().Completed += (obj) => 
        {
            coinSlot = obj.Result;
            itemSlotAsset.LoadAssetAsync().Completed += (obj) => { itemSlot = obj.Result; SetRandomSellItem(); };
        };
        

        StartCoroutine(Timer());
        storeUI.SetActive(false);
    }


    void SetRandomSellItem()
    {
        int index;
        SellItemInfo sellInfo;
        for(int i = 0;i < 5; i++)
        {
            index = UnityEngine.Random.Range(0,sellInfoList.Count);
            sellInfo = sellInfoList[index];
            if(sellInfo.exchangeType == ExchangeType.Item)
            {
                UI_StoreSlot_Item slot = Instantiate(itemSlot,content.transform).GetComponent<UI_StoreSlot_Item>();
                slot.Bind();
                slot.SetValue(sellInfo.sellItem,sellInfo.needItem,sellInfo.needItemCount,sellInfo.exchangeCount,sellInfo.sellItemCount);
                buyEvent += slot.UpdateUI;
            }
            else
            {
                UI_StoreSlot_Coin slot = Instantiate(coinSlot,content.transform).GetComponent<UI_StoreSlot_Coin>();
                slot.Bind();
                slot.SetValue(sellInfo.sellItem, sellInfo.needItemCount, sellInfo.exchangeCount, sellInfo.sellItemCount);
                buyEvent += slot.UpdateUI;
            }
        }
        buyEvent?.Invoke();
    }

    IEnumerator Timer()
    {
        while (true)
        {
            yield return new WaitForSeconds(1);
            Time--;
            if (Time == 0)
            {
                Debug.Log("상점이 문을 닫습니다");
                yield break; 
            }
        }
    }

    //void IInteractObject.Interact()
    //{
    //    timerEvent?.Invoke(time);
    //    buyEvent?.Invoke();
    //    storeUI.SetActive(true);
    //}

    public void ShowInteractSign()
    {
        canInteractSign.SetActive(true);
    }

    public void HideInteractSign()
    {
        canInteractSign.SetActive(false);
    }

    public void SetAction(PlayerInteract playerInteract)
    {
        
    }

    void IInteractObject.CancelAction(PlayerInteract playerInteract)
    {
        throw new NotImplementedException();
    }
}
