using UnityEngine;

public sealed class NorthwestFactoryGate : MonoBehaviour
{
    MecamossReceivingHall hall;

    void Awake()
    {
        gameObject.AddComponent<LabInteraction>().Configure("북서 공장 정문 · F 조사", Investigate);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!FieldSession.Active || !other.TryGetComponent<PlayerController>(out _)) return;
        if (FieldSession.State.DiscoverNorthwestFactoryGate())
            FieldHUD.Notify("북서 공장 정문을 지도에 기록했다.");
    }

    void Investigate()
    {
        if (!FieldSession.Active || Managers.UI.PopUIList.Count > 0) return;
        if (!FieldSession.State.foxtonReturnSlipCollected)
        {
            FieldHUD.Notify("반송 전표가 있어야 인수장 문을 열 수 있다.");
            return;
        }
        if (Managers.Game.isMovingTower || TowerMoveController.IsHandling)
        {
            FieldHUD.Notify("거점을 설치한 뒤 공장에 들어가세요.");
            return;
        }
        FieldSession.State.DiscoverNorthwestFactoryGate();
        if (!hall)
        {
            var prefab = Resources.Load<MecamossReceivingHall>("Marsh/ReceivingHall");
            hall = Instantiate(prefab, new Vector3(-575, 350), Quaternion.identity, transform.root);
        }
        hall.Enter((Vector2)transform.position + Vector2.down * 4);
    }
}
