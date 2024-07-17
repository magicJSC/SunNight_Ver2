using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MouseController : MonoBehaviour
{
    bool init;
    public void Init()
    {
        if(init)
            return;

        init = true;
        StartCoroutine(UpdateMouse());
    }

    public IEnumerator UpdateMouse()
    {
        while (true)
        {
            if (EventSystem.current.IsPointerOverGameObject())
            {
                if (CursorType != Define.CursorType.UI && CursorType != Define.CursorType.Drag)
                { 
                    CursorType = Define.CursorType.UI;
                }
            }
            else
            {
                if (CursorType == Define.CursorType.UI)
                {
                    Managers.Inven.hotBarUI.CheckChoice();
                }
            }
            yield return null;
        }
    }

    #region 플레이 타입
    Define.CursorType _cursorType = Define.CursorType.Normal;
    public Define.CursorType CursorType
    {
        get { return _cursorType; }
        set
        {
            _cursorType = value;

            switch (value)
            {
                case Define.CursorType.Normal:
                    SetNormalMode();
                    break;
                case Define.CursorType.Builder:
                    SetBuildingMode();
                    break;
                case Define.CursorType.Drag:
                    SetDragMode();
                    break;
                case Define.CursorType.Battle:
                    SetBattleMode();
                    break;
                case Define.CursorType.UI:
                    SetHandleUIMode();
                    break;
            }
        }
    }

    public Texture2D normal;
    public Texture2D drag;
   
    void SetNormalMode()
    {
        Cursor.SetCursor(normal, Vector2.zero, CursorMode.Auto);
        Managers.Game.build.gameObject.SetActive(false);
        Managers.Game.isHandleUI = false;
    }

    void SetBuildingMode()
    {
        Cursor.SetCursor(normal, Vector2.zero, CursorMode.Auto);
        Managers.Game.build.gameObject.SetActive(true);
        Managers.Game.isHandleUI = false;
    }

    void SetDragMode()
    {
        Cursor.SetCursor(drag, Vector2.zero, CursorMode.Auto);
        Managers.Game.build.gameObject.SetActive(false);
    }

    void SetBattleMode()
    {
        Cursor.SetCursor(normal, Vector2.zero, CursorMode.Auto);
        Managers.Game.build.gameObject.SetActive(false); 
        Managers.Game.isHandleUI = true;
    }

    void SetHandleUIMode()
    {
        Cursor.SetCursor(normal, Vector2.zero, CursorMode.Auto);
        Managers.Game.build.gameObject.SetActive(false);
        Managers.Game.isHandleUI = true;
    }
    #endregion
}
