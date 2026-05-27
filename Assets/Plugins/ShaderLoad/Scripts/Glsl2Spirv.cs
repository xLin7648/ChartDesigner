using System;
using System.Runtime.InteropServices;

public static class Glsl2Spirv
{
#if UNITY_IOS
    private const string LibName = "__Internal";
#else
    private const string LibName = "glsl_to_spirv";
#endif

    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
    private static extern int glsl_to_spirv_compile(
        string source,
        out IntPtr spvData,
        out int spvLen,
        out IntPtr error
    );

    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
    private static extern void glsl_to_spirv_free_result(IntPtr spvData, int spvLen);

    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
    private static extern void glsl_to_spirv_free_error(IntPtr error);

    public static bool Compile(string glslSource, out string error, out byte[] spvBytes)
    {
        error = string.Empty;
        spvBytes = null;

        int result = glsl_to_spirv_compile(glslSource, out IntPtr spvData, out int spvLen, out IntPtr errorPtr);

        if (result != 0)
        {
            error = Marshal.PtrToStringAnsi(errorPtr) ?? "未知错误";
            glsl_to_spirv_free_error(errorPtr);
            return false;
        }

        spvBytes = new byte[spvLen];
        Marshal.Copy(spvData, spvBytes, 0, spvLen);
        glsl_to_spirv_free_result(spvData, spvLen);

        return true;
    }
}
