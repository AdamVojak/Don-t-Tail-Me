using System.Collections.Generic;
using UnityEngine;

public class SashaPath : MonoBehaviour
{
    public static SashaPath Instance;

    [HideInInspector]
    public List<Vector3> points = new List<Vector3>();

    [SerializeField] private float minDistanceBetweenPoints = 1.0f;
    [SerializeField] private int maxPoints = 25;

    private void Awake()
    {
        Instance = this;
    }

    private void Update()
    {
        if (points.Count == 0 || Vector3.Distance(transform.position, points[points.Count - 1]) > minDistanceBetweenPoints)
        {
            points.Add(transform.position);

            if (points.Count > maxPoints)
            {
                points.RemoveAt(0);
            }
        }
    }
    public void ForceAddPoint(Vector3 pos)
    {
        points.Add(pos);
        if (points.Count > maxPoints)
        {
            points.RemoveAt(0);
        }
    }
}