using UnityEngine;
using ShaderLoad.Util;
using System.Collections.Generic;

namespace ShaderLoad.PostProcess
{
    /// <summary>
    /// Post-processing effect that manages a Material and flushes registered float/vector
    /// properties before each blit. Not inheritable — use composition instead.
    /// </summary>
    [System.Serializable]
    public sealed class PostProcessEffect
    {
        private readonly Material _material;
        private readonly Shader _shader;
        [SerializeField] private FloatDictionary FloatUniforms = new();
        [SerializeField] private VectorDictionary VectorUniforms = new();

        public bool IsSupported => _shader != null && _shader.isSupported;

        /// <param name="shader">Shader for this effect.</param>
        /// <param name="uniforms">uniforms.</param>
        public PostProcessEffect(Shader shader, IReadOnlyList<UniformInfo> uniforms = null)
        {
            _shader = shader;
            if (shader != null)
            {
                _material = new(shader)
                {
                    hideFlags = HideFlags.DontSave
                };
            }

            if (uniforms == null) return;

            foreach (var uniform in uniforms)
            {
                if (uniform.Name is "_Time" or "_ScreenSize") continue;

                if (uniform.Type is "float")
                {
                    FloatUniforms[uniform.Name] = 0f;
                }
                else if (uniform.Type is "vec2" or "vec3" or "vec4")
                {
                    VectorUniforms[uniform.Name] = Vector4.zero;
                }
            }
        }

        /// <summary>
        /// Set a float property. Logs an error if the property name is not registered.
        /// </summary>
        public void SetFloat(string name, float value)
        {
            if (!FloatUniforms.ContainsKey(name))
            {
                Debug.LogError($"[PostProcessEffect] Float property '{name}' is not registered.");
                return;
            }
            FloatUniforms[name] = value;
        }

        /// <summary>
        /// Set a vector property. Logs an error if the property name is not registered.
        /// </summary>
        public void SetVector(string name, Vector4 value)
        {
            if (!VectorUniforms.ContainsKey(name))
            {
                Debug.LogError($"[PostProcessEffect] Vector property '{name}' is not registered.");
                return;
            }
            VectorUniforms[name] = value;
        }

        /// <summary>
        /// Flush all properties to the material and blit.
        /// </summary>
        internal bool Apply(RenderTexture source, RenderTexture destination)
        {
            if (_shader == null || _material == null)
                return false;

            FlushProperties();
            _material.SetTexture("_MainTex", source);
            _material.SetVector("_ScreenSize", new Vector4(Screen.width, Screen.height));
            Graphics.Blit(source, destination, _material);

            return true;
        }

        internal void Cleanup()
        {
            if (_material != null)
                Object.DestroyImmediate(_material, true);
        }

        private void FlushProperties()
        {
            foreach (var kvp in FloatUniforms)
                _material.SetFloat(kvp.Key, kvp.Value);
            foreach (var kvp in VectorUniforms)
                _material.SetVector(kvp.Key, kvp.Value);
        }
    }
}
