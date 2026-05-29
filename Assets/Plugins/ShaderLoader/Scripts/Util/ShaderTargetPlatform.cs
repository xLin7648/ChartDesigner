namespace ShaderLoader.Util
{
    /// <summary>
    /// Shader 目标平台枚举
    /// </summary>
    internal enum ShaderTargetPlatform : uint
    {
        Metal = 14,
        Vulkan = 18,
        OpenGLES3 = 9,
        OpenGLCore = 15,
        // WebGL = 9    经测试，webgl无法使用，需特殊适配，但懒得适配
    }
}