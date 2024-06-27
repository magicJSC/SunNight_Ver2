using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// 제련 할 때 생성되는 UI입니다
/// </summary>
public class UI_Smelt : UI_BaseSlot,IDragable
{
    GameObject grillingSlot;
    GameObject close;
    GameObject smeltSlot;
    GameObject doSmelt;
    GameObject charcoalSlot;

    enum GameObjects
    {
        GrillingSlot,
        Close,
        SmeltSlot,
        DoSmelt,
        CharcoalSlot
    }

    public override void Init()
    {
        Bind<GameObject>(typeof(GameObjects));
        grillingSlot = Get<GameObject>((int)GameObjects.GrillingSlot);
        close = Get<GameObject>((int)GameObjects.Close);
        smeltSlot = Get<GameObject>((int)GameObjects.SmeltSlot);
        doSmelt = Get<GameObject>((int)GameObjects.DoSmelt);
        charcoalSlot = Get<GameObject>((int)GameObjects.CharcoalSlot);

        
    }

    public void OnDrag(PointerEventData point)
    {
        
    }
}
