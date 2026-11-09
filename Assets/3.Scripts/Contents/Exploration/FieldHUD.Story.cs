using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public sealed partial class FieldHUD
{
    public enum StoryPage { Map, Journal, Signal }

    GameObject storyCard;
    Text storyObjective, storyTitle, storyText;
    RawImage survey;
    Button mapTab, journalTab, signalTab;

    void BuildStoryHud()
    {
        storyObjective = Label("StoryObjective", transform, "", 18);
        var objective = storyObjective.rectTransform;
        objective.anchorMin = objective.anchorMax = objective.pivot = Vector2.up;
        objective.anchoredPosition = new Vector2(18, -14);
        objective.sizeDelta = new Vector2(500, 36);
        storyObjective.alignment = TextAnchor.MiddleLeft;

        var rect = Rect("StoryRecord", transform);
        rect.anchorMin = rect.anchorMax = rect.pivot = Vector2.up;
        rect.anchoredPosition = new Vector2(18, -52);
        rect.sizeDelta = new Vector2(400, 315);
        storyCard = rect.gameObject;
        storyCard.AddComponent<Image>().color = new Color(.055f, .075f, .08f, .97f);
        var layout = storyCard.AddComponent<VerticalLayoutGroup>();
        layout.padding = new RectOffset(16, 16, 12, 12);
        layout.spacing = 5;
        layout.childForceExpandHeight = false;
        storyTitle = Label("Title", rect, "", 22);
        Layout(storyTitle.transform, 32);
        survey = Rect("DistrictSurvey", rect).gameObject.AddComponent<RawImage>();
        survey.raycastTarget = false;
        Layout(survey.transform, 400);
        survey.gameObject.SetActive(false);
        storyText = Label("Record", rect, "", 18);
        storyText.alignment = TextAnchor.MiddleLeft;
        storyText.resizeTextForBestFit = true;
        storyText.resizeTextMinSize = 14;
        storyText.resizeTextMaxSize = 18;
        Layout(storyText.transform, 205);
        var tabs = Rect("Tabs", rect);
        Layout(tabs, 36);
        tabs.gameObject.AddComponent<HorizontalLayoutGroup>().spacing = 4;
        mapTab = Button("Map", tabs, "지도", () => ShowStory(StoryPage.Map), 36);
        journalTab = Button("Journal", tabs, "일지", () => ShowStory(StoryPage.Journal), 36);
        signalTab = Button("Signal", tabs, "신호", () => ShowStory(StoryPage.Signal), 36);
        Button("Dismiss", tabs, "닫기", () => storyCard.SetActive(false), 36);
        storyCard.SetActive(false);
        UpdateStoryHud();
    }

    void UpdateStoryHud()
    {
        var state = FieldSession.State;
        storyObjective.text = !state.HasBasementMap ? (state.basementMapDropped ? "로봇이 떨어뜨린 지도 줍기 · F" : "") : state.sunNightJournalRead
            ? (FieldLaboratory.PlayerInside ? "공장 파괴 0/3 · 남쪽 계단으로 지상 복귀   [M] 지도" : "공장 파괴 0/3 · 동쪽 폭스턴 공장 찾기   [M] 지도")
            : FieldLaboratory.PlayerInside ? "지하 연구실 · 북쪽 단말 조사   [M] 지도" : "남쪽 폐주거지에서 연구소 찾기   [M] 지도";
        if ((!state.HasBasementMap && !state.firstNightCleared) || Keyboard.current == null || !Keyboard.current.mKey.wasPressedThisFrame) return;
        if (storyCard.activeSelf) storyCard.SetActive(false);
        else ShowStory(state.HasBasementMap ? StoryPage.Map : StoryPage.Signal);
    }
    public static void CloseStory() { if (instance != null) instance.storyCard.SetActive(false); }

    public static void ShowStory(StoryPage page)
    {
        if (instance == null || instance.storyCard == null) return;
        var hud = instance;
        var state = FieldSession.State;
        bool map = page == StoryPage.Map;
        if (map && !state.HasBasementMap || page == StoryPage.Journal && !state.sunNightJournalRead ||
            page == StoryPage.Signal && !state.firstNightCleared) return;
        var rect = (RectTransform)hud.storyCard.transform;
        rect.anchorMin = rect.anchorMax = rect.pivot = map ? new Vector2(.5f,1) : Vector2.up;
        rect.anchoredPosition = map ? new Vector2(0,-52) : new Vector2(18,-52);
        rect.sizeDelta = map ? new Vector2(732,558) : new Vector2(400,315);
        hud.survey.gameObject.SetActive(map);
        var body = hud.storyText.GetComponent<LayoutElement>();
        body.minHeight = body.preferredHeight = map ? 38 : 205;
        hud.mapTab.interactable = state.HasBasementMap;
        hud.journalTab.interactable = state.sunNightJournalRead;
        hud.signalTab.interactable = state.firstNightCleared;
        switch (page)
        {
            case StoryPage.Map:
                bool restored = state.sunNightJournalRead;
                hud.survey.texture = Resources.Load<Texture2D>(restored ? "Story/DistrictSurvey" : "Story/LabApproach");
                hud.storyTitle.text = restored ? "복원된 지구도 · 동부 공장" : "접힌 지도 · 연구실 수색 구역";
                hud.storyText.text = restored ? "야영지는 연구실 북동쪽, 폭스턴 공장은 야영지에서 동북쪽이다." : "남문을 나와 폐주거지로 가자. 연구소 남쪽 정문에 다가가 입장을 선택하자.";
                break;
            case StoryPage.Journal:
                hud.storyTitle.text = "PROJECT SUNNIGHT · 연구 일지";
                hud.storyText.text = "로봇들이 나를 쫓고 있다. 내가 남긴 방어 거점을 찾아라.\n\nNIGHT 명령 — 지상 생존 거점 제거\nSUN 단계 기상 예정 인원 — 2명\n전환 조건 — 지상 생존 거점 0개\n\n지구도 복원 완료. 지도 탭에서 야영지와 폭스턴 공장의 위치를 확인하자.";
                break;
            case StoryPage.Signal:
                hud.storyTitle.text = "거점 수신기 · 첫밤 이후 05:30";
                hud.storyText.text = "미등록 거점의 생존 신호를 확인했다. SUN 단계 승인을 보류한다.\n\n발신지: 동쪽 공장\n\n날이 밝았다. 공장은 우리가 살아 있다는 걸 안다.";
                break;
        }
        hud.storyCard.SetActive(true);
    }
}
