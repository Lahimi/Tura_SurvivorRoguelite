using UnityEngine;

// Scales up and fades out, then destroys itself.
// Attach to any SpriteRenderer GameObject used as a one-shot VFX.
[RequireComponent(typeof(SpriteRenderer))]
public class S_VfxFade : MonoBehaviour
{
    public float duration    = 0.3f;
    public float targetScale = 1f; // world-unit diameter at full size — set to 4 on PF_ProximityBurst in the Inspector

    SpriteRenderer _sr;
    float          _elapsed;
    Color          _startColor;

    void Awake()
    {
        _sr         = GetComponent<SpriteRenderer>();
        _startColor = _sr.color;
    }

    void Update()
    {
        _elapsed += Time.deltaTime;
        float t = Mathf.Clamp01(_elapsed / duration);

        // Scale from 0 to targetScale
        float s = Mathf.Lerp(0f, targetScale, t);
        transform.localScale = new Vector3(s, s, 1f);

        // Fade out alpha
        _sr.color = new Color(_startColor.r, _startColor.g, _startColor.b, 1f - t);

        if (t >= 1f) Destroy(gameObject);
    }
}