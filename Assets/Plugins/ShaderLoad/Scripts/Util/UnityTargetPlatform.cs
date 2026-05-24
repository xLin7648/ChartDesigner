namespace ShaderLoad.Util
{
    /// <summary>
    /// Unity 目标平台枚举，对应 AssetsFileMetadata.TargetPlatform 的值。
    /// 值参考 UABEANext4.Logic.AssetInfo.BuildTarget。
    /// </summary>
    internal enum UnityTargetPlatform : uint
    {
        iOS = 9,
        Android = 13,
        StandaloneWindows64 = 19,
        // WebGL = 20,  经测试，webgl无法使用，需特殊适配，但懒得适配
    }
}