using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MouseController : UI_Base
{
    //마우스
    [HideInInspector]
    public GameObject icon;
    [HideInInspector]
    public GameObject count;

    enum GameObjects
    {
        Icon,
        Count
    }

    public override void Init()
    {
        if (_init)
            return;

        _init = true;
        Bind<GameObject>(typeof(GameObjects));
        icon = Get<GameObject>((int)GameObjects.Icon);
        count = Get<GameObject>((int)GameObjects.Count);
    }

    private void OnEnable()
    {
        transform.position = Camera.main.ScreenToWorldPoint(Input.mousePosition) + new Vector3(0, 0, 10);
    }

    private void Update()
    {
        transform.position = Camera.main.ScreenToWorldPoint(Input.mousePosition)+new Vector3(0,0,10);
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
            }
        }
    }

    public Texture2D normal;
    public Texture2D drag;
   
    void SetNormalMode()
    {
        gameObject.SetActive(false);
        Cursor.SetCursor(normal, Vector2.zero, CursorMode.Auto);
        Managers.Game.build.gameObject.SetActive(false);
    }

    void SetBuildingMode()
    {
        Managers.Game.build.gameObject.SetActive(true);
        gameObject.SetActive(false);
    }

    void SetDragMode()
    {
        //gameObject.SetActive(true);
        Cursor.SetCursor(drag, Vector2.zero, CursorMode.Auto);
        Managers.Game.build.gameObject.SetActive(false);
    }

    void SetBattleMode()
    {
        gameObject.SetActive(false);
        Cursor.SetCursor(normal, Vector2.zero, CursorMode.Auto);
        Managers.Game.build.gameObject.SetActive(false);
    }
    #endregion
}
