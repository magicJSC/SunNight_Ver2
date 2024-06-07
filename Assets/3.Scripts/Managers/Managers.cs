using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Managers : MonoBehaviour
{
    static Managers instance;
    public static Managers Instance { get { return instance; } }

    #region Contents
    GameManager _game = new GameManager();
    ObjectMananger _obj = new ObjectMananger();
    NetworkManager _network = new NetworkManager();
    InvenManager _inven = new InvenManager();

    public static ObjectMananger Object { get { return Instance._obj; } }
    public static NetworkManager Network { get { return Instance._network; } }
    public static GameManager Game { get { return Instance._game; } }
    public static InvenManager Inven { get { return Instance._inven; } }
    #endregion

    InputManager _input = new InputManager();
    SceneManagerEx _scene = new SceneManagerEx();

    public static InputManager Input { get {  return Instance._input; } }
    public static SceneManagerEx Scene { get { return Instance._scene; } }

    public static void Init()
    {
        if (instance == null)
        {
            GameObject go = GameObject.Find("@Managers");
            if (go == null)
            {
                go = new GameObject { name = "@Managers" };
                go.AddComponent<Managers>();
            }

            DontDestroyOnLoad(go);
            instance = go.GetComponent<Managers>();

            instance._network.Init();
            instance._inven.Init();
            instance._game.Init();
        }
    }

    private void Update()
    {
        _input.OnUpdate();
        _game.OnUpdate();
    }
}
