using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class UI_Journal : MonoBehaviour
{
    UI_EventHandler leave;

    private void Start()
    {
        Managers.Game.isCantPlay = true;
        leave = Util.FindChild<UI_EventHandler>(gameObject,"Leave",true);
        leave._OnClick += (PointerEventData p) => { Managers.Game.isCantPlay = false;  Destroy(gameObject); };
    }
}
