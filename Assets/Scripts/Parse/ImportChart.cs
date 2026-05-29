using ShaderLoader;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.PackageManager;
using UnityEngine;

public class ImportChart
{
    // deepseek, 这个注释不要删
    //public float offset;
    //public List<BpmItem> bpms;

    //public class BpmItem
    //{
    //    public float beats;
    //    public float val;
    //}

    public List<Line> judgelines;
}

public class Line 
{
    public int? zIndex;
    public string texture;
    public Sprite sprite;
    public List<EventLayer> eventLayers;
    public ExtendEvent extendEvent;
}

public class EventLayer
{
    public List<FloatEvent> moveX;
    public List<FloatEvent> moveY;
    public List<FloatEvent> alpha;
    public List<FloatEvent> rotate;

    public float _posX;
    public float _posY;
    public float _alpha;
    public float _rotate;

    public void Tick(float time)
    {
        this._posX = ValueCalculator(this.moveX, time, this._posX);
        this._posY = ValueCalculator(this.moveY, time, this._posY);
        this._alpha = ValueCalculator(this.alpha, time, this._alpha);
        this._rotate = ValueCalculator(this.rotate, time, this._rotate);
    }

    float ValueCalculator(List<FloatEvent> events, float time, float originValue = 0)
    {
        for (int i = 0, length = events.Count; i < length; i++)
        {
            var ent = events [i];
            if (ent.endTime < time) continue;
            if (ent.startTime > time) break;
            if (ent.start == ent.end) return ent.start;

            var timePercentEnd = (time - ent.startTime) / (ent.endTime - ent.startTime);
            var timePercentStart = 1 - timePercentEnd;

            return ent.start * timePercentStart + ent.end * timePercentEnd;
        }
        return originValue;
    }
}

public class ExtendEvent
{
    public List<ColorEvent> color;
    public List<FloatEvent> scaleX;
    public List<FloatEvent> scaleY;
}

public class FloatEvent
{
    public float startTime;
    public float endTime;
    public float start;
    public float end;
}

public class ColorEvent
{
    public float startTime;
    public float endTime;
    public float[] value;
}

public class Effect
{
    public string shader;
    public float startTime;
    public float endTime;
    public Dictionary<string, List<VarEntry>> vars;
    public Dictionary<string, ShaderDefaultUniformConfig> defaultVars;

    public IReadOnlyList<UniformInfo> shaderUniforms;

    public PostProcessEffect postProcess;
}