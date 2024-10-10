using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ChangeSceneEffecter : MonoBehaviour
{
    Animator anim;
    private void Start()
    {
        Managers.Game.changeSceneEffecter = this;
        anim = GetComponent<Animator>();
        EndChangeScene();
    }

    public void StartChangeScene()
    {
        anim.Play("Start");
    }

    public void EndChangeScene()
    {
       
        anim.Play("End",-1,0);
    }
}
