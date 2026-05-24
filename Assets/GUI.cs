using ShaderLoad;
using ShaderLoad.PostProcess;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GUI : MonoBehaviour
{
    public TMP_InputField Input_ShaderName;
    public TMP_InputField Input_Shader;
    public Button Btn_Run;

    public ShaderBundleGenerator generator;
    public PostProcessManager postProcessManager;

    private ShaderBundle? shaderAB;
    private PostProcessEffect postProcessEffect;

    private void OnEnable()
    {
        Btn_Run.onClick.AddListener(OnBtn_RunClick);
    }

    private void OnDisable()
    {
        Btn_Run.onClick.RemoveListener(OnBtn_RunClick);
    }

    private void OnDestroy()
    {
        Cleanup();
    }

    private void Cleanup()
    {
        if (shaderAB.HasValue)
        {
            shaderAB.Value.Unload();
            shaderAB = null;

            postProcessManager.RemoveEffect(postProcessEffect);
            postProcessEffect = null;
        }
    }

    private void OnBtn_RunClick()
    {
        if (string.IsNullOrEmpty(Input_Shader.text) || string.IsNullOrEmpty(Input_ShaderName.text))
        {
            return;
        }

        Cleanup();

        var name = Input_ShaderName.text.ToLower();

        shaderAB = generator.Create(Input_Shader.text, name, name);
        if (shaderAB.HasValue)
        {
            postProcessEffect = postProcessManager.AddEffect(shaderAB.Value.Shader, shaderAB.Value.ParseResult.Uniforms);
            postProcessEffect.SetFloat("sampleCount", 3);
            postProcessEffect.SetFloat("power", 0.03F);
            //p.SetFloat("size", 10);
        }
    }
}
