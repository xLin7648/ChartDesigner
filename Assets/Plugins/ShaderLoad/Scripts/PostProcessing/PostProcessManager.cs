using UnityEngine;
using System.Collections.Generic;

namespace ShaderLoad.PostProcess
{
    /// <summary>
    /// 在单个 Camera 上串联多个后处理效果。
    /// 挂载到 Camera GameObject 上，效果通过 effects 列表以代码方式注册。
    ///
    /// 性能要点：
    /// - 中间 RT 预分配，不每帧 GetTemporary
    /// - 活动效果列表复用，不每帧 new List
    /// - 分辨率变化时自动重建中间 RT
    /// </summary>
    [RequireComponent(typeof(Camera))]
    public class PostProcessManager : MonoBehaviour
    {
        [Tooltip("启用后处理链。")]
        public bool enablePostProcessing = true;

        private readonly List<PostProcessEffect> effects = new();

        // 复用列表，避免每帧分配
        private readonly List<PostProcessEffect> _activeEffects = new();

        private Camera _camera;

        // 预分配的中间 RT（最多需要 2 个做乒乓）
        private RenderTexture _buffer1;
        private RenderTexture _buffer2;

        private void Awake()
        {
            _camera = GetComponent<Camera>();
        }

        private void OnEnable()
        {
            UpdateBuffers();
        }

        private void OnDisable()
        {
            ReleaseBuffers();
        }

        private void OnDestroy()
        {
            foreach (var effect in effects)
            {
                effect.Cleanup();
            }
            effects.Clear();
            _activeEffects.Clear();
        }

        public PostProcessEffect AddEffect(Shader shader, IReadOnlyList<UniformInfo> uniforms)
        {
            var newEffect = new PostProcessEffect(shader, uniforms);
            effects.Add(newEffect);
            return newEffect;
        }

        public bool RemoveEffect(PostProcessEffect postProcessEffect)
        {
            return effects.Remove(postProcessEffect);
        }

        private void UpdateBuffers()
        {
            int w = _camera.pixelWidth;
            int h = _camera.pixelHeight;
            if (w <= 0 || h <= 0) return;

            RenderTextureFormat fmt = RenderTextureFormat.Default;

            if (_buffer1 != null && _buffer2 != null &&
                _buffer1.width == w && _buffer1.height == h && _buffer1.format == fmt)
            {
                return;
            }

            ReleaseBuffers();

            _buffer1 = new RenderTexture(w, h, 0, fmt);
            _buffer1.hideFlags = HideFlags.DontSave;
            _buffer1.Create();

            _buffer2 = new RenderTexture(w, h, 0, fmt);
            _buffer2.hideFlags = HideFlags.DontSave;
            _buffer2.Create();
        }

        private void ReleaseBuffers()
        {
            if (_buffer1 != null)
            {
                _buffer1.Release();
                DestroyImmediate(_buffer1);
                _buffer1 = null;
            }
            if (_buffer2 != null)
            {
                _buffer2.Release();
                DestroyImmediate(_buffer2);
                _buffer2 = null;
            }
        }

        private void OnRenderImage(RenderTexture source, RenderTexture destination)
        {
            if (!enablePostProcessing || effects == null || effects.Count == 0)
            {
                Graphics.Blit(source, destination);
                return;
            }

            if (_buffer1 == null || _buffer2 == null ||
                _buffer1.width != source.width || _buffer1.height != source.height)
            {
                UpdateBuffers();
            }

            // 复用列表，避免 GC
            _activeEffects.Clear();
            for (int i = 0; i < effects.Count; i++)
            {
                var effect = effects[i];
                if (effect != null && effect.IsSupported)
                {
                    _activeEffects.Add(effect);
                }
            }

            int count = _activeEffects.Count;
            if (count == 0)
            {
                Graphics.Blit(source, destination);
                return;
            }

            if (count == 1)
            {
                _activeEffects[0].Apply(source, destination);
                return;
            }

            if (_buffer1 == null || _buffer2 == null)
            {
                Graphics.Blit(source, destination);
                return;
            }

            // 第一个效果：source → buffer1
            bool applied = _activeEffects[0].Apply(source, _buffer1);
            if (!applied)
                Graphics.Blit(source, _buffer1);

            // 中间效果在 buffer1 / buffer2 之间乒乓
            RenderTexture read = _buffer1;
            RenderTexture write = _buffer2;

            for (int i = 1; i < count - 1; i++)
            {
                applied = _activeEffects[i].Apply(read, write);
                if (!applied)
                    Graphics.Blit(read, write);

                (read, write) = (write, read);
            }

            // 最后一个效果直接写入 destination
            applied = _activeEffects[count - 1].Apply(read, destination);
            if (!applied)
                Graphics.Blit(read, destination);
        }
    }
}