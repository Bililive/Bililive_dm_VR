using System;
using System.Threading;
using UnityEngine;

public class FPSLimit : MonoBehaviour
{
    public bool Enable = true;
    public int TargetFps = 90;

    private const float ONE_SECOND = 1000;

    private float deltaTime = 0.0f;
    private int delay = 0;

    void Update()
    {
        if (!Enable)
            return;

        deltaTime = Time.unscaledDeltaTime;

        // 单位: ms 毫秒
        // 90 FPS = 11.1111 per frame

        int target = Mathf.FloorToInt(ONE_SECOND / TargetFps);
        int msec = Mathf.CeilToInt(deltaTime * ONE_SECOND);

        float fps = 1.0f / deltaTime;

        if (delay != 0)
        {
            if (target < msec)
            {
                // 花的时间太长了
                delay--;
            }
            else if (target > msec)
            {
                // 花的时间太短了
                delay++;
            }
        }
        else
        {
            delay = target - 1;
        }

        Thread.Sleep(Math.Max(0, delay - 1));
    }
}