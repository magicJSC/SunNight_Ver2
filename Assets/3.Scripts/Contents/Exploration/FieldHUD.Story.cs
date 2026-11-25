using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public sealed partial class FieldHUD
{
    public enum StoryPage { Map, Journal, Signal, FieldNotes, NightPrep, RestoredMap, ReturnSlip, SouthboundManifest, EasternSketch }
    static readonly StoryPage[] JournalPages = { StoryPage.FieldNotes, StoryPage.NightPrep, StoryPage.Journal, StoryPage.ReturnSlip, StoryPage.SouthboundManifest };
    static readonly StoryPage[] MapPages = { StoryPage.Map, StoryPage.RestoredMap, StoryPage.EasternSketch };
    static readonly StoryPage[] SignalPages = { StoryPage.Signal };
    static StoryPage[] Section(StoryPage page) => page switch
    {
        StoryPage.Map or StoryPage.RestoredMap or StoryPage.EasternSketch => MapPages,
        StoryPage.Signal => SignalPages,
        _ => JournalPages
    };
    readonly System.Collections.Generic.List<StoryPage> readablePages = new();
    StoryPage currentPage;
    Button previousPage, nextPage;
    Text pageNumber;
    RawImage turningLeaf;
    StoryPage destination;
    int turnDirection;
    float turnStarted;
    const float TurnSeconds = .44f;

    static readonly Color Ink = new Color32(66, 51, 41, 255);
    static readonly Color Pencil = new Color32(92, 81, 58, 255);
    static readonly Color RedInk = new Color32(135, 61, 46, 255);
    GameObject storyCard;
    Text storyObjective, storyTitle, storyText, slipTitle, slipText, folio;
    RectTransform missionBanner;
    Text missionCaption;
    float missionChangedAt;
    bool inFoxtonFactory;
    RawImage survey;
    RectTransform eastSearchArea, eastGateStamp;
    RectTransform dockStamp, gateStamp, surgeStamp;
    bool HasSurgeMap => !inFoxtonFactory && FieldSession.State.southwestGateDiscovered;
    bool HasDockMap => !inFoxtonFactory && FieldSession.State.foxtonReturnSlipCollected;
    Button mapTab, journalTab, signalTab;
    RectTransform notebook, bookmarkMark;
    float openedAt;

    // Coordinates correspond to the 540 x 304 PixelFX illustration. UI text uses twice that resolution.
    static void OnPaper(RectTransform rect, float x, float y, float w, float h)
    {
        rect.anchorMin = rect.anchorMax = rect.pivot = Vector2.up;
        rect.anchoredPosition = new Vector2(x, -y) * 2;
        rect.sizeDelta = new Vector2(w, h) * 2;
    }

    Text NotebookText(string name, int size, Color color, int x, int y, int w, int h)
    {
        Text text = Label(name, notebook, "", size);
        OnPaper(text.rectTransform, x, y, w, h);
        text.alignment = TextAnchor.UpperLeft;
        text.color = color;
        text.lineSpacing = 1.1f;
        return text;
    }

    Button PaperButton(string name, string caption, System.Action action, int x, int y, int w, int h, Color ink)
    {
        var rect = Rect(name, notebook);
        OnPaper(rect, x, y, w, h);
        var hit = rect.gameObject.AddComponent<Image>();
        hit.color = Color.clear;
        var button = rect.gameObject.AddComponent<Button>();
        var label = Label("Label", rect, caption, 17);
        label.color = ink;
        label.rectTransform.anchorMin = Vector2.zero;
        label.rectTransform.anchorMax = Vector2.one;
        label.rectTransform.offsetMin = label.rectTransform.offsetMax = Vector2.zero;
        button.targetGraphic = label;
        var colors = button.colors;
        colors.disabledColor = new Color(1, 1, 1, .45f);
        button.colors = colors;
        button.onClick.AddListener(() => action());
        return button;
    }

    void BuildStoryHud()
    {
        inFoxtonFactory = gameObject.scene.name == "Boss1";
        missionBanner = Rect("MissionBanner", transform);
        OnPaper(missionBanner, 8, 6, 180, 30);
        var plate = missionBanner.gameObject.AddComponent<RawImage>();
        plate.texture = Resources.Load<Texture2D>("Story/MissionBanner");
        plate.raycastTarget = false;
        missionCaption = Label("Caption", missionBanner, "", 13);
        OnPaper(missionCaption.rectTransform, 26, 4, 144, 8);
        missionCaption.alignment = TextAnchor.MiddleLeft;
        missionCaption.color = new Color32(239, 201, 139, 255);
        storyObjective = Label("StoryObjective", missionBanner, "", 20);
        OnPaper(storyObjective.rectTransform, 26, 11, 145, 12);
        storyObjective.horizontalOverflow = HorizontalWrapMode.Overflow;
        storyObjective.alignment = TextAnchor.MiddleLeft;
        storyObjective.color = new Color32(255, 243, 216, 255);

        var overlay = Rect("StoryRecord", transform);
        overlay.anchorMin = Vector2.zero;
        overlay.anchorMax = Vector2.one;
        overlay.offsetMin = overlay.offsetMax = Vector2.zero;
        storyCard = overlay.gameObject;
        storyCard.AddComponent<Image>().color = new Color(.07f, .08f, .075f, .60f);
        notebook = Rect("Notebook", overlay);
        notebook.sizeDelta = new Vector2(1080, 608);
        var book = notebook.gameObject.AddComponent<RawImage>();
        book.texture = Resources.Load<Texture2D>("Story/FieldNotebook");

        storyTitle = NotebookText("Title", 29, Ink, 36, 34, 213, 20);
        storyText = NotebookText("Record", 22, Ink, 38, 75, 204, 104);
        slipTitle = NotebookText("SlipTitle", 25, RedInk, 292, 34, 200, 20);
        slipText = NotebookText("SlipText", 21, Ink, 301, 88, 183, 130);
        folio = NotebookText("Folio", 15, Pencil, 68, 264, 147, 12);
        pageNumber = NotebookText("PageNumber", 15, Pencil, 215, 264, 35, 12);
        pageNumber.alignment = TextAnchor.UpperRight;
        survey = Rect("DistrictSurvey", notebook).gameObject.AddComponent<RawImage>();
        OnPaper(survey.rectTransform, 34, 82, 218, 147);
        // Enlarge the geographic portion of the sheet. Its key is transcribed on the facing page.
        survey.raycastTarget = false;
        dockStamp = BuildSurveyStamp("NorthwestMarshDock", "4", "북서 늪지 인수장",
            new Vector2(-209, 147), 4, 85, new Color32(55, 82, 70, 255));
        gateStamp = BuildSurveyStamp("NorthwestFactoryGate", "5", "공장 정문",
            new Vector2(-222, 153), -17, 58, RedInk);
        surgeStamp = BuildSurveyStamp("SouthwestGate", "6", "남서 송전 공장",
            new Vector2(SouthwestSwitchyard.GateX, SouthwestSwitchyard.GateY), -15, 80, RedInk);

        eastSearchArea = Rect("EastSearchArea", survey.transform);
        OnPaper(eastSearchArea, 135, 86, 29, 19);
        var highlight = eastSearchArea.gameObject.AddComponent<Image>();
        highlight.color = new Color32(174, 83, 55, 65);
        highlight.raycastTarget = false;
        var border = eastSearchArea.gameObject.AddComponent<Outline>();
        border.effectColor = RedInk;
        border.effectDistance = new Vector2(2, -2);
        eastSearchArea.gameObject.SetActive(false);

        eastGateStamp = BuildSurveyStamp("EastRefugeGate", "7", "거점 입구", EasternRefuge.Entrance, -16, 51, RedInk);

        Color tabInk = new Color32(255, 238, 198, 255);
        journalTab = PaperButton("Journal", "일지", () => ShowStory(AvailableNotes()), 68, 1, 54, 17, tabInk);
        mapTab = PaperButton("Map", "지도", () => ShowStory(AvailableMap()), 138, 1, 54, 17, tabInk);
        signalTab = PaperButton("Signal", "신호", () => ShowStory(StoryPage.Signal), 208, 1, 54, 17, tabInk);
        bookmarkMark = Rect("BookmarkMark", notebook);
        var marker = bookmarkMark.gameObject.AddComponent<Image>();
        marker.color = tabInk;
        marker.raycastTarget = false;
        PaperButton("Dismiss", "수첩 덮기  [T]", CloseStory, 381, 258, 82, 18, Pencil);
        var arrow = Resources.Load<Texture2D>("Story/NotebookArrow");
        previousPage = PageArrow("PreviousPage", 31, -1, arrow);
        nextPage = PageArrow("NextPage", 472, 1, arrow);
        turningLeaf = Rect("TurningLeaf", notebook).gameObject.AddComponent<RawImage>();
        OnPaper(turningLeaf.rectTransform, 0, 0, 540, 304);
        turningLeaf.texture = Resources.Load<Texture2D>("Story/NotebookLeafTurn");
        turningLeaf.raycastTarget = false;
        turningLeaf.gameObject.SetActive(false);
        storyCard.SetActive(false);
        UpdateStoryHud();
    }

    RectTransform BuildSurveyStamp(string id, string number, string caption, Vector2 world, int offsetY, int width, Color ink)
    {
        // Match the 436 x 314 freight chart to its 218 x 147 notebook rectangle.
        var stamp = Rect(id, survey.transform);
        OnPaper(stamp, (world.x + 425) * .25f, (282 - world.y) * .5f * 147f / 314f, 0, 0);
        float elbow = offsetY + 6;
        RectTransform Block(string name, float x, float y, float w, float h, Color tint)
        {
            var rect = Rect(name, stamp);
            OnPaper(rect, x, y, w, h);
            var image = rect.gameObject.AddComponent<Image>();
            image.color = tint;
            image.raycastTarget = false;
            return rect;
        }
        Block("Pin", -1.5f, -1.5f, 3, 3, ink);
        Block("Stem", -.5f, Mathf.Min(elbow, 0), 1, Mathf.Abs(elbow), ink);
        Block("Leader", 0, elbow - .5f, 7, 1, ink);
        var paper = Block("Paper", 6, offsetY, width, 12, new Color32(244, 226, 180, 250));
        var mark = Block("Marker", 6, offsetY, 12, 12, ink);
        var numeral = Label("Number", mark, number, 17);
        OnPaper(numeral.rectTransform, 0, 0, 12, 12);
        numeral.color = Color.white;
        var label = Label("Name", paper, caption, 17);
        OnPaper(label.rectTransform, 14, 0, width - 14, 12);
        label.alignment = TextAnchor.MiddleLeft;
        label.color = Ink;
        stamp.gameObject.SetActive(false);
        return stamp;
    }

    static StoryPage AvailableNotes() => FieldSession.State.mecamossCargoOpened ? StoryPage.SouthboundManifest :
        FieldSession.State.foxtonReturnSlipCollected ? StoryPage.ReturnSlip :
        FieldSession.State.sunNightJournalRead ? StoryPage.Journal : StoryPage.FieldNotes;
    StoryPage AvailableMap() => FieldSession.State.mecamossCargoOpened ? StoryPage.EasternSketch :
        HasDockMap || HasSurgeMap || FieldSession.State.sunNightJournalRead ? StoryPage.RestoredMap : StoryPage.Map;

    Button PageArrow(string name, int x, int direction, Texture2D texture)
    {
        var target = Rect(name, notebook);
        OnPaper(target, x, 255, 32, 25);
        target.gameObject.AddComponent<Image>().color = Color.clear;
        var icon = Rect("Ink", target);
        icon.sizeDelta = new Vector2(40, 26);
        var image = icon.gameObject.AddComponent<RawImage>();
        image.texture = texture;
        image.raycastTarget = false;
        if (direction < 0) image.uvRect = new Rect(1, 0, -1, 1);
        var button = target.gameObject.AddComponent<Button>();
        button.targetGraphic = image;
        var colors = button.colors;
        colors.disabledColor = new Color(1, 1, 1, .18f);
        button.colors = colors;
        button.onClick.AddListener(() => TurnPage(direction));
        return button;
    }

    bool CanRead(StoryPage page) => page switch
    {
        StoryPage.Map => FieldSession.State.HasBasementMap,
        StoryPage.RestoredMap => FieldSession.State.sunNightJournalRead || HasDockMap || HasSurgeMap,
        StoryPage.Journal => FieldSession.State.sunNightJournalRead,
        StoryPage.ReturnSlip => FieldSession.State.foxtonReturnSlipCollected,
        StoryPage.SouthboundManifest or StoryPage.EasternSketch => FieldSession.State.mecamossCargoOpened,
        StoryPage.Signal => FieldSession.State.firstNightCleared,
        _ => true
    };

    void TurnPage(int direction)
    {
        int next = readablePages.IndexOf(currentPage) + direction;
        if (turnDirection != 0 || next < 0 || next >= readablePages.Count) return;
        destination = readablePages[next];
        turnDirection = direction;
        turnStarted = Time.unscaledTime;
        previousPage.interactable = nextPage.interactable = false;

        // Reveal the destination underneath the lifting leaf. Keep the opposite
        // half of the old spread in place until the leaf settles over it.
        var oldTitle = storyTitle.text;
        var oldNotes = storyText.text;
        var oldMap = survey.texture;
        var oldCrop = survey.uvRect;
        bool oldDockStamp = dockStamp.gameObject.activeSelf;
        bool oldEastGate = eastGateStamp.gameObject.activeSelf;
        bool oldEastArea = eastSearchArea.gameObject.activeSelf;
        bool oldGateStamp = gateStamp.gameObject.activeSelf;
        bool oldSurgeStamp = surgeStamp.gameObject.activeSelf;
        var oldSlipTitle = slipTitle.text;
        var oldSlip = slipText.text;
        var oldFolio = folio.text;
        FillSpread(destination);
        if (direction > 0)
        {
            storyTitle.text = oldTitle;
            storyText.text = oldNotes;
            survey.texture = oldMap;
            survey.uvRect = oldCrop;
            dockStamp.gameObject.SetActive(oldDockStamp);
            gateStamp.gameObject.SetActive(oldGateStamp);
            eastSearchArea.gameObject.SetActive(oldEastArea);
            eastGateStamp.gameObject.SetActive(oldEastGate);
            surgeStamp.gameObject.SetActive(oldSurgeStamp);
            folio.text = oldFolio;
        }
        else
        {
            slipTitle.text = oldSlipTitle;
            slipText.text = oldSlip;
        }
        turningLeaf.gameObject.SetActive(true);
        AnimateTurn();
    }

    void AnimateTurn()
    {
        if (turnDirection == 0) return;
        float progress = (Time.unscaledTime - turnStarted) / TurnSeconds;
        if (progress >= 1f) { ShowStory(destination); return; }
        // Sixteen PixelFX poses in a 4 x 4 atlas, authored from right to left.
        int frame = Mathf.Min(15, (int)(Mathf.SmoothStep(0f, 1f, progress) * 16));
        float u = (frame % 4) * .25f;
        float v = (3 - frame / 4) * .25f;
        turningLeaf.uvRect = turnDirection > 0 ? new Rect(u, v, .25f, .25f)
            : new Rect(u + .25f, v, -.25f, .25f);
    }

    void CancelTurn()
    {
        turnDirection = 0;
        turningLeaf.gameObject.SetActive(false);
    }

    void UpdatePageArrows(StoryPage page)
    {
        currentPage = page;
        readablePages.Clear();
        foreach (var entry in Section(page))
            if (CanRead(entry)) readablePages.Add(entry);
        int index = readablePages.IndexOf(page);
        previousPage.interactable = index > 0;
        nextPage.interactable = index < readablePages.Count - 1;
        pageNumber.text = $"{index + 1} / {readablePages.Count}";
    }

    void FitBook()
    {
        Vector2 size = ((RectTransform)storyCard.transform).rect.size;
        notebook.localScale = Vector3.one * Mathf.Max(.01f, Mathf.Min((size.x - 24) / 1080, (size.y - 24) / 608));
        float lift = 1f - Mathf.SmoothStep(0, 1, (Time.unscaledTime - openedAt) / .18f);
        notebook.anchoredPosition = Vector2.down * (22 * lift);
        notebook.localRotation = Quaternion.Euler(0, 0, -1.4f * lift);
    }

    void UpdateStoryHud()
    {
        var state = FieldSession.State;
        string objective = !state.HasBasementMap
            ? (state.basementMapDropped ? "로봇이 떨어뜨린 지도 줍기 · F" : "튜토리얼 훈련 방어 완료하기")
            : state.sunNightJournalRead
                ? (FieldLaboratory.PlayerInside ? "남쪽 계단으로 지상 복귀" : "동쪽 폭스턴 공장 찾기")
                : FieldLaboratory.PlayerInside ? "북쪽 단말 조사하기 · F" : "남쪽 폐주거지에서 연구소 찾기";
        if (HasDockMap)
        {
            if (state.surgeDefeated) objective = "남서 송전 공장 파괴 완료";
            else if (state.surgeEncountered) objective = "공장장 서지 처치";
            else if (state.southwestGateOpen) objective = "남서 공장 조사하기";
            else if (state.hasRepairedInsulator) objective = state.southwestGateDiscovered ? "남서 공장 진입하기" : "남서쪽 서지 공장 찾기";
            else if (state.insulatorRepairRequested) objective = "절연 장치 수리하기";
            else if (state.mecamossCargoOpened) objective = state.eastRefugeDiscovered ? "정비사에게 절연 장치 보여주기" : "동쪽 생존자 거점 찾기";
            else if (state.mecamossDefeated) objective = "하역장의 운송함 조사하기";
            else if (state.mecamossEncountered) objective = "공장장 메카모스 처치";
            else objective = state.northwestFactoryGateDiscovered ? "북서 공장 조사하기" : "북서 늪지 공장 찾기";
        }
        else if (state.foxtonFactoryDestroyed) objective = inFoxtonFactory
            ? (state.foxtonReturnSlipCollected ? "지상으로 복귀 중" : "중앙의 반송 전표 줍기 · F")
            : "폭스턴 공장 파괴 완료";
        else if (inFoxtonFactory) objective = "폭스턴 처치";
        if (objective != storyObjective.text)
        {
            storyObjective.text = objective;
            missionCaption.text = state.surgeDefeated ? "현재 목표   ·   공장 파괴 3/3" :
                state.mecamossDefeated ? "현재 목표   ·   공장 파괴 2/3" :
                state.foxtonFactoryDestroyed ? "현재 목표   ·   공장 파괴 1/3" :
                state.sunNightJournalRead || inFoxtonFactory ? "현재 목표   ·   공장 파괴 0/3" : "현재 목표   ·   [T] 생존 일지";
            missionChangedAt = Time.unscaledTime;
        }
        bool showMission = !storyCard.activeSelf && !panel.activeSelf && !(entryPanel && entryPanel.activeSelf);
        missionBanner.gameObject.SetActive(showMission);
        float arrival = Mathf.SmoothStep(0, 1, (Time.unscaledTime - missionChangedAt) / .22f);
        missionBanner.anchoredPosition = new Vector2(8 + 8 * arrival, -12);

        if (storyCard.activeSelf) { FitBook(); AnimateTurn(); }
        else Managers.UI.PopUIList.Remove(storyCard); // Escape may close the top popup through PopUICloser.
        var keys = Keyboard.current;
        if (keys == null || PlayerController.isDie) return;
        bool notes = keys.tKey.wasPressedThisFrame;
        bool map = keys.mKey.wasPressedThisFrame;
        if (!notes && !map) return;
        if (storyCard.activeSelf) { CloseStory(); return; }
        if (Managers.Game.isCantPlay || Managers.UI.PopUIList.Count > 0) return;
        if (notes) ShowStory(AvailableNotes());
        else if (state.HasBasementMap || HasDockMap || HasSurgeMap) ShowStory(AvailableMap());
        else if (state.firstNightCleared) ShowStory(StoryPage.Signal);
    }

    public static void CloseStory()
    {
        if (instance == null || !instance.storyCard) return;
        if (instance.turningLeaf) instance.CancelTurn();
        instance.storyCard.SetActive(false);
        if (Managers.HasInstance) Managers.UI.PopUIList.Remove(instance.storyCard);
    }

    public static void ShowStory(StoryPage page)
    {
        if (instance == null || !instance.storyCard) return;
        var hud = instance;
        var state = FieldSession.State;
        if (!hud.CanRead(page)) return;
        hud.CancelTurn();
        bool map = Section(page) == MapPages;
        hud.survey.gameObject.SetActive(map);
        hud.storyText.gameObject.SetActive(!map);
        hud.mapTab.interactable = state.HasBasementMap || hud.HasDockMap || hud.HasSurgeMap;
        hud.signalTab.interactable = state.firstNightCleared;
        RectTransform selected = (map ? hud.mapTab : page == StoryPage.Signal ? hud.signalTab : hud.journalTab).GetComponent<RectTransform>();
        OnPaper(hud.bookmarkMark, selected.anchoredPosition.x / 2 + 18, 15, 18, 1);
        hud.FillSpread(page);
        hud.UpdatePageArrows(page);
        if (!hud.storyCard.activeSelf) hud.openedAt = Time.unscaledTime;
        hud.storyCard.SetActive(true);
        hud.FitBook();
        if (!Managers.UI.PopUIList.Contains(hud.storyCard)) Managers.UI.PopUIList.Add(hud.storyCard);
    }

    void FillSpread(StoryPage page)
    {
        bool shippingMap = page == StoryPage.RestoredMap && (HasDockMap || HasSurgeMap);
        bool eastGateKnown = FieldSession.State.eastRefugeDiscovered;
        eastGateStamp.gameObject.SetActive(eastGateKnown && (page == StoryPage.EasternSketch || shippingMap));
        eastSearchArea.gameObject.SetActive(page == StoryPage.EasternSketch && !eastGateKnown);
        bool gateRecorded = shippingMap && HasDockMap && FieldSession.State.northwestFactoryGateDiscovered;
        dockStamp.gameObject.SetActive(shippingMap && HasDockMap);
        gateStamp.gameObject.SetActive(gateRecorded);
        surgeStamp.gameObject.SetActive(shippingMap && HasSurgeMap);
        switch (page)
        {
            case StoryPage.Map:
            case StoryPage.RestoredMap:
                bool restored = page == StoryPage.RestoredMap;
                survey.texture = Resources.Load<Texture2D>(shippingMap ? "Story/ShippingSurvey" :
                    restored ? "Story/DistrictSurvey" : "Story/LabApproach");
                survey.uvRect = shippingMap ? new Rect(0, 0, 1, 1) : new Rect(14f / 560, 1 - 292f / 320, 354f / 560, 238f / 320);
                storyTitle.text = shippingMap ? "북서 수로 · 화물 운송도" : restored ? "폐도시 시설 조사도" : "훈련장 남부 수색 약도";
                slipTitle.text = "지도에 적어 둔 것";
                slipText.text = shippingMap
                    ? "①  폐주거지 연구소\n②  중앙 야영지\n③  폭스턴 공장 · 파괴됨\n\n④  북서 늪지 인수장\n" +
                      (gateRecorded ? "⑤  북서 공장 정문\n크레인 옆에서 문을 찾았다." : "바지선이 닿는 물가다.\n하역 크레인을 찾아보자.")
                    : restored
                    ? "①  폐주거지 연구소\n남쪽 계단으로 지상 복귀.\n\n②  중앙 야영지\n연구소 북동쪽. 정비와 강화.\n\n③  폭스턴 공장\n야영지에서 동북쪽으로."
                    : "①  훈련 거점\n남문을 나와 공터를 건넌다.\n\n②  폐주거지 연구소\n남쪽 정문에 다가가 들어가자.";
                if (shippingMap && HasSurgeMap)
                {
                    storyTitle.text = "폐도시 · 화물 운송도";
                    slipText.text = (HasDockMap ? slipText.text + "\n" : "") + "⑥  남서 송전 공장\n" +
                        (FieldSession.State.southwestGateOpen ? "절연 장치를 연결해 정문을 열었다." : "쌍철탑 아래에 잠긴 정문이 있다.");
                }
                folio.text = gateRecorded || (shippingMap && HasSurgeMap) ? "현장 기록 · 정문 위치 확인" : shippingMap ? "반송 전표의 도장을 옮겨 찍음" :
                    restored ? "단말에서 복원한 지도" : "로봇 잔해에서 주운 지도";
                break;
            case StoryPage.EasternSketch:
                survey.texture = Resources.Load<Texture2D>("Story/ShippingSurvey");
                survey.uvRect = new Rect(0, 0, 1, 1);
                storyTitle.text = "동부 피난 거점 약도";
                slipTitle.text = "정비사의 메모";
                slipText.text = "붉게 칠한 곳은 동쪽 피난 구역이다.\n\n밤에는 안테나 아래 등불을 찾아라. 절연 장치를 가져오면 살펴보겠다.";
                if (eastGateKnown)
                {
                    slipTitle.text = "동쪽 생존자 거점";
                    slipText.text = "⑦  등불 사이로 난 입구.\n안테나 너머 차양에서 정비사가 일한다.";
                    if (FieldSession.State.hasRepairedInsulator) slipText.text += "\n\n절연 장치를 고쳤다. 남서쪽 송전탑을 따라 서지 공장을 찾자.";
                    else if (FieldSession.State.insulatorRepairRequested) slipText.text += $"\n\n수리 재료는 {EasternRefuge.Recipe}. 거점 남동쪽 폐정비소의 작업대와 부품함을 조사하자.";
                }
                folio.text = eastGateKnown ? "현장 기록 · 피난 거점 입구" : "하역장 운송함 · 동부 수색 범위";
                break;
            case StoryPage.SouthboundManifest:
                storyTitle.text = "남서행 운송 기록";
                storyText.text = "목적지   남서 고압전류 공장\n인수 책임   서지 (Surge)\n\n절연 장치 외피 파손. 송전 설비에 연결하기 전 동쪽 피난 거점에서 수리할 것.";
                slipTitle.text = "운송함에서 챙긴 것";
                slipText.text = "손상된 절연 장치 1개\n동부 피난 거점 약도 1장\n\n약도는 수첩의 지도 탭에 끼워 두었다.";
                folio.text = "북서 하역장 · 운송 기록";
                break;
            case StoryPage.ReturnSlip:
                storyTitle.text = "투기장에서 주운 전표";
                storyText.text = "투기장 한가운데 종이 한 장이 남아 있었다.\n\n부서진 경기 로봇은 어디로 보내는 걸까? 전표 한쪽에 작은 운송 약도가 찍혀 있다.";
                slipTitle.text = "반송 전표";
                slipText.text = "품목   파손 경기 로봇\n\n반송처   북서 늪지 공장\n수령지   수로 인수장\n\n인수 책임   MecaMØSS";
                folio.text = "폭스턴 공장 · 회수한 전표";
                break;
            case StoryPage.NightPrep:
                storyTitle.text = "해가 지기 전에";
                storyText.text = "낮에 너무 멀리 나가면 돌아올 길을 놓친다. 오늘은 돌아오는 데 걸리는 시간도 기억해 두자.";
                slipTitle.text = "거점에 돌아오면";
                slipText.text = "모은 재료로 벽과 대포를 만들자.\n\n벽은 거점 앞에, 대포는 그 뒤에.\n\n밤을 버티려면 먹을 것도 남겨 둬야 한다.";
                folio.text = "첫날 · 방어 준비";
                break;
            case StoryPage.Journal:
                storyTitle.text = "지하 연구실에서";
                storyText.text = "단말에 누군가의 기록이 남아 있었다.\n\n“로봇들이 나를 쫓고 있다. 내가 남긴 방어 거점을 찾아라.”";
                slipTitle.text = "썬나잇 연구 일지";
                slipText.text = "NIGHT 명령\n지상 생존 거점 제거\n\nSUN 단계 기상 예정 인원\n2명\n\n전환 조건\n지상 생존 거점 0개";
                folio.text = "단말 기록 · 지도 복원";
                break;
            case StoryPage.Signal:
                storyTitle.text = "첫밤이 지난 아침";
                storyText.text = "공장은 우리가 살아 있다는 걸 안다.\n\n수신기에 남은 문장을 옮겨 적었다.";
                slipTitle.text = "수신 기록   05:30";
                slipText.text = "발신지   동쪽 공장\n\n“미등록 거점의 생존 신호를 확인했다. SUN 단계 승인을 보류한다.”";
                folio.text = "거점 수신기";
                break;
            default:
                storyTitle.text = "살아남기 위한 기록";
                storyText.text = "2073년. 부모님과 동생이 사라졌다.\n\n다시 찾으려면, 우선 오늘을 살아남아야 한다.";
                slipTitle.text = "오늘의 메모";
                slipText.text = FieldSession.State.HasBasementMap
                    ? "로봇 잔해에서 접힌 지도를 주웠다.\n\n남문 너머 폐주거지에 연구소가 있다. 문이 아직 열릴까?"
                    : "주울 수 있는 건 챙겨 두자.\n\n해가 지기 전에 돌아올 곳이 필요하다.";
                folio.text = "생존자의 수첩";
                break;
        }
    }
}
