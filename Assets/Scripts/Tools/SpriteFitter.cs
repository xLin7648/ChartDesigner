using JetBrains.Annotations;
using UnityEngine;

/// <summary>
/// 将 Sprite 适配到正交相机视口，支持三种模式：
/// - Shrink: 保持比例覆盖视口（超出部分裁切）
/// - Expand: 保持比例完整显示（可能有黑边）
/// - Fill: 拉伸填满视口（改变原始比例）
/// </summary>
[RequireComponent(typeof(SpriteRenderer))]
public class SpriteFitter : MonoBehaviour
{
    public enum FitMode
    {
        Shrink,
        Expand,
        Fill
    }

    [SerializeField] private Camera cam;
    [SerializeField] private FitMode mode = FitMode.Shrink;

#if UNITY_EDITOR
    private void OnValidate() => OnGameAreaChanged();
#endif

    private void OnEnable()
    {
        UIEventControl.AddEvent(UIEventEnum.GameAreaChanged, OnGameAreaChanged);
        OnGameAreaChanged();
    }

    private void OnDisable()
    {
        UIEventControl.RemoveEvent(UIEventEnum.GameAreaChanged, OnGameAreaChanged);
    }

    private void OnGameAreaChanged(object _ = null)
    {
        if (cam == null)
            cam = Camera.main;

        if (cam == null || !cam.orthographic)
        {
            Debug.LogError("SpriteFitter 需要一张正交相机", this);
            return;
        }

        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr.sprite == null)
        {
            Debug.LogError("SpriteRenderer 上没有 Sprite", this);
            return;
        }

        float worldScreenHeight = cam.orthographicSize * 2f;
        float worldScreenWidth = worldScreenHeight * cam.aspect;

        float spriteWidth = sr.sprite.bounds.size.x;
        float spriteHeight = sr.sprite.bounds.size.y;

        float scaleX = worldScreenWidth / spriteWidth;
        float scaleY = worldScreenHeight / spriteHeight;

        switch (mode)
        {
            case FitMode.Shrink:
                float uniformExpand = Mathf.Max(scaleX, scaleY);
                transform.localScale = new Vector3(uniformExpand, uniformExpand, 1f);
                break;
            case FitMode.Expand:
                float uniformShrink = Mathf.Min(scaleX, scaleY);
                transform.localScale = new Vector3(uniformShrink, uniformShrink, 1f);
                break;
            case FitMode.Fill:
                transform.localScale = new Vector3(scaleX, scaleY, 1f);
                break;
        }
    }
}
