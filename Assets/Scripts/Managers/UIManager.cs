using UnityEngine;

public class UIManager : MonoBehaviour
{
    [SerializeField] private GameObject panelWaitForPlayerConnected;
    [SerializeField] private GameObject panelWaitForPlayerReady;
    [SerializeField] private GameObject panelPlaying;
    [SerializeField] private GameObject panelPaused;
    [SerializeField] private GameObject panelGameOver;

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
        GameManagerNB.OnGameStateChanged += HandleOnGameStateChanged;
    }
    private void OnDisable()
    {
        GameManagerNB.OnGameStateChanged -= HandleOnGameStateChanged;
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