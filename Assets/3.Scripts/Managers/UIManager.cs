using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    GameObject Root { get { if (root == null) MakeRoot(); return root; } }
    GameObject root;

    GameObject Inven { get { if (inven == null) MakeInven(); return inven; } }
    GameObject inven;

    public List<GameObject> PopUIList = new List<GameObject>();

    void MakeRoot()
    {
        root = new GameObject();
        root.name = "Root";
    }

    void MakeInven()
    {
        inven = new GameObject();
        inven.name = "Inven";
        inven.AddComponent<Canvas>().renderMode = RenderMode.ScreenSpaceCamera;
        inven.GetComponent<Canvas>().worldCamera = Camera.main;
        inven.GetComponent<Canvas>().sortingOrder = 4;
        inven.GetComponent<Canvas>().sortingLayerID = 3;
        inven.transform.SetParent(Root.transform);
    }

    public GameObject ShowUI(GameObject go)
    {
        GameObject Go =  Instantiate(go,Root.transform);
        return Go;
    }

    public GameObject ShowInvenUI(string path)
    {
        GameObject go = Instantiate(Resources.Load<GameObject>($"UI/{path}"), Inven.transform);
        go.GetComponent<RectTransform>().anchoredPosition += new Vector2(960, 540);
        go.transform.localScale = new Vector2(1, 1);
        return go;
    }

    public T ShowUI<T>(GameObject go,Transform parent = null)
    {
        if(parent == null)
            go =  Instantiate(go,Root.transform);
        else
            go = Instantiate(go,parent);
        return go.GetComponent<T>();
    }

    public GameObject ShowInvenUI(GameObject go)
    {
        go = Instantiate(go, Inven.transform);
        go.GetComponent<RectTransform>().anchoredPosition += new Vector2(960, 540);
        go.transform.localScale = new Vector2(1, 1);
        return go;
    }

    public T ShowInvenUI<T>(GameObject go)
    {
        go = Instantiate(go, Inven.transform);
        go.GetComponent<RectTransform>().anchoredPosition += new Vector2(960, 540);
        go.transform.localScale = new Vector2(1, 1);
        return go.GetComponent<T>();
    }
}
