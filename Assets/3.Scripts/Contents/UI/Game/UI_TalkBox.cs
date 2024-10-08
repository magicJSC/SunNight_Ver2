using System;
using System.Collections;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.UI;

public class UI_TalkBox : MonoBehaviour
{
    public TalkSO talkSO;

    TalkSO.Talk talk;

    float delayTime;
    Text talkText;
    Text nameText;
    Image profill;

    GameObject nextSign;

    int talkIndex = 0;

    string talkString;
    int index = 0;

    public void Start()
    {
        talkText = Util.FindChild<Text>(gameObject, "TalkText", true);
        nameText = Util.FindChild<Text>(gameObject, "NameText", true);
        profill = Util.FindChild<Image>(gameObject, "Profill", true);
        nextSign = Util.FindChild(gameObject, "NextSign", true);
        talk = talkSO.groups[0];

        StartCoroutine(TypingText());
    }

    public IEnumerator TypingText()
    {
        talkText.text = "";
        talkString = talk.talkText;
        nameText.text = talk.nameText;
        profill.sprite = talk.illust;
        delayTime = talk.perTypingDelay;
        nextSign.SetActive(false);
        Managers.Game.isCantPlay = true;

        while (index < talkString.Length)
        {
            string s = talkString.Substring(index, 1);
            index++;
            talkText.text += s;
            if(delayTime != 0)
                yield return new WaitForSecondsRealtime(delayTime);
        }
        nextSign.SetActive(true);
       
    }

    public void OnNextTalk()
    {
        if (nextSign.activeSelf)
        {
            if (talk.isFinish)
            {
                Managers.Game.isCantPlay = false;
                Destroy(gameObject);
                return;
            }
            else
            {
                talk = talkSO.groups[talkIndex++];
                StartCoroutine(TypingText());
            }
        }
    }
}
