using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using UnityEngine;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;
using YamlDotNet.Serialization;

/// <summary>
/// 从 YAML 文件加载 ShaderConfig（包含 name、glsl 源码、defaultVars）。
/// YAML 文件放置于 Resources/Shader/ 目录下。
/// </summary>
public static class ShaderYamlConfigLoader
{
    private static readonly IDeserializer Deserializer = new DeserializerBuilder()
        .WithTypeConverter(new Vector4YamlConverter())
        .IgnoreUnmatchedProperties()
        .Build();

    /// <summary>
    /// 通过 Resources 路径加载 YAML shader 配置。
    /// 如 Resources/Shader/chromatic.yaml → 传 "Shader/chromatic"
    /// </summary>
    public static ShaderConfig LoadFromResource(string resourcePath)
    {
        var textAsset = Resources.Load<TextAsset>(resourcePath);
        if (textAsset == null)
            throw new FileNotFoundException(
                $"Shader config not found at Resources/{resourcePath}");

        return LoadFromText(textAsset.text);
    }

    /// <summary>
    /// 从 YAML 文本直接反序列化。
    /// </summary>
    public static ShaderConfig LoadFromText(string yamlText)
    {
        return Deserializer.Deserialize<ShaderConfig>(yamlText);
    }
}

// ─── 自定义 Vector4 转换器（支持标量和数组两种 YAML 写法） ───

internal sealed class Vector4YamlConverter : IYamlTypeConverter
{
    public bool Accepts(Type type) => type == typeof(Vector4);

    public object ReadYaml(IParser parser, Type type, ObjectDeserializer rootDeserializer)
    {
        var result = Vector4.zero;

        if (parser.TryConsume<Scalar>(out var scalar))
        {
            result.x = float.Parse(scalar.Value, NumberStyles.Float, CultureInfo.InvariantCulture);
        }
        else if (parser.TryConsume<SequenceStart>(out _))
        {
            int i = 0;
            while (parser.TryConsume<Scalar>(out var item) && i < 4)
            {
                result[i++] = float.Parse(item.Value, NumberStyles.Float, CultureInfo.InvariantCulture);
            }
            parser.TryConsume<SequenceEnd>(out _);
        }

        return result;
    }

    public void WriteYaml(IEmitter emitter, object? value, Type type, ObjectSerializer rootSerializer)
    {
        throw new NotSupportedException("Vector4 YAML 序列化暂不支持");
    }
}

// ─── 公开数据结构 ───

[Serializable]
public sealed class ShaderConfig
{
    public string name;
    public string glsl;
    public Dictionary<string, ShaderDefaultUniformConfig> defaultVars;
}

[Serializable]
public sealed class ShaderDefaultUniformConfig
{
    public string type;
    public Vector4 val; // float→x, vec2→xy, vec3→xyz, vec4→xyzw
}