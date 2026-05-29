using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

[Serializable]
public class VarEntry
{
    public float startTime;
    public float endTime;
    public Vector4 start;
    public Vector4 end;

    public bool IsConstant => Mathf.Approximately(startTime, 0)
                           && Mathf.Approximately(endTime, 0);
}

[Serializable]
public class KData
{
    public Dictionary<string, VarEntry> vars;
}

/// <summary>
/// 自定义Converter：处理 vars 里既有动画对象、又有常量的混合结构
/// </summary>
public class VarEntryConverter : JsonConverter<VarEntry>
{
    public override VarEntry ReadJson(JsonReader reader, Type objectType,
        VarEntry existingValue, bool hasExistingValue, JsonSerializer serializer)
    {
        var token = JToken.Load(reader);
        var entry = new VarEntry();

        if (token.Type == JTokenType.Object)
        {
            // 动画属性：有 startTime/endTime/start/end
            entry.startTime = token["startTime"]?.Value<float>() ?? 0;
            entry.endTime = token["endTime"]?.Value<float>() ?? 0;
            entry.start = TokenToVector4(token["start"]);
            entry.end = TokenToVector4(token["end"]);
        }
        else
        {
            // 常量属性：值直接就是标量/数组
            entry.start = TokenToVector4(token);
            entry.end = entry.start; // 常量的 start === end
        }

        return entry;
    }

    public override void WriteJson(JsonWriter writer, VarEntry value,
        JsonSerializer serializer)
    {
        throw new NotImplementedException();
    }

    private static Vector4 TokenToVector4(JToken token)
    {
        if (token == null) return Vector4.zero;

        switch (token.Type)
        {
            case JTokenType.Float:
            case JTokenType.Integer:
                return new Vector4(token.Value<float>(), 0, 0, 0);

            case JTokenType.Array:
                var arr = token.ToObject<List<float>>();
                var v = Vector4.zero;
                for (int i = 0; i < Mathf.Min(arr.Count, 4); i++)
                    v[i] = arr[i];
                return v;

            default:
                return Vector4.zero;
        }
    }
}