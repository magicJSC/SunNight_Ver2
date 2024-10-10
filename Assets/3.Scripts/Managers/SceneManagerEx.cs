using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneManagerEx : MonoBehaviour
{
    private Define.SceneType _curSceneType = Define.SceneType.None;

    public Define.SceneType CurrentSceneType
    {
        get
        {
            if (_curSceneType != Define.SceneType.None)
                return _curSceneType;
            return CurrentScene.SceneType;
        }
        set { _curSceneType = value; }
    }

    public BaseScene CurrentScene { get { return GameObject.FindObjectOfType<BaseScene>(); } }

    public IEnumerator ChangeScene(Define.SceneType type)
    {
        CurrentScene.Clear();

        _curSceneType = type;
        yield return new WaitForSeconds(3);
        SceneManager.LoadScene(GetSceneName(type));
    }


    string GetSceneName(Define.SceneType type)
    {
        string name = System.Enum.GetName(typeof(Define.SceneType), type);
        char[] letters = name.ToLower().ToCharArray();
        letters[0] = char.ToUpper(letters[0]);
        return new string(letters);
    }
}
