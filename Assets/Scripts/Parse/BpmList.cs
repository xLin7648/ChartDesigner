using System;
using System.Linq;
using System.Collections.Generic;

[Serializable]
public class BpmList
{
    private readonly List<(float beats, float time, float bpm)> _elements;
    private int _cursor;

    public BpmList()
    {
        _elements = new List<(float, float, float)>(0);
        _cursor = 0;
    }

    public BpmList(IEnumerable<(float beats, float bpm)> ranges)
    {
        _elements = new List<(float, float, float)>();
        _cursor = 0;

        float time = 0f;
        float lastBeats = 0f;
        float? lastBpm = null;

        foreach (var (nowBeats, bpm) in ranges)
        {
            if (lastBpm.HasValue)
                time += (nowBeats - lastBeats) * (60f / lastBpm.Value);

            lastBeats = nowBeats;
            lastBpm = bpm;
            _elements.Add((nowBeats, time, bpm));
        }
    }

    public float GetAvgBpm() => _elements.Average(x => x.bpm);
    public float GetMinBpm() => _elements.Min(x => x.bpm);
    public float GetMaxBpm() => _elements.Max(x => x.bpm);

    /// <summary>
    /// 给定 beats 返回对应的时间（秒）。
    /// </summary>
    public float TimeBeats(float beats)
    {
        if (_elements == null || _elements.Count == 0)
        {
            return 0;
        }

        // 向后找
        while (_cursor + 1 < _elements.Count && _elements[_cursor + 1].beats <= beats)
            _cursor++;

        // 向前找
        while (_cursor > 0 && _elements[_cursor].beats > beats)
            _cursor--;

        var (startBeats, time, bpm) = _elements[_cursor];
        return time + (beats - startBeats) * (60f / bpm);
    }

    /// <summary>
    /// 通过 Triple 结构（i + n/d）获取时间。
    /// 假设 Triple 有 beats 方法返回 float。
    /// </summary>
    public float Time(TimeTriple triple) => TimeBeats(triple.Beats());

    /// <summary>
    /// 给定时间（秒）返回对应的 beats。
    /// </summary>
    public float Beat(float time)
    {
        if (_elements == null || _elements.Count == 0)
        {
            return 0;
        }

        // 向后找
        while (_cursor + 1 < _elements.Count && _elements[_cursor + 1].time <= time)
            _cursor++;

        // 向前找
        while (_cursor > 0 && _elements[_cursor].time > time)
            _cursor--;

        var (beats, startTime, bpm) = _elements[_cursor];
        return beats + (time - startTime) / (60f / bpm);
    }

    /// <summary>
    /// [新增] 线程安全版本：使用二分查找，不依赖 _cursor。
    /// 可以在任何线程上安全调用。
    /// </summary>
    public float TimeBeatsThreadSafe(float beats)
    {
        if (_elements == null || _elements.Count == 0)
        {
            return 0;
        }

        // 使用二分查找找到最后一个 <= beats 的元素
        int left = 0;
        int right = _elements.Count - 1;
        int resultIndex = 0; // 默认使用第一个元素

        while (left <= right)
        {
            int mid = left + (right - left) / 2;
            if (_elements[mid].beats <= beats)
            {
                resultIndex = mid; // 这是一个可能的有效段
                left = mid + 1;    // 尝试在右侧寻找更接近的
            }
            else
            {
                right = mid - 1;
            }
        }

        var (startBeats, time, bpm) = _elements[resultIndex];
        return time + (beats - startBeats) * (60f / bpm);
    }
}