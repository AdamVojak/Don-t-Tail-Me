using System.Collections;
using UnityEngine;

public class ShortCircuitLight : MonoBehaviour
{
    [Header("Lista Svjetala")]
    public Light[] lights;

    [Header("Postavke Intenziteta")]
    public float normalIntensity = 15f;

    public float surgeIntensity = 25f;

    [Header("Učestalost Kvara")]
    public float minStableTime = 1.5f;

    public float maxStableTime = 5.0f;

    [Header("Postavke Iskrenja")]
    public int minSparks = 4;

    public int maxSparks = 12;

    public float blackoutDuration = 0.2f;

    private Coroutine shortCircuitCoroutine;

    private void OnEnable()
    {
        shortCircuitCoroutine = StartCoroutine(ShortCircuitRoutine());
    }

    private void OnDisable()
    {
        if (shortCircuitCoroutine != null)
        {
            StopCoroutine(shortCircuitCoroutine);
        }
    }

    private IEnumerator ShortCircuitRoutine()
    {
        while (true)
        {
            SetIntensity(normalIntensity);
            float stableTime = Random.Range(minStableTime, maxStableTime);
            yield return new WaitForSeconds(stableTime);

            int sparkCount = Random.Range(minSparks, maxSparks);
            for (int i = 0; i < sparkCount; i++)
            {
                float randomSpark = (Random.value > 0.4f) ? Random.Range(0f, surgeIntensity) : 0f;
                SetIntensity(randomSpark);

                float sparkSpeed = Random.Range(0.01f, 0.06f);
                yield return new WaitForSeconds(sparkSpeed);
            }

            if (blackoutDuration > 0f)
            {
                SetIntensity(0f);
                yield return new WaitForSeconds(blackoutDuration);
            }
        }
    }

    private void SetIntensity(float value)
    {
        if (lights == null) return;

        for (int i = 0; i < lights.Length; i++)
        {
            if (lights[i] != null)
            {
                lights[i].intensity = value;
            }
        }
    }
}