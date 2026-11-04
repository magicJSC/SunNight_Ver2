using UnityEngine;

public sealed class FieldLaboratory : MonoBehaviour
{
    public static readonly Vector2 Entrance = new Vector2(-9, -58);
    public static readonly Vector2 Center = new Vector2(-520, 350);
    public static bool PlayerInside => Managers.Game.player != null &&
        Vector2.Distance(Managers.Game.player.transform.position, Center) < 15;
    GameObject room;

    public void Build(ExplorationRuntime field)
    {
        var stairs = Resources.Load<Sprite>("Story/ResearchStairs");
        var hatch = field.Prop("LaboratoryEntrance", Entrance, stairs, Color.white);
        hatch.AddComponent<LabInteraction>().Configure("지하 연구실 계단 · F 내려가기", Enter);

        room = new GameObject("UndergroundLaboratory");
        room.transform.SetParent(transform);
        room.transform.position = Center;
        var floor = room.AddComponent<SpriteRenderer>();
        floor.sprite = Resources.Load<Sprite>("Story/LaboratoryFloor");
        floor.sharedMaterial = field.content.material;
        floor.sortingOrder = -2;

        Wall("North wall", new Vector2(0, 5), new Vector2(20, 2));
        Wall("South wall", new Vector2(0, -7), new Vector2(20, 1));
        Wall("West wall", new Vector2(-10, 0), new Vector2(1, 14));
        Wall("East wall", new Vector2(10, 0), new Vector2(1, 14));
        var terminal = field.Prop("BasementTerminal", Center + new Vector2(0, 2), field.content.workbench, Color.white);
        terminal.transform.SetParent(room.transform, true);
        terminal.AddComponent<BasementTerminal>();
        var exit = field.Prop("LaboratoryExit", Center + new Vector2(0, -5), stairs, Color.white);
        exit.transform.SetParent(room.transform, true);
        exit.AddComponent<LabInteraction>().Configure("지상으로 · F 올라가기", Leave);
        room.SetActive(false);
    }

    void Wall(string label, Vector2 offset, Vector2 size)
    {
        var wall = new GameObject(label);
        wall.transform.SetParent(room.transform, false);
        wall.transform.localPosition = offset;
        wall.AddComponent<BoxCollider2D>().size = size;
    }
    void Enter()
    {
        if (!FieldSession.State.HasBasementMap)
        {
            FieldHUD.Notify("잠긴 계단이다. 훈련장의 로봇이 단서를 가지고 있을지도 모른다.");
            return;
        }
        room.SetActive(true);
        MovePlayer(Center + new Vector2(0, -1));
        FieldHUD.Notify("지하 연구실 · 북쪽 책상의 단말을 조사하자.");
    }
    void Leave()
    {
        MovePlayer(Entrance + Vector2.up * 3.5f);
        room.SetActive(false);
        if (FieldSession.State.sunNightJournalRead)
            FieldHUD.Notify("지상으로 돌아왔다. M으로 야영지와 동쪽 공장의 위치를 확인하자.");
    }
    static void MovePlayer(Vector2 destination)
    {
        FieldHUD.CloseStory();
        var body = Managers.Game.player.GetComponent<Rigidbody2D>();
        body.linearVelocity = Vector2.zero;
        body.position = destination;
        Managers.Game.player.transform.position = destination;
        Physics2D.SyncTransforms();
    }
}
