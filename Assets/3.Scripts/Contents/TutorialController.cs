using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialController : MonoBehaviour
{
    public ItemSO branch;

    private void Start()
    {
        MapManager.matter.SetTile(new Vector3Int(0,-2),branch.tile);
        MapManager.matter.SetTile(new Vector3Int(1,0),branch.tile);
        MapManager.matter.SetTile(new Vector3Int(3,-1),branch.tile);
        MapManager.matter.SetTile(new Vector3Int(2,-3),branch.tile);
        MapManager.matter.SetTile(new Vector3Int(3,1),branch.tile);
    }
}
