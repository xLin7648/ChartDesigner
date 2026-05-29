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
    public float[] color = new float[4] { 1, 1, 1, 1 };

    private bool isDefaultLine;

    private Line line;

    internal void Bind(Line line, bool isDefaultLine)
    {
        this.line = line;
        this.sp.sprite = line.sprite;
        this.sp.sortingOrder = line.zIndex ??= 0;
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


        for (int i = 0, length = line.extendEvent.scaleX.Count; i < length; i++)
        {
            var ent = line.extendEvent.scaleX [i];
            if (ent.endTime < time) continue;
            if (ent.startTime > time) break;

            var timePercentEnd = (time - ent.startTime) / (ent.endTime - ent.startTime);
            var timePercentStart = 1 - timePercentEnd;

            this.scaleX = ent.start * timePercentStart + ent.end * timePercentEnd;
        }

        for (int i = 0, length = line.extendEvent.scaleY.Count; i < length; i++)
        {
            var ent = line.extendEvent.scaleX [i];
            if (ent.endTime < time) continue;
            if (ent.startTime > time) break;

            var timePercentEnd = (time - ent.startTime) / (ent.endTime - ent.startTime);
            var timePercentStart = 1 - timePercentEnd;

            this.scaleY = ent.start * timePercentStart + ent.end * timePercentEnd;
        }

        for (int i = 0, length = line.extendEvent.color.Count; i < length; i++)
        {
            var ent = line.extendEvent.color[i];
            if (ent.endTime < time) continue;
            if (ent.startTime > time) break;

            this.color = ent.value;
        }

        var height = 5F;
        var width = height / 9F * 16F;

        this.sp.enabled = this.alpha > 0;

        trans.position = new Vector2(x * width, y * height * 2);
        trans.eulerAngles = new Vector3(0, 0, deg * Mathf.Rad2Deg);
        trans.localScale = new Vector2(scaleX * (isDefaultLine ? 3 : 1), scaleY);

        sp.color = new Color(color[0], color[1], color[2], alpha);
    }
}