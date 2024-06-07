using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class UI_EventHandler : MonoBehaviour,IPointerClickHandler,IDragHandler,IPointerDownHandler,IPointerEnterHandler,IPointerExitHandler,IPointerUpHandler
{
    public Action<PointerEventData> _OnClick;
    public Action<PointerEventData> _OnDrag;
    public Action<PointerEventData> _OnDown;
    public Action<PointerEventData> _OnExit;
    public Action<PointerEventData> _OnEnter;
    public Action<PointerEventData> _OnUp;

    public void OnDrag(PointerEventData eventData)
    {
        if (_OnDrag != null)
            _OnDrag?.Invoke(eventData);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (_OnClick != null)
            _OnClick?.Invoke(eventData);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (_OnDown != null)
            _OnDown?.Invoke(eventData);

    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (_OnEnter != null)
            _OnEnter?.Invoke(eventData);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (_OnExit != null)
            _OnExit?.Invoke(eventData);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (_OnUp != null)
            _OnUp?.Invoke(eventData);
    }
}
