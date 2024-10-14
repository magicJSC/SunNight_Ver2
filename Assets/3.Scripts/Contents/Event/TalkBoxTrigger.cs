using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;

public class TalkBoxTrigger : MonoBehaviour
{
    [SerializeField] TalkSO talkSO;

    public AssetReferenceGameObject talkUIAsset;

    public bool oneTime;

    GameObject talkUIPrefab;

    private void Start()
    {
        talkUIAsset.LoadAssetAsync().Completed += (obj) => 
        {
            talkUIPrefab = obj.Result;
        };
    }


    public void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.GetComponent<IPlayer>() != null)
        {
            Instantiate(talkUIPrefab).GetComponent<UI_TalkBox>().talkSO = talkSO;
            if(oneTime)
                Destroy(gameObject);
        }
    }
}
