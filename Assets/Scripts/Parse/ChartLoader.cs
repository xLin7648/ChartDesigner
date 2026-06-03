using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ChartLoader : MonoBehaviour
{
    public TextAsset chartAsset;
    public Chart chart;

    public void Start()
    {
        var input = JsonConvert.DeserializeObject<ImportChart>(chartAsset.text);

        chart = new Chart()
        {
            offset = input.offset
        };

        var judges = new List<Chart.Judge>(input.judges.Length);

        foreach (var judge in input.judges)
        {
            var alphasHead = BuildEventChain(judge.alphas, e => new EventNode<float>(e));
            var alphasJump = new JumpArray<EventNode<float>>(alphasHead);

            judges.Add(new Chart.Judge
            {
                zIndex = judge.zIndex.GetValueOrDefault(),
                texture = judge.texture,
                alphasJump = alphasJump
            });
        }
        chart.judges = judges;
    }

    /// <summary>将 ImportChart 的 List<T> 事件转换为双向链表</summary>
    private static EventNode<T> BuildEventChain<T>(List<ImportChart.LineEvent<T>> sourceEvents,
        Func<ImportChart.LineEvent<T>, EventNode<T>> factory)
    {
        if (sourceEvents == null || sourceEvents.Count == 0) return null;

        EventNode<T> prev = null;
        EventNode<T> head = null;
        foreach (var evt in sourceEvents)
        {
            var node = factory(evt);
            if (prev != null)
            {
                prev.next = node;
                node.prev = prev;
            }
            else
            {
                head = node;
            }
            prev = node;
        }
        return head;
    }
}
