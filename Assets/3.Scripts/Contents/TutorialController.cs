using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Playables;
using UnityEngine.SceneManagement;

public class TutorialController : MonoBehaviour
{
    public AssetReferenceT<AudioClip> completeSoundAsset;

    AudioClip completeSound;

    public static GameObject timeController;

    public ItemSO material;

    GameObject moveHelp;
    GameObject invenHelp;
    GameObject interactHelp;
    GameObject produceHelp;
    GameObject findHelp;
    GameObject unlockHelp;
    GameObject surviveHelp;
    GameObject buildHelp;
    GameObject laborlatoryHelp;

    GameObject interactHelpWall;
    GameObject invenHelpWall;
    TriggerEvent findHelpWall;
    GameObject towerHelp1Wall;
    GameObject towerHelp2Wall;

    PlayableDirector towerCutScene;
    PlayableDirector surviveCutScene;

    private void Start()
    {
        Transform tutorialUI = Util.FindChild<Transform>(gameObject, "UI_Tutorial", true);

        moveHelp = Util.FindChild(gameObject, "Move", true);
        invenHelp = Util.FindChild(gameObject, "Inventory", true);
        interactHelp = Util.FindChild(gameObject, "Interact", true);
        produceHelp = Util.FindChild(gameObject, "Produce", true);
        findHelp = Util.FindChild(gameObject, "Find", true);
        surviveHelp = Util.FindChild(gameObject, "Survive", true);
        buildHelp = Util.FindChild(gameObject, "Build", true);
        laborlatoryHelp = Util.FindChild(gameObject, "Laboratory", true);
        findHelpWall = Util.FindChild<TriggerEvent>(gameObject, "FindCut", true);
        interactHelpWall = Util.FindChild(gameObject, "InteractHelpWall",true);
        invenHelpWall = Util.FindChild(gameObject, "InvenHelpWall",true);
        towerHelp1Wall = Util.FindChild(gameObject, "TowerHelp1Wall", true);
        towerHelp2Wall = Util.FindChild(gameObject, "TowerHelp2Wall", true);
        unlockHelp = Util.FindChild(gameObject, "Unlock",true);

        towerCutScene = Util.FindChild<PlayableDirector>(gameObject,"TowerCut", true);
        surviveCutScene = Util.FindChild<PlayableDirector>(gameObject,"SurviveCut", true);

        PlayerController.tutorial1Event = Move;
        UI_Inventory.tutorial1Event = Inventory;
        CanInteractChecker.interactCheckerEvent = ShowInteractHelp;
        TrashPileController.interactEvent = Interact;
        findHelpWall.triggerEvent = Find;
        TowerPosUnLock.unlockEvent = Unlock;
        UI_Produce.tutorialEvent = Produce;


        invenHelp.SetActive(false);
        interactHelp.SetActive(false);
        produceHelp.SetActive(false);
        findHelp.SetActive(false);
        unlockHelp.SetActive(false);
        surviveHelp.SetActive(false);
        buildHelp.SetActive(false);
        laborlatoryHelp.SetActive(false);   

        BuildController.tutorialEvent = Build;
        MonsterWaveController.surviveEvent = Survive;

        completeSoundAsset.LoadAssetAsync().Completed += (clip) =>
        {
            completeSound = clip.Result;
        };
    }

    void Move()
    {
        if (moveHelp.activeSelf)
            Clear(moveHelp);
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
            Clear(invenHelp, findHelp);
        else
            return;

        UI_Inventory.tutorial1Event = null;
        Destroy(invenHelpWall);
    }

    void Find()
    {
        if (findHelp.activeSelf)
            Clear(findHelp, unlockHelp);
        else
            return;
        findHelpWall.triggerEvent = null;
    }
    void Unlock()
    {
        if (unlockHelp.activeSelf)
            Clear(unlockHelp);
        else
            return;

        TowerPosUnLock.unlockEvent = null;
        Destroy(towerHelp1Wall);
        Destroy(towerHelp2Wall);
        StartCoroutine(SetTower());
    }

    void Clear(GameObject currentHelp,GameObject nextHelp = null)
    {
        currentHelp.GetComponent<Animator>().Play("Hide");
        Managers.Sound.Play(Define.Sound.Effect, completeSound);
        if(nextHelp != null)
         StartCoroutine(Next(nextHelp));
    }

    public IEnumerator Next(GameObject nextHelp)
    {
        yield return new WaitForSeconds(1.5f);
        nextHelp.SetActive(true);
    }

    void Produce()
    {
        if (produceHelp.activeSelf)
            Clear(produceHelp);
        else
            return;

        UI_Produce.tutorialEvent = null;
    }

    void Build()
    {
        if (buildHelp.activeSelf)
            Clear(buildHelp,surviveHelp);
        else
            return;

        BuildController.tutorialEvent = null;
        TimeController.timeSpeed = 1.2f;
    }

    void Survive()
    {
        if (surviveHelp.activeSelf)
            Clear(surviveHelp);
        else
            return;

        UI_MiniMap.targetPos = new Vector2(-157, -36);
        UI_MiniMap.isTargeting = true;
        MonsterWaveController.surviveEvent -= Survive;
        StartCoroutine(Show());
    }

    void End()
    {
        Managers.Game.completeTutorial = true;
        SceneManager.LoadScene("GameScene");
    }

    IEnumerator SetTower()
    {
        yield return new WaitForSeconds(1.5f);
        Managers.Game.tower.gameObject.SetActive(true);
        Managers.Game.tower.GetComponent<Animator>().Play("End");
        towerCutScene.Play();
        Managers.Game.timeController.gameObject.SetActive(true);
        TimeController.TimeAmount = 1170;
        TimeController.timeSpeed = 0;
        Managers.Inven.AddItems(material,3);
    }

    IEnumerator Show()
    {
        yield return new WaitForSeconds(2f);
        surviveCutScene.Play();
    }
}
