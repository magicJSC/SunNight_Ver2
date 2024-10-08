using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.SceneManagement;

public class TutorialController : MonoBehaviour
{
    public AssetReferenceT<AudioClip> completeSoundAsset;

    AudioClip completeSound;

    public static GameObject timeController;

    GameObject moveHelp;
    GameObject invenHelp;
    GameObject interactHelp;
    GameObject produceHelp;

    GameObject interactHelpWall;
    GameObject invenHelpWall;

    private void Start()
    {
        Transform tutorialUI = Util.FindChild<Transform>(gameObject, "UI_Tutorial", true);

        moveHelp = Util.FindChild(gameObject, "Move", true);
        invenHelp = Util.FindChild(gameObject, "Inventory", true);
        interactHelp = Util.FindChild(gameObject, "Interact", true);
        produceHelp = Util.FindChild(gameObject, "Produce", true);
        interactHelpWall = Util.FindChild(gameObject, "InteractHelpWall");
        invenHelpWall = Util.FindChild(gameObject, "InvenHelpWall");

        invenHelp.SetActive(false);
        interactHelp.SetActive(false);
        produceHelp.SetActive(false);

        PlayerController.tutorial1Event = Move;
        UI_Inventory.tutorial1Event = Inventory;
        CanInteractChecker.interactCheckerEvent = ShowInteractHelp;
        TrashPileController.interactEvent = Interact;
        //UI_Inventory.tutorial2Event = Produce1;
        //UI_Produce.tutorialEvent = Produce2;
        //TowerController.tutorial1Event = MoveTower;
        //UI_HotBar.tutorialEvent = ChoiceHotBar;
        //TowerController.tutorial2Event = InstallTower;
        //BuildController.tutorialEvent = InstallBuildItem;
        //TimeController.tutorialEvent = Survive;
        //TowerController.tutorial3Event = Sleep;

        completeSoundAsset.LoadAssetAsync().Completed += (clip) =>
        {
            completeSound = clip.Result;
        };
    }

    void Move()
    {
        if (moveHelp.activeSelf)
            Clear(moveHelp, invenHelp);
        else
            return;

        PlayerController.tutorial1Event = null;
    }

    void ShowInteractHelp()
    {
        interactHelp.SetActive(true);
    }

    void Interact()
    {
        if (interactHelp.activeSelf)
            Clear(interactHelp, produceHelp);
        else
            return;

        CanInteractChecker.interactCheckerEvent = null;
        TrashPileController.interactEvent = null;
        Destroy(interactHelpWall);
    }

    void Inventory()
    {
        if (invenHelp.activeSelf)
            Clear(invenHelp, produceHelp);
        else
            return;

        UI_Inventory.tutorial1Event = null;
        Destroy(invenHelpWall);
    }


    

    void Clear(GameObject currentHelp,GameObject nextHelp)
    {
        currentHelp.GetComponent<Animator>().Play("Hide");
        Managers.Sound.Play(Define.Sound.Effect, completeSound);
        //StartCoroutine(Next(nextHelp));
    }

    public IEnumerator Next(GameObject nextHelp)
    {
        yield return new WaitForSeconds(1.5f);
        nextHelp.SetActive(true);
    }

    void End()
    {
        Managers.Game.completeTutorial = true;
        SceneManager.LoadScene("GameScene");
    }
}
