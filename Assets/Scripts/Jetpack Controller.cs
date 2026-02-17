using UnityEditor;
using UnityEngine;

public class JetpackController : MonoBehaviour
{
    public float maxFuel;
    public float currentFuel;
    public float jetForce = 25;
    public float hoverDamping = 40;
    private Rigidbody rb;
    public bool isJetpackOn { get; private set; }

    private enum JetpackState
    {
        Hover,
        Ascend,
        Descend
    }

    [SerializeField] private JetpackState currentState;

    private AdvancedMoveController advancedMoveController;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        advancedMoveController ??= GetComponent<AdvancedMoveController>();
        rb ??= GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
    }

    private void FixedUpdate()
    {
        if (!isJetpackOn) return;
        HandleJetpackState();
    }

    public void RequestJetpack()
    {
        if (rb == null) return;

        if (!advancedMoveController.isGrounded)
        {
            isJetpackOn = !isJetpackOn;
            RequestHover();
        }
    }

    public void RequestAscend()
    {
        if (!isJetpackOn) return;
        currentState = JetpackState.Ascend;
    }
    public void RequestDescend()
    {
        if (!isJetpackOn) return;
        currentState = JetpackState.Descend;
    }

    public void RequestHover()
    {
        if (!isJetpackOn) return;
        currentState = JetpackState.Hover;
    }

    private void HandleJetpackState()
    {
        switch (currentState)
        {
            case JetpackState.Hover:
                HandleHover(); 
                break;
            case JetpackState.Ascend:
                HandleAscend();
                break;
            case JetpackState.Descend:
                HandleDescend();
                break;
        }
    }
    public void HandleHover()
    {
        if (rb != null)
        {
            float fixedForce = jetForce - (rb.linearVelocity.y * hoverDamping);
            rb.AddForce(Vector3.up * fixedForce, ForceMode.Acceleration);
        }
    }

    public void HandleAscend()
    {
        rb.AddForce(Vector3.up * (jetForce * 2), ForceMode.Acceleration);
    }

    public void HandleDescend()
    {
        rb.AddForce(Vector3.down * jetForce, ForceMode.Acceleration);
    }
}

