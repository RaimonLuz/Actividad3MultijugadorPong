using System;
using UnityEditor;
using UnityEngine;

public class PlayerController : MonoBehaviour
{

    [SerializeField] private BallController ballController;

    [SerializeField][Range(1f, 10f)] float maxMoveSpeed = 5f;
    [SerializeField][Range(1f, 10f)] float hitRange = 5f;
    [SerializeField][Range(1f, 30f)] float shotPower = 5f;


    private Vector3 verticalVelocity;
    private Transform cameraTransform;
    private InputController inputController;
    private CharacterController characterController;

    private void Awake()
    {
        inputController = GetComponent<InputController>();
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        characterController = GetComponent<CharacterController>();

        cameraTransform = Camera.main.transform;
        
    }

    private void OnEnable()
    {
        inputController.OnShot += OnShot;
    }
    private void OnDisable()
    {
        inputController.OnShot -= OnShot;
    }

    private void OnShot()
    {
        if (Vector3.Distance(transform.position, ballController.gameObject.transform.position) < hitRange)
        { 
            ballController.ApplyShot(transform.position, shotPower);
        }
    }


    // Update is called once per frame
    void Update()
    {
        MovePlayer();
        ApplyGravity();
    }

    private void MovePlayer()
    {
        // Convert input to world space movement
        Vector3 camForward = cameraTransform.forward;
        Vector3 camRight = cameraTransform.right;

        camForward.y = 0f;
        camRight.y = 0f;
        camForward.Normalize();
        camRight.Normalize();

        Vector3 direction = camRight * inputController.MoveInput.x + camForward * inputController.MoveInput.y;
        characterController.Move(Time.deltaTime * maxMoveSpeed * direction);
    }

    private void ApplyGravity()
    {
        if (characterController.isGrounded && verticalVelocity.y < 0)
        {
            verticalVelocity.y = -2f; // Move player to the ground  
        }
        else
        {
            verticalVelocity.y += Physics.gravity.y * Time.deltaTime;
        }

        characterController.Move(verticalVelocity * Time.deltaTime);
    }


    #if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;

        Gizmos.DrawWireSphere(transform.position, hitRange);
    }
    #endif

}
