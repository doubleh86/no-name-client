using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerMove : MonoBehaviour
{
    public float moveSpeed = 6f;
    public float gravity = -20f;
    public float jumpHeight = 1.2f;

    private CharacterController _cc;
    private Vector3 _velocity;

    void Awake()
    {
        _cc = GetComponent<CharacterController>();
    }

    void Update()
    {
        // WASD 입력
        float x = Input.GetAxisRaw("Horizontal");
        float z = Input.GetAxisRaw("Vertical");

        Vector3 move = (transform.right * x + transform.forward * z).normalized;
        _cc.Move(move * moveSpeed * Time.deltaTime);

        // 바닥 체크
        if (_cc.isGrounded && _velocity.y < 0)
            _velocity.y = -2f;

        // 점프
        if (Input.GetKeyDown(KeyCode.Space) && _cc.isGrounded)
            _velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);

        // 중력
        _velocity.y += gravity * Time.deltaTime;
        _cc.Move(_velocity * Time.deltaTime);
    }
}