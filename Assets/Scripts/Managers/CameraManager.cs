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

    // Constants
    private const int CAMERA_PRIORITY_LOW = 0;
    private const int CAMERA_PRIORITY_HIGH = 10;

    // Variables
    private Players localPlayer = Players.PlayerA;
    private bool localPlayerKnown = false;


    private void OnEnable()
    {
        GameManagerNB.OnGameStateChanged += HandleOnGameStateChanged;
        PlayerControllerNB.OnLocalPlayerIdentified += HandleOnLocalPlayerIdentified;
    }
    private void OnDisable()
    {
        GameManagerNB.OnGameStateChanged -= HandleOnGameStateChanged;
        PlayerControllerNB.OnLocalPlayerIdentified -= HandleOnLocalPlayerIdentified;
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

            case GameState.PlayingServe:
                if (localPlayer == Players.PlayerA)
                {
                    ChangeCameraView(CamerasViewsEnum.NorthPlayer);
                }
                else if (localPlayer == Players.PlayerB)
                {
                    ChangeCameraView(CamerasViewsEnum.SouthPlayer);
                }
                break;

            case GameState.Finished:
                ChangeCameraView(CamerasViewsEnum.EastMain);
                break;
        }
    }

    private void HandleOnLocalPlayerIdentified(Players player)
    {
        Debug.Log($"CameraManager - Local player identified: {player}");
        localPlayer = player;
        localPlayerKnown = true;
    }

    private void ChangeCameraView(CamerasViewsEnum cameraView)
    {
        northPlayerCamera.Priority = CAMERA_PRIORITY_LOW;
        southPlayerCamera.Priority = CAMERA_PRIORITY_LOW;
        eastMainCamera.Priority = CAMERA_PRIORITY_LOW;

        Transform activeCameraTransform = null;

        switch (cameraView)
        {
            case CamerasViewsEnum.NorthPlayer:
                northPlayerCamera.Priority = CAMERA_PRIORITY_HIGH;
                activeCameraTransform = northPlayerCamera.transform;
                break;

            case CamerasViewsEnum.SouthPlayer:
                southPlayerCamera.Priority = CAMERA_PRIORITY_HIGH;
                activeCameraTransform = southPlayerCamera.transform;
                break;

            case CamerasViewsEnum.EastMain:
                eastMainCamera.Priority = CAMERA_PRIORITY_HIGH;
                activeCameraTransform = eastMainCamera.transform;
                break;
        }
    }

}
