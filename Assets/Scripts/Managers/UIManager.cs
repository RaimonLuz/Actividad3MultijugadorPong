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
    [Header("Text References")]
    [SerializeField] private TextMeshProUGUI playerAServeText;
    [SerializeField] private TextMeshProUGUI playerBServeText;
    [SerializeField] private TextMeshProUGUI pointsPlayerAText;
    [SerializeField] private TextMeshProUGUI pointsPlayerBText;
    [SerializeField] private TextMeshProUGUI gamesPlayerAText;
    [SerializeField] private TextMeshProUGUI gamesPlayerBText;
    [SerializeField] private TextMeshProUGUI hudHintText;

    [Header("Texts")]
    [SerializeField][Multiline] private string waitForOtherPlayerReadyMessage = "¡Genial estás apunto! Espera a que el otro jugador esté listo...";
    [SerializeField][Multiline] private string waitForOtherPlayerServeMessage = "Esperando a que el otro jugador saque";
    [SerializeField][Multiline] private string playerMustServeMessage = "Pulsa Space para sacar";


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

    private void HandleOnPointsPlayerAChanged(int points)
    {
        pointsPlayerAText.text = FormatTennisPoints(points);
    }

    private void HandleOnPointsPlayerBChanged(int points)
    {
        pointsPlayerBText.text = FormatTennisPoints(points);
    }

    private void HandleOnGamesPlayerAChanged(int games)
    {
        gamesPlayerAText.text = games.ToString();
    }

    private void HandleOnGamesPlayerBChanged(int games)
    {
        gamesPlayerBText.text = games.ToString();
    }

    private string FormatTennisPoints(int points)
    {
        return points switch
        {
            0 => "0",
            1 => "15",
            2 => "30",
            3 => "40",
            _ => "AD"
        };
    }


    private void HandleOnThisPlayerIsReady()
    {
        panelWaitForPlayerReady.GetComponentInChildren<TextMeshProUGUI>().text = waitForOtherPlayerReadyMessage;
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
            case GameState.PlayingServe:
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