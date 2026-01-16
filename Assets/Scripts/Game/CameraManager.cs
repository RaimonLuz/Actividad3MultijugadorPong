using Unity.Cinemachine;
using UnityEngine;

public class CameraManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private MatchManagerSO matchManagerSO;
    [SerializeField] private CinemachineCamera northCamera;
    [SerializeField] private CinemachineCamera southCamera;

    private void OnEnable()
    {
        matchManagerSO.OnActivateCamera += ActivateCamera;
    }
    private void OnDisable()
    {
        matchManagerSO.OnActivateCamera -= ActivateCamera;
    }

    private void ActivateCamera(CourtSides side)
    {
        northCamera.gameObject.SetActive(side == CourtSides.North);
        southCamera.gameObject.SetActive(side == CourtSides.South);
    }
}
