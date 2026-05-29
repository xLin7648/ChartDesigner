using TMPro;
using UnityEngine;

public class FpsSetter : MonoBehaviour
{
    [SerializeField] private TMP_Text Tex_Fps;

    private void Awake()
    {
        Application.targetFrameRate = -1;
        QualitySettings.vSyncCount = 0;
    }

    private void Update()
    {
        Tex_Fps.text = $"FPS: {1F / Time.deltaTime:F0}";
    }
}
