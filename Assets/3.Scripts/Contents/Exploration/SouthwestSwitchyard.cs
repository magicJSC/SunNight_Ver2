using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(Tilemap))]
public sealed class SouthwestSwitchyard : MonoBehaviour
{
    public FieldRewardDefinition[] rewards;
    public FieldGuardDefinition[] guards;
    public SpriteRenderer[] beacons;
    public Transform entrance;
    [SerializeField] BoxCollider2D shutter;
    [SerializeField] SpriteMask openPassage;
    public const int GateX = -292, GateY = -232;
    public static SouthwestSwitchyard Current { get; private set; }
    Tilemap terrain;
    bool atDoor, loadingHall;

    void OnEnable()
    {
        terrain = GetComponent<Tilemap>();
        Current = this;
    }

    void OnDisable()
    {
        if (Current == this) Current = null;
    }

    public static bool HasGround(Vector2 point) =>
        Current && Current.terrain.HasTile(Current.terrain.WorldToCell(point));

    void Start()
    {
        entrance.gameObject.AddComponent<LabInteraction>().Configure("정문 접지 단자 · F 절연 장치 연결", Connect);
    }

    void Connect()
    {
        if (!FieldSession.Active || Managers.UI.PopUIList.Count > 0) return;
        var state = FieldSession.State;
        if (!state.hasRepairedInsulator)
        {
            FieldHUD.Notify("고압 잠금장치다. 절연 장치를 수리할 동쪽 정비사를 찾아보자.");
            return;
        }
        state.southwestGateDiscovered = true;
        state.southwestGateOpen = true;
        SetGateOpen(true);
        FieldHUD.Notify("접지 완료 · 서지 공장 정문이 열렸다.");
    }

    void SetGateOpen(bool open)
    {
        shutter.enabled = !open;
        openPassage.enabled = open;
        entrance.gameObject.SetActive(!open);
    }

    void Update()
    {
        bool firstLight = Time.time % 2f < 1f;
        beacons[0].enabled = firstLight;
        beacons[1].enabled = !firstLight;
        if (FieldSession.Active && openPassage.enabled != FieldSession.State.southwestGateOpen)
            SetGateOpen(FieldSession.State.southwestGateOpen);
        if (FieldSession.Active && FieldSession.State.southwestGateOpen && Managers.HasInstance &&
            Managers.Game.player && !PlayerController.isDie && !Managers.Game.isCantPlay)
        {
            Vector2 offset = Managers.Game.player.transform.position - entrance.position;
            bool crossed = Mathf.Abs(offset.x) <= 2.35f && offset.y >= 1.6f && offset.y <= 9.5f;
            if (crossed && !atDoor && !loadingHall && !SurgeFactoryHall.Current)
            {
                if (Managers.Game.isMovingTower || TowerMoveController.IsHandling)
                    FieldHUD.Notify("거점을 설치한 뒤 공장에 들어가세요.");
                else StartCoroutine(EnterHall());
            }
            atDoor = crossed;
        }
        if (!FieldSession.Active || FieldSession.State.southwestGateDiscovered || !Managers.HasInstance ||
            !Managers.Game.player || PlayerController.isDie) return;
        if (((Vector2)(Managers.Game.player.transform.position - entrance.position)).sqrMagnitude > 9f) return;
        FieldSession.State.southwestGateDiscovered = true;
        FieldHUD.Notify("남서 송전 공장 정문을 발견했다. 일지 지도에 위치를 표시했다.");
    }

    System.Collections.IEnumerator EnterHall()
    {
        loadingHall = true;
        yield return SceneManager.LoadSceneAsync(SurgeFactoryHall.SceneName, LoadSceneMode.Additive);
        var hall = SurgeFactoryHall.Current;
        if (hall && !PlayerController.isDie && FieldSession.Active)
            hall.Enter((Vector2)entrance.position + Vector2.down * 2);
        else if (hall) SceneManager.UnloadSceneAsync(hall.gameObject.scene);
        loadingHall = false;
    }
}

