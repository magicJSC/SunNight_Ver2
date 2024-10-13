using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.EventSystems;

public class UI_Journal : MonoBehaviour
{
    [SerializeField] TalkSO talkSO;

    UI_EventHandler leave;
    public AssetReferenceGameObject talkUIAsset;
    GameObject talkUI;
    private void Start()
    {
        Managers.Game.isCantPlay = true;
        leave = Util.FindChild<UI_EventHandler>(gameObject,"Leave",true);
        leave._OnClick += (PointerEventData p) => 
        {
            Managers.Game.isCantPlay = false;  
            Instantiate(talkUI).GetComponent<UI_TalkBox>().talkSO = talkSO;
            Destroy(gameObject); 
        };
        talkUIAsset.LoadAssetAsync().Completed += (obj) =>
        {
            talkUI = obj.Result;
        };
    }
}
