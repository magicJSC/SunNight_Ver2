using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Playables;
using UnityEngine.UI;

public class Dialog
{
    public string Massage;
    public float PausePerLetter;
    public bool Pause;
    public string Name;
    public double NextTime;
    public Sprite Profill;
}

public class DialogAnimator : MonoBehaviour
{
    [SerializeField] Text dialogText;
    [SerializeField] Text nameText;
    [SerializeField] Image profillImage;
    PlayableDirector director;

    GameObject nextSign;

    private void Start()
    {
        director = GetComponent<PlayableDirector>();
        nextSign = Util.FindChild(gameObject, "NextSign", true);
    }

    bool isEnd = false;
    double nextTime;

    public void AddDialog(Dialog dialog)
    {
        if(nameText != null)
            nameText.text = dialog.Name;
        StartCoroutine(Typing(dialog));
    }

    string dialogMassage;
    IEnumerator Typing(Dialog dialog)
    {
        dialogText.text = "";
        dialogMassage = "";
        profillImage.sprite = dialog.Profill;
        int index = 0;
        bool isColor = false;
        isEnd = false;
        nextSign.SetActive(false);
        while (index < dialog.Massage.Length)
        {
            string s = dialog.Massage.Substring(index, 1);

            if (s == "%")  //»ö±òÀÌ³ª ÆùÆ®³ª ±ÛÀÚ Å©±â¸¦ ¹Ù²Ü¶§ ¾²´Â ²©¼è
            {
                isColor = !isColor;
                index++;
                if (isColor)
                    colorText = "";
                else
                    dialogMassage += "<color=#E33131>" + $"{colorText}" + "</color>";
                continue;
            }
            else
            {
                if (isColor)
                    SetTextColor(s);
                else
                {
                    dialogMassage += s;
                    dialogText.text = dialogMassage;
                }
            }
            index++;
            if(dialog.PausePerLetter != 0)
                yield return new WaitForSecondsRealtime(dialog.PausePerLetter);
        }

        if (dialog.Pause)
        {
            director.Pause();
            nextSign.SetActive(true);
            isEnd = true;
            nextTime = dialog.NextTime;
        }
    }

    string colorText = "";

    void SetTextColor(string s)
    {
        colorText += s;
        string text = "<color=#E33131>" + $"{colorText}"+"</color>";
        dialogText.text = dialogMassage + text;
    }

    public void TalkBoxAction(InputAction.CallbackContext context)
    {
        if(context.canceled)
        {
            if (isEnd)
            {
                director.time = nextTime;
                director.Resume();
            }
        }
    }
}
