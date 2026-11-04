using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public sealed partial class FieldHUD : MonoBehaviour
{
    static FieldHUD instance;
    public static bool IsOpen => instance != null && (instance.panel.activeSelf || instance.entryPanel.activeSelf);
    GameObject entryPanel;
    FieldLaboratory offeredLab;
    UnityEngine.UI.Text prompt, notice, status;
    UnityEngine.UI.Button[] researchButtons;
    UnityEngine.UI.Text[] researchLabels;
    GameObject panel;
    Font font;
    Object promptOwner;
    FieldWorkbench workbench;
    bool busy;
    float noticeUntil;
    public static void Create(Font font)
    {
        if(instance!=null) return;
        var go=new GameObject("ExplorationHUD"); instance=go.AddComponent<FieldHUD>(); instance.Build(font);
    }
    public static void Prompt(Object owner,string text) { if(instance==null) return; instance.promptOwner=owner; instance.prompt.text=text; }
    public static void ClearPrompt(Object owner) { if(instance!=null && instance.promptOwner==owner) { instance.prompt.text=""; instance.promptOwner=null; } }
    public static void Notify(string text) { if(instance==null) return; instance.notice.text=text; instance.noticeUntil=Time.unscaledTime+4; }
    public static void Open(FieldWorkbench station)
    {
        if(instance==null || IsOpen || PlayerController.isDie) return;
        instance.workbench=station; instance.panel.SetActive(true);
        Managers.UI.PopUIList.Add(instance.panel);
        instance.Refresh();
    }
    RectTransform Rect(string label,Transform parent)
    {
        var go=new GameObject(label,typeof(RectTransform)); go.transform.SetParent(parent,false); return go.GetComponent<RectTransform>();
    }
    UnityEngine.UI.Text Label(string label,Transform parent,string value,int size)
    {
        var rect=Rect(label,parent); var text=rect.gameObject.AddComponent<UnityEngine.UI.Text>();
        text.font=font; text.fontSize=size; text.text=value; text.color=new Color(.95f,.95f,.9f);
        text.alignment=TextAnchor.MiddleCenter; text.raycastTarget=false; return text;
    }
    void Layout(Transform target,float height)
    { var element=target.gameObject.AddComponent<UnityEngine.UI.LayoutElement>(); element.preferredHeight=height; element.minHeight=height; }
    UnityEngine.UI.Button Button(string label,Transform parent,string value,System.Action action,float height)
    {
        var rect=Rect(label,parent); Layout(rect,height);
        var image=rect.gameObject.AddComponent<UnityEngine.UI.Image>(); image.color=new Color(.16f,.24f,.23f);
        var button=rect.gameObject.AddComponent<UnityEngine.UI.Button>(); button.targetGraphic=image;
        var text=Label("Label",rect,value,19); var tr=text.rectTransform;
        tr.anchorMin=Vector2.zero; tr.anchorMax=Vector2.one; tr.offsetMin=new Vector2(10,3); tr.offsetMax=new Vector2(-10,-3);
        button.onClick.AddListener(()=>action()); return button;
    }
    void Build(Font selectedFont)
    {
        font=selectedFont;
        var canvas=gameObject.AddComponent<Canvas>(); canvas.renderMode=RenderMode.ScreenSpaceOverlay; canvas.sortingOrder=300;
        var scaler=gameObject.AddComponent<UnityEngine.UI.CanvasScaler>(); scaler.uiScaleMode=UnityEngine.UI.CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution=new Vector2(1280,720); scaler.matchWidthOrHeight=.5f;
        gameObject.AddComponent<UnityEngine.UI.GraphicRaycaster>();
        prompt=Label("Interaction",transform,"",21); prompt.rectTransform.anchorMin=prompt.rectTransform.anchorMax=new Vector2(.5f,0);
        prompt.rectTransform.anchoredPosition=new Vector2(0,175); prompt.rectTransform.sizeDelta=new Vector2(1050,55);
        notice=Label("Notice",transform,"",23); notice.rectTransform.anchorMin=notice.rectTransform.anchorMax=new Vector2(.5f,1);
        notice.rectTransform.anchoredPosition=new Vector2(0,-70); notice.rectTransform.sizeDelta=new Vector2(1100,60);
        var rect=Rect("CampResearchPanel",transform); rect.anchorMin=rect.anchorMax=new Vector2(.5f,.5f); rect.sizeDelta=new Vector2(880,590);
        panel=rect.gameObject; panel.AddComponent<UnityEngine.UI.Image>().color=new Color(.055f,.075f,.08f,.98f);
        var layout=panel.AddComponent<UnityEngine.UI.VerticalLayoutGroup>(); layout.padding=new RectOffset(24,24,18,18); layout.spacing=8;
        layout.childControlWidth=true; layout.childControlHeight=true; layout.childForceExpandWidth=true; layout.childForceExpandHeight=false;
        var title=Label("Title",rect,"중앙 야영지 · 연구 작업대",26); Layout(title.transform,38);
        status=Label("Resources",rect,"",18); Layout(status.transform,66);
        researchButtons=new UnityEngine.UI.Button[4]; researchLabels=new UnityEngine.UI.Text[4];
        for(int i=0;i<4;i++)
        {
            int index=i;
            researchButtons[i]=Button(FieldResearch.All[i].id,rect,"",()=>BeginResearch(index),70);
            researchLabels[i]=researchButtons[i].GetComponentInChildren<UnityEngine.UI.Text>();
        }
        Button("Save",rect,"안전한 야영지에서 탐색 진행 저장",()=>Save(),40);
        Button("Close",rect,"닫기 / 연구 취소 (Esc)",()=>Close(),40);
        panel.SetActive(false);
        BuildStoryHud();
        var entry = Rect("LaboratoryEntry", transform);
        entry.anchorMin = entry.anchorMax = new Vector2(.5f,.5f);
        entry.sizeDelta = new Vector2(360,172);
        entryPanel = entry.gameObject;
        entryPanel.AddComponent<UnityEngine.UI.Image>().color = new Color(.055f,.075f,.08f,.98f);
        var stack = entryPanel.AddComponent<UnityEngine.UI.VerticalLayoutGroup>();
        stack.padding = new RectOffset(16,16,12,12); stack.spacing = 6;
        Layout(Label("Title",entry,"폐주거지 연구소에 들어갈까요?",22).transform,48);
        Button("Enter",entry,"들어가기",()=> { var lab = offeredLab; CloseLabEntry(); if (lab) lab.Enter(); },40);
        Button("Return",entry,"돌아가기",CloseLabEntry,40);
        entryPanel.SetActive(false);
    }
    public static void OfferLabEntry(FieldLaboratory lab)
    {
        if (!instance || IsOpen || PlayerController.isDie || Managers.Game.isCantPlay) return;
        CloseStory();
        instance.offeredLab = lab;
        instance.entryPanel.SetActive(true);
        Managers.UI.PopUIList.Add(instance.entryPanel);
    }
    public static void CloseLabEntry()
    {
        if (!instance || !instance.entryPanel) return;
        instance.entryPanel.SetActive(false);
        instance.offeredLab = null;
        if (Managers.HasInstance) Managers.UI.PopUIList.Remove(instance.entryPanel);
    }
    void Update()
    {
        UpdateStoryHud();
        if(Time.unscaledTime>noticeUntil) notice.text="";
        if(entryPanel.activeSelf && (PlayerController.isDie || Keyboard.current?.escapeKey.wasPressedThisFrame == true)) CloseLabEntry();
        if(!panel.activeSelf) { Managers.UI.PopUIList.Remove(panel); return; }
        if(workbench==null || Managers.Game.player==null || PlayerController.isDie ||
            Vector2.Distance(workbench.transform.position,Managers.Game.player.transform.position)>2.2f ||
            Keyboard.current!=null && Keyboard.current.escapeKey.wasPressedThisFrame) { Close(); return; }
        if(!busy) Refresh();
    }
    void Refresh()
    {
        var c=ExplorationRuntime.Instance.content;
        int scrap=FieldInventory.Count(c.scrap),parts=FieldInventory.Count(c.parts);
        var reason=ExplorationRuntime.WorkBlockReason(workbench.transform.position);
        status.text=$"고철 {scrap} · 정비 부품 {parts} · 대포 {ExplorationEffects.CannonCount()}/{FieldSession.CannonLimit}\n"+(reason ?? "연구 3초 · 이동/피격/취소 시 비용을 소비하지 않습니다.");
        for(int i=0;i<4;i++)
        {
            var r=FieldResearch.All[i]; bool owned=FieldSession.State.Has(r.id);
            researchLabels[i].text=$"{r.name}  ·  {(owned ? "연구 완료" : $"고철 {r.scrap} + 부품 {r.parts}")}\n{r.effect}";
            researchButtons[i].interactable=!busy && reason==null && FieldSession.CanBuy(r,scrap,parts);
        }
    }
    void BeginResearch(int index) { if(!busy) StartCoroutine(Research(FieldResearch.All[index])); }
    IEnumerator Research(FieldResearch research)
    {
        var c=ExplorationRuntime.Instance.content;
        if(ExplorationRuntime.WorkBlockReason(workbench.transform.position)!=null || !FieldSession.CanBuy(research,FieldInventory.Count(c.scrap),FieldInventory.Count(c.parts))) yield break;
        busy=true; Refresh();
        Vector2 start=Managers.Game.player.transform.position;
        var stat=Managers.Game.player.GetComponent<PlayerStat>(); bool damaged=false;
        System.Action onDamage=()=>damaged=true; stat.damagedEvent+=onDamage;
        try
        {
            for(float time=0;time<3;time+=Time.deltaTime)
            {
                if(!IsOpen || damaged || Vector2.Distance(start,Managers.Game.player.transform.position)>.1f ||
                    ExplorationRuntime.WorkBlockReason(workbench.transform.position)!=null) { Notify("연구가 취소되었습니다. 비용은 소비되지 않았습니다."); yield break; }
                status.text=$"{research.name} 연구 중 · {Mathf.CeilToInt(3-time)}초\n예약 비용: 고철 {research.scrap} / 부품 {research.parts}";
                yield return null;
            }
            if(!FieldSession.CanBuy(research,FieldInventory.Count(c.scrap),FieldInventory.Count(c.parts)) ||
                !FieldInventory.Spend(c.scrap,research.scrap,c.parts,research.parts)) { Notify("재료가 부족해 연구를 취소했습니다."); yield break; }
            ExplorationEffects.PurchaseResearch(research.id);
            Notify(research.name+" 완료! 안전한 야영지에서 저장할 수 있습니다.");
        }
        finally { if(stat!=null) stat.damagedEvent-=onDamage; busy=false; }
    }
    void Save()
    {
        if(busy) return;
        string reason=ExplorationRuntime.WorkBlockReason(workbench.transform.position);
        if(reason!=null) { Notify(reason); return; }
        try { FieldCheckpoint.Save(); Notify("탐색 진행을 저장했습니다. 메인 화면에서 이어갈 수 있습니다."); }
        catch(System.Exception e) { Debug.LogException(e); Notify("저장하지 못했습니다. 기존 저장은 유지됩니다."); }
    }
    void Close()
    {
        panel.SetActive(false); Managers.UI.PopUIList.Remove(panel);
    }
    void OnDestroy() { if(panel!=null && Managers.HasInstance) Managers.UI.PopUIList.Remove(panel); if(instance==this) { CloseLabEntry(); instance=null; } }
}
