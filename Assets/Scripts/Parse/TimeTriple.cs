using System;

public struct TimeTriple
{
    public int i;
    public uint n;
    public uint d;

    public TimeTriple(float i, float n, float d)
    {
        this.i = (int)i;
        this.n = (uint)n;
        this.d = (uint)d;
    }

    public float Beats() => (float)i + (float)n / (float)d;

    public static TimeTriple Default() => new()
    {
        i = 0,
        n = 0,
        d = 1
    };

    public override bool Equals(object obj)
    {
        if (obj is TimeTriple other)
        {
            return this.i == other.i && this.n == other.n && this.d == other.d;
        }
        return false;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(i, n, d);
    }

    public static bool operator ==(TimeTriple left, TimeTriple right)
    {
        if (ReferenceEquals(left, right))
            return true;
        return left.Equals(right);
    }

    public static bool operator !=(TimeTriple left, TimeTriple right)
    {
        return !(left == right);
    }
}

public class FractionSimplifier
{
    public static TimeTriple SimplifyMixedNumber(int integerPart, int numerator, int denominator)
    {
        // 检查分母是否为0
        if (denominator == 0)
        {
            throw new ArgumentException("分母不能为0。");
        }

        // 检查分子或分母是否为负数
        if (numerator < 0 || denominator < 0)
        {
            throw new ArgumentException("分子和分母均不能为负数。");
        }

        // 如果分子为0，则分数部分为0，直接返回整数部分
        if (numerator == 0)
        {
            return new TimeTriple(integerPart, 0, 1);
        }

        // 计算分子和分母的最大公约数（GCD）
        int gcd = GCD(numerator, denominator);
        int simplifiedNumerator = numerator / gcd;
        int simplifiedDenominator = denominator / gcd;

        // 如果分数部分是假分数，调整整数部分和分数部分
        if (simplifiedNumerator >= simplifiedDenominator)
        {
            int additionalInteger = simplifiedNumerator / simplifiedDenominator;
            integerPart += additionalInteger;
            simplifiedNumerator = simplifiedNumerator % simplifiedDenominator;
        }

        // 如果调整后分数部分为0，只返回整数部分
        if (simplifiedNumerator == 0)
        {
            return new TimeTriple(integerPart, 0, 1);
        }

        // 返回格式化的字符串：整数:最简真分数
        return new TimeTriple(integerPart, simplifiedNumerator, simplifiedDenominator);
    }

    // 辅助方法：计算两个整数的最大公约数（GCD）
    private static int GCD(int a, int b)
    {
        while (b != 0)
        {
            int temp = b;
            b = a % b;
            a = temp;
        }
        return a;
    }
}