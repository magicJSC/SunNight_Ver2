using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class UI_StoreSlot_Item : MonoBehaviour
{
    Seller seller;

    GameObject buyButton;

    Image changeItemIcon;
    Image needItemIcon;

    Text needItemText;
    Text curItemText;
    Text changeItemCountText;
    Text changeItemNameText;
    Text changeCountText;

    ItemSO needItemSO;
    ItemSO changeItemSO;
    int needItemCount;
    int curItemCount;
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

        needItemText = Util.FindChild<Text>(gameObject, "NeedItemText", true);
        curItemText = Util.FindChild<Text>(gameObject, "CurItemText", true);

        changeItemIcon = Util.FindChild<Image>(gameObject, "ChangeItemIcon", true);
        needItemIcon = Util.FindChild<Image>(gameObject, "NeedItemIcon", true);
        changeItemCountText = Util.FindChild<Text>(gameObject, "ChangeItemCount", true);
        changeItemNameText = Util.FindChild<Text>(gameObject, "ChangeItemName", true);
        changeCountText = Util.FindChild<Text>(gameObject, "ChangeCountText", true);
    }

    public void SetValue(ItemSO changeItemSO,ItemSO needItemSO, int needItemCount, int canChangeCount, int changeItemCount)
    {
        this.changeItemSO = changeItemSO;
        this.needItemSO = needItemSO;
        this.needItemCount = needItemCount;
        
        this.canChangeCount = canChangeCount;
        this.changeItemCount = changeItemCount;
    }

    public void UpdateUI()
    {
        curItemCount = Managers.Inven.GetItemCount(needItemSO);

        changeItemNameText.text = $"{changeItemSO.itemName}";
        needItemIcon.sprite = needItemSO.itemIcon;
        needItemText.text = $"{needItemCount}";
        curItemText.text = $"{curItemCount}";
        changeItemCountText.text = $"X{changeItemCount}";
        changeCountText.text = $"{canChangeCount}";
        changeItemIcon.sprite = changeItemSO.itemIcon;
    }

    void Buy(PointerEventData p)
    {
        if (canChangeCount <= 0)
            return;

        if (curItemCount >= needItemCount)
        {
            Managers.Inven.RemoveItem(needItemSO, needItemCount);
            Managers.Inven.GetItem(changeItemSO, changeItemCount);
            canChangeCount--;
            seller.buyEvent.Invoke();
        }
    }
}
