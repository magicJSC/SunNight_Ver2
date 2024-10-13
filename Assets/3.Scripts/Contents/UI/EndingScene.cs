using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class EndingScene : MonoBehaviour
{
    private void Start()
    {
        StartCoroutine(Main());
    }

    IEnumerator Main()
    {
        yield return new WaitForSeconds(3f);
        Managers.Game.changeSceneEffecter.StartChangeScene();
        yield return new WaitForSeconds(1.5f);
        SceneManager.LoadScene("MainScene");
    }
}
