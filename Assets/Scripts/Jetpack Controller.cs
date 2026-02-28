using UnityEditor;
using UnityEngine;
using DG.Tweening;
[RequireComponent(typeof(ProgressBar))]
public class JetpackController : MonoBehaviour
{
    [Header("Fuel Settings")]
    [Tooltip("The max cap for fuel")]
    [SerializeField] private float maxFuel = 100;
    [Tooltip("Current amount of fuel")]
    private float MaxFuel
    {
        get {  return maxFuel; }
        set { maxFuel = Mathf.Max(0,value); }
    }
    [Tooltip("Current amount of fuel")]
    [SerializeField] private float currentFuel;
    [Tooltip("Current amount of fuel")]
    private float CurrentFuel
    {
        get { return currentFuel; }
        set { currentFuel = Mathf.Clamp(value,0,MaxFuel); }
    }
    [Tooltip("Rate of how much fuel is used")]
    [SerializeField] private float fuelUsageRate = 4;
    [Tooltip("Rate of how much fuel is refilled")]
    [SerializeField] private float fuelRefillRate = 3;

    [Header("Jetpack Settings")]
    [Tooltip("How much force for hovering & de/as-cending")]
    [SerializeField] private float jetForce = 25;
    [Tooltip("How much boost ascending *")]
    [SerializeField] private float ascendBoost = 2;
    [Tooltip("How much the player gets pushed down")]
    [SerializeField] private float hoverDamping = 40;

    [Header("Camera Setting")]
    [Tooltip("How much the FOV increases by")]
    [SerializeField] private float fovIncrease;
    [Tooltip("How long it takes to transition from default to target fov")]
    [SerializeField] float fovTransitionSpeed;
    private Camera mainCam;
    [Tooltip("Target FOV that set the main camera FOV")]
    private float fovTarget;
    [Tooltip("The default Fov when jetpack is off")]
    private float fovDefault;

    
    

    [Header("Feedback Settings")]
    [Tooltip("How much to spike the force when changing states")]
    [SerializeField] private float feedbackForce = 45;
    [Tooltip("How long the feedback force last")]
    [SerializeField] private float feedbackDuration = 1;
    [Tooltip("How many times should feedback loop (if * 1 loop will never end)")]
    [SerializeField] private int feedbackLoop = 2;
    [Tooltip("The feedback force that is used to add force")]
    private float feedbackForceCurrent = 0f;

    [Tooltip("Bool used to handle is jetpack states should update")]
    private bool isJetpackOn { get; set; }
    [SerializeField] private Animator animator;
    private Rigidbody rb;

    private enum JetpackState
    {
        Hover,
        Ascend,
        Descend
    }

    private JetpackState currentState;

    private AdvancedMoveController advancedMoveController;

    [SerializeField] private ParticleSystem jet_1, jet_2;

    private ProgressBar progressBar;

    void Start()
    {
        CurrentFuel = MaxFuel;
        advancedMoveController ??= GetComponent<AdvancedMoveController>();
        rb ??= GetComponent<Rigidbody>();
        animator = GetComponentInChildren<Animator>();
        progressBar ??= GetComponent<ProgressBar>();
        progressBar.SetFillMax(MaxFuel);
        progressBar.UpdateBar(CurrentFuel);
        mainCam = Camera.main;
        if (mainCam != null )
        {
            fovDefault = mainCam.fieldOfView;
            fovTarget = fovDefault + fovIncrease;
        }
    }

    private void Update()
    {
        if (!isJetpackOn)
        {
            RefillFuel();
        }
        //Turns off the jetpack when grounded
        if (advancedMoveController.isGrounded)
        {
            TurnOffJetpack();
        }
    }

    private void FixedUpdate()
    {
        if (!isJetpackOn) return;
        HandleJetpackState();
    }
    
    #region Requests
    /// <summary>
    /// Checks if player is grounded & can use jetpack, then flips jetpack on bool and enters hover state
    /// </summary>
    public void RequestJetpack()
    {
        if (rb == null) return;

        if (!advancedMoveController.isGrounded && CanUseJetpack())
        {
            isJetpackOn = !isJetpackOn;
            SetJetState(isJetpackOn);
            RequestHover();
        }
    }
    /// <summary>
    /// Switches current state to ascending, while flip feedback force
    /// </summary>
    public void RequestAscend()
    {
        if (!isJetpackOn) return;
        currentState = JetpackState.Ascend;
        feedbackForce = Mathf.Abs(feedbackForce);
    }
    /// <summary>
    /// Switches current state to decending, while flip feedback force
    /// </summary>
    public void RequestDescend()
    {
        if (!isJetpackOn) return;
        currentState = JetpackState.Descend;
        feedbackForce = -Mathf.Abs(feedbackForce);
    }

    /// <summary>
    /// Switches current state to hover and using the feedback force
    /// </summary>
    public void RequestHover()
    {
        if (!isJetpackOn) return;
        currentState = JetpackState.Hover;
        FeedbackForce();
    }
    #endregion

    #region Handlers
    /// <summary>
    /// A method that calls a jetpack methods using switch cases based on current state, while also draining fuel
    /// </summary>
    private void HandleJetpackState()
    {
        if (rb == null) return;

        DrainFuel();

        switch (currentState)
        {
            case JetpackState.Hover:HandleHover();break;
            case JetpackState.Ascend:HandleAscend();break;
            case JetpackState.Descend:HandleDescend();break;
        }
    }

    /// <summary>
    /// Adds upward force to rigidbody with damping and feedback for hover effect
    /// </summary>
    public void HandleHover()
    {
        float fixedForce = jetForce - (rb.linearVelocity.y * hoverDamping) + feedbackForceCurrent;
        rb.AddForce(Vector3.up * fixedForce, ForceMode.Acceleration);
    }

    /// <summary>
    /// Adds upward force to rigidbody with ascend boost
    /// </summary>
    public void HandleAscend()
    {
        rb.AddForce(Vector3.up * (jetForce * ascendBoost), ForceMode.Acceleration);
    }

    /// <summary>
    /// Adds downward force to rigidbody
    /// </summary>
    public void HandleDescend()
    {
        rb.AddForce(Vector3.down * jetForce, ForceMode.Acceleration);
    }
    /// <summary>
    /// Drains the fuel overtime using fuel usage rate and checks if jetpack should turn off
    /// </summary>

    #endregion
    /// <summary>
    /// Bool to check if current fuel is greater than 0 returns true else returns false
    /// </summary>
    /// <returns></returns>
    private bool CanUseJetpack()
    {
        return CurrentFuel > 0;
    }
    private void DrainFuel()
    {
        CurrentFuel -= fuelUsageRate * Time.deltaTime;
        progressBar.UpdateBar(CurrentFuel);
        if (!CanUseJetpack())
        {
            TurnOffJetpack();
        }
    }
    /// <summary>
    /// Refills the fuel overtime using fuel refill rate
    /// </summary>
    private void RefillFuel()
    {
        CurrentFuel += fuelRefillRate * Time.deltaTime;
        progressBar.UpdateBar(CurrentFuel);
    }
    /// <summary>
    /// Hard sets jetpack to off
    /// </summary>
    private void TurnOffJetpack()
    {
        isJetpackOn = false;
        animator.SetBool(MovementController.AnimationID_JetpackBool, false);
        SetJetState(false);
    }
    /// <summary>
    /// Turns animation, particles and FOV off/on for jetpack (true = on, false = off)
    /// </summary>
    /// <param name="value"></param>
    public void SetJetState(bool value)
    {
        animator.SetBool(MovementController.AnimationID_JetpackBool, isJetpackOn);
        if (value)
        {
            mainCam.DOFieldOfView(fovTarget, fovTransitionSpeed).SetEase(Ease.InOutSine);
            if (jet_1 && jet_2 != null)
            {
                jet_1.Play();
                jet_2.Play();
            }
        }
        else
        {
            mainCam.DOFieldOfView(fovDefault, fovTransitionSpeed).SetEase(Ease.InOutSine);
            if (jet_1 && jet_2 != null)
            {
                jet_1.Stop();
                jet_2.Stop();
            }
        }
    }
    /// <summary>
    /// Adds extra force for player feedback when ascending/descending
    /// </summary>
    private void FeedbackForce()
    {
        DOTween.Kill(this);

        feedbackForceCurrent = 0;
        DOTween.To(
            () => feedbackForceCurrent,
            x => feedbackForceCurrent = x,
            feedbackForce,
            feedbackDuration
        )
        .SetLoops(feedbackLoop, LoopType.Yoyo)
        .SetEase(Ease.InOutBack)
        .SetId(this);
    }
}

