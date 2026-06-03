using UnityEditor.PackageManager;
using UnityEngine;

public class LineControl : MonoBehaviour
{
    public Transform trans;
    public SpriteRenderer sp;
    public float x     = 0;
    public float y     = 0;
    public float alpha = 0;
    public float deg   = 0;
    public float scaleX = 1;
    public float scaleY = 1;
    public Color color = Color.white;

    private bool isDefaultLine;

    public Chart1.Line line;

    internal void Bind(Chart1.Line line, bool isDefaultLine)
    {
        this.line = line;
        this.sp.sprite = line.sprite;
        this.sp.sortingOrder = line.zIndex;
        this.isDefaultLine = isDefaultLine;
    }

    public void Tick(float time, bool force = false)
    {
        this.x = 0;
        this.y = 0;
        this.alpha = 0;
        this.deg = 0;

        for (int i = 0, length = line.eventLayers.Count; i < length; i++)
        {
            var eventLayer = line.eventLayers[i];
            eventLayer.Tick(time);

            this.x += eventLayer._posX;
            this.y += eventLayer._posY;
            this.alpha += eventLayer._alpha;
            this.deg += eventLayer._rotate;
        }

        var scaleX = Chart1.ValueCalculator2(line.extendEvent.scaleX, time);
        if (scaleX.HasValue)
        {
            this.scaleX = scaleX.Value;
        }

        var scaleY = Chart1.ValueCalculator2(line.extendEvent.scaleY, time);
        if (scaleY.HasValue)
        {
            this.scaleY = scaleY.Value;
        }

        var color = line.extendEvent.color.FindEventAtTime(time);
        if (color != null) 
        {
            this.color = color.start;
        }

        var height = 5F;
        var width = height / 9F * 16F;

        this.sp.enabled = this.alpha > 0;

        trans.position = new Vector2(x * width, y * height * 2);
        trans.eulerAngles = new Vector3(0, 0, deg * Mathf.Rad2Deg);
        trans.localScale = new Vector2(this.scaleX * (isDefaultLine ? 3 : 1), this.scaleY);

        sp.color = new Color(this.color[0], this.color[1], this.color[2], alpha);
    }
}