using System.Collections;
using UnityEngine;

public sealed class FieldReward : MonoBehaviour,IInteractObject
{
    public GameObject canInteractSign => gameObject;
    FieldRewardDefinition definition;
    ExplorationContent content;
    bool selected, collecting;
    Coroutine routine;
    System.Action detachDamage;
    GameObject marker;
    SpriteRenderer visual;
    public void Configure(FieldRewardDefinition data, ExplorationContent config)
    {
        definition=data; content=config;
        visual=GetComponentInChildren<SpriteRenderer>();
        marker=new GameObject("SupplyMarker"); marker.transform.SetParent(transform,false);
        marker.transform.localPosition=new Vector3(0,1.3f,0);
        var icon=marker.AddComponent<SpriteRenderer>();
        icon.sprite=config.Item(data.items[0]).itemIcon; icon.sharedMaterial=config.material; icon.sortingOrder=3;
        icon.color=string.IsNullOrEmpty(data.group) ? new Color(1,.9f,.55f) : new Color(.55f,.95f,1);
        if(icon.sprite!=null) marker.transform.localScale=Vector3.one*(.85f/Mathf.Max(.01f,icon.sprite.bounds.size.x));
    }
    public bool Empty
    {
        get { for(int i=0;i<definition.items.Length;i++) if(Remaining(i)>0) return false; return true; }
    }
    int Remaining(int index) => OuterSalvageLedger.Replaced(content,definition,index) ? 0 :
        Mathf.Max(0,definition.amounts[index]-FieldSession.State.Claimed(definition.id+":"+index));
    string BlockReason => definition.requiresFirstNight && !FieldSession.State.firstNightCleared ? "첫밤 완료 후 수령" :
        !ExplorationRuntime.Instance.Cleared(definition.group) ? "주변 경비를 모두 처치하세요" : null;
    void Update()
    {
        if(definition==null) return;
        if(marker!=null) marker.SetActive(!Empty);
        visual.color=Empty ? new Color(.4f,.4f,.4f) : definition.salvageFragments ? new Color(1, .85f, .35f) : Color.white;
        if(selected && !collecting) FieldHUD.Prompt(this, Empty ? definition.region+" · 수령 완료" :
            definition.region+" · "+(BlockReason ?? $"F를 {definition.collectSeconds:0}초간 누르고 있어 조사")+
            (definition.salvageFragments ? $" · 조각 +{Remaining(0)} · 고철 {OuterSalvageLedger.Total(FieldSession.State,content)%OuterSalvageLedger.PiecesPerScrap}/{OuterSalvageLedger.PiecesPerScrap}" : ""));
    }
    void Interact()
    {
        if (collecting || Empty || BlockReason!=null || !FieldInventory.Ready || FieldHUD.IsOpen || PlayerController.isDie) return;
        routine=StartCoroutine(Collect());
    }
    IEnumerator Collect()
    {
        collecting=true;
        Vector2 start=Managers.Game.player.transform.position;
        var stat=Managers.Game.player.GetComponent<PlayerStat>();
        bool damaged=false; System.Action onDamage=()=>damaged=true; stat.damagedEvent+=onDamage;
        detachDamage=()=> { if(stat!=null) stat.damagedEvent-=onDamage; };
        try
        {
            for(float time=0;time<definition.collectSeconds;time+=Time.deltaTime)
            {
                if(!selected || damaged || PlayerController.isDie || Managers.Game.isCantPlay ||
                    Vector2.Distance(start,Managers.Game.player.transform.position)>.2f) yield break;
                FieldHUD.Prompt(this,definition.region+" 조사 중 "+Mathf.CeilToInt(definition.collectSeconds-time)+"초");
                yield return null;
            }
            if(definition.salvageFragments)
            {
                bool success=OuterSalvageLedger.TryCollect(FieldSession.State,content,definition,
                    amount=>FieldInventory.Give(content.scrap,amount),out int pieces,out int scrap);
                FieldHUD.Notify(!success ? "가방이 가득 찼습니다. 잔해는 그대로 남습니다." :
                    $"고철 조각 +{pieces}"+(scrap>0 ? " · 고철 1개 완성!" : "")+
                    $" · 다음 고철 {OuterSalvageLedger.Total(FieldSession.State,content)%OuterSalvageLedger.PiecesPerScrap}/{OuterSalvageLedger.PiecesPerScrap}");
                yield break;
            }
            int accepted=0;
            for(int i=0;i<definition.items.Length;i++)
            {
                int count=FieldInventory.Give(content.Item(definition.items[i]),Remaining(i));
                FieldSession.State.AddClaim(definition.id+":"+i,count); accepted+=count;
            }
            FieldHUD.Notify(accepted>0 ? (Empty ? "보급품을 모두 수령했습니다." : "일부 수령했습니다. 남은 보급품은 상자에 보관됩니다.") : "가방이 가득 찼습니다. 공간을 확보하세요.");
        }
        finally { detachDamage?.Invoke(); detachDamage=null; collecting=false; routine=null; }
    }
    void Cancel() { if(collecting && selected) FieldHUD.Notify("조사 취소 · F를 누르고 있으면 수령합니다."); if(routine!=null) StopCoroutine(routine); routine=null; collecting=false; detachDamage?.Invoke(); detachDamage=null; }
    public void ShowInteractSign() { selected=true; }
    public void HideInteractSign() { selected=false; FieldHUD.ClearPrompt(this); }
    public void SetAction(PlayerInteract player) { player.interactAction+=Interact; player.cancelAction+=Cancel; }
    public void CancelAction(PlayerInteract player) { player.interactAction-=Interact; player.cancelAction-=Cancel; }
    void OnDisable() { Cancel(); FieldHUD.ClearPrompt(this); }
}
