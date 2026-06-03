using ShaderLoader;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static ImportChart;

[Serializable]
public class Chart1
{
    public List<Line> judgelines;

    [Serializable]
    public class Line
    {
        public int id;
        public int zIndex;
        public string texture;
        public Sprite sprite;
        public List<EventLayer> eventLayers;
        public ExtendEvent extendEvent;
    }

    [Serializable]
    public class EventLayer
    {
        public JumpArray<EventNode<float>> moveX;
        public JumpArray<EventNode<float>> moveY;
        public JumpArray<EventNode<float>> alpha;
        public JumpArray<EventNode<float>> rotate;

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
    }

    [Serializable]
    public class ExtendEvent
    {
        public JumpArray<EventNode<Color>> color;
        public JumpArray<EventNode<float>> scaleX;
        public JumpArray<EventNode<float>> scaleY;
    }

    public static float ValueCalculator(JumpArray<EventNode<float>> events, float time, float originValue = 0)
    {
        var node = events.FindEventAtTime(time);
        if (node == null) return originValue;
        if (node.start == node.end) return node.start;

        var timePercentEnd = (time - node.startTime) / (node.endTime - node.startTime);
        var timePercentStart = 1 - timePercentEnd;

        return node.start * timePercentStart + node.end * timePercentEnd;
    }

    public static float? ValueCalculator2(JumpArray<EventNode<float>> events, float time)
    {
        var node = events.FindEventAtTime(time);
        if (node == null) return null;
        if (node.start == node.end) return node.start;

        var timePercentEnd = (time - node.startTime) / (node.endTime - node.startTime);
        var timePercentStart = 1 - timePercentEnd;

        return node.start * timePercentStart + node.end * timePercentEnd;
    }
}

public class ImportChart1
{
    public List<Line> judgelines;

    public class Line
    {
        public int id;
        public int? zIndex;
        public string texture;
        public List<EventLayer> eventLayers;
        public ExtendEvent extendEvent;
    }

    public class EventLayer
    {
        public List<LineEvent<float>> moveX;
        public List<LineEvent<float>> moveY;
        public List<LineEvent<float>> alpha;
        public List<LineEvent<float>> rotate;

        //public float _posX;
        //public float _posY;
        //public float _alpha;
        //public float _rotate;

        //public void Tick(float time)
        //{
        //    this._posX = ValueCalculator(this.moveX, time, this._posX);
        //    this._posY = ValueCalculator(this.moveY, time, this._posY);
        //    this._alpha = ValueCalculator(this.alpha, time, this._alpha);
        //    this._rotate = ValueCalculator(this.rotate, time, this._rotate);
        //}

        //float ValueCalculator(List<LineEvent<float>> events, float time, float originValue = 0)
        //{
        //    for (int i = 0, length = events.Count; i < length; i++)
        //    {
        //        var ent = events [i];
        //        if (ent.endTime < time) continue;
        //        if (ent.startTime > time) break;
        //        if (ent.start == ent.end) return ent.start;

        //        var timePercentEnd = (time - ent.startTime) / (ent.endTime - ent.startTime);
        //        var timePercentStart = 1 - timePercentEnd;

        //        return ent.start * timePercentStart + ent.end * timePercentEnd;
        //    }
        //    return originValue;
        //}
    }

    public class ExtendEvent
    {
        public List<ColorEvent> color;
        public List<LineEvent<float>> scaleX;
        public List<LineEvent<float>> scaleY;
    }
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