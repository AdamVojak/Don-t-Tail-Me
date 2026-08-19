using System.Collections;
using UnityEngine;

public class ViperFishController : MonoBehaviour
{
    public enum FishState { Roaming, Positioning, FakeOut, Attacking }
    public FishState currentState = FishState.Roaming;

    [Header("Reference Točaka (Vezane za Giovannija)")]
    [SerializeField] private Transform leftStart;
    [SerializeField] private Transform leftEnd;
    [SerializeField] private Transform frontStart;
    [SerializeField] private Transform frontEnd;
    [SerializeField] private Collider headCollider;
    private float yOffset;

    [Header("Reference Igrača")]
    [SerializeField] private GiovanniController giovanni;
    [SerializeField] private GiovanniStats giovanniStats;

    [Header("Postavke Jumpscare-a")]
    [SerializeField] private float zoomSpeed = 35f;
    [SerializeField] private float attackSpeed = 15f;
    [SerializeField] private float swimToPositionSpeed = 20f;
    [SerializeField] private Transform jumpscareStart;
    [SerializeField] private Transform cameraRoot;
    [SerializeField] private Vector3 forwardOffset = new Vector3(0, 180, 0);

    [Header("Postavke Lutanja (Roaming)")]
    [SerializeField] private float roamSpeed = 5f;
    [SerializeField] private float escapeSpeed = 15f;
    [SerializeField] private float minTurnTime = 2f;
    [SerializeField] private float maxTurnTime = 6f;
    [SerializeField] private float turnSpeedMultiplier = 30f;

    [Header("Postavke Granica i Izbjegavanja")]
    [SerializeField] private LayerMask terrainLayer;
    [SerializeField] private float terrainDetectionDistance = 10f;
    [SerializeField] private float avoidanceTurnSpeed = 120f;

    [Header("Granice Izbjegavanja")]
    [SerializeField] private float innerAvoidRadius = 15f;
    [SerializeField] private float outerBoundaryRadius = 80f;

    [Header("Postavke Reakcije na Svjetlo")]
    [SerializeField] private float threatDamageAmount = 25f; // Koliko threat-a skida po udarcu
    [SerializeField] private float illuminationTimeRequired = 0.15f; // Koliko dugo mora biti osvijetljena za damage
    [SerializeField] private float damageCooldown = 1f; // Cooldown između dva damage-a
    [SerializeField] private float wavyFrequency = 3f;
    [SerializeField] private float wavyAmplitude = 45f;

    private bool isIlluminated = false;
    private float currentIlluminationTimer = 0f; // Prati koliko dugo je pod svjetlom
    private float currentCooldownTimer = 0f;     // Prati cooldown

    private int currentPhase = 1;
    private float fixedYPosition;
    private float turnTimer;
    private float currentTurnSpeed;
    private Vector3 currentMoveDirection;

    private Vector3 targetRoamDirection; // NOVO: Smjer u kojem riba želi lutati

    void Start()
    {
        if (headCollider != null) yOffset = headCollider.bounds.center.y - transform.position.y;
        fixedYPosition = transform.position.y;

        currentMoveDirection = -transform.forward;
        if (currentMoveDirection == Vector3.zero) currentMoveDirection = Vector3.forward;

        CalculateNewTurn();
        currentState = FishState.Roaming;
    }

    void Update()
    {
        if (giovanni.currentState == GiovanniController.GiovanniState.Dead) return;

        if (currentState == FishState.Roaming)
        {
            HandleRoaming();
        }
    }

    private void HandleRoaming()
    {
        // OSIGURAČ ZA UGAŠENO SVJETLO
        if (giovanni.flashlightLight == null || !giovanni.flashlightLight.enabled)
        {
            isIlluminated = false;
            currentIlluminationTimer = 0f;
        }

        // NOVO: Računamo poziciju GLAVE ribe, a ne repa (pivota)
        Vector3 fishHeadPos = headCollider != null ? headCollider.bounds.center : transform.position;

        // NOVO: Udaljenost računamo isključivo po tlu (XZ ravnina) kako visina ne bi kvarila radijus
        Vector2 flatFish = new Vector2(fishHeadPos.x, fishHeadPos.z);
        Vector2 flatPlayer = new Vector2(giovanni.transform.position.x, giovanni.transform.position.z);
        float distanceToPlayer = Vector2.Distance(flatFish, flatPlayer);

        // 1. LOGIKA SKIDANJA THREAT-a
        if (currentCooldownTimer > 0)
        {
            currentCooldownTimer -= Time.deltaTime;
        }

        if (isIlluminated)
        {
            currentIlluminationTimer += Time.deltaTime;

            if (currentIlluminationTimer >= illuminationTimeRequired && currentCooldownTimer <= 0f)
            {
                giovanniStats.ReduceThreat(threatDamageAmount);
                currentCooldownTimer = damageCooldown;
                currentIlluminationTimer = 0f;
            }
        }
        else
        {
            currentIlluminationTimer = 0f;
        }

        // 2. PRIORITETI KRETANJA
        bool isAvoiding = false;
        Vector3 targetDirection = Vector3.zero;
        float currentSpeed = roamSpeed;
        float currentTurnSpeedLimit = avoidanceTurnSpeed;

        Vector3 headFwd = currentMoveDirection;
        Vector3 rightAngle = Quaternion.Euler(0, 30, 0) * headFwd;
        Vector3 leftAngle = Quaternion.Euler(0, -30, 0) * headFwd;
        RaycastHit hit;

        // PRIORITET 1: Teren (Zidovi)
        if (Physics.Raycast(transform.position, headFwd, out hit, terrainDetectionDistance, terrainLayer))
        {
            isAvoiding = true;
            targetDirection = Vector3.ProjectOnPlane(hit.normal, Vector3.up);
        }
        else if (Physics.Raycast(transform.position, rightAngle, out hit, terrainDetectionDistance, terrainLayer))
        {
            isAvoiding = true; targetDirection = Quaternion.Euler(0, -45, 0) * headFwd;
        }
        else if (Physics.Raycast(transform.position, leftAngle, out hit, terrainDetectionDistance, terrainLayer))
        {
            isAvoiding = true; targetDirection = Quaternion.Euler(0, 45, 0) * headFwd;
        }
        // PRIORITET 2: APSOLUTNA BLOKADA RADIJUSA (Računa se od glave + Fizički Pushback)
        else if (distanceToPlayer <= innerAvoidRadius)
        {
            isAvoiding = true;
            Vector3 dirAway = (fishHeadPos - giovanni.transform.position).normalized;
            dirAway.y = 0;

            targetDirection = dirAway;
            currentSpeed = escapeSpeed;
            currentTurnSpeedLimit = avoidanceTurnSpeed * 4f; // Ekstremno brzo okretanje od igrača

            // NOVO (FIZIČKA BLOKADA): Ako glava uđe unutra, trenutno je izbaci na rub radijusa!
            Vector3 clampedPos = giovanni.transform.position + (dirAway * innerAvoidRadius);
            clampedPos.y = fixedYPosition;
            transform.position = clampedPos;
        }
        // PRIORITET 3: Reakcija na svjetlo (Samo dok je IZVAN unutarnjeg radijusa)
        else if (isIlluminated)
        {
            isAvoiding = true;
            Vector3 dirToPlayer = (giovanni.transform.position - fishHeadPos).normalized;
            dirToPlayer.y = 0;

            float waveAngle = Mathf.Sin(Time.time * wavyFrequency) * wavyAmplitude;
            targetDirection = Quaternion.Euler(0, waveAngle, 0) * dirToPlayer;

            currentTurnSpeedLimit = avoidanceTurnSpeed * 1.5f;
            currentSpeed = roamSpeed * 0.5f;
        }
        // PRIORITET 4: Vanjska granica mape
        else if (distanceToPlayer > outerBoundaryRadius)
        {
            isAvoiding = true;
            targetDirection = (giovanni.transform.position - transform.position).normalized;
        }

        // 3. PRIMJENA ROTACIJE I KRETANJA
        if (isAvoiding)
        {
            targetDirection.y = 0;
            if (targetDirection != Vector3.zero)
            {
                Quaternion currentRot = Quaternion.LookRotation(currentMoveDirection);
                Quaternion targetRot = Quaternion.LookRotation(targetDirection);
                currentRot = Quaternion.RotateTowards(currentRot, targetRot, currentTurnSpeedLimit * Time.deltaTime);
                currentMoveDirection = currentRot * Vector3.forward;
            }
        }
        else
        {
            // NOVO: Prirodnije lutanje (Wandering)
            turnTimer -= Time.deltaTime;
            if (turnTimer <= 0) CalculateNewTurn();

            // Glatko se okreće prema nasumično odabranom smjeru i onda pliva ravno
            Quaternion currentRot = Quaternion.LookRotation(currentMoveDirection);
            Quaternion targetRot = Quaternion.LookRotation(targetRoamDirection);
            currentRot = Quaternion.RotateTowards(currentRot, targetRot, turnSpeedMultiplier * Time.deltaTime);
            currentMoveDirection = currentRot * Vector3.forward;
        }

        currentMoveDirection.Normalize();

        transform.position += currentMoveDirection * currentSpeed * Time.deltaTime;
        if (currentMoveDirection != Vector3.zero)
        {
            transform.rotation = Quaternion.LookRotation(currentMoveDirection) * Quaternion.Euler(forwardOffset);
        }

        EnforceFixedY();
    }

    private void CalculateNewTurn()
    {
        turnTimer = Random.Range(minTurnTime, maxTurnTime);

        float randomAngle = Random.Range(-60f, 60f);
        targetRoamDirection = Quaternion.Euler(0, randomAngle, 0) * currentMoveDirection;
        targetRoamDirection.y = 0;
    }

    private void EnforceFixedY()
    {
        Vector3 currentPos = transform.position;
        currentPos.y = fixedYPosition;
        transform.position = currentPos;

        Vector3 currentEuler = transform.eulerAngles;
        currentEuler.x = 0; currentEuler.z = 0;
        transform.eulerAngles = currentEuler;
    }

    public void TriggerThreatEvent()
    {
        if (currentState != FishState.Roaming || giovanni.currentState == GiovanniController.GiovanniState.Dead) return;

        isIlluminated = false;
        currentIlluminationTimer = 0f;
        currentCooldownTimer = 0f;

        if (currentPhase == 1)
            StartCoroutine(SwimToPositionAndAttack(leftStart, leftEnd, 1));
        else if (currentPhase == 2)
            StartCoroutine(SwimToPositionAndAttack(frontStart, frontEnd, 2));
        else if (currentPhase == 3)
            StartCoroutine(FakeOutAndKill());
    }

    private IEnumerator SwimToPositionAndAttack(Transform startT, Transform endT, int phaseCompleted)
    {
        currentState = FishState.Positioning;

        Vector3 flatFishPos = new Vector3(transform.position.x, 0, transform.position.z);
        Vector3 flatStartPos = new Vector3(startT.position.x, 0, startT.position.z);

        // OSIGURAČ: Ako ne stigne u 3.5 sekunde, automatski kreni u napad da ne zapne u osmici
        float maxPositioningTime = 3.5f;
        float timer = 0f;

        // Povećan radijus na 4f (puno lakše za pogoditi pri brzini)
        while (Vector2.Distance(new Vector2(flatFishPos.x, flatFishPos.z), new Vector2(flatStartPos.x, flatStartPos.z)) > 4f && timer < maxPositioningTime)
        {
            timer += Time.deltaTime;

            Vector3 dirToStart = (startT.position - transform.position).normalized;
            dirToStart.y = 0;

            // Oštrije skretanje da lakše pogodi točku
            Quaternion currentRot = Quaternion.LookRotation(currentMoveDirection);
            Quaternion targetRot = Quaternion.LookRotation(dirToStart);
            currentRot = Quaternion.RotateTowards(currentRot, targetRot, avoidanceTurnSpeed * 2f * Time.deltaTime);
            currentMoveDirection = currentRot * Vector3.forward;

            transform.position += currentMoveDirection * swimToPositionSpeed * Time.deltaTime;
            transform.rotation = Quaternion.LookRotation(currentMoveDirection) * Quaternion.Euler(forwardOffset);

            EnforceFixedY();

            flatFishPos = transform.position;
            flatStartPos = startT.position;

            yield return null;
        }

        // Siguran prijelaz u napad
        currentState = FishState.Attacking;
        yield return StartCoroutine(ZoomBetweenPoints(startT, endT, phaseCompleted));
    }

    private IEnumerator FakeOutAndKill()
    {
        currentState = FishState.FakeOut;

        Vector3 awayDir = (transform.position - giovanni.transform.position).normalized;
        awayDir.y = 0;
        currentMoveDirection = awayDir;

        float fakeOutTimer = 1.5f;
        while (fakeOutTimer > 0)
        {
            fakeOutTimer -= Time.deltaTime;
            transform.position += currentMoveDirection * (swimToPositionSpeed * 2f) * Time.deltaTime;
            transform.rotation = Quaternion.LookRotation(currentMoveDirection) * Quaternion.Euler(forwardOffset);
            EnforceFixedY();
            yield return null;
        }

        currentState = FishState.Attacking;
        yield return StartCoroutine(AttackPlayer());
    }

    private IEnumerator ZoomBetweenPoints(Transform startT, Transform endT, int phaseCompleted)
    {
        float distance = Vector3.Distance(startT.position, endT.position);
        float duration = distance / zoomSpeed;
        float elapsed = 0;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;

            Vector3 targetPos = Vector3.Lerp(startT.position, endT.position, t);
            transform.position = new Vector3(targetPos.x, targetPos.y - yOffset, targetPos.z);

            Vector3 lookTarget = new Vector3(endT.position.x, endT.position.y - yOffset, endT.position.z);
            transform.LookAt(lookTarget);
            transform.Rotate(forwardOffset);

            yield return null;
        }

        currentPhase = phaseCompleted + 1;

        Vector3 awayFromPlayer = (transform.position - giovanni.transform.position).normalized;
        awayFromPlayer.y = 0;
        currentMoveDirection = awayFromPlayer;
        transform.rotation = Quaternion.LookRotation(currentMoveDirection) * Quaternion.Euler(forwardOffset);

        fixedYPosition = transform.position.y;
        currentState = FishState.Roaming;
    }

    private IEnumerator AttackPlayer()
    {
        float distance = Vector3.Distance(jumpscareStart.position, cameraRoot.position);
        float duration = distance / attackSpeed;
        float elapsed = 0;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;

            Vector3 targetPos = Vector3.Lerp(jumpscareStart.position, cameraRoot.position, t);
            transform.position = new Vector3(targetPos.x, targetPos.y - yOffset, targetPos.z);

            Vector3 lookTarget = new Vector3(cameraRoot.position.x, cameraRoot.position.y - yOffset, cameraRoot.position.z);
            transform.LookAt(lookTarget);
            transform.Rotate(forwardOffset);

            yield return null;
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("BeamCone"))
        {
            isIlluminated = true;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (currentState == FishState.Attacking && currentPhase == 3 && other.CompareTag("Giovanni"))
        {
            giovanni.Die(1);
            currentPhase = 1;
            gameObject.SetActive(false);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("BeamCone"))
        {
            isIlluminated = false;
            currentIlluminationTimer = 0f;
        }
    }

    private void OnDrawGizmos()
    {
        if (giovanni != null)
        {
            Gizmos.color = new Color(1, 0, 0, 0.3f);
            Gizmos.DrawWireSphere(giovanni.transform.position, innerAvoidRadius);

            Gizmos.color = new Color(0, 1, 0, 0.3f);
            Gizmos.DrawWireSphere(giovanni.transform.position, outerBoundaryRadius);
        }
    }
}