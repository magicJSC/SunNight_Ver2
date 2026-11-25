using UnityEngine;

public sealed class EasternRefuge : MonoBehaviour
{
    public static readonly Vector2 Entrance = new Vector2(193, -143);
    const int ScrapCost = 2, PartsCost = 1;
    public static string Recipe => $"고철 {ScrapCost}개와 정비 부품 {PartsCost}개";
    [SerializeField] LabInteraction mechanic;

    void Awake()
    {
        mechanic.Configure("거점 정비사 · F 대화", Talk);
    }

    void Start()
    {
        var field = ExplorationRuntime.Instance;
        var content = field.content;
        var workshop = Entrance + new Vector2(13, -12);
        AddSupply("RefugeScrap", "폐정비소 · 해체 작업대", workshop, content.workbench, "IronOre", ScrapCost);
        AddSupply("RefugeParts", "폐정비소 · 부품함", workshop + Vector2.right * 4, content.chest, "MaintenancePart", PartsCost);

        void AddSupply(string id, string name, Vector2 at, Sprite sprite, string item, int count)
        {
            var supply = field.Prop(id, at, sprite, Color.white);
            supply.AddComponent<FieldReward>().Configure(new FieldRewardDefinition {
                id = id, region = name, position = at, items = new[] { item }, amounts = new[] { count }, collectSeconds = 2
            }, content);
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!FieldSession.Active || !other.TryGetComponent<PlayerController>(out _) || PlayerController.isDie) return;
        var state = FieldSession.State;
        if (state.eastRefugeDiscovered) return;
        state.eastRefugeDiscovered = true;
        FieldHUD.Notify("동쪽 생존자 거점 발견 · 입구 위치를 기록했다.");
    }

    void Talk()
    {
        var state = FieldSession.State;
        if (state.hasRepairedInsulator)
        {
            FieldHUD.Notify("정비사  “장치는 다 고쳤어. 남서쪽 송전탑을 따라가.”");
            return;
        }
        if (!state.hasDamagedInsulator)
        {
            FieldHUD.Notify("정비사  “하역장에서 절연 장치를 찾으면 이리 가져와.”");
            return;
        }
        if (!state.insulatorRepairRequested)
        {
            state.insulatorRepairRequested = true;
            FieldHUD.Notify($"정비사  “남동쪽 폐정비소에서 {Recipe}를 가져와.”");
            return;
        }
        var content = ExplorationRuntime.Instance.content;
        if (!FieldInventory.Spend(content.scrap, ScrapCost, content.parts, PartsCost))
        {
            FieldHUD.Notify($"남동쪽 폐정비소 · 고철 {FieldInventory.Count(content.scrap)}/{ScrapCost} · 정비 부품 {FieldInventory.Count(content.parts)}/{PartsCost}");
            return;
        }
        state.hasDamagedInsulator = false;
        state.hasRepairedInsulator = true;
        FieldHUD.Notify("절연 장치 수리 완료 · 남서쪽 송전탑을 따라가자.");
    }
}
