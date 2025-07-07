using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using System.Collections;

public class PlayerController : MonoBehaviour
{
    public bool safe;
    public bool usingZipline;
    public bool paused;
    public bool inDialogue;

    [Header("Key Binds")]
    public KeyCode jump = KeyCode.Space;
    public KeyCode sprint = KeyCode.Mouse1;
    public KeyCode crouch = KeyCode.LeftControl;
    public KeyCode interact = KeyCode.E;
    public KeyCode useItem = KeyCode.Mouse0;
    public KeyCode itemSpecial = KeyCode.Q;

    [Header("Base Movement Stats (No Upgrades)")]
    public float moveAcceleration = 130;
    public float moveDeceleration = 150;
    public float slideDeceleration = 7;
    public float moveSpeed = 9;
    public float sprintSpeed = 20;
    public float jumpPower = 85;
    public float gravityScale = 30;
    public float slideRotationSpeed = 45;
    public float forwardBoostOnWallJump = 15;
    public int maxWallJumps = 2;
    public float wallJumpBoostDecay = 15;
    public float slideAccelerationScale = 2.5f;

    [Header("Player Sound Effects")]
    public AudioClip step;
    public AudioClip landing;
    public AudioClip jumpSfx;
    public AudioClip wooshSfx;
    public AudioClip kickSfx;

    public bool sliding;

    [Header("Player Events")]
    public UnityEvent Jump;
    public UnityEvent StartSprint;
    public UnityEvent Slide;
    public UnityEvent LandOnGround;

    [Header("Other References")]
    public GameObject head;
    public new Camera camera;
    public InventoryScript inventory;
    public PlayerUpgrades upgradeTracker;
    public PauseMenuScr pauseMenu;
    public LevelStatsBehavior levelStats;
    public GameObject abilitiesObject;
    private SpeedLinesScr speedLinesScr;

    private const float MoveStepInterval = 0.3f;
    private const float SprintStepInterval = 0.15f;
    private float stepInterval;

    private float tempWallBoost;
    public int currWallJumps;
    public AudioSource audioSource;
    public AudioSource slideSource;
    private CharacterController charController;
    private float currXVelocity;
    private float currYVelocity;
    private float currZVelocity;
    private Vector3 movementVector;
    private bool grounded;
    private bool tweeningRotation;
    private Vector3 origHeadPos;

    [Space]
    public PlayerSettings settings;

    public PlayerStats stats;

    // ACTUAL MOVEMENT VALUES
    private float actualMoveSpeed;
    private float actualSprintSpeed;
    private float actualJumpPower;
    private float actualForwardBoostOnWallJump;

    private GameObject lastObjectStoodOn;

    public bool Grounded => grounded;
    public float ActualMoveSpeed => actualMoveSpeed;
    public float ActualSprintSpeed => actualSprintSpeed;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Start()
    {
        inDialogue = usingZipline = safe = paused = false;
        gameObject.GetComponent<MeshRenderer>().enabled = false;
        sliding = false;
        lastObjectStoodOn = null;
        origHeadPos = head.transform.localPosition;
        
        speedLinesScr = GetComponentInChildren<SpeedLinesScr>();
        charController = GetComponent<CharacterController>();
        stepInterval = 0;
        currWallJumps = maxWallJumps;
        tempWallBoost = 0;
        tweeningRotation = false;

        RefreshActualMovementValues();
        RefreshKeyBindControls();

        if (settings.useWorldCanvasUI)
        {
            pauseMenu.worldRunStatsUI.SetActive(true);
            pauseMenu.cameraCanvasObject.SetActive(false);
        }
        else
        {
            pauseMenu.cameraCanvasObject.SetActive(true);
            pauseMenu.worldRunStatsUI.SetActive(false);
        }
        
        slideSource.loop = true;
    }

    // Update is called once per frame
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab)) {
            if (!pauseMenu.gameObject.activeInHierarchy)
            {
                paused = true;
                pauseMenu.gameObject.SetActive(true);
            } else
            {
                paused = false;
                pauseMenu.gameObject.SetActive(false);
            }
        }

        if (!grounded && !usingZipline)
        {
            currYVelocity -= gravityScale * Time.deltaTime;
            stepInterval = 0;
        }
        else if (!usingZipline)
        {
            currYVelocity = 0;
            if (sliding)
            {
                if (grounded && !slideSource.isPlaying)
                {
                    slideSource.Play();
                }
                else if (!grounded)
                {
                    slideSource.Stop();
                }
            } else {
                slideSource.Stop();
                
                if (movementVector.magnitude > 0)
                    stepInterval += Time.deltaTime;

                if (Input.GetKey(sprint))
                {
                    if (stepInterval >= SprintStepInterval)
                    {
                        stepInterval = 0;
                        audioSource.PlayOneShot(step);
                    }
                }
                else
                {
                    if (stepInterval >= MoveStepInterval)
                    {
                        stepInterval = 0;
                        audioSource.PlayOneShot(step);
                    }
                }
            }
        }

        HandleMovementKeys();
        HandleSprinting();

        
        Vector3 headEuler = head.transform.localRotation.eulerAngles;
        if (!sliding)
        {
            head.transform.localRotation = Quaternion.Euler(0, headEuler.y, headEuler.z);
        }

        movementVector = Quaternion.AngleAxis(transform.localRotation.eulerAngles.y, Vector3.up) * new Vector3(currXVelocity, currYVelocity, currZVelocity + tempWallBoost);
        float oldYPos = transform.position.y;
        
        switch (charController.Move(movementVector * Time.deltaTime))
        {
            case CollisionFlags.Below:
                if (!grounded)
                {
                    grounded = true;
                    LandOnGround?.Invoke();
                    tempWallBoost = 0;
                    currWallJumps = maxWallJumps;

                    Physics.Raycast(transform.position, Vector3.down, out RaycastHit groundHit, Mathf.Infinity);
                    lastObjectStoodOn = groundHit.collider.gameObject;
                    
                    audioSource.PlayOneShot(landing);
                }
                break;
            case CollisionFlags.None:
            default:
                break;
        }
        
        bool hit = Physics.SphereCast(transform.position, 1, Vector3.down, out RaycastHit castHit);
        if (hit && grounded && castHit.distance < 0.8f && lastObjectStoodOn == castHit.collider.gameObject)
        {
            charController.Move(new Vector3(0, -Mathf.Abs(castHit.distance), 0));
        } else if (castHit.distance >= 0.8f)
        {
            grounded = false;
        }

        float diff = Mathf.Abs(oldYPos - transform.position.y);
        if (sliding && grounded)
        {
            if (transform.position.y > oldYPos)
            {
                currZVelocity -= diff * slideAccelerationScale;
            }
            else if (transform.position.y < oldYPos)
            {
                currZVelocity += diff * slideAccelerationScale;
            }
        }
    }

    private void HandleMovementKeys()
    {
        Vector3 euler = transform.rotation.eulerAngles;
        Vector3 headEuler = head.transform.localRotation.eulerAngles;
        if (Input.GetKeyDown(jump))
        {
            if (!usingZipline && !inDialogue)
            {
                if (grounded)
                {
                    currYVelocity = actualJumpPower / -Physics.gravity.y;
                    audioSource.PlayOneShot(jumpSfx);
                    grounded = false;
                    Jump?.Invoke();
                }
                else if (!sliding && Input.GetKey(KeyCode.W) && Input.GetKey(sprint) && currWallJumps > 0) // wall jump segment
                {
                    bool wallJumpedThisFrame = false;

                    Vector3 leftDirVector = (transform.rotation * Vector3.left).normalized;
                    RaycastHit[] leftHits = Physics.RaycastAll(transform.position, leftDirVector, 0.8f);
                    foreach (RaycastHit lHit in leftHits)
                    {
                        if (lHit.collider.CompareTag("Wall"))
                        {
                            LeftWallJump();
                            wallJumpedThisFrame = true;
                            break;
                        }
                    }

                    if (!wallJumpedThisFrame)
                    {
                        Vector3 rightDirVector = (transform.rotation * Vector3.right).normalized;
                        RaycastHit[] rightHits = Physics.RaycastAll(transform.position, rightDirVector, 0.8f);
                        foreach (RaycastHit rHit in rightHits)
                        {
                            if (rHit.collider.CompareTag("Wall"))
                            {
                                RightWallJump();
                                break;
                            }
                        }
                    }
                }
            }
        }

        if (sliding && Input.GetKeyUp(crouch))
        {
            sliding = false;
            charController.height = 2;
            charController.center = Vector3.zero;
            head.transform.localRotation = Quaternion.Euler(0, 0, 0);
            head.transform.localPosition = origHeadPos;
            transform.rotation = Quaternion.Euler(0, euler.y, euler.z);
        }
            

        if (Input.GetKey(KeyCode.W) && !sliding && !usingZipline && !inDialogue)
        {
            if (Input.GetKey(sprint))
                currZVelocity = Mathf.Min(actualSprintSpeed, currZVelocity + (moveAcceleration * Time.deltaTime));
            else
            {
                if (currZVelocity > actualMoveSpeed)
                    currZVelocity = Mathf.MoveTowards(currZVelocity, actualMoveSpeed, Time.deltaTime * moveDeceleration);
                else
                    currZVelocity = Mathf.Min(actualMoveSpeed, currZVelocity + (moveAcceleration * Time.deltaTime));
            }
        }
            
        if (Input.GetKey(KeyCode.S) && !sliding && !usingZipline && !inDialogue)
        {
            if (Input.GetKey(sprint))
                currZVelocity = Mathf.Max(-actualSprintSpeed, currZVelocity - (moveAcceleration * Time.deltaTime));
            else
            {
                if (currZVelocity < -actualMoveSpeed)
                    currZVelocity = Mathf.MoveTowards(currZVelocity, -actualMoveSpeed, Time.deltaTime * moveDeceleration);
                else
                    currZVelocity = Mathf.Max(-actualMoveSpeed, currZVelocity - (moveAcceleration * Time.deltaTime));
            }
        }

        if (sliding)
        {
            currZVelocity = Mathf.MoveTowards(currZVelocity, 0, Time.deltaTime * slideDeceleration);
        }
        else if (!usingZipline && !inDialogue)
        {
            if (!(Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.S)))
            {
                currZVelocity = Mathf.MoveTowards(currZVelocity, 0, Time.deltaTime * moveDeceleration);
            }
        }

        if (Input.GetKey(KeyCode.D) && !usingZipline && !inDialogue)
        {
            if (sliding)
            {
                transform.Rotate(new Vector3(0, slideRotationSpeed * Time.deltaTime, 0));
            } else
            {
                if (Input.GetKey(sprint))
                    currXVelocity = Mathf.Min(actualSprintSpeed, currXVelocity + (moveAcceleration * Time.deltaTime));
                else
                {
                    if (currXVelocity > actualMoveSpeed)
                        currXVelocity = Mathf.MoveTowards(currXVelocity, actualMoveSpeed, Time.deltaTime * moveDeceleration);
                    else
                        currXVelocity = Mathf.Min(actualMoveSpeed, currXVelocity + (moveAcceleration * Time.deltaTime));
                }
            }
        }
        if (Input.GetKey(KeyCode.A) && !usingZipline && !inDialogue)
        {
            if (sliding)
            {
                transform.Rotate(new Vector3(0, -slideRotationSpeed * Time.deltaTime, 0));
            }
            else
            {
                if (Input.GetKey(sprint))
                    currXVelocity = Mathf.Max(-actualSprintSpeed, currXVelocity - (moveAcceleration * Time.deltaTime));
                else
                {
                    if (currXVelocity < -actualMoveSpeed)
                        currXVelocity = Mathf.MoveTowards(currXVelocity, -actualMoveSpeed, Time.deltaTime * moveDeceleration);
                    else
                        currXVelocity = Mathf.Max(-actualMoveSpeed, currXVelocity - (moveAcceleration * Time.deltaTime));
                }
            }
        }

        if (sliding)
        {
            currXVelocity = Mathf.MoveTowards(currXVelocity, 0, Time.deltaTime * slideDeceleration);
        }
        else if (!usingZipline && !inDialogue)
        {
            if (!(Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.D)))
            {
                currXVelocity = Mathf.MoveTowards(currXVelocity, 0, Time.deltaTime * moveDeceleration);
            }
        }

        if (Input.GetKey(sprint))
        {
            if (Input.GetKeyDown(sprint))
            {
                StartSprint?.Invoke();
            }
            if (Input.GetKeyDown(crouch) && movementVector.magnitude != 0 && !usingZipline && !inDialogue)
            {
                if (!sliding)
                {
                    sliding = true;
                    charController.height = 0.5f;
                    charController.center = new Vector3(0, -0.5f, 0);
                    head.transform.localPosition = origHeadPos - new Vector3(0, 0.8f, 0);
                    tweeningRotation = false;
                    currZVelocity += tempWallBoost;
                    currXVelocity += tempWallBoost * Mathf.Sin(Mathf.Deg2Rad * transform.rotation.eulerAngles.y);

                    Slide?.Invoke();
                    
                    if (!grounded)
                        currYVelocity = 0;
                }
            }
        }
    }

    private void HandleSprinting()
    {
        Vector3 betaVector = new Vector3(currXVelocity, 0, currZVelocity);
        if (Input.GetKey(sprint) && !sliding)
        {
            if (betaVector.magnitude > actualSprintSpeed)
            {
                betaVector = betaVector.normalized * actualSprintSpeed;
                currXVelocity = betaVector.x;
                currZVelocity = betaVector.z;
            }
        }
        else if (!sliding)
        {
            if (betaVector.magnitude > actualMoveSpeed)
            {
                betaVector = betaVector.normalized * actualMoveSpeed;
                currXVelocity = Mathf.MoveTowards(currXVelocity, betaVector.x, Time.deltaTime * 20);
                currZVelocity = Mathf.MoveTowards(currZVelocity, betaVector.z, Time.deltaTime * 20);
            }
        }
    }

    private void LeftWallJump()
    {
        currWallJumps--;
        tempWallBoost += actualForwardBoostOnWallJump;
        currYVelocity = Mathf.Max(currYVelocity + (actualJumpPower / -Physics.gravity.y) * 1.5f, (actualJumpPower / -Physics.gravity.y) * 1.3f);
        currXVelocity += 8f;
        audioSource.PlayOneShot(jumpSfx, 0.75f);
        audioSource.PlayOneShot(landing, 0.75f);
        if (tempWallBoost > 0)
            StartCoroutine(DecreaseWallBoost());

        StartCoroutine(StandingCameraRotationTween(45, 0.2f));
    }

    private void RightWallJump()
    {
        currWallJumps--;
        tempWallBoost += actualForwardBoostOnWallJump;
        currYVelocity = Mathf.Max(currYVelocity + (actualJumpPower / -Physics.gravity.y) * 1.5f, (actualJumpPower / -Physics.gravity.y) * 1.3f);
        currXVelocity -= 8f;
        audioSource.PlayOneShot(jumpSfx, 0.75f);
        audioSource.PlayOneShot(landing, 0.75f);
        if (tempWallBoost > 0)
            StartCoroutine(DecreaseWallBoost());

        StartCoroutine(StandingCameraRotationTween(-45, 0.2f));
    }

    private IEnumerator StandingCameraRotationTween(float targetRot, float time)
    {
        float progress = 0;
        tweeningRotation = true;
        while (progress < 1 && tweeningRotation)
        {
            progress += Time.deltaTime / time;
            transform.Rotate(new Vector3(0, (targetRot / 15) * ((-2 * progress) + 2), 0));
            yield return null;
        }
        tweeningRotation = false;
    }

    private IEnumerator DecreaseWallBoost()
    {
        while (tempWallBoost > 0)
        {
            tempWallBoost = Mathf.MoveTowards(tempWallBoost, 0, wallJumpBoostDecay * Time.deltaTime);
            yield return null;
        }
    }

    public Vector3 GetMovementVector() { return new Vector3(currXVelocity, currYVelocity, currZVelocity); }
    
    public Vector3 GetTrueMovementVector() { return new Vector3(currXVelocity, currYVelocity, currZVelocity + tempWallBoost); }

    public void HaltMovement()
    {
        currXVelocity = 0;
        currYVelocity = 0;
        currZVelocity = 0;
        tempWallBoost = 0;
        grounded = false;
    }

    public void RefreshActualMovementValues()
    {
        actualMoveSpeed = (moveSpeed + upgradeTracker.statBoosts.GetBoost(Stat.WalkSpeed).additiveChangeAmount) * (upgradeTracker.statBoosts.GetBoost(Stat.WalkSpeed).multiplicativeChangeAmount + 1);
        actualSprintSpeed = (sprintSpeed + upgradeTracker.statBoosts.GetBoost(Stat.SprintSpeed).additiveChangeAmount) * (upgradeTracker.statBoosts.GetBoost(Stat.SprintSpeed).multiplicativeChangeAmount + 1);
        actualJumpPower = (jumpPower + upgradeTracker.statBoosts.GetBoost(Stat.JumpPower).additiveChangeAmount) * (upgradeTracker.statBoosts.GetBoost(Stat.JumpPower).multiplicativeChangeAmount + 1);
        actualForwardBoostOnWallJump = (forwardBoostOnWallJump + upgradeTracker.statBoosts.GetBoost(Stat.WallBoost).additiveChangeAmount) * (upgradeTracker.statBoosts.GetBoost(Stat.WallBoost).multiplicativeChangeAmount + 1);
    }

    public void RefreshKeyBindControls()
    {
        interact = settings.interact;
        useItem = settings.useItem;
        itemSpecial = settings.itemSpecial;
        sprint = settings.sprint;
        crouch = settings.crouch;
        jump = settings.jump;

        if (GetComponent<InteractableUIScript>())
        {
            GetComponent<InteractableUIScript>().ChangeDisplayingBind(interact);
        }
    }

    public void AddLinearVelocity(Vector3 direction, float magnitude)
    {
        if (direction == Vector3.right)
            currXVelocity += magnitude;
        else if (direction == Vector3.up)
            currYVelocity += magnitude;
        else if (direction == Vector3.forward)
            currZVelocity += magnitude;
        else
        {
            print($"velocity failed to add, input direction: {direction}");
        }
    }
    
    public void SetLinearVelocity(Vector3 direction, float magnitude)
    {
        if (direction == Vector3.right)
            currXVelocity = magnitude;
        else if (direction == Vector3.up)
            currYVelocity = magnitude;
        else if (direction == Vector3.forward)
            currZVelocity = magnitude;
        else
        {
            print($"velocity failed to set, input direction: {direction}");
        }
    }
    
    public void AddTempWallBoost(float boost) {tempWallBoost += boost;}

    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        if (hit.collider.CompareTag("BreakableWall") && sliding)
        {
            StartCoroutine(hit.gameObject.GetComponent<BreakableWallScr>().BreakWall());
        } else if (hit.collider.CompareTag("Limb") && sliding)
        {
            BaseEnemyScript enemyScript = hit.collider.GetComponentInParent<BaseEnemyScript>();
            audioSource.PlayOneShot(kickSfx);
            if (!grounded && tempWallBoost > 0)
            {
                enemyScript.SlideKick(30, this, 175);
            }
            else
            {
                enemyScript.SlideKick(20, this, 100);
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("SafeZone"))
        {
            safe = true;
        } else if (other.CompareTag("EnemyHitbox"))
        {
            print("hit");
            EnemyHitboxScript hitboxInfo = other.gameObject.GetComponent<EnemyHitboxScript>();
            if (levelStats.enabled)
            {
                levelStats.ChangeLevelTime(hitboxInfo.timePenalty);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("SafeZone"))
        {
            safe = false;
        }
    }
}
