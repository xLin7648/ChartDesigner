using ShaderLoad.Util;
using System;
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

    public static class VulkanBinaryEditor
    {
        // 已知常量
        private const int OldFragmentSmolvSize = 0x0263;
        private const int FragmentStart = 0x0138;
        private const int VertexStart = 0x039B;
        private const int VertexSmolvSize = 0x02CF;
        private const int TrailerDataStart = 0x066C; // trailer实际数据起始 (不包含padding)
        private const int OriginalModule2Size = 0x061C;
        private const int OriginalTotalHeadSmolv = 0x05E2;

        /// <summary>
        /// 修改shader二进制数据
        /// </summary>
        /// <param name="fragmentCbSize">PGlobals新的cbSize值</param>
        /// <param name="newFragmentSmolv">新的片元着色器smolv数据</param>
        /// <param name="originalData">原始二进制文件byte[]</param>
        /// <returns>修改后的byte[]</returns>
        internal static byte[] BuildTemplate(
            int fragmentCbSize,
            byte[] newFragmentSmolv,
            ShaderTargetPlatform shaderTargetPlatform
        )
        {
            byte[] src = Resources.Load<TextAsset>($"Templates/{(int)shaderTargetPlatform}/Binary").bytes;

            int newFragSize = newFragmentSmolv.Length;

            // 顶点着色器结束位置 (smolv数据结尾)
            int newVertexEnd = FragmentStart + newFragSize + VertexSmolvSize;
            // trailer数据需要4字节对齐
            int newTrailerDataStart = (newVertexEnd + 3) & ~3;
            int trailerDataLen = src.Length - TrailerDataStart; // 原始trailer数据长度

            int newTotalSize = newTrailerDataStart + trailerDataLen;

            byte[] result = new byte[newTotalSize];

            // ---- 数据搬移 ----

            // 1. 复制片元着色器之前的所有数据 (0x0000~0x0137)
            Buffer.BlockCopy(src, 0, result, 0, FragmentStart);

            // 2. 插入新的片元着色器smolv
            Buffer.BlockCopy(newFragmentSmolv, 0, result, FragmentStart, newFragSize);

            // 3. 复制顶点着色器smolv
            int newVertexStart = FragmentStart + newFragSize;
            Buffer.BlockCopy(src, VertexStart, result, newVertexStart, VertexSmolvSize);

            // 4. 复制trailer数据 (对齐到4字节, padding用0填充)
            Buffer.BlockCopy(src, TrailerDataStart, result, newTrailerDataStart, trailerDataLen);

            // ---- 修改字段数值 ----

            // 模块2总尺寸 0x0014~0x0017
            WriteInt32LE(result, 0x14, newTotalSize - 0x68);

            // PGlobals cbSize 0x0040~0x0043
            WriteInt32LE(result, 0x40, fragmentCbSize);

            // head+双smolv总尺寸 0x0084~0x0087 = 从0x88到vertex smolv末尾
            WriteInt32LE(result, 0x84, newVertexEnd - 0x88);

            // Entry 1 (顶点着色器): 0x8C~0x8F = 偏移, 0x90~0x93 = 大小 (不变)
            int newEntry1Offset = 0x00B0 + newFragSize; // 相对0x88
            WriteInt32LE(result, 0x8C, newEntry1Offset);
            // 0x90~0x93 大小保持0x02CF不变, 已从原始数据中复制

            // Entry 2 (片元着色器): 0x94~0x97 = 偏移 (不变), 0x98~0x9B = 大小
            // 0x94~0x97 偏移保持0x00B0不变, 已从原始数据中复制
            WriteInt32LE(result, 0x98, newFragSize);

            return result;
        }

        /// <summary>
        /// 以小端序写入int32
        /// </summary>
        private static void WriteInt32LE(byte[] data, int offset, int value)
        {
            data[offset + 0] = (byte)(value & 0xFF);
            data[offset + 1] = (byte)((value >> 8) & 0xFF);
            data[offset + 2] = (byte)((value >> 16) & 0xFF);
            data[offset + 3] = (byte)((value >> 24) & 0xFF);
        }
    }
}