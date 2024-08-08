using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.SceneManagement;

public class TutorialController : MonoBehaviour
{
    public AssetReferenceT<AudioClip> completeSoundAsset;

    public ItemSO cannon;
    public GameObject monsterSpawner;

    AudioClip completeSound;

    GameObject timeController;

    int tutorialIndex = 0;

    List<GameObject> tutorialList = new List<GameObject>();

    private void Start()
    {

        Transform tutorialUI = Util.FindChild<Transform>(gameObject, "UI_Tutorial", true);
        for(int i = 0; i < tutorialUI.childCount; i++)
        {
            tutorialList.Add(tutorialUI.GetChild(i).gameObject);
        }
        tutorialList[0].SetActive(true);

        PlayerController.tutorial1Event = Move;
        PlayerController.tutorial2Event = Pick;
        UI_Inventory.tutorial1Event = Inventory;
        UI_Inventory.tutorial2Event = Produce1;
        UI_Produce.tutorialEvent = Produce2;
        TowerController.tutorial1Event = MoveTower;
        UI_HotBar.tutorialEvent = ChoiceHotBar;
        TowerController.tutorial2Event = InstallTower;
        BuildController.tutorialEvent = InstallBuildItem;
        TimeController.tutorialEvent = Survive;
        TowerController.tutorial3Event = Sleep;

        completeSoundAsset.LoadAssetAsync().Completed += (clip) =>
        {
            completeSound = clip.Result;
        };

        timeController = (GameObject)Instantiate(Resources.Load("UI/UI_Time"));
        timeController.SetActive(false);
        monsterSpawner.SetActive(false);
    }

    void Move()
    {
        if(tutorialIndex == 0)
            Clear();
    }

    void Pick()
    {
        if (tutorialIndex == 1)
            Clear();
    }

    void Inventory()
    {
        if (tutorialIndex == 2)
            Clear();
    }

    void Produce1()
    {
        if (tutorialIndex == 3)
            Clear();
    }

    void Produce2()
    {
        if (tutorialIndex == 4)
            Clear();
    }

    void MoveTower()
    {
        if (tutorialIndex == 5)
            Clear();
    }

    void ChoiceHotBar()
    {
        if (tutorialIndex == 6)
        {
            Clear();
        }
    }

    void InstallTower()
    {
        if (tutorialIndex == 7)
        {
            Clear();
            Managers.Inven.AddOneItem(cannon);
        }
    }

    void InstallBuildItem()
    {
        if (tutorialIndex == 8)
        {
            Clear();
            timeController.SetActive(true);
            monsterSpawner.SetActive(true);
            TimeController.timeSpeed = 15;
        }
    }

    void Survive()
    {
        if (tutorialIndex == 9)
        {
            Clear();
            TimeController.timeSpeed = 0;
        }
    }

    void Sleep()
    {
        if (tutorialIndex == 10)
        {
            Clear();

            GetComponent<Animator>().Play("End");
        }
    }

    void Clear()
    {
        tutorialIndex++;
        tutorialList[tutorialIndex - 1].GetComponent<Animator>().Play("Hide");
        Managers.Sound.Play(Define.Sound.Effect, completeSound);
        StartCoroutine(Next());
    }

    public IEnumerator Next()
    {
        yield return new WaitForSeconds(1.5f);
        tutorialList[tutorialIndex].SetActive(true);
    }

    void End()
    {
        Managers.Game.completeTutorial = true;
        SceneManager.LoadScene("GameScene");
    }
}
