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
    [SerializeField] float endTime;
    float curTime = 0;

    bool cancel = false;

    bool ispPlaying;

    Image skipGauge;
    GameObject skipUI;



    private void Start()
    {
        skipGauge = Util.FindChild<Image>(gameObject, "SkipGauge",true);
        skipUI = Util.FindChild(gameObject, "SkipUI", true);
        if(skipUI != null)
            skipUI.SetActive(false);
    }

    public void StartTimeline()
    {
        ispPlaying = true;
        Managers.Game.isCantPlay = true;
    }

    public void EndTimeline()
    {
        ispPlaying = false;
        Destroy(gameObject);
        Managers.Game.isCantPlay = false;
    }

    public void ActionSkip(InputAction.CallbackContext context)
    {
        if (context.performed && ispPlaying)
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
        GetComponent<PlayableDirector>().time = endTime;
    }
}
