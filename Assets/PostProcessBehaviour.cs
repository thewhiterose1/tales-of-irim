using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using Random = UnityEngine.Random;

public class PostProcessBehaviour : MonoBehaviour
{
    private Vignette vignette;
    private Volume volume;

    private float vignetteBase = 0.65f;
    private float vignetteDeviation;

    private float targetVignette = 0.65f;
    // Start is called before the first frame update
    void Start()
    {
        volume = GetComponent<Volume>();
        volume.profile.TryGet(out vignette);
        vignette.intensity.value = vignetteBase;
    }

    // Update is called once per frame
    void Update()
    {
        if (Math.Abs(vignette.intensity.value - targetVignette) > 0.01f)
        {
            if (targetVignette > vignette.intensity.value)
            {
                vignette.intensity.value += 0.00005f;
            }
            else
            {
                vignette.intensity.value -= 0.00005f;
            }
        }
        else
        {
            vignetteDeviation = Random.Range(-0.13f, 0.13f);
            targetVignette = vignetteDeviation + vignetteBase;
        }

    }
}
