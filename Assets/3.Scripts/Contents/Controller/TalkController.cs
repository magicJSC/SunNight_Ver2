using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TalkController : MonoBehaviour
{
    int talkIndex = 0;

    [HideInInspector]
    public bool canNextTalk;
    private void Start()
    {
        Time.timeScale = 0;
        for(int i = 0; i < transform.childCount; i++)
        {
            UI_TalkBox talkBox = transform.GetChild(i).GetComponent<UI_TalkBox>();
            talkBox.Bind();
            talkBox.gameObject.SetActive(false);
        }
       StartCoroutine(transform.GetChild(talkIndex).GetComponent<UI_TalkBox>().TypingText());
    }

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.Space))
        {
            if (!canNextTalk)
                return;

            if (talkIndex + 1 < transform.childCount)
            {
                canNextTalk = false;
                transform.GetChild(talkIndex).gameObject.SetActive(false);
                talkIndex++; 
                StartCoroutine(transform.GetChild(talkIndex).GetComponent<UI_TalkBox>().TypingText());
            }
            else
            {
                Time.timeScale = 1;
                Destroy(gameObject);
            }
        }
    }
}
