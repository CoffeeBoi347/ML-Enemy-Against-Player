using System;
using UnityEngine;

public class PlayerInput : MonoBehaviour
{
    public Action onJumpReleased;
    public bool IsRunning => Input.GetKey(KeyCode.LeftShift);
    public bool IsCrouching => Input.GetKey(KeyCode.C);
    public Vector2 MoveInput { get; private set; }

    private void Update()
    {
        MoveInput = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));

        if (Input.GetKeyDown(KeyCode.Space))
            onJumpReleased?.Invoke();
    }
}