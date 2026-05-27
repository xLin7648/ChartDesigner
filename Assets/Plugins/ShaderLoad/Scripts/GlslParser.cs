using ShaderLoad.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;

namespace ShaderLoad
{
    public static class UniformParser
    {
        /// <summary>
        /// 支持的 uniform 类型白名单。vec(n) 和 float 以外的类型不计入 CB。
        /// </summary>
        private static readonly HashSet<string> SupportedUniformTypes = new()
        {
            "float", "vec2", "vec3", "vec4"
        };

        /// <summary>
        /// 内置的Uniform
        /// </summary>
        private static readonly List<UniformInfo> UnityBuiltInUniforms = new()
        {
            new("vec4", "_Time"),
            // new("vec2", "_ScreenSize")
        };

        /// <summary>
        /// 内置的Uniform
        /// </summary>
        private static readonly List<UniformInfo> BuiltInUniforms = new()
        {
            // new("vec4", "_Time"),
            new("vec2", "_ScreenSize")
        };

        // 修改后的正则：只允许 uniform 类型 变量名;
        public static readonly Regex UniformRegex = new(
            @"uniform\s+" +
            @"(\w+)\s+" +
            @"(\w+)\s*" +
            @";",
            RegexOptions.Multiline
        );

        /// <summary>
        /// 从 Shader 源码中提取 // __UNIFORMS__ 和 // __MAIN__ 之间的内容，并解析出所有 uniform 变量。
        /// </summary>
        /// <returns>返回字典：变量名 -> 类型名</returns>
        internal static UniformInfo[] ParseUniformsFromShader(ShaderTargetPlatform target, string shaderSource, List<string> errors = null)
        {
            var uniformDic = new Dictionary<string, string>();
            var matches = UniformRegex.Matches(shaderSource);
            var builtInUniformNames = UnityBuiltInUniforms
                .Concat(BuiltInUniforms)
                .Select(x => x.Name)
                .ToList();

            foreach (Match match in matches)
            {
                if (match.Success)
                {
                    string type = match.Groups[1].Value;   // 类型，例如 float, vec3
                    string name = match.Groups[2].Value;   // 变量名，例如 u_time

                    if (builtInUniformNames.Contains(name))
                    {
                        errors?.Add($"\"{name}\" 是内置 Uniform，请换个名称");
                        continue;
                    }

                    if (!SupportedUniformTypes.Contains(type))
                    {
                        errors?.Add($"不支持的 uniform 类型 \"{name}\"（{name}）：只允许 float/vec2/vec3/vec4");
                        continue;
                    }

                    uniformDic[name] = type;                 // 存储
                }
            }

            return UnityBuiltInUniforms
                .Concat(BuiltInUniforms)
                .Concat(uniformDic.Select(x => new UniformInfo(x.Value, x.Key)))
                .ToArray();
        }
    }

    /// <summary>
    /// 解析 1.glsl 格式的用户着色器，提取 uniform 信息和做基本语法校验。
    /// </summary>
    public static class GlslParser
    {
        // ===== Public API =====

        /// <summary>
        /// 解析 GLSL 源码，提取 uniform 声明并进行校验。
        /// 语法层面的校验（类型检查、未定义变量等）已交由 glslang Rust 库处理。
        /// </summary>
        /// <param name="source">GLSL 源码</param>
        internal static ShaderParseResult Parse(
            string source,
            ShaderTargetPlatform target)
        {
            var errors = new List<string>();
            try
            {
                bool isUbo = target is ShaderTargetPlatform.Vulkan or ShaderTargetPlatform.Metal;

                var uniforms = new List<UniformInfo>();
               
                // 模板结构约束校验（预编译指令、layout、precision 等）
                ValidateFragmentOnly(source, errors);

                // 提取 uniform
                string clean = StripCommentsAndPreprocessor(source);

                if (!MainFuncRegex.IsMatch(clean))
                {
                    throw new Exception("无论如何都必须有 void mainImage(out vec4 fragColor, in vec2 fragCoord)");
                }

                uniforms.AddRange(UniformParser.ParseUniformsFromShader(target, clean, errors));
                var sortedUniforms = uniforms.OrderBy(x => x.Name).ToList();

                var mainFunc = UniformParser.UniformRegex.Replace(clean, string.Empty).TrimStart();

                var uniformSb = new StringBuilder();
                for (int i = 0, uc = sortedUniforms.Count; i < uc; i++)
                {
                    var uniform = sortedUniforms[i];
                    if (isUbo)
                    {
                        uniformSb.Append("\t");
                        uniformSb.Append(uniform.Type);
                        uniformSb.Append(" ");
                        uniformSb.Append(uniform.Name);

                        mainFunc = mainFunc.Replace(uniform.Name, $"ubo.{uniform.Name}");
                    }
                    else
                    {
                        uniformSb.Append("uniform ");
                        uniformSb.Append(uniform.Type);
                        uniformSb.Append(" ");
                        uniformSb.Append(uniform.Name);
                    }
                    uniformSb.Append(i == uc - 1 ? ";" : ";\n");
                }

                var fragTemplate = Resources.Load<TextAsset>($"Templates/{(int)target}/frag").text;
                var fragShader = fragTemplate
                    .Replace("__UNIFORMS__", uniformSb.ToString())
                    .Replace("__MAIN__", mainFunc);

                // 调用 Rust glslang 库验证最终 GLSL 语法
                if (!GlslTools.Verify(fragShader, isUbo, out var verifyErr))
                {
                    throw new Exception(verifyErr);
                }
               
                if (!isUbo)
                {
                    var vertTemplate = Resources.Load<TextAsset>($"Templates/{(int)target}/vert").text;
                    var shaderText = $"{vertTemplate}\n#ifdef FRAGMENT\n{fragShader}\n#endif";
                    return new ShaderParseResult(uniforms, sortedUniforms, errors, shaderText);
                }
                else
                {
                    
                    if (GlslTools.ToSpirv(fragShader, out var spv, out var error))
                    {
                        var smolv = Smolv.Encode(spv);
                        return new ShaderParseResult(uniforms, sortedUniforms, errors, shaderBytes: smolv);
                    }
                    else
                    {
                        throw new Exception(error);
                    }
                }
            }
            catch (Exception e)
            {
                errors.Add(e.Message);
                return ShaderParseResult.Empty;
            }
        }

        // 模式解释：
        // void\s+mainImage   : void 和 mainImage 之间至少一个空白字符
        // \s*\(\s*           : 左括号前后允许空白
        // out\s+vec4\s+fragColor : out 与 vec4、vec4 与 fragColor 之间至少一个空白
        // \s*,\s*            : 逗号前后允许空白
        // in\s+vec2\s+fragCoord : 同理
        // \s*\)              : 右括号前允许空白
        private static readonly Regex MainFuncRegex = 
            new(@"void\s+mainImage\s*\(\s*out\s+vec4\s+fragColor\s*,\s*in\s+vec2\s+fragCoord\s*\)");

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
