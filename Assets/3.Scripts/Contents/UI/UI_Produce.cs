using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class UI_Produce : UI_Base
{
    [Header("Produce")]



    [Header("UI")]
    GameObject back;
    GameObject content;
    GameObject toMake;
    GameObject produce;


    enum GameObjects
    {
        Background,
        Content,
        ToMake,
        Produce
    }

    public override void Init()
    {
        if (_init)
            return;

        Bind<GameObject>(typeof(GameObjects));
        back = Get<GameObject>((int)GameObjects.Background);
        content = Get<GameObject>((int)GameObjects.Content);
        toMake = Get<GameObject>((int)GameObjects.ToMake);
        produce = Get<GameObject>((int)GameObjects.Produce);

        UI_EventHandler evt = back.GetComponent<UI_EventHandler>();
        evt._OnEnter += (PointerEventData p) => { Managers.Game.mouse.CursorType = Define.CursorType.Normal; };

        evt._OnExit += (PointerEventData p) => { Managers.Inven.Set_HotBar_Choice(); };

        Remove_ToMake();
    }

    private void OnEnable()
    {
        Remove_ToMake();    
    }

    void Set_ToMake()
    {

    }

    void Remove_ToMake()
    {

    } 
}
