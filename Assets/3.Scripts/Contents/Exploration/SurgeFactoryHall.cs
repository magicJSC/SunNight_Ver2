using UnityEngine;
using UnityEngine.SceneManagement;

public sealed class SurgeFactoryHall : MonoBehaviour
{
    public const string SceneName = "Boss3";
    public static SurgeFactoryHall Current { get; private set; }
    public static bool PlayerInside => Current && Managers.Game.player &&
        Current.floor.bounds.Contains(Managers.Game.player.transform.position);

    [SerializeField] SpriteRenderer floor;
    [SerializeField] Transform arrival;
    [SerializeField] LabInteraction exit;
    public Vector2 Outside { get; private set; }

    void Awake()
    {
        Current = this;
        exit.Configure("남서 공장 정문 · F 밖으로", Leave);
    }

    public void Enter(Vector2 outside)
    {
        Outside = outside;
        Managers.Game.player.WarpTo(arrival.position);
        FieldHUD.Notify("남서 송전 공장 · 중앙 개폐실");
    }

    void Leave()
    {
        if (!PlayerInside) return;
        Managers.Game.player.WarpTo(Outside);
        SceneManager.UnloadSceneAsync(gameObject.scene);
    }

    public Vector3 CameraPosition(Vector3 player, Camera cam)
    {
        Bounds b = floor.bounds;
        float y = Mathf.Max(0,b.extents.y-cam.orthographicSize);
        float x = Mathf.Max(0,b.extents.x-cam.orthographicSize*cam.aspect);
        return new Vector3(Mathf.Clamp(player.x,b.center.x-x,b.center.x+x),
            Mathf.Clamp(player.y,b.center.y-y,b.center.y+y),-10);
    }

    void OnDestroy()
    {
        if (Current == this) Current = null;
    }
}
