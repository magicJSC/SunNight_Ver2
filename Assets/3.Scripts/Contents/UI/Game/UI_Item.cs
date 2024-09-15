using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UI_Item : UI_Base
{
    [HideInInspector]
    public Transform parDragBefore;
    [HideInInspector]
    public Transform dropingSlot;
    [HideInInspector]
    public StorageManager.SlotInfo slotInfo;

    [SerializeField]
    AssetReferenceT<Sprite> buildIconAsset;
    [SerializeField]
    AssetReferenceT<Sprite> materialIconAsset;
    [SerializeField]
    AssetReferenceT<Sprite> weaponIconAsset;
    [SerializeField]
    AssetReferenceT<Sprite> consumeIconAsset;

    Sprite buildIcon;
    Sprite materialIcon;
    Sprite weaponIcon;
    Sprite consumeIcon;

    [SerializeField]
    AssetReferenceT<AudioClip> changeSoundAsset;

    public AssetReferenceGameObject abandonUIAsset;

    AudioClip changeSound;

    Image icon;
    Image itemTypeIcon;
    Text count;

    RectTransform rect;

    public new void Init()
    {
        if (_init)
            return;

        _init = true;
        count = Util.FindChild(gameObject, "Count", true).GetComponent<Text>();
        icon = Util.FindChild(gameObject, "Icon", true).GetComponent<Image>();
        itemTypeIcon = Util.FindChild<Image>(gameObject, "ItemTypeIcon", true);

        parDragBefore = transform.parent;
        rect = GetComponent<RectTransform>();
        UI_EventHandler evt = icon.GetComponent<UI_EventHandler>();
        evt._OnBeginDrag += (PointerEventData p) =>
        {
            if (p.pointerDrag.transform.parent.GetComponentInParent<IDragable>() == null)
                return;
            Managers.Game.mouse.CursorType = Define.CursorType.Drag;
            transform.parent = transform.root.GetChild(0);
            icon.raycastTarget = false;
        };
        evt._OnDrag += (PointerEventData p) =>
        {
            if (p.pointerDrag.transform.parent.GetComponentInParent<IDragable>() != null)
                return;
            transform.position = Input.mousePosition;
        };
        evt._OnEndDrag += (PointerEventData p) =>
        {
            transform.parent = parDragBefore;
            rect.anchoredPosition = Vector2.zero;
            icon.raycastTarget = true;
            Drop();
            Managers.Inven.hotBarUI.CheckChoice();
        };
        evt._OnDrop += (PointerEventData p) =>
        {
            GameObject item = p.pointerDrag;

            if (item.transform.parent.GetComponent<UI_Item>() != null)
                item.transform.parent.GetComponent<UI_Item>().dropingSlot = transform;
        };
        if (slotInfo.itemInfo != null)
            SetInfo();
        else
            SetEmptyItem();

        changeSoundAsset.LoadAssetAsync().Completed += (clip) =>
        {
            changeSound = clip.Result;
        };
    }


    public void SetInfo()
    {
        if (slotInfo.count == 0)
            MakeEmptySlot();
        else
        {
            SetExistItem();
            count.text = $"{slotInfo.count}";
            icon.sprite = slotInfo.itemInfo.itemIcon;

            if (slotInfo.itemInfo.itemType == Define.ItemType.Pick)
            {
                if(materialIcon == null)
                {
                    materialIconAsset.LoadAssetAsync().Completed += (sprite) =>
                    {
                        materialIcon = sprite.Result;
                        itemTypeIcon.sprite = materialIcon;
                    };
                }
                else
                    itemTypeIcon.sprite = materialIcon;
            }
            else if (slotInfo.itemInfo.itemType == Define.ItemType.Tool)
            {
                if (weaponIcon == null)
                {
                    weaponIconAsset.LoadAssetAsync().Completed += (sprite) =>
                    {
                        weaponIcon = sprite.Result;
                        itemTypeIcon.sprite = weaponIcon;
                    };
                }
                else
                    itemTypeIcon.sprite = weaponIcon;
            }
            else if (slotInfo.itemInfo.itemType == Define.ItemType.Building)
            {
                if (buildIcon == null)
                {
                    buildIconAsset.LoadAssetAsync().Completed += (sprite) =>
                    {
                        buildIcon = sprite.Result;
                        itemTypeIcon.sprite = buildIcon;
                    };
                }
                else
                    itemTypeIcon.sprite = buildIcon;
            }
            else if (slotInfo.itemInfo.itemType == Define.ItemType.Consumable)
            {
                if (consumeIcon == null)
                {
                    consumeIconAsset.LoadAssetAsync().Completed += (sprite) =>
                    {
                        consumeIcon = sprite.Result;
                        itemTypeIcon.sprite = consumeIcon;
                    };
                }
                else
                    itemTypeIcon.sprite = consumeIcon;
            }

            rect.anchoredPosition = Vector2.zero;
        }
    }

    public void MakeEmptySlot()
    {
        slotInfo.count = 0;
        slotInfo.itemInfo = null;
        SetEmptyItem();
    }

    public void SetEmptyItem()
    {
        icon.gameObject.SetActive(false);
        count.gameObject.SetActive(false);
        itemTypeIcon.gameObject.SetActive(false);
    }

    public void SetExistItem()
    {
        icon.gameObject.SetActive(true);
        itemTypeIcon.gameObject.SetActive(true);
        if (slotInfo.count != 1)
            count.gameObject.SetActive(true);
        else
            count.gameObject.SetActive(false);
    }

    void Drop()
    {
        if (dropingSlot == null)
        {
            AbandonItem();
            return;
        }
        UI_Item s2 = dropingSlot.GetComponentInChildren<UI_Item>();

        if (s2 == this)
            ReturnItemToSlot();
        else if (s2.slotInfo.itemInfo == null)
            Managers.Inven.ChangeItem(this, s2);
        else if (s2.slotInfo.itemInfo.idName != slotInfo.itemInfo.idName)
            Managers.Inven.ChangeItem(this, s2);
        else
            Managers.Inven.AddItem(this, s2);

        Managers.Sound.Play(Define.Sound.Effect, changeSound);
        dropingSlot = null;
    }

    void ReturnItemToSlot()
    {
        rect.anchoredPosition = Vector2.zero;
    }

    void AbandonItem()
    {
        if (!StorageManager.canAbandon)
            return;
        abandonUIAsset.InstantiateAsync().Completed += (obj) =>
        {
            obj.Result.GetComponent<UI_Abandon>().itemUI = this;
        };
        rect.anchoredPosition = Vector2.zero;
    }
}
