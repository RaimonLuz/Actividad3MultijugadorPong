using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputController : MonoBehaviour
{
    InputSystem_Actions inputActions;
    private Vector2 moveInput;

    public Vector2 MoveInput { get => moveInput; }
    public event Action OnShot;

    private void Awake()
    {
    }

    private void OnEnable()
    {
        inputActions = new InputSystem_Actions();
        inputActions.Player.Enable();
        inputActions.Player.Shot.performed += OnShotInput;
    }

    private void OnDisable()
    {
        inputActions.Player.Disable();
    }



    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        moveInput = inputActions.Player.Move.ReadValue<Vector2>();
    }


    private void OnShotInput(InputAction.CallbackContext context)
    {
        OnShot?.Invoke();
    }
}
