using SuperliVR.Picking;
using UnityEngine;

[RequireComponent(typeof(WandPicker))]
public class WandLight : MonoBehaviour
{
    [SerializeField]
    private MeshRenderer   _indicatorObject;
    [SerializeField]       
    private Light          _lightSource;
    [SerializeField]       
    private Gradient       _gradient;
    [SerializeField]
    private ParticleSystem _particleSystem;

    private WandPicker     _wandPicker;
                           
    private Color          _currentColor = Color.black;
    
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

        // TODO: Replace hard-coded constants
        _currentColor = Vector3ToColor(ReachValueSmooth(ColorToVector3(_currentColor), ColorToVector3(color), 0.9f, 0.5f,
            Time.deltaTime));

        _indicatorObject.material.SetColor("_EmissionColor", _currentColor);
        _indicatorObject.material.SetColor("_Color", _currentColor);
        _lightSource.color = _currentColor;
        
        var emission = _particleSystem.emission;
        var main = _particleSystem.main;
        emission.rateOverTime = particlesStrengths * 10.0f; // TODO: Replace hard-coded constant.
        main.startColor = _currentColor;
    }

    private Vector3 ColorToVector3(Color col)
    {
        return new Vector3(col.r, col.g, col.b);
    }

    private Color Vector3ToColor(Vector3 vec)
    {
        return new Color(vec.x, vec.y, vec.z);
    }

    private Vector3 ReachValueSmooth(Vector3 from, Vector3 to, float centering, float alignmentDistance, float deltaTime)
    {
        var distanceBetween = Vector3.Distance(from, to);
        var t = 1.0f;

        if (distanceBetween > 0.01f && centering > 0.0f)
            t = Mathf.Pow(1.0f - centering, deltaTime);

        if (distanceBetween > alignmentDistance)
            t = Mathf.Min(t, alignmentDistance / distanceBetween);

        from = Vector3.Lerp(to, from, t);

        return from;
    }
}
