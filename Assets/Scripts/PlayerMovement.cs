using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 5f;
    public float touchSensitivity = 2f;

    [Header("Movement Boundaries (Optional)")]
    public bool useBoundaries = false;
    public Vector2 minBounds = new Vector2(-10f, -10f);
    public Vector2 maxBounds = new Vector2(10f, 10f);

    private Vector2 movement;
    private Vector2 touchStartPos;
    private bool isTouching = false;

    void Start()
    {
        SetCameraBoundaries();
    }


    void Update()
    {
        HandleInput();
        MovePlayer();
    }

    void HandleInput()
    {
        // Reset movement
        movement = Vector2.zero;

        // Check if we're on a touch device
        if (Input.touchCount > 0 || Input.GetMouseButton(0))
        {
            HandleTouchInput();
        }
        else
        {
            HandleKeyboardInput();
        }
    }

    void HandleKeyboardInput()
    {
        // Arrow keys and WASD
        if (Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.A))
            movement.x = -1f;
        else if (Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.D))
            movement.x = 1f;

        if (Input.GetKey(KeyCode.UpArrow) || Input.GetKey(KeyCode.W))
            movement.y = 1f;
        else if (Input.GetKey(KeyCode.DownArrow) || Input.GetKey(KeyCode.S))
            movement.y = -1f;
    }

    void HandleTouchInput()
    {
        Vector2 currentTouchPos;

        // Handle both touch and mouse input for WebGL compatibility
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
            currentTouchPos = Camera.main.ScreenToWorldPoint(touch.position);

            if (touch.phase == TouchPhase.Began)
            {
                touchStartPos = currentTouchPos;
                isTouching = true;
            }
            else if (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled)
            {
                isTouching = false;
            }
        }
        else if (Input.GetMouseButton(0))
        {
            currentTouchPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);

            if (Input.GetMouseButtonDown(0))
            {
                touchStartPos = currentTouchPos;
                isTouching = true;
            }
            else if (Input.GetMouseButtonUp(0))
            {
                isTouching = false;
            }
        }
        else
        {
            currentTouchPos = Vector2.zero;
            isTouching = false;
        }

        // Calculate movement direction from touch/drag
        if (isTouching)
        {
            Vector2 touchDelta = currentTouchPos - touchStartPos;
            movement = touchDelta.normalized * touchSensitivity;

            // Clamp movement to prevent too fast movement
            movement = Vector2.ClampMagnitude(movement, 1f);
        }
    }

    void MovePlayer()
    {
        if (movement != Vector2.zero)
        {
            // Calculate new position
            Vector3 newPosition = transform.position + (Vector3)(movement * moveSpeed * Time.deltaTime);

            // Apply boundaries if enabled
            if (useBoundaries)
            {
                newPosition.x = Mathf.Clamp(newPosition.x, minBounds.x, maxBounds.x);
                newPosition.y = Mathf.Clamp(newPosition.y, minBounds.y, maxBounds.y);
            }

            // Apply the movement
            transform.position = newPosition;
        }
    }

    // Optional: Visual debug for boundaries
    void OnDrawGizmosSelected()
    {
        if (useBoundaries)
        {
            Gizmos.color = Color.yellow;
            Vector3 center = new Vector3((minBounds.x + maxBounds.x) / 2, (minBounds.y + maxBounds.y) / 2, 0);
            Vector3 size = new Vector3(maxBounds.x - minBounds.x, maxBounds.y - minBounds.y, 0);
            Gizmos.DrawWireCube(center, size);
        }
    }

    void SetCameraBoundaries()
    {
        Camera cam = Camera.main;
        if (cam != null)
        {
            float camHeight = cam.orthographicSize;
            float camWidth = camHeight * cam.aspect;

            // Add small offset so player doesn't clip edge
            float offset = 0.2f;

            minBounds = new Vector2(-camWidth + offset, -camHeight + offset);
            maxBounds = new Vector2(camWidth - offset, camHeight - offset);
            useBoundaries = true;
        }
    }
}

