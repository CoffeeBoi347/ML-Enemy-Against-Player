using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    #region Fields

    [SerializeField] private PlayerInput _input;
    [SerializeField] private float _walkSpeed;
    [SerializeField] private float _crouchSpeed;
    [SerializeField] private float _runSpeed;
    [SerializeField] private float _jumpHeight;
    [SerializeField] private float _crouchHeight;
    [SerializeField] private LayerMask _groundLayer;
    
    private const float _gravity = -9.81f;
    private const float MAX_DIAGONAL_MOVEMENT = 1.0f;
    private const float JUMP_FACTOR = 2.0f;
    private const float GROUND_CHECK_RADIUS = 1f;

    public PlayerState _currentState { get; private set; }
    private CharacterController _controller;
    private float _verticalVelocity;
    private float _originalHeight;
   
    public enum PlayerState
    {
        Idle,
        Walking,
        Running,
        Jumping,
        Crouching
    }

    #endregion

    #region Setup

    private void Awake()
    {
        _controller = GetComponent<CharacterController>();

        if (_input == null)
        {
            Debug.LogError($"Player controller on {this.gameObject.name} is missing a reference to Player Input component.");
            this.enabled = false;
        }
    }

    private void Start()
    {
        _originalHeight = _controller.height;
    }

    private void OnEnable()
    {
        _input.onJumpReleased += OnPlayerJump;
    }

    private void OnDisable()
    {
        _input.onJumpReleased -= OnPlayerJump;
    }

    private void Update()
    {
        UpdateState();
        OnPlayerMove();
    }

    #endregion

    #region Input

    private void OnPlayerMove()
    {
        Vector3 moveDirection = _input.MoveInput.x * transform.right + _input.MoveInput.y * transform.forward;
        if (moveDirection.magnitude > MAX_DIAGONAL_MOVEMENT)
            moveDirection.Normalize();

        float currentSpeed = _walkSpeed;

        switch (_currentState)
        {
            case PlayerState.Walking:
                currentSpeed = _walkSpeed;
                break;

            case PlayerState.Crouching:
                currentSpeed = _crouchSpeed;
                break;

            case PlayerState.Running:
                currentSpeed = _runSpeed;
                break;
        }

        if (_controller.isGrounded && _verticalVelocity < 0f)
            _verticalVelocity = -JUMP_FACTOR;

        _verticalVelocity += _gravity * Time.deltaTime; 
        moveDirection.y = _verticalVelocity;

        _controller.Move(moveDirection * currentSpeed * Time.deltaTime);
    }

    private void UpdateState()
    {
        bool grounded = IsGrounded();
        bool isMoving = _input.MoveInput.magnitude > 0;

        if (_input.IsCrouching && grounded)
        {
            _currentState = PlayerState.Crouching;
            _controller.height = _crouchHeight;
        }

        else
        {
            _controller.height = _originalHeight;

            if (_input.IsRunning && isMoving)
                _currentState = PlayerState.Running;

            else if (_input.MoveInput.magnitude > 0)
                _currentState = PlayerState.Walking;
            else
                _currentState = PlayerState.Idle;
        }
    }

    private void OnPlayerJump()
    {
        if (!_controller.isGrounded) return;

        _verticalVelocity = Mathf.Sqrt(_jumpHeight * -JUMP_FACTOR * _gravity); // uses the velocity formula for jumping: v = sqrt(-2gd)
        _currentState = PlayerState.Jumping;
    }

    private bool IsGrounded()
    {
        Vector3 origin = transform.position + Vector3.down * (_controller.height/2);
        return Physics.CheckSphere(origin, GROUND_CHECK_RADIUS, _groundLayer);
    }

    #endregion
}