using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(TMP_Dropdown))]
public class GameProportionSetter : MonoBehaviour
{
    [Serializable]
    private class ProportionConfig
    {
        public uint w;
        public uint h;
    }

    [SerializeField] private Camera Camera_Game;
    [SerializeField] private RawImage RawImage_GameView;
    [SerializeField] private AspectRatioFitter Arf;
    [SerializeField] private AspectRatioFitter Arf2;

    [SerializeField] private List<ProportionConfig> ProportionConfigs;
    private TMP_Dropdown m_Dropdown;

    private void Reset()
    {
        m_Dropdown = GetComponent<TMP_Dropdown>();
    }

    private void Awake()
    {
        m_Dropdown = GetComponent<TMP_Dropdown>();

        if (
            Camera_Game == null || m_Dropdown == null || 
            Arf == null || Arf2 == null || RawImage_GameView == null
        )
        {
            enabled = false;
            return;
        }

        ProportionConfigs.RemoveAll(x => x == null || (x.w == 0 && x.h == 0));

        if (!ProportionConfigs.IsEmpty())
        {
            m_Dropdown.ClearOptions();

            var options = ProportionConfigs
                .Select(x => $"{x.w}:{x.h}")
                .ToList();

            m_Dropdown.AddOptions(options);
            OnDropDownValueChanged(0);
        }
    }

    private void OnEnable()
    {
        m_Dropdown.onValueChanged.AddListener(OnDropDownValueChanged);
    }

    private void OnDisable()
    {
        m_Dropdown.onValueChanged.RemoveListener(OnDropDownValueChanged);
    }

    private void OnDropDownValueChanged(int value)
    {
        var cfg = ProportionConfigs.Get(value);
        if (cfg == null) return;
        if (
            Camera_Game == null || m_Dropdown == null ||
            Arf == null || Arf2 == null || RawImage_GameView == null
        )
        {
            enabled = false;
            return;
        }

        var rt = Camera_Game.targetTexture;
        if (rt != null) rt.Release();

        if (
            ResolutionMatcher.Match(
                Screen.width, Screen.height, cfg.w, cfg.h,
                out var resultWidth, out var resultHeight
            )
        )
        {
            Debug.Log($"Width: {resultWidth}, Height: {resultHeight}");
            rt = new RenderTexture(
                resultWidth, resultHeight,
                32, RenderTextureFormat.ARGB32
            )
            {
                antiAliasing = 4
            };
            RawImage_GameView.texture =
                Camera_Game.targetTexture = rt;

            Camera_Game.Render();
            Arf.aspectRatio = Arf2.aspectRatio = (float)cfg.w / cfg.h;

            UIEventControl.DispensEvent(UIEventEnum.GameAreaChanged);
        }
        
    }
}