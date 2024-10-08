using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Dialog
{
    public string Massage;
    public float PausePerLetter;
    public bool Timeline;
    public string Name;
    public Sprite Profill;
}

public class DialogAnimator : MonoBehaviour
{
    [SerializeField] Text dialogText;
    [SerializeField] Text nameText;
    [SerializeField] Image profillImage;

    

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
            yield return new WaitForSecondsRealtime(dialog.PausePerLetter);
        }
    }

    string colorText = "";

    void SetTextColor(string s)
    {
        colorText += s;
        string text = "<color=#E33131>" + $"{colorText}"+"</color>";
        dialogText.text = dialogMassage + text;
    }
}
