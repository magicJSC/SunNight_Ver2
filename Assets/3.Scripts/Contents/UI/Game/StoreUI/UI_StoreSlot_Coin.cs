using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UI_StoreSlot_Coin : MonoBehaviour
{
    Seller seller;

    GameObject buyButton;

    Image changeItemIcon;


    Text needMoneyAmountText;
    Text curMoneyAmountText;
    Text changeItemCountText;
    Text changeItemNameText;
    Text changeCountText;

    ItemSO changeItemSO;
    int needMoneyAmount;
    int curMoneyAmount;
    int canChangeCount;
    int changeItemCount;

    private void Start()
    {
        seller = transform.root.GetComponent<Seller>();
    }

    public void Bind()
    {
        buyButton = Util.FindChild(gameObject, "BuyButton", true);
        buyButton.GetComponent<UI_EventHandler>()._OnClick += Buy;

        needMoneyAmountText = Util.FindChild<Text>(gameObject, "NeedItemText", true);
        curMoneyAmountText = Util.FindChild<Text>(gameObject, "CurItemText", true);

        changeItemIcon = Util.FindChild<Image>(gameObject, "ChangeItemIcon", true);
        changeItemCountText = Util.FindChild<Text>(gameObject, "ChangeItemCount", true);
        changeItemNameText = Util.FindChild<Text>(gameObject, "ChangeItemName", true);
        changeCountText = Util.FindChild<Text>(gameObject, "ChangeCountText", true);
    }

    public void SetValue(ItemSO changeItemSO,int needMoneyAmount, int canChangeCount,int changeItemCount)
    {
        this.changeItemSO = changeItemSO;
        this.needMoneyAmount = needMoneyAmount;
        this.canChangeCount = canChangeCount;
        this.changeItemCount = changeItemCount;
    }

    public void UpdateUI()
    {
        curMoneyAmount = Managers.Inven.Coin;

        changeItemNameText.text = $"{changeItemSO.itemName}";
        needMoneyAmountText.text = $"{needMoneyAmount}";
        curMoneyAmountText.text = $"{curMoneyAmount}";
        changeCountText.text = $"{changeCountText}";
        changeItemCountText.text = $"X{changeItemCount}";
        changeCountText.text = $"{canChangeCount}";
        changeItemIcon.sprite = changeItemSO.itemIcon;
    }

    void Buy(PointerEventData p)
    {
        if (canChangeCount <= 0)
            return;

        if (curMoneyAmount >= needMoneyAmount)
        {
            Managers.Inven.Coin -= needMoneyAmount;
            Managers.Inven.GetItem(changeItemSO, changeItemCount);
            canChangeCount--;
            seller.buyEvent.Invoke();
        }
    }
}
