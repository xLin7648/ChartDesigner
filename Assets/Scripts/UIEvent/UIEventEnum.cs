using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum UIEventEnum 
{
    /// <summary>
    /// 场景加载完毕
    /// </summary>
    SceneLoaded,

    /// <summary>
    /// 场景尺寸变更
    /// </summary>
    ScreenSizeChanged,

    /// <summary>
    /// 游戏场景尺寸变更
    /// </summary>
    GameAreaChanged,

    /// <summary>
    /// 鼠标移动
    /// </summary>
    MouseMoved,

    /// <summary>
    /// 选择的节拍线发生变更
    /// </summary>
    SelectedBeatLineChanged,
}