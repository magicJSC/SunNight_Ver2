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
        if (!BaseReadyForEntry()) yield break;
        Managers.Game.changeSceneEffecter.StartChangeScene();
        yield return new WaitForSeconds(2);
        if (!BaseReadyForEntry())
        {
            Managers.Game.changeSceneEffecter.EndChangeScene();
            yield break;
        }
        if (FieldSession.Active && FieldInventory.Ready) FieldCheckpoint.TransferPreparation = FieldCheckpoint.Capture();
        SceneManager.LoadScene("Boss1");
    }

    bool BaseReadyForEntry()
    {
        if (!Managers.Game.isMovingTower && !TowerMoveController.IsHandling) return true;
        enterUI.SetActive(false);
        FieldHUD.Notify("거점을 설치한 뒤 공장에 들어가세요.");
        return false;
    }


    private void OnCollisionEnter2D(Collision2D collision)
    {
        if(collision.transform.GetComponent<PlayerController>() != null)
        {
            enterUI.SetActive(true);
        }
    }
}
