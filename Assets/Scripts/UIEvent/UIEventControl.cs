using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class UIEventControl 
{

    /// <summary>
    /// 委托事件
    /// </summary>
    /// <param name="data"></param>
    public delegate void EventHandler(object data);
    /// <summary>
    /// 委托事件
    /// </summary>
    /// <param name="data"></param>
    public delegate void EventObjHandler(Object data);
    /// <summary>
    /// 事件派发注册字典
    /// </summary>
    private static Dictionary<UIEventEnum, List<EventHandler>> mEventDic = new Dictionary<UIEventEnum, List<EventHandler>>();

    /// <summary>
    /// 事件派发字典，里面存放的的Object,这个
    /// </summary>
    private static Dictionary<UIEventEnum, List<EventObjHandler>> ObjDic = new Dictionary<UIEventEnum, List<EventObjHandler>>();

    /// <summary>
    /// 注册事件
    /// </summary>
    /// <param name="eventType"></param>
    /// <param name="eventHandler"></param>
    public static void AddEvent(UIEventEnum eventType, EventHandler eventHandler)
    {
        if (!mEventDic.ContainsKey(eventType))
        {
            mEventDic.Add(eventType, new List<EventHandler>());
        }
        if (!mEventDic[eventType].Contains(eventHandler))
        {
            mEventDic[eventType].Add(eventHandler);
        }
    }

    /// <summary>
    /// 注册事件
    /// </summary>
    /// <param name="eventType"></param>
    /// <param name="eventHandler"></param>
    public static void AddObjEvent(UIEventEnum eventType, EventObjHandler eventHandler)
    {
        if (!ObjDic.ContainsKey(eventType))
        {
            ObjDic.Add(eventType, new List<EventObjHandler>());
        }
        if (!ObjDic[eventType].Contains(eventHandler))
        {
            ObjDic[eventType].Add(eventHandler);
        }
    }

    /// <summary>
    /// 移除事件
    /// </summary>
    /// <param name="eventType"></param>
    /// <param name="eventHandler"></param>
    public static void RemoveObjEvent(UIEventEnum eventType, EventObjHandler eventHandler)
    {
        if (ObjDic.ContainsKey(eventType))
        {
            if (ObjDic[eventType].Contains(eventHandler))
            {
                ObjDic[eventType].Remove(eventHandler);
                return;
            }
        }

    }

    /// <summary>
    /// 移除事件
    /// </summary>
    /// <param name="eventType"></param>
    /// <param name="eventHandler"></param>
    public static void RemoveEvent(UIEventEnum eventType, EventHandler eventHandler)
    {
        if (mEventDic.ContainsKey(eventType))
        {
            if (mEventDic[eventType].Contains(eventHandler))
            {
                mEventDic[eventType].Remove(eventHandler);
                return;

            }
        }

    }
    /// <summary>
    /// 分发事件
    /// </summary>
    /// <param name="eventType"></param>
    /// <param name="data"></param>
    public static void DispensObjEvent(UIEventEnum eventType, Object data = null)
    {
        List<EventObjHandler> eventList = null;
        if (ObjDic.ContainsKey(eventType))
        {
            eventList = ObjDic[eventType];
        }
        else
        {
            Debug.LogError("你没有往事件中心注册事件或者你注销了事件");
            return;
        }
        for (int i = 0; i < eventList.Count; i++)
        {
            eventList[i]?.Invoke(data);
        }
    }

    /// <summary>
    /// 分发事件
    /// </summary>
    /// <param name="eventType"></param>
    /// <param name="data"></param>
    public static void DispensEvent(UIEventEnum eventType, object data = null)
    {
        List<EventHandler> eventList = null;
        if (mEventDic.ContainsKey(eventType))
        {
            eventList = mEventDic[eventType];
        }
        else
        {
            // Debug.Log("你没有往事件中心注册事件或者你注销了事件");
            return;
        }
        for (int i = 0; i < eventList.Count; i++)
        {
            eventList[i]?.Invoke(data);
        }

    }
}
