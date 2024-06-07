using System;
using System.Collections.Generic;
using UnityEngine;

public class ObjectMananger
{
    Dictionary<int, GameObject> _objects = new Dictionary<int, GameObject>();

    public void Add(int id, GameObject go)
    {
        _objects.Add(id, go);
    }

    public void Remove(int id)
    {
        _objects.Remove(id);
    }

    public GameObject Find(Vector3 Pos)
    {
        foreach (GameObject obj in _objects.Values)
        {
        }

        return null;
    }

    public void Clear()
    {
        _objects.Clear();
    }
}