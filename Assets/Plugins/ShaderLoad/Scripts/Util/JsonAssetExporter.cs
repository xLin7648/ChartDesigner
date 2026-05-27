using System;
using AssetsTools.NET;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace ShaderLoad.Util
{
    /// <summary>
    /// 将 AssetTypeValueField 树导出为 JSON 字符串（供用户参考和编辑）。
    /// 参考 UABEANext4.Logic.ImportExport.AssetExport 的实现。
    /// </summary>
    internal static class JsonAssetExporter
    {
        public static string ExportToJson(AssetTypeValueField baseField)
        {
            var jToken = RecurseJsonDump(baseField);
            return jToken.ToString(Formatting.Indented);
        }

        private static JToken RecurseJsonDump(AssetTypeValueField field)
        {
            var template = field.TemplateField;
            var isArray = template.IsArray;

            if (isArray)
            {
                var jArray = new JArray();
                if (template.ValueType != AssetValueType.ByteArray)
                {
                    for (int i = 0; i < field.Children.Count; i++)
                    {
                        jArray.Add(RecurseJsonDump(field.Children[i]));
                    }
                }
                else
                {
                    var byteArrayData = field.AsByteArray;
                    for (int i = 0; i < byteArrayData.Length; i++)
                    {
                        jArray.Add(byteArrayData[i]);
                    }
                }
                return jArray;
            }
            else
            {
                if (field.Value != null)
                {
                    if (field.Value.ValueType == AssetValueType.ManagedReferencesRegistry)
                    {
                        throw new NotSupportedException("ManagedReferencesRegistry 暂不支持导出为 JSON。");
                    }

                    // 尝试按类型取值
                    var value = field.Value.ValueType switch
                    {
                        AssetValueType.Bool => (object)field.AsBool,
                        AssetValueType.Int8 or AssetValueType.Int16 or AssetValueType.Int32 => field.AsInt,
                        AssetValueType.Int64 => field.AsLong,
                        AssetValueType.UInt8 or AssetValueType.UInt16 or AssetValueType.UInt32 => field.AsUInt,
                        AssetValueType.UInt64 => field.AsULong,
                        AssetValueType.String => field.AsString,
                        AssetValueType.Float => field.AsFloat,
                        AssetValueType.Double => field.AsDouble,
                        _ => null // 无法识别的类型，降级为容器
                    };

                    if (value != null)
                        return JToken.FromObject(value);

                    // 降级：作为容器处理，输出子字段
                    var jObject = new JObject();
                    foreach (var child in field)
                    {
                        jObject.Add(child.FieldName, RecurseJsonDump(child));
                    }
                    return jObject;
                }
                else
                {
                    var jObject = new JObject();
                    foreach (var child in field)
                    {
                        jObject.Add(child.FieldName, RecurseJsonDump(child));
                    }
                    return jObject;
                }
            }
        }
    }
}