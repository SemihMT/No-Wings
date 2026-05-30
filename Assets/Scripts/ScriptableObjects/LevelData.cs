using UnityEngine;

[CreateAssetMenu(fileName = "Level", menuName = "No Wings/LevelData")]
public class LevelData : ScriptableObject
{
  [Header("Layout")]
  public Vector2 birdStartPosition;
  public Vector2 goalPosition;

  [Header("Budget")]
  public int blowerCount;

  [Header("Obstacles")]
  public ObstacleData[] obstacles;
}

[System.Serializable]
public class ObstacleData
{
  public ObstacleType type;
  public Vector2 position;
  public float rotation;
}

public enum ObstacleType
{
  Platform,
}
