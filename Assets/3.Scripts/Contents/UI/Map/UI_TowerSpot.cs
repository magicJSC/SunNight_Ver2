using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class UI_TowerSpot : MonoBehaviour,IPointerClickHandler,IPointerEnterHandler,IPointerExitHandler
{
    RectTransform rectTransform;

    [SerializeField] Vector2 spotPostion;

    UI_CheckTransport checkUI;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        checkUI = Util.FindChild<UI_CheckTransport>(transform.root.gameObject, "UI_CheckTransport", true);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        checkUI.gameObject.SetActive(true);
        checkUI.spotPos = spotPostion;    
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        rectTransform.sizeDelta = new Vector2(50,50);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        rectTransform.sizeDelta = new Vector2(40, 40);
    }
}
