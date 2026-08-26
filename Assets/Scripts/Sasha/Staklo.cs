using System.Collections;
using UnityEngine;

public class Staklo : MonoBehaviour
{
    [Header("Postavke Fizike Krhotina")]
    [SerializeField] private float scatterDistance = 2.5f; // Koliko daleko lete krhotine
    [SerializeField] private float scatterDuration = 0.6f; // Koliko dugo traje let
    [SerializeField] private float fadeDuration = 0.4f;    // Koliko dugo nestaju (fade out)
    [SerializeField] private float maxSpinSpeed = 360f;    // Brzina rotacije krhotina u zraku
    private SFX sfx;

    private void Start()
    {
        PlayBreakSound();

        StartCoroutine(AnimateShards());
    }

    private void PlayBreakSound()
    {
        sfx = FindFirstObjectByType<SFX>();
        if (sfx != null && sfx.ZvukLomaStakla != null)
        {
            sfx.ZvukLomaStakla.Play();
        }
    }

    private IEnumerator AnimateShards()
    {
        SpriteRenderer[] shards = GetComponentsInChildren<SpriteRenderer>();

        int shardCount = shards.Length;
        if (shardCount == 0)
        {
            Destroy(gameObject);
            yield break;
        }

        Vector3[] startPositions = new Vector3[shardCount];
        Vector3[] targetPositions = new Vector3[shardCount];
        float[] rotationSpeeds = new float[shardCount];

        float angleStep = 360f / shardCount;
        float randomAngleOffset = Random.Range(0f, 360f);

        for (int i = 0; i < shardCount; i++)
        {
            startPositions[i] = shards[i].transform.localPosition;

            float currentAngle = randomAngleOffset + (i * angleStep) + Random.Range(-15f, 15f);
            float angleRad = currentAngle * Mathf.Deg2Rad;
            Vector3 direction = new Vector3(Mathf.Cos(angleRad), Mathf.Sin(angleRad), 0f);

            float randomDist = scatterDistance * Random.Range(0.8f, 1.2f);
            targetPositions[i] = startPositions[i] + direction * randomDist;

            rotationSpeeds[i] = Random.Range(-maxSpinSpeed, maxSpinSpeed);
        }

        float time = 0f;
        while (time < scatterDuration)
        {
            time += Time.deltaTime;
            float t = time / scatterDuration;

            float smoothT = Mathf.Sin(t * Mathf.PI * 0.5f);

            for (int i = 0; i < shardCount; i++)
            {
                if (shards[i] != null)
                {
                    shards[i].transform.localPosition = Vector3.Lerp(startPositions[i], targetPositions[i], smoothT);
                    shards[i].transform.Rotate(0, 0, rotationSpeeds[i] * Time.deltaTime);
                }
            }
            yield return null;
        }
        time = 0f;
        while (time < fadeDuration)
        {
            time += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0f, time / fadeDuration);

            for (int i = 0; i < shardCount; i++)
            {
                if (shards[i] != null)
                {
                    Color color = shards[i].color;
                    color.a = alpha;
                    shards[i].color = color;
                }
            }
            yield return null;
        }
        Destroy(gameObject);
    }
}