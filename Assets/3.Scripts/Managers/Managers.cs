using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Managers : MonoBehaviour
{
    static Managers instance;
    public static bool IsShuttingDown { get; private set; }
    // Cleanup callbacks must never acquire (and therefore create) a singleton.
    public static bool HasInstance => !IsShuttingDown && instance != null;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void ResetLifetime()
    {
        instance = null;
        IsShuttingDown = false;
    }

    void OnApplicationQuit()
    {
        IsShuttingDown = true;
    }

    void OnDestroy()
    {
        if (instance != this) return;
        IsShuttingDown = true;
        instance = null;
    }
    public static Managers Instance { get { Init(); return instance; } }

    #region Contents
    GameManager _game;
    ObjectMananger _obj = new();
    NetworkManager _network;
    InvenManager _inven;

    public static ObjectMananger Object { get { return Instance._obj; } }
    public static NetworkManager Network { get { return Instance._network; } }
    public static GameManager Game { get { return Instance._game; } }
    public static InvenManager Inven { get { return Instance._inven; } }
    #endregion

    UIManager _ui;
    InputManager _input;
    SceneManagerEx _scene;
    SoundManager _sound = new();
    DataManager _datas;

    public static UIManager UI { get { return Instance._ui; } }
    // MapManager belongs to the loaded scene, never to the persistent services root.
    public static MapManager Map { get { return Game.grid; } }
    public static InputManager Input { get { return Instance._input; } }
    public static SceneManagerEx Scene { get { return Instance._scene; } }
    public static SoundManager Sound { get { return Instance._sound; } }
    public static DataManager Data { get { return Instance._datas; } }

    public static void Init()
    {
        if (IsShuttingDown || !Application.isPlaying) return;
        if (instance == null)
        {
            GameObject go = null;

            go = new GameObject { name = "@Managers" };
            go.AddComponent<Managers>();

            DontDestroyOnLoad(go);
            instance = go.GetComponent<Managers>();

            instance._game = go.AddComponent<GameManager>();
            instance._network = go.AddComponent<NetworkManager>();
            instance._inven = go.AddComponent<InvenManager>();
            instance._ui = go.AddComponent<UIManager>();
            instance._input = go.AddComponent<InputManager>();
            instance._scene = go.AddComponent<SceneManagerEx>();
            instance._datas = go.AddComponent<DataManager>();

            instance._network.Init(3333);
            instance._sound.Init();        
        }
    }
}
