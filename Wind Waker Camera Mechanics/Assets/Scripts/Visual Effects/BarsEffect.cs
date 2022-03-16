using UnityEngine;
using UnityEngine.Rendering;

[ExecuteInEditMode]
public class BarsEffect : MonoBehaviour
{
    // Constants
    public static float NO_COVERAGE = -0.5f;
    public static float FULL_COVERAGE = 0.0f;

    [SerializeField] private Material material;
    [Range(0f, 1f)]
    [SerializeField] private float coverage = 0.1f;
    public float Coverage { 
        get => coverage;
        set {
            coverage = Mathf.Clamp01(value);
        } 
    }
    

    private void Update()
    {
        material.SetFloat("_Coverage", Mathf.Lerp(NO_COVERAGE, FULL_COVERAGE, coverage));
    }
}
