using System;
using System.Runtime.InteropServices;

namespace ShaderLoad
{
    public static class GlslTools
    {
#if UNITY_IOS
        private const string LibName = "__Internal";
#else
        private const string LibName = "glsl_tools";
#endif

        // ============================================================
        //  GLSL → SPIR-V 编译
        // ============================================================

        [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
        private static extern int to_spirv(
            string source,
            out IntPtr spvData,
            out int spvLen,
            out IntPtr error
        );

        [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
        private static extern void free_spv(IntPtr spvData, int spvLen);

        /// 编译 GLSL 片元着色器为 SPIR-V 字节码
        public static bool ToSpirv(string glslSource, out byte[] spvBytes, out string error)
        {
            error = string.Empty;
            spvBytes = null;

            int result = to_spirv(glslSource, out IntPtr spvData, out int spvLen, out IntPtr errorPtr);

            if (result != 0)
            {
                error = Marshal.PtrToStringAnsi(errorPtr) ?? "未知错误";
                FreeError(errorPtr);
                return false;
            }

            spvBytes = new byte[spvLen];
            Marshal.Copy(spvData, spvBytes, 0, spvLen);
            free_spv(spvData, spvLen);

            return true;
        }

        // ============================================================
        //  GLSL 语法验证
        // ============================================================

        [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
        private static extern int verify(
            string source,
            [MarshalAs(UnmanagedType.U1)] bool isVulkan,
            out IntPtr error
        );

        /// 验证 GLSL 片元着色器语法
        public static bool Verify(string glslSource, bool isVulkan, out string error)
        {
            error = string.Empty;

            int result = verify(glslSource, isVulkan, out IntPtr errorPtr);

            if (result != 0)
            {
                error = Marshal.PtrToStringAnsi(errorPtr) ?? "未知错误";
            }

            FreeError(errorPtr);
            return result == 0;
        }

        // ============================================================
        //  共享内存管理
        // ============================================================

        [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
        private static extern void free_error(IntPtr error);

        /// 释放由 Compile 或 Verify 分配的错误字符串
        private static void FreeError(IntPtr error)
        {
            if (error != IntPtr.Zero)
            {
                free_error(error);
            }
        }
    }
}