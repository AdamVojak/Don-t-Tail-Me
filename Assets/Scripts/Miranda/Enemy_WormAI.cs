using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VentWormAI : MonoBehaviour
{
    private struct TransformData
    {
        public Vector3 position;
        public Quaternion rotation;

        public TransformData(Vector3 pos, Quaternion rot)
        {
            position = pos;
            rotation = rot;
        }
    }

    [Header("Targeting")]
    [SerializeField] private Transform player;
    [SerializeField] private MirandaController mirandaController;
    [SerializeField] private float attackDistance = 1.2f;

    [Header("Movement Settings")]
    [SerializeField] private float stepSize = 2.65f;
    [SerializeField] private float moveInterval = 0.3f;
    [SerializeField] private LayerMask obstacleLayer;

    [Header("Worm Anatomy")]
    [SerializeField] private GameObject bodyPrefab;
    [SerializeField] private int targetBodyCount = 3;
    [SerializeField] private GameObject tailPrefab;

    [Header("Activation & Kill Settings")]
    [SerializeField] private TimerUI timerUI;
    private bool isTargetDead = false;

    private List<Transform> allBodyParts = new List<Transform>();
    private List<TransformData> history = new List<TransformData>();

    private float moveTimer;
    private int currentBodyCount = 0;
    private bool isTailSpawned = false;
    private Vector3 lastDirection = Vector3.zero;

    private bool aktivna;

    void Start()
    {
        history.Add(new TransformData(transform.position, transform.rotation));

        if (mirandaController == null && player != null)
        {
            mirandaController = player.GetComponent<MirandaController>();
        }

        if (timerUI != null)
        {
            timerUI = FindFirstObjectByType<TimerUI>();
        }

        aktivna = mirandaController.isControlled;
    }

    void Update()
    {
        if (isTargetDead) return;

        aktivna = mirandaController.isControlled;

        if (mirandaController == null || !aktivna)
        {
            return;
        }

        if (Vector3.Distance(transform.position, player.position) <= attackDistance)
        {
            return;
        }

        moveTimer += Time.deltaTime;

        if (moveTimer >= moveInterval)
        {
            MoveWorm();
            moveTimer = 0f;
        }
    }
    public void SetMoveInterval(float newInterval)
    {
        moveInterval = newInterval;
    }
    private void MoveWorm()
    {
        Vector3 currentPos = transform.position;
        Vector3 nextPos = CalculateNextStep(currentPos);

        if (nextPos != currentPos)
        {
            Vector3 direction = (nextPos - currentPos).normalized;
            UpdateHeadRotation(direction);

            lastDirection = direction;

            transform.position = nextPos;
            history.Insert(0, new TransformData(transform.position, transform.rotation));

            if (currentBodyCount < targetBodyCount)
            {
                GameObject newBody = Instantiate(bodyPrefab, history[1].position, history[1].rotation);
                allBodyParts.Add(newBody.transform);
                currentBodyCount++;
            }
            else if (!isTailSpawned)
            {
                GameObject newTail = Instantiate(tailPrefab, history[1].position, history[1].rotation);
                allBodyParts.Add(newTail.transform);
                isTailSpawned = true;
            }

            for (int i = 0; i < allBodyParts.Count; i++)
            {
                allBodyParts[i].position = history[i + 1].position;
                allBodyParts[i].rotation = history[i + 1].rotation;
            }

            if (history.Count > allBodyParts.Count + 1)
            {
                history.RemoveAt(history.Count - 1);
            }
        }
    }

    private Vector3 CalculateNextStep(Vector3 currentPos)
    {
        Vector3 targetPos = player.position;

        Vector3[] directions = new Vector3[]
        {
        new Vector3(0, stepSize, 0),
        new Vector3(0, -stepSize, 0),
        new Vector3(0, 0, stepSize),
        new Vector3(0, 0, -stepSize)
        };

        List<Vector3> validForwardMoves = new List<Vector3>();
        Vector3 backwardPos = history.Count > 1 ? history[1].position : currentPos;

        foreach (Vector3 dir in directions)
        {
            Vector3 potentialPos = currentPos + dir;

            if (!Physics.CheckSphere(potentialPos, stepSize * 0.3f, obstacleLayer))
            {
                if (potentialPos != backwardPos)
                {
                    validForwardMoves.Add(potentialPos);
                }
            }
        }

        if (validForwardMoves.Count > 0)
        {
            Vector3 bestStep = currentPos;
            float shortestDistance = float.MaxValue;

            foreach (Vector3 potentialPos in validForwardMoves)
            {
                float distToPlayer = Vector3.Distance(potentialPos, targetPos);

                if (distToPlayer < shortestDistance)
                {
                    shortestDistance = distToPlayer;
                    bestStep = potentialPos;
                }
            }

            return bestStep;
        }
        else
        {
            if (!Physics.CheckSphere(backwardPos, stepSize * 0.3f, obstacleLayer))
            {
                return backwardPos;
            }
        }

        return currentPos;
    }

    private void UpdateHeadRotation(Vector3 dir)
        {
            if (dir.z > 0)
            {
                transform.rotation = Quaternion.Euler(0, 0, 0);
            }
            else if (dir.z < 0)
            {
                transform.rotation = Quaternion.Euler(180, 0, 0);
            }
            else if (dir.y > 0)
            {
                transform.rotation = Quaternion.Euler(-90, 0, 0);
            }
            else if (dir.y < 0)
            {
                transform.rotation = Quaternion.Euler(90, 0, 0);
            }
        }

    private void OnTriggerEnter(Collider other)
    {
        MirandaController mc = other.GetComponent<MirandaController>();

        if (mc != null && mc.currentState != MirandaController.MirandaState.Dead)
        {
            isTargetDead = true;
            mc.Die();

            StartCoroutine(StopAndDestroyWorm());
        }
    }

    private IEnumerator StopAndDestroyWorm()
    {
        moveInterval = float.MaxValue;
        Debug.Log("Crv je ulovio metu i gasi se...");

        yield return new WaitForSeconds(0.5f);

        foreach (Transform part in allBodyParts)
        {
            if (part != null)
            {
                Destroy(part.gameObject);
            }
        }
        Destroy(gameObject);
    }

    // NOVO: Provjera je li rep stvoren
    public bool IsTailSpawned()
    {
        return isTailSpawned;
    }

    // NOVO: Vraća Transform repa (zadnjeg dijela tijela)
    public Transform GetTailTransform()
    {
        if (allBodyParts.Count > 0)
        {
            return allBodyParts[allBodyParts.Count - 1]; // Rep je zadnji element
        }
        return null;
    }
}