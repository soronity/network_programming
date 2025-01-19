using UnityEngine;
using Unity.Netcode;

public class PlayerMovement : NetworkBehaviour
{
    private Rigidbody2D rb;
    private Animator animator;
    private SpriteRenderer spriteRenderer;

    // NetworkVariables to synchronize state across clients
    private NetworkVariable<bool> isFacingLeft = new NetworkVariable<bool>();
    private NetworkVariable<bool> isRunning = new NetworkVariable<bool>();

    public float speed = 5f;
    public float jumpForce = 7f;
    private bool isGrounded = true;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        // Subscribe to changes in direction and running state
        isFacingLeft.OnValueChanged += OnDirectionChanged;
        isRunning.OnValueChanged += OnRunningStateChanged;
    }

    private void Update()
    {
        if (!IsOwner) return; // Only the local player controls movement

        // Handle movement input
        float move = Input.GetAxis("Horizontal");
        rb.linearVelocity = new Vector2(move * speed, rb.linearVelocity.y);

        // Update direction through ServerRpc when moving
        if (move < 0 && !isFacingLeft.Value)
        {
            UpdateDirectionServerRpc(true); // Facing left
        }
        else if (move > 0 && isFacingLeft.Value)
        {
            UpdateDirectionServerRpc(false); // Facing right
        }

        // Handle animations
        bool currentlyRunning = move != 0;
        if (isRunning.Value != currentlyRunning)
        {
            UpdateRunningStateServerRpc(currentlyRunning); // Sync running state
        }

        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
            animator.SetBool("isJumping", true);
            isGrounded = false;
        }
    }

    [ServerRpc]
    private void UpdateDirectionServerRpc(bool facingLeft)
    {
        isFacingLeft.Value = facingLeft; // Update direction for all clients
    }

    [ServerRpc]
    private void UpdateRunningStateServerRpc(bool running)
    {
        isRunning.Value = running; // Update running state for all clients
    }

    private void OnDirectionChanged(bool oldValue, bool newValue)
    {
        // Flip the sprite when direction changes
        spriteRenderer.flipX = newValue;
    }

    private void OnRunningStateChanged(bool oldValue, bool newValue)
    {
        // Update the running animation state
        animator.SetBool("isRunning", newValue);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            isGrounded = true;
            animator.SetBool("isJumping", false);
        }
    }

    public void ResetAnimation()
    {
        animator.SetBool("isRunning", false);
        animator.SetBool("isJumping", false);
    }

    public void EnableMovement()
    {
        enabled = true;
    }
}
