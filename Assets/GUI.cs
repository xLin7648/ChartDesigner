using ShaderLoader;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GUI : MonoBehaviour
{
    public Camera cam;
    public TMP_InputField Input_ShaderName;
    public TMP_InputField Input_Shader;
    public Button Btn_Run;
    public Button Btn_Clean;

    public ShaderBundleGenerator generator;
    public PostProcessManager postProcessManager;

    private ShaderBundle? shaderAB;
    private PostProcessEffect postProcessEffect;

    private void OnEnable()
    {
        Btn_Run.onClick.AddListener(OnBtn_RunClick);
        Btn_Clean.onClick.AddListener(OnBtn_CleanClick);
    }

    private void OnDisable()
    {
        Btn_Run.onClick.RemoveListener(OnBtn_RunClick);
        Btn_Clean.onClick.RemoveListener(OnBtn_CleanClick);
    }

    private void OnDestroy()
    {
        Cleanup();
    }

    private void Cleanup()
    {
        if (shaderAB.HasValue)
        {
            postProcessManager.RemoveEffect(postProcessEffect);
            postProcessEffect = null;

            shaderAB.Value.Unload();
            shaderAB = null;
        }
    }

    private void OnBtn_RunClick()
    {
        StartCoroutine(nameof(Run));
    }

    private IEnumerator Run()
    {
        if (string.IsNullOrEmpty(Input_Shader.text) || string.IsNullOrEmpty(Input_ShaderName.text))
        {
            yield break;
        }

        Cleanup();
        yield return null;

        var name = Input_ShaderName.text.ToLower();

        shaderAB = generator.Create(Input_Shader.text, name, name);
        if (shaderAB.HasValue && shaderAB.Value.TryGetShader(name, out var shader, out var parseResult))
        {
            postProcessEffect = postProcessManager.AddEffect(shader, parseResult.Uniforms);
            postProcessEffect.SetFloat("sampleCount", 3);
            postProcessEffect.SetFloat("power", 0.03F);
            postProcessEffect.IsActive = true;
            //p.SetFloat("size", 10);
        }
    }

    private void OnBtn_CleanClick()
    {
        Cleanup();
    }
}
