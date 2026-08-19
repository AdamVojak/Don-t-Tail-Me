using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RazbijenAkvarij : MonoBehaviour
{
    [Header("Postavke stakla (2D Sprite-ovi)")]
    [SerializeField] private float scatterDistance = 3.0f;
    [SerializeField] private float scatterDuration = 1.0f;

    private SFX sfx;
    private AudioSource zvukLoma;

    [Header("Oružje")]
    public GameObject gun;

    void Start()
    {
        if (sfx == null)
        {
            sfx = FindFirstObjectByType<SFX>();
            zvukLoma = sfx.ZvukLomaStakla;
            if (zvukLoma != null)
            {
                zvukLoma.Play();
            }
        }


        GameObject sashaTaggedObject = GameObject.FindWithTag("Sasha");

        if (sashaTaggedObject != null)
        {
            Transform sashaPlayerTransform = sashaTaggedObject.transform.parent;

            if (sashaPlayerTransform != null)
            {
                Transform hintsTransform = sashaPlayerTransform.Find("Hints");

                if (hintsTransform != null)
                {
                    hintsTransform.gameObject.SetActive(false);
                }
                else
                {
                    Debug.LogWarning("Objekt 'Hints' nije pronađen pod objektom 'Sasha_Player'.");
                }
            }
            else
            {
                Debug.LogWarning("Objekt s tagom 'Sasha' nema roditelja.");
            }
        }
        else
        {
            Debug.LogWarning("Nije pronađen nijedan objekt s tagom 'Sasha'.");
        }


        if (gun != null)
        {
            Instantiate(gun, transform.position, transform.rotation);
        }
        ScatterExistingShards();
    }

    void ScatterExistingShards()
    {
        List<Transform> shards = new List<Transform>();

        Transform[] allTransforms = GetComponentsInChildren<Transform>(true);

        foreach (Transform t in allTransforms)
        {

            if (t == transform) continue;

            if (t.CompareTag("Staklo"))
            {
                shards.Add(t);
            }
        }

        Debug.Log($"[BrokenAquarium] Pronađeno krhotina s tagom 'Staklo': {shards.Count}");

        int shardCount = shards.Count;
        if (shardCount == 0) return;

        float angleStep = 360f / shardCount;
        float startAngleOffset = Random.Range(0f, 360f);

        for (int i = 0; i < shardCount; i++)
        {
            Transform shard = shards[i];

            float currentAngle = startAngleOffset + (i * angleStep);
            float angleRad = currentAngle * Mathf.Deg2Rad;

            Vector3 direction = new Vector3(Mathf.Cos(angleRad), Mathf.Sin(angleRad), 0f);

            Vector3 targetLocalPosition = direction * scatterDistance;
            Vector3 startLocalPosition = shard.localPosition;

            StartCoroutine(LerpLocalPosition(shard, startLocalPosition, targetLocalPosition, scatterDuration));
        }
    }

    IEnumerator LerpLocalPosition(Transform shardTransform, Vector3 startPos, Vector3 endPos, float duration)
    {
        float time = 0f;

        while (time < duration)
        {
            if (shardTransform == null) yield break;

            time += Time.deltaTime;
            float t = time / duration;

            shardTransform.localPosition = Vector3.Lerp(startPos, endPos, t);

            yield return null;
        }

        if (shardTransform != null)
        {
            shardTransform.localPosition = endPos;
        }
    }
}