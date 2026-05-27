using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

public static class GlslVerify
{
#if UNITY_IOS
    private const string LibName = "__Internal";
#else
    private const string LibName = "glsl_verify";
#endif

    /// <summary>
    /// Rust 库的 FFI 入口。
    /// 返回值：-1 = 有效，>= 0 = 无效，返回值为完整错误信息长度（不含空终止）。
    /// 若返回值 >= errorBufSize，说明 buffer 不够，调用者应重试。
    ///
    /// DLL 名称说明：
    /// - 直接使用 cdylib 时改为 "glsl_verify"
    /// - 包装为 Unity Native Plugin 时改为插件 DLL 名
    /// - IL2CPP 静态链接时改为 "__Internal"
    /// </summary>
    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
    private static extern int glsl_validate_bytes(
        byte[] source, int sourceLen,
        [Out] byte[] errorBuf, int errorBufSize);

    /// <summary>
    /// 将 GLSL 源码通过 FFI 传递给 Rust glslang 库进行语法验证，
    /// 错误信息写入 errors 列表。
    /// </summary>
    public static void Validate(string source, List<string> errors)
    {
        if (string.IsNullOrEmpty(source))
        {
            errors.Add("源码为空");
            return;
        }
        byte[] utf8 = Encoding.UTF8.GetBytes(source);
        byte[] errorBuf = new byte[4096];
        int result = glsl_validate_bytes(utf8, utf8.Length, errorBuf, errorBuf.Length);
        if (result < 0) return;

        int copyLen = Math.Min(result, errorBuf.Length);
        string errorMsg = Encoding.UTF8.GetString(errorBuf, 0, copyLen).TrimEnd('\0');
        errors.Add(errorMsg);
    }
}