using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Managers : MonoBehaviour
{
    static Managers instance;
    public static Managers Instance { get { Init(); return instance; } }

    #region Contents
    GameManager _game = new();
    ObjectMananger _obj = new();
    NetworkManager _network = new();
    InvenManager _inven = new();

    public static ObjectMananger Object { get { return Instance._obj; } }
    public static NetworkManager Network { get { return Instance._network; } }
    public static GameManager Game { get { return Instance._game; } }
    public static InvenManager Inven { get { return Instance._inven; } }
    #endregion

    UIManager _ui = new();
    InputManager _input = new();
    SceneManagerEx _scene = new();
    MapManager _map = new();
    SoundManager _sound = new();
    DataManager _datas = new();

    public static UIManager UI { get { return Instance._ui; } }
    public static MapManager Map { get { return Instance._map; } }
    public static InputManager Input { get { return Instance._input; } }
    public static SceneManagerEx Scene { get { return Instance._scene; } }
    public static SoundManager Sound { get { return Instance._sound; } }
    public static DataManager Data { get { return Instance._datas; } }

    public static void Init()
    {
        if (instance == null)
        {
            GameObject go = null;

            go = new GameObject { name = "@Managers" };
            go.AddComponent<Managers>();

            DontDestroyOnLoad(go);
            instance = go.GetComponent<Managers>();

            instance._network.Init(3333);
            instance._sound.Init();        
        }
    }
}
