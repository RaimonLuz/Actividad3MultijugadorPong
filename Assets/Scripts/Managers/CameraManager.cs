using System;
using Unity.Cinemachine;
using UnityEngine;

public class CameraManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private MatchManagerSO matchManagerSO;
    [SerializeField] private CinemachineCamera northPlayerCamera;
    [SerializeField] private CinemachineCamera southPlayerCamera;
    [SerializeField] private CinemachineCamera eastMainCamera;

    // Variables   
    private const int CAMERA_PRIORITY_LOW = 0;
    private const int CAMERA_PRIORITY_HIGH = 10;

    private void OnEnable()
    {
        GameManagerNB.OnGameStateChanged += HandleOnGameStateChanged;
    }
    private void OnDisable()
    {
        GameManagerNB.OnGameStateChanged -= HandleOnGameStateChanged;
    }

    private void Start()
    {
        ChangeCameraView(CamerasViewsEnum.EastMain);
    }

    private void HandleOnGameStateChanged(GameState state)
    {
        switch (state)
        {
            case GameState.WaitingForAllPlayersConnected:
                ChangeCameraView(CamerasViewsEnum.EastMain);
                break;

            case GameState.Playing:
                ChangeCameraView(CamerasViewsEnum.NorthPlayer);
                break;

            case GameState.Finished:
                ChangeCameraView(CamerasViewsEnum.EastMain);
                break;
        }
    }

    private void ChangeCameraView(CamerasViewsEnum cameraView)
    {

        // Deactivate all cameras
        northPlayerCamera.Priority = CAMERA_PRIORITY_LOW;
        southPlayerCamera.Priority = CAMERA_PRIORITY_LOW;
        eastMainCamera.Priority = CAMERA_PRIORITY_LOW;

        // Activate the selected camera
        switch (cameraView)
        {
            case CamerasViewsEnum.NorthPlayer:
                northPlayerCamera.Priority = CAMERA_PRIORITY_HIGH;
                break;

            case CamerasViewsEnum.SouthPlayer:
                southPlayerCamera.Priority = CAMERA_PRIORITY_HIGH;
                break;

            case CamerasViewsEnum.EastMain:
                eastMainCamera.Priority = CAMERA_PRIORITY_HIGH;
                break;
        }
    }
}
