using AssetsTools.NET;
using Newtonsoft.Json.Linq;
using System;
using System.IO;

namespace ShaderLoad.Util
{
    /// <summary>
    /// 将 JSON 字符串按 AssetTypeTemplateField 结构反序列化为 Unity 对象二进制数据。
    /// 参考 UABEANext4.Logic.ImportExport.AssetImport 的实现。
    /// </summary>
    internal static class JsonAssetImporter
    {
        public static byte[] Import(AssetTypeTemplateField tempField, string jsonText)
        {
            using var ms = new MemoryStream();
            using var writer = new AssetsFileWriter(ms)
            {
                BigEndian = false
            };

            JToken token = JToken.Parse(jsonText);
            RecurseJsonImport(writer, tempField, token);

            return ms.ToArray();
        }

        private static void RecurseJsonImport(AssetsFileWriter writer, AssetTypeTemplateField tempField, JToken token)
        {
            bool align = tempField.IsAligned;

            // 如果模板只有一个数组子节点且 token 是数组，直接进入子节点
            if (tempField.Children.Count == 1 && tempField.Children[0].IsArray &&
                token.Type == JTokenType.Array)
            {
                RecurseJsonImport(writer, tempField.Children[0], token);
                return;
            }

            // 复合对象：遍历子字段，从 JSON 中按名称查找
            if (!tempField.HasValue && !tempField.IsArray)
            {
                foreach (AssetTypeTemplateField childTempField in tempField.Children)
                {
                    var childToken = token[childTempField.Name]
                        ?? throw new Exception($"JSON 中缺少字段 \"{childTempField.Name}\" (父字段: {tempField.Type} {tempField.Name})");

                    RecurseJsonImport(writer, childTempField, childToken);
                }

                if (align)
                    writer.Align();

                return;
            }

            // ManagedReferencesRegistry
            if (tempField.HasValue && tempField.ValueType == AssetValueType.ManagedReferencesRegistry)
            {
                throw new NotSupportedException("ManagedReferencesRegistry 暂不支持通过 JSON 导入。");
            }

            // 基本类型
            switch (tempField.ValueType)
            {
                case AssetValueType.Bool:
                    writer.Write((bool)token);
                    break;
                case AssetValueType.UInt8:
                    writer.Write((byte)token);
                    break;
                case AssetValueType.Int8:
                    writer.Write((sbyte)token);
                    break;
                case AssetValueType.UInt16:
                    writer.Write((ushort)token);
                    break;
                case AssetValueType.Int16:
                    writer.Write((short)token);
                    break;
                case AssetValueType.UInt32:
                    writer.Write((uint)token);
                    break;
                case AssetValueType.Int32:
                    writer.Write((int)token);
                    break;
                case AssetValueType.UInt64:
                    writer.Write((ulong)token);
                    break;
                case AssetValueType.Int64:
                    writer.Write((long)token);
                    break;
                case AssetValueType.Float:
                    writer.Write((float)token);
                    break;
                case AssetValueType.Double:
                    writer.Write((double)token);
                    break;
                case AssetValueType.String:
                    align = true;
                    writer.WriteCountStringInt32(token.ToString() ?? "");
                    break;
                case AssetValueType.Array:
                    // Array 类型由 switch 之后的数组处理代码处理
                    break;
                case AssetValueType.ByteArray:
                    {
                        var byteArrayJArray = token as JArray ?? new JArray();
                        byte[] byteArrayData = new byte[byteArrayJArray.Count];
                        for (int i = 0; i < byteArrayJArray.Count; i++)
                            byteArrayData[i] = (byte)byteArrayJArray[i];
                        writer.Write(byteArrayData.Length);
                        writer.Write(byteArrayData);
                        break;
                    }
                default:
                    throw new NotSupportedException($"不支持的 AssetValueType: {tempField.ValueType}");
            }

            // 数组类型处理（非 ByteArray）
            if (tempField.IsArray && tempField.ValueType != AssetValueType.ByteArray)
            {
                // children[0] = size, children[1] = data
                AssetTypeTemplateField childTempField = tempField.Children[1];
                var tokenArray = token as JArray
                    ?? throw new Exception($"字段 \"{tempField.Name}\" 在 JSON 中不是数组。");

                writer.Write(tokenArray.Count);
                foreach (JToken childToken in tokenArray.Children())
                {
                    RecurseJsonImport(writer, childTempField, childToken);
                }
            }

            if (align)
                writer.Align();
        }
    }
}