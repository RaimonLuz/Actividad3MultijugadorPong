using System;
using TMPro;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    [SerializeField] private MatchManagerSO matchManagerSO;

    [Header("Panels")]
    [SerializeField] private GameObject panelWaitForPlayerConnected;
    [SerializeField] private GameObject panelWaitForPlayerReady;
    [SerializeField] private GameObject panelPlaying;
    [SerializeField] private GameObject panelPaused;
    [SerializeField] private GameObject panelGameOver;

    [Header("Texts")]
    [SerializeField] private String waitForOtherPlayerReadyText = "¡Genial estás apunto! Espera a que el otro jugador esté apunto...";

    private enum PanelsOnScreen
    {
        WaitingForAllPlayersConnected,
        WaitingForAllPlayersReady,
        Playing,
        Paused,
        Finished
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        SetPanelOn(PanelsOnScreen.WaitingForAllPlayersConnected);
    }

    private void OnEnable()
    {
        // Subscribe to MatchManagerSO's events
        matchManagerSO.OnThisPlayerIsReady += HandleOnThisPlayerIsReady;

        // Subscribe to GameManagerNB's events
        GameManagerNB.OnGameStateChanged += HandleOnGameStateChanged;
        GameManagerNB.OnPointsPlayerAChanged += HandleOnPointsPlayerAChanged;
        GameManagerNB.OnPointsPlayerBChanged += HandleOnPointsPlayerBChanged;
        GameManagerNB.OnGamesPlayerAChanged += HandleOnGamesPlayerAChanged;
        GameManagerNB.OnGamesPlayerBChanged += HandleOnGamesPlayerBChanged;
    }
    private void OnDisable()
    {
        // Unsubscribe from MatchManagerSO's events
        matchManagerSO.OnThisPlayerIsReady -= HandleOnThisPlayerIsReady;

        // Unsubscribe from MatchManagerSO's events
        GameManagerNB.OnGameStateChanged -= HandleOnGameStateChanged;
        GameManagerNB.OnPointsPlayerAChanged -= HandleOnPointsPlayerAChanged;
        GameManagerNB.OnPointsPlayerBChanged -= HandleOnPointsPlayerBChanged;
        GameManagerNB.OnGamesPlayerAChanged -= HandleOnGamesPlayerAChanged;
        GameManagerNB.OnGamesPlayerBChanged -= HandleOnGamesPlayerBChanged;
    }

    private void HandleOnPointsPlayerAChanged(int obj)
    {
        throw new NotImplementedException();
    }

    private void HandleOnPointsPlayerBChanged(int obj)
    {
        throw new NotImplementedException();
    }

    private void HandleOnGamesPlayerBChanged(int obj)
    {
        throw new NotImplementedException();
    }

    private void HandleOnGamesPlayerAChanged(int obj)
    {
        throw new NotImplementedException();
    }




    private void HandleOnThisPlayerIsReady()
    {
        panelWaitForPlayerReady.GetComponentInChildren<TextMeshProUGUI>().text = waitForOtherPlayerReadyText;
    }

    private void HandleOnGameStateChanged(GameState newGameState)
    {
        switch(newGameState)
        {
            case GameState.WaitingForAllPlayersConnected:
                SetPanelOn(PanelsOnScreen.WaitingForAllPlayersConnected);
                break;
            case GameState.WaitingForAllPlayersReady:
                SetPanelOn(PanelsOnScreen.WaitingForAllPlayersReady);
                break;
            case GameState.Playing:
                SetPanelOn(PanelsOnScreen.Playing);
                break;
            case GameState.Paused:
                SetPanelOn(PanelsOnScreen.Paused);
                break;
            case GameState.Finished:
                SetPanelOn(PanelsOnScreen.Finished);
                break;
        }
    }

    private void SetPanelOn(PanelsOnScreen panelOnScreen)
    {
        panelWaitForPlayerConnected.SetActive(panelOnScreen == PanelsOnScreen.WaitingForAllPlayersConnected);
        panelWaitForPlayerReady.SetActive(panelOnScreen == PanelsOnScreen.WaitingForAllPlayersReady);
        panelPlaying.SetActive(panelOnScreen == PanelsOnScreen.Playing);
        panelPaused.SetActive(panelOnScreen == PanelsOnScreen.Paused);
        panelGameOver.SetActive(panelOnScreen == PanelsOnScreen.Finished);
    }
}