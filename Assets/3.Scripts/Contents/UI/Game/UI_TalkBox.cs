using System;
using System.Collections;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.UI;

public class UI_TalkBox : MonoBehaviour
{
    public AssetReferenceT<AudioClip> typingSoundAsset;

    AudioClip typingSound;


    [SerializeField]
    float delayTime;
    float beforeDelayTime;
    Text talkText;

    GameObject nextSign;

    string talkString;
    int index = 0;

    public void Bind()
    {
        talkText = Util.FindChild<Text>(gameObject, "TalkText", true);
        nextSign = Util.FindChild(gameObject, "NextSign", true);
        talkString = talkText.text;
        beforeDelayTime = delayTime;

        typingSoundAsset.LoadAssetAsync().Completed += (clip) =>
        {
            typingSound = clip.Result;
        };
        nextSign.SetActive(false);
    }

    public IEnumerator TypingText()
    {
        talkText.text = "";
        gameObject.SetActive(true);
        while (index < talkString.Length)
        {
            string s = talkString.Substring(index, 1);
            index++;
            if (s == "%")   //전체띄우기(문장 앞에 띄울것)
            {
                delayTime = 0;
            }
            else if (s == "/")  //끝에 있을때 대화가 끝난다
            {
                break;
            }
            else if (s == "<")  //색깔이나 폰트나 글자 크기를 바꿀때 쓰는 꺽쇠
            {
                talkText.text += s;
                delayTime = 0;
            }
            else if (s == ">")
            {
                talkText.text += s;
                delayTime = beforeDelayTime;
            }
            else
                talkText.text += s;
            Managers.Sound.Play(Define.Sound.Effect, typingSound);
            yield return new WaitForSecondsRealtime(delayTime);
        }
        transform.GetComponentInParent<TalkController>().canNextTalk = true;
        nextSign.SetActive(true);
    }
}
