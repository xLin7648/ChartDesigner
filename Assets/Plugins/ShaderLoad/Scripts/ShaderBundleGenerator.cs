using AssetsTools.NET;
using AssetsTools.NET.Extra;
using Codice.Client.BaseCommands;
using LZ4ps;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using ShaderLoad.Util;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;

namespace ShaderLoad
{
    public readonly struct UniformInfo
    {
        public string Type { get; }
        public string Name { get; }
        public int Size => 16;  // 或者保留 switch 以备扩展

        internal UniformInfo(string type, string name)
        {
            Type = type;
            Name = name;
        }
    }

    public readonly struct ShaderParseResult
    {
        public IReadOnlyList<UniformInfo> Uniforms { get; }
        public IReadOnlyList<UniformInfo> SortedUniforms { get; }
        public IReadOnlyList<string> Errors { get; }
        public string ShaderText { get; }
        public byte[] ShaderBytes { get; }

        public bool IsValid => Errors != null && Errors.Count == 0;

        public static ShaderParseResult Empty { get; } = new(
            Array.Empty<UniformInfo>(),
            Array.Empty<UniformInfo>(),
            Array.Empty<string>(),
            string.Empty
        );

        public ShaderParseResult(
            IReadOnlyList<UniformInfo> uniforms, 
            IReadOnlyList<UniformInfo> sortedUniforms, 
            IReadOnlyList<string> errors, 
            string shaderText = null,
            byte[] shaderBytes = null
        )
        {
            Uniforms = uniforms ?? Array.Empty<UniformInfo>();
            SortedUniforms = sortedUniforms ?? Array.Empty<UniformInfo>();
            Errors = errors ?? Array.Empty<string>();

            if (errors.Count > 0)
            {
                ShaderText = string.Empty;
                ShaderBytes = shaderBytes;
            }
            else
            {
                ShaderText = shaderText ?? string.Empty;
                ShaderBytes = shaderBytes ?? new byte[0];
            }
        }
    }

    public readonly struct ShaderBundle
    {
        public string Name { get; }
        public Shader Shader { get; }
        public ShaderParseResult ParseResult { get; }
        private readonly AssetBundle m_bundle;

        public ShaderBundle(string name, AssetBundle bundle, ShaderParseResult parseResult)
        {
            if (bundle == null)
                throw new ArgumentNullException(nameof(bundle), "AssetBundle cannot be null");

            if (string.IsNullOrEmpty(name))
                throw new ArgumentException("Shader name cannot be null or empty", nameof(name));

            m_bundle = bundle;
            Name = name;
            ParseResult = parseResult;
            Shader = bundle.LoadAsset<Shader>(name);

            if (Shader == null)
            {
                throw new InvalidOperationException($"Failed to load shader '{name}' from bundle");
            }
        }

        public void Unload()
        {
            if (Shader != null)
            {
                UnityEngine.Object.DestroyImmediate(Shader, true);
            }

            if (m_bundle != null)
            {
                m_bundle.Unload(true);
            }
        }
    }

    public class ShaderBundleGenerator : MonoBehaviour
    {
        public static ShaderBundleGenerator Ins { get; private set; }

        // 这两个值可以通过打一个基本包然后读出来
        private const uint AssetsFileVersion = 22; // AssetsFile 版本需 >= 16 才能支持类型树填充
        private const uint BundleVersion = 8;
        private const string ClassDataPath = "classdata";
        private const string ABTemplatePath = "Templates/ABTemplate";

        private readonly AssetsManager Manager = new();
        private ClassDatabaseFile ClassDB;

        private readonly HashSet<int> GpuProgramIDs = new();

        private string UnityVersion => Application.unityVersion;

        private void Awake()
        {
            Ins = this;

            var classDbFileBytes = Resources.Load<TextAsset>(ClassDataPath).bytes;
            using var stream = new MemoryStream(classDbFileBytes, writable: false);
            Manager.LoadClassPackage(stream);
            Manager.LoadClassDatabaseFromPackage(UnityVersion);
            ClassDB = Manager.ClassDatabase;
            if (ClassDB == null)
            {
                Debug.LogError("失败！无法加载类数据库。");
                this.enabled = false;
            }
        }

        public ShaderBundle? Create(
            string glsl,
            string bundleName, 
            string shaderName
        )
        {
            var parseResult = GlslParser.Parse(glsl, GetGraphicsApiPlatform());
            if (!parseResult.IsValid)
            {
                foreach (var err in parseResult.Errors)
                {
                    Debug.LogError(err);
                }
                return null;
            }

            bundleName = bundleName.ToLower();
            shaderName = shaderName.ToLower();

            var savePath = Application.persistentDataPath;
            Create_Internal(bundleName, shaderName, savePath, parseResult);
            var bundle = AssetBundle.LoadFromFile(Path.Combine(savePath, $"{bundleName}.assets"));

            return new ShaderBundle(shaderName, bundle, parseResult);
            try
            {
                
            }
            catch (Exception ex)
            {
                Debug.LogError(ex.Message);
            }

            return null;
        }

        private void Create_Internal(
            string bundleName, 
            string shaderName, 
            string savePath,
            ShaderParseResult parseResult
        )
        {
            if (ClassDB == null) return;

            var targetPlatform = GetUnityTargetPlatform();
            var gpuProgramID = 0;

            for (int i = 0, id = 0; i < 200; i++)
            {
                id = UnityEngine.Random.Range(30000, 60000);
                if (GpuProgramIDs.Add(id))
                {
                    gpuProgramID = id;
                    break;
                }
            }
            if (gpuProgramID == 0)
            {
                throw new Exception("失败！GpuProgramID 获取异常！");
            }

            // 创建中层：AssetsFile（序列化的 Unity 资产文件）
            var assetsFile = new AssetsFile
            {
                Header = new AssetsFileHeader
                {
                    MetadataSize = 0,
                    FileSize = 0,
                    Version = AssetsFileVersion,
                    DataOffset = 0,
                    Endianness = false
                },
                Metadata = new AssetsFileMetadata
                {
                    UnityVersion = UnityVersion,
                    TargetPlatform = (uint)targetPlatform,
                    TypeTreeEnabled = true,
                    TypeTreeTypes = new(),
                    AssetInfos = new List<AssetFileInfo>(),
                    ScriptTypes = new(),
                    Externals = new(),
                    RefTypes = new(),
                    UserInformation = ""
                }
            };

            // 创建内层对象

            // ab 对象
            string abTemplateText = Resources.Load<TextAsset>(ABTemplatePath).text;
            if (string.IsNullOrEmpty(abTemplateText))
            {
                Debug.LogError("失败！AB模板参数加载异常。");
                this.enabled = false;
                return;
            }
            abTemplateText = abTemplateText
                .Replace("__AssetsBundleFullName__", $"{bundleName}.assets")
                .Replace("__AssetsBundleName__", bundleName!)
                .Replace("__PATH__", $"assets/{shaderName}.Shader");

            AddAssetsObjectFromJson(abTemplateText, assetsFile, AssetClassID.AssetBundle, 1);

            // shader 对象
            var gfxApiPlatform = GetGraphicsApiPlatform();
            byte[] compressedBlob = null;

            string shaderConfigTemplateText = Resources.Load<TextAsset>($"Templates/{(int)gfxApiPlatform}/ConfigTemplate").text;
            if (string.IsNullOrEmpty(shaderConfigTemplateText))
            {
                Debug.LogError("失败！Shader模板参数加载异常。");
                this.enabled = false;
                return;
            }

            JObject jobj = (JObject)JsonConvert.DeserializeObject(shaderConfigTemplateText);

            var pass = jobj["m_ParsedForm"]["m_SubShaders"]["Array"][0]["m_Passes"]["Array"][0];
            pass["m_State"]["gpuProgramID"] = gpuProgramID;

            jobj["m_ParsedForm"]["m_Name"] = shaderName;
            jobj["platforms"]["Array"][0] = pass["m_Platforms"]["Array"][0] = (int)gfxApiPlatform;

            var properties = jobj["m_ParsedForm"]["m_PropInfo"]["m_Props"]["Array"] as JArray;
            foreach (var uniform in parseResult.Uniforms)
            {
                if (uniform.Name == "_Time") continue;
                if (uniform.Name == "_ScreenSize") continue;

                var propertieTmp = (JObject)JsonConvert.DeserializeObject(ShaderJsonDatas.PropertieDatas);
                propertieTmp["m_Name"] = uniform.Name;
                propertieTmp["m_Description"] = $"{uniform.Name}_D";
                propertieTmp["m_Type"] = uniform.Type switch
                {
                    "float" => 2,
                    "vec2" or "vec3" or "vec4" => 1,
                    _ => 0
                };

                properties.Add(propertieTmp);
            }

            var uniformCount = parseResult.SortedUniforms.Count;
            var names = pass["m_NameIndices"]["Array"] as JArray;

            if (gfxApiPlatform == ShaderTargetPlatform.Vulkan)
            {
                {
                    // var max = 0;

                    for (int i = 0; i < uniformCount; i++)
                    {
                        var uniform = parseResult.SortedUniforms[i];
                        names.Add(JToken.FromObject(new
                        {
                            first = uniform.Name,
                            second = i + 3,
                        }));
                    }

                    var c = uniformCount + 3 - 1;
                    names.Add(JToken.FromObject(new
                    {
                        first = "unity_MatrixVP",
                        second = c + 2,
                    }));

                    names.Add(JToken.FromObject(new
                    {
                        first = "unity_ObjectToWorld",
                        second = c + 3,
                    }));

                    names.Insert(3, JToken.FromObject(new
                    {
                        first = "_MainTex_ST",
                        second = c + 1,
                    }));
                }

                var namesDic = names.ToDictionary(
                    x => x["first"].ToString(),
                    x => x["second"]
                );

                var uniformSize = parseResult.SortedUniforms.Sum(x => x.Type switch
                {
                    "float" => 4,
                    "vec2" or "vec3" or "vec4" => 16,
                    _ => 0
                });

                {
                    var buffer = pass["progVertex"]["m_CommonParameters"]["m_ConstantBuffers"]["Array"][0];
                    buffer["m_Size"] = uniformSize;

                    var parmas = buffer["m_VectorParams"]["Array"] as JArray;
                    parmas.Clear();

                    var idx = 0;
                    for (int i = 0; i < uniformCount; i++)
                    {
                        var uniform = parseResult.SortedUniforms[i];
                        parmas.Add(JToken.FromObject(new
                        {
                            m_NameIndex = i + 3,
                            m_Index = idx,
                            m_ArraySize = 0,
                            m_Type = 0,
                            m_Dim = uniform.Type switch
                            {
                                "float" => 1,
                                "vec2" or "vec3" or "vec4" => 4,
                                _ => 0
                            },
                        }));

                        idx += uniform.Type switch
                        {
                            "float" => 4,
                            "vec2" or "vec3" or "vec4" => 16,
                            _ => 0
                        };
                    }
                }

                {
                    var buffer = pass["progVertex"]["m_CommonParameters"]["m_ConstantBuffers"]["Array"][1];

                    {
                        buffer["m_MatrixParams"]["Array"][0]["m_NameIndex"] = namesDic["unity_MatrixVP"];
                        buffer["m_MatrixParams"]["Array"][1]["m_NameIndex"] = namesDic["unity_ObjectToWorld"];
                        buffer["m_VectorParams"]["Array"][0]["m_NameIndex"] = namesDic["_MainTex_ST"];
                    }
                }

                // pass["progVertex"]["m_CommonParameters"]["m_ConstantBufferBindings"]["Array"][1]["m_Index"] = 134283265;

                compressedBlob = VulkanBinaryEditor.BuildTemplate(uniformSize, parseResult.ShaderBytes, gfxApiPlatform);
            }
            else if (gfxApiPlatform == ShaderTargetPlatform.OpenGLCore)
            {
                var uniformSize = parseResult.SortedUniforms.Sum(x => x.Size);
                {
                    var max = 0;

                    for (int i = 0; i < uniformCount; i++)
                    {
                        var uniform = parseResult.SortedUniforms[i];
                        names.Add(JToken.FromObject(new
                        {
                            first = uniform.Name,
                            second = max = i + 2,
                        }));
                    }

                    names.Add(JToken.FromObject(new
                    {
                        first = "unity_MatrixVP",
                        second = max + 3,
                    }));

                    names.Add(JToken.FromObject(new
                    {
                        first = "unity_ObjectToWorld",
                        second = max + 2,
                    }));

                    names.Insert(2, JToken.FromObject(new
                    {
                        first = "_MainTex_ST",
                        second = max + 1,
                    }));
                }

                var namesDic = names.ToDictionary(
                    x => x["first"].ToString(),
                    x => x["second"]
                );

                {
                    var buffer = pass["progVertex"]["m_CommonParameters"]["m_ConstantBuffers"]["Array"][0];
                    buffer["m_Size"] = uniformSize;

                    var parmas = buffer["m_VectorParams"]["Array"] as JArray;
                    parmas.Clear();

                    for (int i = 0; i < uniformCount; i++)
                    {
                        var uniform = parseResult.SortedUniforms[i];
                        parmas.Add(JToken.FromObject(new
                        {
                            m_NameIndex = i + 2,
                            m_Index = i * 16,
                            m_ArraySize = 0,
                            m_Type = 0,
                            m_Dim = uniform.Type switch
                            {
                                "float" => 1,
                                "vec2" or "vec3" or "vec4" => 4,
                                _ => 0
                            },
                        }));
                    }
                }

                {
                    var buffer = pass["progVertex"]["m_CommonParameters"]["m_ConstantBuffers"]["Array"][1];

                    {
                        buffer["m_MatrixParams"]["Array"][0]["m_NameIndex"] = namesDic["unity_ObjectToWorld"];
                        buffer["m_MatrixParams"]["Array"][1]["m_NameIndex"] = namesDic["unity_MatrixVP"];
                        buffer["m_VectorParams"]["Array"][0]["m_NameIndex"] = namesDic["_MainTex_ST"];
                    }
                }

                compressedBlob = GlCoreBinaryEditor.BuildTemplate(uniformSize, parseResult.ShaderText, gfxApiPlatform);
            }
            else if (gfxApiPlatform == ShaderTargetPlatform.OpenGLES3)
            {
                var uniformSize = parseResult.SortedUniforms.Sum(x => x.Size);
                for (int i = 0; i < uniformCount; i++)
                {
                    var uniform = parseResult.SortedUniforms[i];
                    names.Add(JToken.FromObject(new
                    {
                        first = uniform.Name,
                        second = i + 2,
                    }));
                }

                var buffer = pass["progVertex"]["m_CommonParameters"]["m_ConstantBuffers"]["Array"][0];
                buffer["m_Size"] = uniformSize;

                var parmas = buffer["m_VectorParams"]["Array"] as JArray;
                parmas.Clear();

                for (int i = 0; i < uniformCount; i++)
                {
                    var uniform = parseResult.SortedUniforms[i];
                    parmas.Add(JToken.FromObject(new
                    {
                        m_NameIndex = i + 2,
                        m_Index = i * 16,
                        m_ArraySize = 0,
                        m_Type = 0,
                        m_Dim = uniform.Type switch
                        {
                            "float" => 1,
                            "vec2" or "vec3" or "vec4" => 4,
                            _ => 0
                        },
                    }));
                }

                compressedBlob = Gles3BinaryEditor.BuildTemplate(uniformSize, parseResult.ShaderText, gfxApiPlatform);
            }

            // LZ4 压缩 shader blob（对应 Python compress_shader.py）
            int decompLen = compressedBlob.Length;
            byte[] compressed = LZ4Codec.Encode64HC(compressedBlob, 0, decompLen);
            int compLen = compressed.Length;

            // 更新 compressedBlob
            jobj["compressedBlob"]["Array"] = new JArray(compressed.Select(b => (int)b));

            // 更新 offsets / compressedLengths / decompressedLengths
            var stageCounts = (JArray)jobj["stageCounts"]["Array"];
            var offsets = (JArray)jobj["offsets"]["Array"];
            var compressedLengths = (JArray)jobj["compressedLengths"]["Array"];
            var decompressedLengths = (JArray)jobj["decompressedLengths"]["Array"];

            int cumulativeOffset = 0;
            for (int i = 0; i < stageCounts.Count; i++)
            {
                int nStages = (int)stageCounts[i];

                var offsetArr = (JArray)offsets[i]["Array"];
                var compLenArr = (JArray)compressedLengths[i]["Array"];
                var decompLenArr = (JArray)decompressedLengths[i]["Array"];

                offsetArr.Clear();
                compLenArr.Clear();
                decompLenArr.Clear();

                for (int j = 0; j < nStages; j++)
                {
                    offsetArr.Add(cumulativeOffset);
                    compLenArr.Add(compLen);
                    decompLenArr.Add(decompLen);
                    cumulativeOffset += compLen;
                }
            }

            AddAssetsObjectFromJson(JsonConvert.SerializeObject(jobj), assetsFile, AssetClassID.Shader, 2);

            // 创建最外层：Bundle
            Console.Write("正在创建外层结构（AssetBundle）...");
            var bundle = new AssetBundleFile
            {
                Header = new AssetBundleHeader
                {
                    Signature = "UnityFS",
                    Version = BundleVersion,
                    GenerationVersion = "5.x.x",
                    EngineVersion = UnityVersion,
                    FileStreamHeader = new AssetBundleFSHeader
                    {
                        TotalFileSize = 0,
                        CompressedSize = 0,
                        DecompressedSize = 0,
                        Flags = AssetBundleFSHeaderFlags.HasDirectoryInfo
                    }
                },
                BlockAndDirInfo = new AssetBundleBlockAndDirInfo
                {
                    Hash = new AssetsTools.NET.Hash128(),
                    BlockInfos = new[] 
                    {
                        new AssetBundleBlockInfo
                        {
                            CompressedSize = 0,
                            DecompressedSize = 0,
                            Flags = 0x40
                        }
                    },
                    DirectoryInfos = new() 
                    {
                        AssetBundleDirectoryInfo.Create("0", isSerialized: true)
                    }
                },
                DataIsCompressed = false
            };

            // 将 AssetsFile 设置到目录条目的 Replacer
            bundle.BlockAndDirInfo.DirectoryInfos[0].SetNewData(assetsFile);

            using var fs = new FileStream(Path.Combine(savePath, $"{bundleName}.assets"), FileMode.Create);
            using var writer = new AssetsFileWriter(fs);
            bundle.Write(writer);
        }

        private UnityTargetPlatform GetUnityTargetPlatform() => Application.platform switch
        {
            RuntimePlatform.Android => UnityTargetPlatform.Android,
            RuntimePlatform.IPhonePlayer => UnityTargetPlatform.iOS,
            // RuntimePlatform.WebGLPlayer => UnityTargetPlatform.WebGL,
            RuntimePlatform.WindowsPlayer or RuntimePlatform.WindowsEditor => UnityTargetPlatform.StandaloneWindows64,
            _ => throw new Exception("不支持的平台。")
        };

        private ShaderTargetPlatform GetGraphicsApiPlatform() => SystemInfo.graphicsDeviceType switch
        {
            GraphicsDeviceType.Vulkan => ShaderTargetPlatform.Vulkan,
            GraphicsDeviceType.OpenGLES3 => ShaderTargetPlatform.OpenGLES3,
            GraphicsDeviceType.OpenGLCore => ShaderTargetPlatform.OpenGLCore,
            _ => throw new Exception("不支持的平台。")
        };

        private void AddAssetsObjectFromJson(string json, AssetsFile assetsFile, AssetClassID classID, int pathID)
        {
            var (assetBundleInfo, ttType) = CreateDefaultAssetsObject(assetsFile, classID, pathID);

            var template = new AssetTypeTemplateField();
            template.FromTypeTree(ttType);

            var objectData = JsonAssetImporter.Import(template, json);

            assetBundleInfo.SetNewData(objectData);
            assetsFile.Metadata.AddAssetInfo(assetBundleInfo);
        }

        private (AssetFileInfo assetBundleInfo, TypeTreeType ttType) CreateDefaultAssetsObject(
            AssetsFile assetsFile, AssetClassID classID, int pathID)
        {
            var assetBundleInfo = AssetFileInfo.Create(assetsFile, pathID, (int)classID, ClassDB)
                ?? throw new Exception($"失败！无法创建 {classID} 类型的资源信息。");

            // 从类型树构建模板
            var ttType = assetsFile.Metadata.FindTypeTreeTypeByID((int)classID)
                ?? throw new Exception($"失败！未找到 {classID} 的类型树定义。");

            return (assetBundleInfo, ttType);
        }
    }
}