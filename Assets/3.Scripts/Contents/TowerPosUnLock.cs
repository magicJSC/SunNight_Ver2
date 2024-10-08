using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;

public class TowerPosUnLock : MonoBehaviour
{
    public AssetReferenceGameObject unlockTowerPosUIAsset;

    GameObject unlockTowerPosUI;

    private void Start()
    {
        unlockTowerPosUIAsset.LoadAssetAsync().Completed += (obj) =>
        {
            unlockTowerPosUI = Instantiate(obj.Result);
            unlockTowerPosUI.SetActive(false);
        };
    }

    [SerializeField] int index;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (Managers.Game.isUnlockTowerPos[index])
            return;
        if(collision.GetComponent<IPlayer>() != null)
        {
            unlockTowerPosUI.SetActive(true);
        }
    }
}
