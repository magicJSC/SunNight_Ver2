using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class Boss1EnterTrigger : MonoBehaviour
{
    GameObject enterUI;
    GameObject enterText;
    GameObject returnText;



    private void Start()
    {
        enterUI = transform.GetChild(0).gameObject;
        enterText = Util.FindChild(gameObject, "Enter", true);
        returnText = Util.FindChild(gameObject, "Return", true);

        UI_EventHandler evt = enterText.GetComponent<UI_EventHandler>();
        evt._OnClick += (PointerEventData p) => { StartCoroutine(EnterFactory()); };
        evt = returnText.GetComponent<UI_EventHandler>();
        evt._OnClick += (PointerEventData p) => { enterUI.SetActive(false); };
        enterUI.SetActive(false);
    }

    IEnumerator EnterFactory()
    {
        Managers.Game.changeSceneEffecter.StartChangeScene();
        yield return new WaitForSeconds(2);
        SceneManager.LoadScene("Boss1");
    }


    private void OnCollisionEnter2D(Collision2D collision)
    {
        if(collision.transform.GetComponent<PlayerController>() != null)
        {
            enterUI.SetActive(true);
        }
    }
}
