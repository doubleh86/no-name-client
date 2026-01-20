using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    public InputActionReference moveAction;

    public float moveSpeed = 6f;
    public float rotateSpeed = 14f;

    public float gravity = -20f;
    public float groundedStick = -2f;

    private CharacterController _cc;
    private float _verticalVel;

    void Awake()
    {
        _cc = GetComponent<CharacterController>();
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void OnEnable() => moveAction.action.Enable();
    void OnDisable() => moveAction.action.Disable();

    void Update()
    {
        Vector2 move = moveAction.action.ReadValue<Vector2>();
        move = Vector2.ClampMagnitude(move, 1f);

        // Cinemachine 쓰면 Main Camera 기준이 정석
        Transform cam = Camera.main.transform;

        Vector3 camForward = cam.forward;
        camForward.y = 0f;
        camForward.Normalize();

        Vector3 camRight = cam.right;
        camRight.y = 0f;
        camRight.Normalize();

        Vector3 moveDir = (camForward * move.y + camRight * move.x);
        if (moveDir.sqrMagnitude > 0.0001f)
        {
            moveDir.Normalize();

            Quaternion targetRot = Quaternion.LookRotation(moveDir);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime * rotateSpeed);
        }
        else
        {
            moveDir = Vector3.zero;
        }

        // 중력/접지
        if (_cc.isGrounded) 
            _verticalVel = groundedStick;
        else 
            _verticalVel += gravity * Time.deltaTime;

        Vector3 velocity = moveDir * moveSpeed;
        velocity.y = _verticalVel;

        _cc.Move(velocity * Time.deltaTime);
    }
}