using SuperliVR.Picking;
using UnityEngine;
using Utils;

namespace SuperliVR.Visuals
{
    [RequireComponent(typeof(WandPicker))]
    public class WandLight : MonoBehaviour
    {
        [SerializeField] 
        private MeshRenderer   _indicatorObject     = null;
        [SerializeField]                            
        private Light          _lightSource         = null;
        [SerializeField]                            
        private Gradient       _gradient            = null;
        [SerializeField]                            
        private ParticleSystem _particleSystem      = null;

        [SerializeField, Range(0f, 1f)]
        private float          _colorCentering      = 0.9f;
        [SerializeField, Range(0f, 1f)]             
        private float          _colorMaxDistance    = 0.5f;
        [SerializeField]
        private float          _maxParticleEmission = 10.0f;

        private WandPicker     _wandPicker;

        private Color          _currentColor        = Color.black;

        private void Awake() =>
            _wandPicker = GetComponent<WandPicker>();

        private void Update()
        {
            var colorGradient = 0.0f;

            var pickedObject = _wandPicker.PickedObject;
            if (pickedObject != null)
            {
                if (pickedObject.CurrentScaleMultiplier > pickedObject.PlacedScaleMultiplier)
                    colorGradient = pickedObject.CurrentScaleMultiplier / pickedObject.PlacedScaleMultiplier - 1.0f;
                else
                    colorGradient = 1.0f - pickedObject.PlacedScaleMultiplier / pickedObject.CurrentScaleMultiplier;
            }

            var particlesStrengths = Mathf.Abs(colorGradient);

            colorGradient = colorGradient * 0.5f + 0.5f;
            colorGradient = Mathf.Clamp01(colorGradient);

            var color = _gradient.Evaluate(colorGradient);
            
            _currentColor = MathHelper.Vector3ToColor(
                MathHelper.ReachValueSmooth(
                    MathHelper.ColorToVector3(_currentColor),
                    MathHelper.ColorToVector3(color), 
                    _colorCentering, 
                    _colorMaxDistance, 
                    Time.deltaTime));

            _indicatorObject.material.SetColor("_EmissionColor", _currentColor);
            _indicatorObject.material.SetColor("_Color", _currentColor);
            _lightSource.color = _currentColor;

            var emission = _particleSystem.emission;
            var main     = _particleSystem.main;

            emission.rateOverTime = particlesStrengths * _maxParticleEmission;
            main.startColor       = _currentColor;
        }
    }
}