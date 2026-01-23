using System;
using UnityEngine;

[CreateAssetMenu(fileName = "MyMatchManagerSO", menuName = "Tenis Scriptable Objects/MyMatchManagerSO")]
public class MatchManagerSO : ScriptableObject
{
    // Constants
    // The x line of the court ground
    public const float COURT_GROUND_MIN_X = -5f;
    public const float COURT_GROUND_MAX_X = 5f;
    public const float COURT_GROUND_CENTER_X = 0f;

    // The z line of the court ground
    public const float COURT_GROUND_MAX_Z = 10f;
    public const float COURT_GROUND_CENTER_Z = 0f;
    public const float COURT_GROUND_MIN_Z = -10f;

    // net height
    public const float NET_HEIGHT = 1.1f;

    // court height
    public const float COURT_GROUND_Y = 0f;

    // Variables
    private BallController currentBallController;
    private GameManagerNB gameManager;

    // Events
    public event Action<Vector3> OnShotBall;
    public event Action OnThisPlayerIsReady;
    public event Action<ulong> OnPlayerIsReady;


    public void RegisterGameManager(GameManagerNB gameManager)
    {
        this.gameManager = gameManager;
    }

    public GameState GetGameState()
    {
        if (gameManager == null)
            throw new Exception("GameManager not registered");

        return gameManager.CurrentGameState;
    }

    public void SetCurrentBallController(BallController ballController)
    {
        if(currentBallController == null)
        {
            currentBallController = ballController;
        }
    }


    public BallController GetCurrentBallController() 
    { 
        return currentBallController;
    }


    public void NotifyShotBall(Vector3 targetPosition)
    {
        OnShotBall?.Invoke(targetPosition);
    }

    public void NotifyThisPlayerIsReady()
    {
        OnThisPlayerIsReady?.Invoke();
    }


    public static Vector3 GetRandomPositionInsideCourtSide(CourtSides targetSide, float internalMargin)
    {
        float z = 0f;
        float y = COURT_GROUND_Y;
        float x = UnityEngine.Random.Range(COURT_GROUND_MIN_X + internalMargin, COURT_GROUND_MAX_X - internalMargin);
        
        if (targetSide == CourtSides.South) { 
            z = UnityEngine.Random.Range(COURT_GROUND_MIN_Z + internalMargin, COURT_GROUND_CENTER_Z - internalMargin);
        }
        else
        {
            z = UnityEngine.Random.Range(COURT_GROUND_CENTER_Z + internalMargin, COURT_GROUND_MAX_Z - internalMargin);
        }

        return new Vector3(x, y, z);
    }

    public static CourtSides GetCourtSideFromPosition(Vector3 position)
    {
        if (position.z < COURT_GROUND_CENTER_Z)
            return CourtSides.South;
        else
            return CourtSides.North;
    }

    public static CourtSides GetOppositeCourtSide(CourtSides courtSide)
    {
        if (courtSide == CourtSides.North)
            return CourtSides.South;
        else
            return CourtSides.North;
    }

    internal void NotifyPlayerReady(ulong ownerClientId)
    {
        OnPlayerIsReady?.Invoke(ownerClientId);
    }
}
