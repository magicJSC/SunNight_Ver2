using UnityEngine;

// Small dock beacons keep phase two visible after the announcement fades.
public sealed class MecamossAlarm : MonoBehaviour
{
    SpriteRenderer[] lamps;
    Texture2D texture;
    Sprite sprite;
    bool lit;
    float elapsed;

    void Awake()
    {
        texture = new Texture2D(7, 9) { filterMode = FilterMode.Point };
        var pixels = new Color[63];
        for (int y = 0; y < 9; y++)
            for (int x = 0; x < 7; x++)
                pixels[y * 7 + x] = y < 2 ? new Color(.15f, .13f, .11f) :
                    x == 0 || x == 6 || y == 8 ? new Color(.3f, .13f, .08f) :
                    x == 2 && y > 3 ? new Color(1, .87f, .45f) : new Color(1, .3f, .08f);
        texture.SetPixels(pixels);
        texture.Apply();
        sprite = Sprite.Create(texture, new Rect(0, 0, 7, 9), new Vector2(.5f, 0), 10);
        lamps = new SpriteRenderer[4];
        for (int i = 0; i < lamps.Length; i++)
        {
            var lamp = new GameObject("Dock alarm beacon").AddComponent<SpriteRenderer>();
            lamp.transform.SetParent(transform, false);
            lamp.transform.localPosition = new Vector3(i % 2 == 0 ? -8 : 8, i < 2 ? 5.6f : -5.6f, 0);
            lamp.sprite = sprite;
            lamp.sortingOrder = 15;
            lamps[i] = lamp;
        }
        SetActive(false);
    }

    public void SetActive(bool active)
    {
        lit = active;
        elapsed = 0;
        foreach (var lamp in lamps) lamp.enabled = active;
    }

    void Update()
    {
        if (!lit || FieldHUD.IsOpen || Managers.Game.isCantPlay) return;
        elapsed += Time.deltaTime;
        float brightness = .65f + .35f * Mathf.Sin(elapsed * Mathf.PI * 2);
        foreach (var lamp in lamps) lamp.color = new Color(brightness, brightness, brightness, 1);
    }

    void OnDestroy()
    {
        if (sprite) Destroy(sprite);
        if (texture) Destroy(texture);
    }
}
