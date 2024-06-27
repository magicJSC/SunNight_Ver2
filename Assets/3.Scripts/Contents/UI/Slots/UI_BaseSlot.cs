using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public interface IDroppable 
{
    void OnDrop(PointerEventData point);
}

public interface IDragable
{
    void OnDrag(PointerEventData point);
}

public abstract class UI_BaseSlot : UI_Base
{
   
}
