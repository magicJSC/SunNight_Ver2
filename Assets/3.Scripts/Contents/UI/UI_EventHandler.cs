using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class UI_EventHandler : MonoBehaviour,IPointerClickHandler,IDragHandler,IPointerDownHandler,IPointerEnterHandler,IPointerExitHandler,IPointerUpHandler,IBeginDragHandler,IEndDragHandler,IDropHandler
{
    public Action<PointerEventData> _OnClick;
    public Action<PointerEventData> _OnDrag;
    public Action<PointerEventData> _OnDown;
    public Action<PointerEventData> _OnExit;
    public Action<PointerEventData> _OnEnter;
    public Action<PointerEventData> _OnUp;
    public Action<PointerEventData> _OnBeginDrag;
    public Action<PointerEventData> _OnEndDrag;
    public Action<PointerEventData> _OnDrop;

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (_OnBeginDrag != null)
            _OnBeginDrag?.Invoke(eventData);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (_OnDrag != null)
            _OnDrag?.Invoke(eventData);
    }

    public void OnDrop(PointerEventData eventData)
    {
        if (_OnDrop != null)
            _OnDrop?.Invoke(eventData);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (_OnEndDrag != null)
            _OnEndDrag?.Invoke(eventData);
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
