using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public interface IPickable 
{
    public void Pick();
}

public interface IConsumable
{
    public void Consume();
}

public class Item : MonoBehaviour
{
    public ItemSO itemSo;
}


