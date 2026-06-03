using System;
using System.Collections.Generic;
using TreeEditor;
using UnityEngine;

public class ImportChartVersion
{
    public int formatVersion;
}

public class ImportChart : ImportChartVersion
{
    public float offset;
    // public List<BpmItem> bpms;
    public Judge[] judges;

    //public class BpmItem
    //{
    //    public float beats;
    //    public float val;
    //}

    public class Judge
    {
        public int? zIndex;
        public string texture;

        public List<LineEvent<float>> alphas;
        // public List<LineEvent<float[]>> moves = new();
        // public List<LineEvent<float>> rots;
        // public List<LineEvent<float[]>> colors;
    }

    public class LineEvent<T>
    {
        public float startTime;
        public float endTime;
        public T start;
        public T end;
    }

    public class ColorEvent
    {
        public float startTime;
        public float endTime;
        public float[] value;
    }
}

/// <summary>
/// 跳跃数组 — 为双向链表提供近似 O(1) 的随机访问
///
/// 原理：
///   维护一个均匀时间 → 节点的映射数组，像一把尺子比量着链表。
///   分度值（tickSpacing）根据节点密度分配，一般用 2 的幂以减小浮点误差。
///
/// 随机访问：time / tickSpacing → 数组下标 → 直接读取节点（一次命中或走几步）
/// 插入节点：只更新插入位置前后局部范围的刻度
/// </summary>
[Serializable]
public class JumpArray<T> where T : RangeNode
{
    public T[] ticks;              // 刻度数组（指向链表中对应位置的节点）
    public T head;                 // 链表表头（由 JumpArray 统一管理）
    public float tickSpacing;      // 分度值（刻度间距）
    public float minTime;          // 最早时间（刻度 0 对应的时间偏移）
    public int tickCount;          // 刻度数量
    public int rebuildThreshold;   // 触发重建的节点数变化阈值

    public int nodeCount;
    public bool dirty;             // 标记是否需要重建

    public float TickSpacing => tickSpacing;
    public int TickCount => tickCount;
    public int NodeCount => nodeCount;
    public T Head => head;

    public JumpArray(T head) => Build(head);

    // 工具：时间 → 刻度下标
    private int TimeToIndex(float time) =>
        Mathf.Clamp(Mathf.FloorToInt((time - minTime) / tickSpacing), 0, ticks.Length - 1);

    /// <summary>构建或重建跳跃数组</summary>
    public void Build(T head)
    {
        if (head == null) { ticks = Array.Empty<T>(); this.head = null; nodeCount = 0; return; }
        this.head = head;

        // 1. 统计节点数 & 计算密度
        int count = 0;
        var node = head;
        minTime = head.startTime;
        float maxTime = minTime;
        while (node != null)
        {
            count++;
            float t = node.startTime;
            if (t < minTime) minTime = t;
            if (t > maxTime) maxTime = t;
            node = (T)node.next;
        }

        nodeCount = count;
        if (count == 0) { ticks = Array.Empty<T>(); return; }

        // 2. 确定分度值（使用 2 的幂，减少浮点误差）
        float duration = maxTime - minTime;
        if (duration <= 0) duration = 1f;

        int desiredTicks = Mathf.Max(16, count / 8);           // 刻度数约为节点数的 1/4
        float rawSpacing = duration / desiredTicks;
        tickSpacing = Mathf.Pow(2, Mathf.Round(Mathf.Log(rawSpacing) / Mathf.Log(2))); // 取 2 的幂
        if (tickSpacing <= 0) tickSpacing = 0.25f;

        // 3. 填充刻度数组
        tickCount = Mathf.CeilToInt(duration / tickSpacing) + 1;
        ticks = new T[tickCount];

        node = head;
        for (int i = 0; i < tickCount; i++)
        {
            float tickTime = minTime + i * tickSpacing;
            // 将 node 推进到 >= tickTime 的位置
            while (node != null && node.next != null && ((T)node.next).startTime <= tickTime)
                node = (T)node.next;
            ticks[i] = node;
        }

        dirty = false;
        rebuildThreshold = count / 2; // 节点数变化超过一半时触发重建
    }

    /// <summary>通过时间查找节点（近似 O(1)）</summary>
    public T FindByTime(float time)
    {
        if (ticks == null || ticks.Length == 0) return null;

        // 通过跳跃数组定位
        int index = TimeToIndex(time);
        T node = ticks[index];
        if (node == null) return null;

        // 向后微调（没命中就走几个节点）
        while (node.next != null && node.next is T next && next.startTime < time)
            node = next;
        // 向前微调
        while (node.prev != null && node.prev is T prev && prev.startTime >= time)
            node = prev;

        return node;
    }

    /// <summary>
    /// 查找包含 time 的事件节点（用于插值计算）
    /// 完全对应原算法行为：
    ///   先跳到 time 附近 → 往前追溯确保不漏 → 从前往后遍历
    ///   返回第一个 startTime ≤ time ≤ endTime 的事件
    /// </summary>
    public T FindEventAtTime(float time)
    {
        if (ticks == null || ticks.Length == 0) return null;

        int index = TimeToIndex(time);
        T node = ticks[index];
        if (node == null) return null;

        // 先判断 time 相对于当前 node 的位置
        if (time < node.startTime)
        {
            // time 在 node 之前 → 往前找
            while (node != null)
            {
                if (node.prev == null) return null;
                node = (T)node.prev;
                if (time >= node.startTime) break;
            }
        }

        // 从当前 node 往后找
        while (node != null)
        {
            if (node.startTime > time && node.prev is T prev)
            {
                return prev.startTime >= time && prev.endTime <= time
                    ? prev
                    : null;   // 之后的都超过了，不可能有
            }
            if (time <= node.endTime) return node;     // 命中了
            if (node.next == null) return null;
            node = (T)node.next;
        }

        return null;
    }

    /// <summary>
    /// 获取 [startTime, endTime) 范围内的所有节点
    /// 先用跳跃数组一步跳到 startTime，再沿链表 next 遍历到 endTime
    /// 复杂度 O(1 + K)，K = 范围内的节点数（本来就要绘制的量，省不掉）
    /// </summary>
    public List<T> GetNodesInRange(float startTime, float endTime)
    {
        var result = new List<T>();

        // 1. 用跳跃数组一步定位到 startTime 附近
        T node = FindByTime(startTime);
        if (node == null) return result;

        // 2. 确保 node 是范围内 ≤ startTime 的第一个节点
        //    如果 node.time > startTime，往前走
        while (node.prev != null && node.prev is T prev && prev.startTime >= startTime)
            node = prev;
        //    如果 node 后面那个 ≤ startTime，往后走
        while (node.next != null && node.next is T next && next.startTime < startTime)
            node = next;

        // 3. 沿着 next 往后收集，直到超出 endTime
        while (node != null && node.startTime < endTime)
        {
            result.Add(node);
            node = (T)node.next;
        }

        return result;
    }

    /// <summary>
    /// 获取时间范围与 [startTime, endTime) 有重叠的所有事件节点
    /// 比如事件 [28, 35]，查 30~32 时也应该被包含
    ///
    /// 重叠条件：event.startTime < endTime AND event.endTime > startTime
    /// </summary>
    public List<T> GetEventsInRange(float startTime, float endTime)
    {
        var result = new List<T>();

        // 1. 跳到 startTime 附近
        int index = TimeToIndex(startTime);
        T node = ticks[index];
        if (node == null) return result;

        // 2. 往前走：确保不遗漏那些 startTime < startTime 但延续到范围内的
        //    一直走到某个事件彻底结束在 startTime 之前，再往前不可能有重叠了
        while (node.prev != null && node.prev is T prev)
        {
            if (prev.endTime <= startTime) break;
            node = prev;
        }

        // 3. 往后收集所有重叠事件
        while (node != null)
        {
            if (node.startTime >= endTime) break;   // 事件在范围之后 → 后面的也不用看了
            if (node.endTime > startTime)            // 重叠
                result.Add(node);
            node = (T)node.next;
        }

        return result;
    }

    /// <summary>在指定节点后插入新节点，只更新局部跳跃数组</summary>
    public void InsertAfter(T target, T newNode)
    {
        if (target == null || newNode == null) return;

        // 链表操作 O(1)
        newNode.prev = target;
        newNode.next = target.next;
        if (target.next != null) target.next.prev = newNode;
        target.next = newNode;

        nodeCount++;

        // 跳跃数组局部更新：只更新受影响的几个刻度
        if (ticks != null && ticks.Length > 0)
        {
            float newNodeTime = newNode.startTime;
            int startIdx = TimeToIndex(target.startTime);
            int endIdx = Mathf.Min(ticks.Length - 1,
                Mathf.CeilToInt((newNodeTime - minTime + tickSpacing) / tickSpacing));

            for (int i = startIdx; i <= endIdx; i++)
            {
                float tickTime = i * tickSpacing;
                if (ticks[i] == null) continue;
                float tickNodeTime = ticks[i].startTime;
                // 如果刻度指向的节点在新节点之后，或者刻度时间落到了新节点范围内
                if (tickNodeTime >= target.startTime && tickNodeTime <= newNodeTime)
                {
                    ticks[i] = newNode;
                }
                else if (tickNodeTime > newNodeTime && tickNodeTime < tickTime + tickSpacing)
                {
                    // 不需要动
                }
            }
        }

        // 检查是否需要完全重建
        if (nodeCount >= rebuildThreshold * 2)
        {
            dirty = true;
        }
    }

    /// <summary>在指定节点前插入新节点，自动管理表头</summary>
    public void InsertBefore(T target, T newNode)
    {
        if (target == null || newNode == null) return;

        // 链表操作 O(1)
        newNode.prev = target.prev;
        newNode.next = target;
        target.prev = newNode;
        if (newNode.prev != null)
            newNode.prev.next = newNode;
        else
            head = newNode;  // 插在表头前 → 新节点成为表头

        nodeCount++;

        // 如果插在表头前，时间范围变了 → 直接重建
        if (newNode == head)
        {
            dirty = true;
        }
        // 否则局部更新
        else if (ticks != null && ticks.Length > 0)
        {
            float newNodeTime = newNode.startTime;
            float targetTime = target.startTime;
            int startIdx = TimeToIndex(newNodeTime);
            int endIdx = Mathf.Min(ticks.Length - 1,
                Mathf.CeilToInt((targetTime - minTime + tickSpacing) / tickSpacing));

            for (int i = startIdx; i <= endIdx; i++)
            {
                if (ticks[i] == null) continue;
                float tickNodeTime = ticks[i].startTime;
                if (tickNodeTime >= newNodeTime && tickNodeTime <= targetTime)
                {
                    ticks[i] = newNode;
                }
            }
        }

        if (nodeCount >= rebuildThreshold * 2) dirty = true;
    }

    /// <summary>删除节点，自动管理表头，只更新局部跳跃数组</summary>
    public void Remove(T node)
    {
        if (node == null) return;

        // 链表操作 O(1)
        if (node.prev != null) node.prev.next = node.next;
        if (node.next != null) node.next.prev = node.prev;

        // 如果删的是表头，自动移到下一个
        if (node == head) head = (T)node.next;

        nodeCount--;

        // 跳跃数组局部更新
        if (ticks != null && ticks.Length > 0)
        {
            float nodeTime = node.startTime;
            int idx = TimeToIndex(node.startTime);

            // 如果这个刻度指向被删除的节点，向前或向后调整
            if (ticks[idx] == node)
            {
                ticks[idx] = node.next as T ?? node.prev as T;
            }
        }

        node.prev = null;
        node.next = null;

        if (nodeCount <= rebuildThreshold / 2) dirty = true;
    }

    /// <summary>跳跃数组是否需要重建</summary>
    public bool NeedsRebuild() => dirty;

    /// <summary>获取头节点（基于刻度 0）</summary>
    public T GetHead()
    {
        return ticks != null && ticks.Length > 0 ? ticks[0] : null;
    }
}

/// <summary>
/// 双向链表节点基类
/// </summary>
[Serializable]
public abstract class LinkedNode<T>
{
    public T prev;
    public T next;
}

/// <summary>
/// 带时间范围的双向链表节点（适用于事件，有起止时间）
/// </summary>
[Serializable]
public abstract class RangeNode : LinkedNode<RangeNode>
{
    public float startTime;
    public float endTime;

    protected RangeNode(float startTime, float endTime)
    {
        this.startTime = startTime;
        this.endTime = endTime;
    }
}

/// <summary>事件节点（有起止时间的泛型事件）</summary>
[Serializable]
public class EventNode<T> : RangeNode
{
    public T start;
    public T end;

    public EventNode() : base(0, 0) { }

    public EventNode(ImportChart.LineEvent<T> input) 
        : base(input.startTime, input.endTime)
    {
        this.start = input.start;
        this.end = input.end;
    }

    public EventNode(float startTime, float endTime, T from, T to) 
        : base(startTime, endTime)
    {
        this.start = from;
        this.end = to;
    }

    public EventNode<T> Clone() => new() {
        startTime = startTime,
        endTime = endTime,
        start = start,
        end = end
    };
}

[Serializable]
public class Chart : ImportChartVersion
{
    public float offset;
    public List<Judge> judges;

    [Serializable]
    public class Judge
    {
        public int zIndex;
        public string texture;

        public JumpArray<EventNode<float>> alphasJump;
    }
}