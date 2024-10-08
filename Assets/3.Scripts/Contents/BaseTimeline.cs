using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.Playables;
using UnityEngine.UI;

public class BaseTimeline : MonoBehaviour
{
    [SerializeField] float skipTime;
    float curTime = 0;

    bool cancel = false;

    Image skipGauge;
    GameObject skipUI;

    private void Start()
    {
        skipGauge = Util.FindChild<Image>(gameObject, "SkipGauge",true);
        skipUI = Util.FindChild(gameObject, "SkipUI", true);
        skipUI.SetActive(false);
    }

    public void StartTimeline()
    {
        Managers.Game.isCantPlay = true;
    }

    public void EndTimeline()
    {
        Managers.Game.isCantPlay = false;
    }

    public void ActionSkip(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            skipUI.SetActive(true);
            cancel = false;
            StartCoroutine(FillSkipGauge());
        }
        else if(context.canceled)
        {
            cancel = true;
            curTime = 0;
            SetGauge();
            skipUI.SetActive(false);
        }
    }

    IEnumerator FillSkipGauge()
    {
        while (curTime < skipTime)
        {
            yield return null;
            if (cancel)
                yield break;
            curTime += Time.deltaTime;
            SetGauge();
        }
        Skip();
    }

    void SetGauge()
    {
        skipGauge.fillAmount = curTime / skipTime;
    }

    void Skip()
    {
        EndTimeline();
       Destroy(gameObject);
    }
}
