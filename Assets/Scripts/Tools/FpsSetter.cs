using TMPro;
using UnityEngine;

public class FpsSetter : MonoBehaviour
{
    [SerializeField] private TMP_Text Tex_Fps;

    private void Awake()
    {
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = 120;
    }

    private void Update()
    {
        Tex_Fps.text = $"FPS: {1F / Time.deltaTime:F0}";
    }
}
