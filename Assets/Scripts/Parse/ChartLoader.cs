using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using ShaderLoader;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;

public class ChartLoader : MonoBehaviour
{
    public Camera cam;
    public AudioSource au;
    public ShaderBundleGenerator generator;
    public PostProcessManager postProcessManager;
    public Texture2D defaultLineTex2D;

    public LineControl LinePrefab;
    public Transform LineParent;

    private List<Effect> effects = new();
    private readonly List<LineControl> lineControls = new();
    private bool loaded;
    private float startTime;

    public async void Start()
    {
        effects?.Clear();
        lineControls?.Clear();

        var music = Resources.Load<AudioClip>("Chart/Music");
        var chartAsset = Resources.Load<TextAsset>("Chart/Chart");
        var effectJsonStr = Resources.Load<TextAsset>("Chart/Effect").text;

        var ppu = Screen.height / (2f * cam.orthographicSize);
        var lineSprites = Resources.LoadAll<Texture2D>("Chart")
            .Select(x => CreateSpriteFromTexture(x, ppu))
            .ToDictionary(x => x.name, x => x);
        lineSprites.Add("Default", CreateSpriteFromTexture(defaultLineTex2D, ppu));

        var settings = new JsonSerializerSettings();
        settings.Converters.Add(new VarEntryConverter());
        var serializer = JsonSerializer.Create(settings); // 创建序列化器

        foreach (var item in JArray.Parse(effectJsonStr))
        {
            var effect = new Effect
            {
                shader = item["shader"].ToString().ToLower(),
                startTime = item["startTime"].ToObject<float>(),
                endTime = item["endTime"].ToObject<float>(),
                vars = new Dictionary<string, List<VarEntry>>()
            };

            var varsObj = (JObject)item["vars"];
            foreach (var prop in varsObj.Properties())
            {
                string varName = prop.Name;
                JToken varValue = prop.Value;

                try
                {
                    var entrys = varValue.ToObject<List<VarEntry>>(serializer);
                    effect.vars.Add(varName, entrys);
                }
                catch
                {
                    var entry = varValue.ToObject<VarEntry>(serializer);
                    entry.endTime = music.length;
                    effect.vars.Add(varName, new List<VarEntry>() { entry });
                }
            }

            effects.Add(effect);
        }

        var shaderConfigs = effects
            .GroupBy(x => x.shader)
            .ToDictionary(
                x => x.Key,
                x => ShaderYamlConfigLoader.LoadFromText(Resources.Load<TextAsset>($"Shader/{x.Key}").text)
            );

        var shaderbundle = generator.Creates(
            chartAsset.name,
            shaderConfigs
                .ToDictionary(
                    x => x.Key,
                    x => x.Value.glsl
                )
        );

        foreach (var effect in effects)
        {
            if (
                shaderbundle.HasValue &&
                shaderbundle.Value.TryGetShader(effect.shader, out var shader, out var shaderParseResult)
            )
            {
                effect.shaderUniforms = shaderParseResult.Uniforms;
                effect.postProcess = postProcessManager.AddEffect(shader, shaderParseResult.Uniforms);

                if (effect.postProcess != null)
                {
                    effect.postProcess.IsActive = false;
                }
            }

            if (shaderConfigs.TryGetValue(effect.shader, out var shaderConfig))
            {
                effect.defaultVars = shaderConfig.defaultVars;
            }
        }

        var chart = JsonConvert.DeserializeObject<ImportChart>(chartAsset.text);

        foreach (var line in chart.judgelines)
        {
            var textureID = Regex.Replace(line.texture ?? "Default", @"\.[^.]*$", "");

            if (lineSprites.TryGetValue(textureID, out var lineSprite))
            {
                line.sprite = lineSprite;
            }

            var newLine = Instantiate(LinePrefab, LineParent);
            newLine.Bind(line, line.texture == null);

            lineControls.Add(newLine);
        }

        au.mute = true;
        au.clip = music;
        au.Play();

        await UniTask.Delay(3);
        au.time = 0;
        au.mute = false;
        au.Play();

        loaded = true;
        startTime = Time.time;
    }

    private void Update()
    {
        if (!loaded) return;

        var time = Time.time - startTime;
        foreach (var line in lineControls)
        {
            line.Tick(time);
        }

        foreach(var eff in effects)
        {
            if (eff == null || eff.postProcess == null) continue;

            if (time >= eff.startTime && time <= eff.endTime)
            {
                eff.postProcess.IsActive = true;

                foreach (var uniform in eff.shaderUniforms)
                {
                    var name = uniform.Name;
                    if (name is "time")
                    {
                        eff.postProcess.SetFloat(name, time);
                        continue;
                    }

                    if (eff.vars != null && eff.vars.TryGetValue(name, out var events))
                    {
                        for (int i = 0, entLen = events.Count; i < entLen; i++)
                        {
                            var ent = events[i];
                            if (ent.endTime < time) continue;
                            if (ent.startTime > time) break;
                            if (ent.IsConstant)
                            {
                                if (uniform.Type is "float")
                                {
                                    eff.postProcess.SetFloat(name, ent.start.x);
                                }
                                else
                                {
                                    eff.postProcess.SetVector(name, ent.start);
                                }
                                continue;
                            }

                            var timePercentEnd = (time - ent.startTime) / (ent.endTime - ent.startTime);
                            var timePercentStart = 1 - timePercentEnd;

                            if (uniform.Type is "float")
                            {
                                eff.postProcess.SetFloat(name, ent.start.x * timePercentStart + ent.end.x * timePercentEnd);
                            }
                            else
                            {
                                eff.postProcess.SetVector(name, ent.start * timePercentStart + ent.end * timePercentEnd);
                            }
                        }
                    }
                    else if (eff.defaultVars != null && eff.defaultVars.TryGetValue(name, out var defaultValCfg))
                    {
                        if (defaultValCfg.type is "float")
                        {
                            eff.postProcess.SetFloat(name, defaultValCfg.val.x);
                        }
                        else if (defaultValCfg.type is "int")
                        {
                            eff.postProcess.SetFloat(name, (int)defaultValCfg.val.x);
                        }
                        else
                        {
                            eff.postProcess.SetVector(name, defaultValCfg.val);
                        }
                    }
                }
            }
            else
            {
                eff.postProcess.IsActive = false;
            }
        }
    }

    private Sprite CreateSpriteFromTexture(Texture2D texture, float pixelsPerUnit)
    {
        // 整个纹理的矩形区域（从 (0,0) 到 (width, height)）
        Rect rect = new Rect(0, 0, texture.width, texture.height);
        // 锚点：中心点 (0.5, 0.5)
        Vector2 pivot = new Vector2(0.5f, 0.5f);
        // 创建 Sprite，使用指定的 PPU
        Sprite sprite = Sprite.Create(texture, rect, pivot, pixelsPerUnit);
        sprite.name = texture.name;
        return sprite;
    }
}
