using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Util
{
    public static int RandomNumber(int start,int range)
    {
        int index = Random.Range(start, range);
        return index;
    }
    public static T GetOrAddComponent<T>(GameObject go) where T : UnityEngine.Component
    {
        T component = go.GetComponent<T>();
        if (component == null)
            component = go.AddComponent<T>();
        return component;
    }
    public static GameObject FindChild(GameObject go, string name = null, bool recursive = false)
    {
        Transform transform = FindChild<Transform>(go, name, recursive);
        if (transform == null)
            return null;

        return transform.gameObject;
    }

    //여기서 recursive는 이름을 찾을때 자식의 자식까지 찾을지 bool값으로 확인
    public static T FindChild<T>(GameObject go, string name = null, bool recursive = false) where T : UnityEngine.Object
    {
        if (go == null)
            return null;

        if (recursive == false)
        {
            for (int i = 0; i < go.transform.childCount; i++)
            {
                Transform transform = go.transform.GetChild(i);
                if (string.IsNullOrEmpty(name) || transform.name == name)
                {
                    T component = transform.GetComponent<T>();
                    if (component != null)
                        return component;
                }
            }
        }
        else
        {
            foreach (T component in go.GetComponentsInChildren<T>())
            {
                if (string.IsNullOrEmpty(name) | component.name == name)
                    return component;


            }
        }

        return null;
    }

    public static string GetOriginalName(string name)
    {
        if (!name.Contains('('))
            return name;

        name = name.Substring(0,name.IndexOf('('));
        return name;
    }

    public static int GetTotalHp(float hp,float def,float damage,float pen=0)
    {
        int result = (int)(hp - (100 / (100 + def - pen)) * damage);
        return result;
    }
}
