using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    GameObject Root { get { return root; } set { root = value; root.name = "Root";  } }
    GameObject root;

    public GameObject ShowUI(string path)
    {
        if(Root == null)
            Root = new GameObject();

        GameObject go =  Instantiate(Resources.Load<GameObject>($"UI/{path}"),Root.transform);
        return go;
    }
}
