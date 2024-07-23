using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PopUICloser : MonoBehaviour
{
    GameObject gameMenuUI;

    private void Start()
    {
        gameMenuUI = Managers.UI.ShowUI("UI_GameMenu");
    }

    void OnClosePopUI()
    {
        int count = Managers.UI.PopUIList.Count;
        if (count > 0)
        {
            Managers.UI.PopUIList[count - 1].SetActive(false);
        }
        else
        {
            gameMenuUI.SetActive(true);
        }
    }
}
