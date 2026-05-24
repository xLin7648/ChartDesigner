using System;

public static class ResolutionMatcher
{
    /// <summary>
    /// 计算离目标分辨率最近的同比例整数分辨率（精确匹配比例）
    /// </summary>
    public static bool Match(int screenWidth, int screenHeight, uint ratioW, uint ratioH,
        out int resultWidth, out int resultHeight)
    {
        return Match(screenWidth, screenHeight, ratioW, ratioH, 0.0, out resultWidth, out resultHeight);
    }

    /// <summary>
    /// 计算离目标分辨率最近的同比例整数分辨率
    /// </summary>
    /// <param name="screenWidth">屏幕宽（像素）</param>
    /// <param name="screenHeight">屏幕高（像素）</param>
    /// <param name="ratioW">比例宽</param>
    /// <param name="ratioH">比例高</param>
    /// <param name="tolerance">
    /// 比例允许偏差。0=精确比例；0.01=允许 1% 的偏差。
    /// 允许微小偏差时可能找到离原始分辨率更近的整数解。
    /// </param>
    /// <param name="resultWidth">匹配后的宽</param>
    /// <param name="resultHeight">匹配后的高</param>
    /// <returns>true 找到有效分辨率；false 输入无效</returns>
    public static bool Match(int screenWidth, int screenHeight, uint ratioW, uint ratioH,
        double tolerance, out int resultWidth, out int resultHeight)
    {
        resultWidth = 0;
        resultHeight = 0;

        if (screenWidth <= 0 || screenHeight <= 0 || ratioW <= 0 || ratioH <= 0)
            return false;

        if (tolerance < 0) tolerance = 0;

        // 约分比例，避免比例本身有公约数导致搜索跳过有效解
        uint g = Gcd(ratioW, ratioH);
        ratioW /= g;
        ratioH /= g;

        if (tolerance == 0)
        {
            // —— 精确比例 ——
            // 通过最小二乘找到最优 k，使得点 (w*k, h*k) 距离 (W, H) 最近
            // 距离平方 = (w*k - W)^2 + (h*k - H)^2
            // 求导得 k = (w*W + h*H) / (w^2 + h^2)
            double kOpt = (double)(ratioW * screenWidth + ratioH * screenHeight)
                          / (ratioW * ratioW + ratioH * ratioH);

            long bestW = 0, bestH = 0;
            long bestDistSq = long.MaxValue;

            int kFloor = (int)Math.Floor(kOpt);
            int kCeil  = (int)Math.Ceiling(kOpt);
            int kMin   = Math.Max(1, Math.Min(kFloor, kCeil) - 2);
            int kMax   = Math.Max(Math.Max(kFloor, kCeil) + 2, kMin + 4);

            for (int k = kMin; k <= kMax; k++)
            {
                long w = (long)ratioW * k;
                long h = (long)ratioH * k;
                long dsq = (w - screenWidth) * (w - screenWidth)
                         + (h - screenHeight) * (h - screenHeight);

                if (dsq < bestDistSq)
                {
                    bestDistSq = dsq;
                    bestW = w;
                    bestH = h;
                }
            }

            if (bestW > int.MaxValue || bestH > int.MaxValue)
                return false;

            resultWidth  = (int)bestW;
            resultHeight = (int)bestH;
            return true;
        }
        else
        {
            // —— 允许比例偏差 ——
            // 在原始分辨率附近搜索整数宽高，满足比例偏差 ≤ tolerance，
            // 从中选出像素距离最近的。
            double targetRatio = (double)ratioW / ratioH;

            // 搜索半径：取屏幕长边的一半，确保覆盖足够范围
            int searchRadius = Math.Max(screenWidth, screenHeight);

            long bestW = 0, bestH = 0;
            long bestDistSq = long.MaxValue;

            int hMin = Math.Max(1, screenHeight - searchRadius);
            int hMax = screenHeight + searchRadius;

            for (int h = hMin; h <= hMax; h++)
            {
                int w = (int)Math.Round((double)h * ratioW / ratioH);
                if (w <= 0) continue;

                double actualRatio = (double)w / h;
                double ratioError = Math.Abs(actualRatio / targetRatio - 1.0);

                if (ratioError <= tolerance)
                {
                    long dsq = (long)(w - screenWidth) * (w - screenWidth)
                             + (long)(h - screenHeight) * (h - screenHeight);
                    if (dsq < bestDistSq)
                    {
                        bestDistSq = dsq;
                        bestW = w;
                        bestH = h;
                    }
                }
            }

            int wMin = Math.Max(1, screenWidth - searchRadius);
            int wMax = screenWidth + searchRadius;

            for (int w = wMin; w <= wMax; w++)
            {
                int h = (int)Math.Round((double)w * ratioH / ratioW);
                if (h <= 0) continue;

                double actualRatio = (double)w / h;
                double ratioError = Math.Abs(actualRatio / targetRatio - 1.0);

                if (ratioError <= tolerance)
                {
                    long dsq = (long)(w - screenWidth) * (w - screenWidth)
                             + (long)(h - screenHeight) * (h - screenHeight);
                    if (dsq < bestDistSq)
                    {
                        bestDistSq = dsq;
                        bestW = w;
                        bestH = h;
                    }
                }
            }

            if (bestDistSq == long.MaxValue)
                return false;

            resultWidth  = (int)bestW;
            resultHeight = (int)bestH;
            return true;
        }
    }

    private static uint Gcd(uint a, uint b)
    {
        while (b != 0)
        {
            uint t = b;
            b = a % b;
            a = t;
        }
        return a;
    }
}
