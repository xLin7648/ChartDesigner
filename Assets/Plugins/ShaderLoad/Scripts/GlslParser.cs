using ShaderLoad.Util;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;

namespace ShaderLoad
{
    /// <summary>
    /// 解析 1.glsl 格式的用户着色器，提取 uniform 信息和做基本语法校验。
    /// </summary>
    public static class GlslParser
    {
        /// <summary>
        /// 支持的 uniform 类型白名单。vec(n) 和 float 以外的类型不计入 CB。
        /// </summary>
        private static readonly HashSet<string> SupportedUniformTypes = new()
        {
            "float", "vec2", "vec3", "vec4"
        };

        private static readonly List<UniformInfo> BuiltInUniforms = new()
        {
            new("vec4", "_Time"),
            new("vec2", "_ScreenSize")
        };

        private static readonly Regex UniformRegex = new Regex(
            @"(?:\blayout\s*\([^)]*\)\s*)?" +
            @"uniform\s+" +
            @"(?:(?:highp|mediump|lowp)\s+)?" +
            @"(\w+)\s+" +
            @"(\w+)\s*" +
            @";"
        );

        // 无需再维护 GLSL 关键字/类型/内置函数/内置变量等符号表，
        // 语法相关的校验已交由 glslang Rust 库处理。

        // ===== FFI: 调用 Rust glslang 验证库 =====

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
        private static void ValidateGlsl(string source, List<string> errors)
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

        // ===== Public API =====

        /// <summary>
        /// 解析 GLSL 源码，提取 uniform 声明并进行校验。
        /// 语法层面的校验（类型检查、未定义变量等）已交由 glslang Rust 库处理。
        /// </summary>
        /// <param name="source">GLSL 源码</param>
        internal static ShaderParseResult Parse(
            string source,
            ShaderTargetPlatform shaderTargetPlatform)
        {
            var uniforms = new List<UniformInfo>();
            var errors = new List<string>();

            // 模板结构约束校验（预编译指令、layout、precision 等）
            ValidateFragmentOnly(source, errors);

            // 提取 uniform
            string clean = StripCommentsAndPreprocessor(source);

            foreach (Match m in UniformRegex.Matches(clean))
            {
                uniforms.Add(new UniformInfo(m.Groups[1].Value, m.Groups[2].Value));
            }

            // 校验 uniform 类型（仅允许 float/vec2/vec3/vec4 四种）
            foreach (var u in uniforms)
            {
                if (!SupportedUniformTypes.Contains(u.Type))
                    errors.Add($"不支持的 uniform 类型 \"{u.Name}\"（{u.Name}）：只允许 float/vec2/vec3/vec4");
            }

            // 追加内置 uniform（跳过用户已声明的）
            foreach (var builtin in BuiltInUniforms)
            {
                if (!uniforms.Exists(u => u.Name == builtin.Name))
                    uniforms.Add(builtin);
            }

            // 提取 uniform外的内容
            var mainFunc = UniformRegex.Replace(clean, string.Empty);

            // 4. 构建最终 GLSL：将用户代码合并到模板中
            var vertTemplate = Resources.Load<TextAsset>($"Templates/{(int)shaderTargetPlatform}/vert").text;
            var fragTemplate = Resources.Load<TextAsset>($"Templates/{(int)shaderTargetPlatform}/frag").text;

            // 提取 uniform 声明的原始文本（不含 _Time，模板已自带）
            var uniformDecls = new List<string>();
            foreach (Match m in UniformRegex.Matches(clean))
            {
                string uniformName = m.Groups[2].Value;
                if (BuiltInUniforms.Exists(b => b.Name == uniformName)) continue;
                uniformDecls.Add(m.Value.Trim());
            }

            var fragShader = fragTemplate
                .Replace("__UNIFORMS__", string.Join("\n", uniformDecls))
                .Replace("__MAIN__", mainFunc);

            // 调用 Rust glslang 库验证最终 GLSL 语法
            ValidateGlsl(fragShader, errors);

            var shaderText = $"{vertTemplate}\n#ifdef FRAGMENT\n{fragShader}\n#endif";
            return new ShaderParseResult(uniforms, errors, shaderText);
        }

        // ===== 工具 =====

        /// <summary>
        /// 去除注释和预编译指令，便于正则匹配。
        /// </summary>
        private static string StripCommentsAndPreprocessor(string source)
        {
            string s = Regex.Replace(source, @"//.*", "");
            s = Regex.Replace(s, @"/\*.*?\*/", "", RegexOptions.Singleline);
            s = Regex.Replace(s, @"^\s*#.*$", "", RegexOptions.Multiline);
            return s;
        }

        /// <summary>
        /// 校验源码是否混入了模板管理的内容（预编译指令、layout 等）。
        /// 语法级别的校验已由 glslang Rust 库处理，此处仅检查模板结构约束。
        /// </summary>
        private static void ValidateFragmentOnly(string source, List<string> errors)
        {
            // 检查任何 # 开头的预编译指令（模板已自带 #version/#define 等，用户源码不应再出现）
            if (Regex.IsMatch(source, @"^\s*#", RegexOptions.Multiline))
            {
                errors.Add("源码中不允许使用预编译指令（#version / #define / #ifdef 等）");
                return;
            }

            // 检查 layout(（由模板统一管理）
            if (source.Contains("layout("))
            {
                errors.Add("源码中不允许使用 layout 关键字，由模板统一管理");
                return;
            }

            // 检查 precision 精度声明（由模板统一管理）
            if (Regex.IsMatch(source, @"\bprecision\s+(highp|mediump|lowp)\b"))
            {
                errors.Add("精度声明（precision）由模板统一管理，源码中无需设置");
                return;
            }
        }

    }
}
