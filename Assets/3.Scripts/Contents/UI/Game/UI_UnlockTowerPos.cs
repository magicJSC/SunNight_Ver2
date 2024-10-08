using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UI_UnlockTowerPos : MonoBehaviour
{
    [HideInInspector]
    public int index;

    RectTransform correct;
    RectTransform reject;
    void Start()
    {
        correct = Util.FindChild<RectTransform>(gameObject,"Correct",true);
        reject = Util.FindChild<RectTransform>(gameObject,"Reject",true);

        UI_EventHandler evt = correct.GetComponent<UI_EventHandler>();
        evt._OnEnter += (PointerEventData p) => { correct.sizeDelta = new Vector2(130,130); };
        evt._OnExit += (PointerEventData p) => { correct.sizeDelta = new Vector2(100,100); };
        evt._OnClick += (PointerEventData p) => { Managers.Game.isUnlockTowerPos[index] = true; gameObject.SetActive(false); };

        evt = reject.GetComponent<UI_EventHandler>();
        evt._OnEnter += (PointerEventData p) => { reject.sizeDelta = new Vector2(130, 130); };
        evt._OnExit += (PointerEventData p) => { reject.sizeDelta = new Vector2(100, 100); };
        evt._OnClick += (PointerEventData p) => { gameObject.SetActive(false); };
    }

    private void OnEnable()
    {
        Managers.Game.isCantPlay = true;
    }

    private void OnDisable()
    {
        Managers.Game.isCantPlay = false;
    }
}
