using UnityEngine;
using System.Collections;

public class CommonDetection : MonoBehaviour
{
    private float oldWidth;
    private float oldHeight;
    private Vector3 oldMousePosition;

    void Start()
    {
        // 初始化尺寸记录
        oldWidth = Screen.width;
        oldHeight = Screen.height;
        oldMousePosition = Input.mousePosition;

        // 启动检测协程
        StartCoroutine(DetectionCoroutine());
    }

    private IEnumerator DetectionCoroutine()
    {
        while (true)
        {
            // 检测屏幕尺寸变化
            float newWidth = Screen.width;
            float newHeight = Screen.height;

            if (oldWidth != newWidth || oldHeight != newHeight)
            {
                // 等待一帧
                yield return null;

                // 发送屏幕尺寸改变事件
                UIEventControl.DispensEvent(UIEventEnum.ScreenSizeChanged, new Vector2(newWidth, newHeight));

                // 更新记录
                oldWidth = newWidth;
                oldHeight = newHeight;
            }

            // 检测鼠标移动变化
            Vector3 newMousePosition = Input.mousePosition;
            if (oldMousePosition != newMousePosition)
            {
                // 发送鼠标移动事件
                UIEventControl.DispensEvent(UIEventEnum.MouseMoved, newMousePosition);

                // 更新记录
                oldMousePosition = newMousePosition;
            }

            // 每帧检查一次
            yield return null;
        }
    }

    void OnDestroy()
    {
        // 确保协程在对象销毁时停止
        StopAllCoroutines();
    }
}
