using ShaderLoad.Util;
using System.Text;
using UnityEngine;

namespace ShaderLoad
{
    internal static class Gles3BinaryEditor
    {
        private const int Module1Offset = 0x1C;
        private const int Module1Size = 0xD0;

        private const int CbSizeOffset = 0x40;

        private const int Module2Offset = 0xEC;
        private const int Module2FixedHeaderSize = 0x20; // 0xEC~0x10B = 32字节
        private const int ShaderSizeOffset = 0x108;
        private const int ShaderContentOffset = 0x10C;

        private const int FooterSize = 8;
        private const int FooterSrcOffset = 0xA48;

        private const int GlobalModule2SizeOffset = 0x14;

        /// <summary>
        /// 构建修改后的shader模板
        /// </summary>
        /// <param name="fragmentCbSize">片元CB的新大小</param>
        /// <param name="glslText">第二个模块内的新GLSL文本</param>
        /// <returns>修改后的完整字节数组</returns>
        internal static byte[] BuildTemplate(int fragmentCbSize, string glslText, ShaderTargetPlatform shaderTargetPlatform)
        {
            byte[] src = Resources.Load<TextAsset>($"Templates/{(int)shaderTargetPlatform}/Binary").bytes;

            // 提取末尾固定8字节
            byte[] footer = new byte[FooterSize];
            System.Buffer.BlockCopy(src, FooterSrcOffset, footer, 0, FooterSize);

            // GLSL文本转UTF-8字节
            byte[] glslBytes = Encoding.UTF8.GetBytes(glslText);
            int padLen = (4 - (glslBytes.Length % 4)) % 4;

            // 计算新模块2大小
            int module2Size = Module2FixedHeaderSize + glslBytes.Length + padLen + FooterSize;

            // 构造新文件
            int newSize = Module2Offset + module2Size;
            byte[] result = new byte[newSize];

            // 1. 复制全局头部 + 模块1开头 (0x00~0x3F)
            System.Buffer.BlockCopy(src, 0, result, 0, 0x40);

            // 2. 写片元CB大小 at 0x40~0x43
            WriteInt32LE(result, CbSizeOffset, fragmentCbSize);

            // 3. 复制模块1剩余部分 (0x44~0xEB)
            int module1TailSize = (Module1Offset + Module1Size) - 0x44; // 0xEC - 0x44 = 0xA8
            System.Buffer.BlockCopy(src, 0x44, result, 0x44, module1TailSize);

            // 4. 复制模块2头部 (0xEC~0x107)，不包括shader size字段
            int module2HeadSize = ShaderSizeOffset - Module2Offset; // 0x1C
            System.Buffer.BlockCopy(src, Module2Offset, result, Module2Offset, module2HeadSize);

            // 5. 写shader size at 0x108~0x10B
            WriteInt32LE(result, ShaderSizeOffset, glslBytes.Length);

            // 6. 写GLSL内容 at 0x10C
            System.Buffer.BlockCopy(glslBytes, 0, result, ShaderContentOffset, glslBytes.Length);

            // 7. 写pad对齐
            int padStart = ShaderContentOffset + glslBytes.Length;
            for (int i = 0; i < padLen; i++)
                result[padStart + i] = 0;

            // 8. 写footer
            System.Buffer.BlockCopy(footer, 0, result, padStart + padLen, FooterSize);

            // 9. 更新全局头部的模块2大小 at 0x14~0x17
            WriteInt32LE(result, GlobalModule2SizeOffset, module2Size);

            return result;
        }

        private static void WriteInt32LE(byte[] buffer, int offset, int value)
        {
            buffer[offset] = (byte)(value & 0xFF);
            buffer[offset + 1] = (byte)((value >> 8) & 0xFF);
            buffer[offset + 2] = (byte)((value >> 16) & 0xFF);
            buffer[offset + 3] = (byte)((value >> 24) & 0xFF);
        }
    }

    internal static class GlCoreBinaryEditor
    {
        private const int Module1Offset = 0x1C;
        private const int Module1Size = 0x4C;

        private const int CbSizeOffset = 0x40;

        private const int Module2Offset = 0x68;
        private const int Module2FixedHeaderSize = 0x20; // 0xEC~0x10B = 32字节
        private const int ShaderSizeOffset = 0x84;
        private const int ShaderContentOffset = 0x88;

        private const int FooterSize = 8;
        private const int FooterSrcOffset = 0x9A4;

        private const int GlobalModule2SizeOffset = 0x14;

        /// <summary>
        /// 构建修改后的shader模板
        /// </summary>
        /// <param name="fragmentCbSize">片元CB的新大小</param>
        /// <param name="glslText">第二个模块内的新GLSL文本</param>
        /// <returns>修改后的完整字节数组</returns>
        internal static byte[] BuildTemplate(int fragmentCbSize, string glslText, ShaderTargetPlatform shaderTargetPlatform)
        {
            byte[] src = Resources.Load<TextAsset>($"Templates/{(int)shaderTargetPlatform}/Binary").bytes;

            // 提取末尾固定8字节
            byte[] footer = new byte[FooterSize];
            System.Buffer.BlockCopy(src, FooterSrcOffset, footer, 0, FooterSize);

            // GLSL文本转UTF-8字节
            byte[] glslBytes = Encoding.UTF8.GetBytes(glslText);
            int padLen = (4 - (glslBytes.Length % 4)) % 4;

            // 计算新模块2大小
            int module2Size = Module2FixedHeaderSize + glslBytes.Length + padLen + FooterSize;

            // 构造新文件
            int newSize = Module2Offset + module2Size;
            byte[] result = new byte[newSize];

            // 1. 复制全局头部 + 模块1开头 (0x00~0x3F)
            System.Buffer.BlockCopy(src, 0, result, 0, 0x40);

            // 2. 写片元CB大小 at 0x40~0x43
            WriteInt32LE(result, CbSizeOffset, fragmentCbSize);

            // 3. 复制模块1剩余部分 (0x44~0xEB)
            int module1TailSize = (Module1Offset + Module1Size) - 0x44; // 0xEC - 0x44 = 0xA8
            System.Buffer.BlockCopy(src, 0x44, result, 0x44, module1TailSize);

            // 4. 复制模块2头部 (0xEC~0x107)，不包括shader size字段
            int module2HeadSize = ShaderSizeOffset - Module2Offset; // 0x1C
            System.Buffer.BlockCopy(src, Module2Offset, result, Module2Offset, module2HeadSize);

            // 5. 写shader size at 0x108~0x10B
            WriteInt32LE(result, ShaderSizeOffset, glslBytes.Length);

            // 6. 写GLSL内容 at 0x10C
            System.Buffer.BlockCopy(glslBytes, 0, result, ShaderContentOffset, glslBytes.Length);

            // 7. 写pad对齐
            int padStart = ShaderContentOffset + glslBytes.Length;
            for (int i = 0; i < padLen; i++)
                result[padStart + i] = 0;

            // 8. 写footer
            System.Buffer.BlockCopy(footer, 0, result, padStart + padLen, FooterSize);

            // 9. 更新全局头部的模块2大小 at 0x14~0x17
            WriteInt32LE(result, GlobalModule2SizeOffset, module2Size);

            return result;
        }

        private static void WriteInt32LE(byte[] buffer, int offset, int value)
        {
            buffer[offset] = (byte)(value & 0xFF);
            buffer[offset + 1] = (byte)((value >> 8) & 0xFF);
            buffer[offset + 2] = (byte)((value >> 16) & 0xFF);
            buffer[offset + 3] = (byte)((value >> 24) & 0xFF);
        }
    }
}