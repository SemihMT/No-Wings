using UnityEngine;
using System.Collections.Generic;

public class LevelDesignRoot : MonoBehaviour
{
  private GameObject birdStart;
  private GameObject goal;
  private List<GameObject> obstacles = new List<GameObject>();
  private List<BlowerElement> blowers = new List<BlowerElement>();

  public Transform BirdStart => birdStart?.transform;
  public Transform Goal => goal?.transform;

  public void SetBirdStart(Vector2 position, GameObject prefab)
  {
    if (birdStart != null) DestroyImmediate(birdStart);

    if (prefab != null)
      birdStart = Instantiate(prefab, position, Quaternion.identity, transform);
    else
    {
      birdStart = new GameObject("BirdStart");
      birdStart.transform.position = position;
      birdStart.transform.parent = transform;
    }

    birdStart.name = "BirdStart";
  }

  public void SetGoal(Vector2 position, GameObject prefab)
  {
    if (goal != null) DestroyImmediate(goal);

    if (prefab != null)
      goal = Instantiate(prefab, position, Quaternion.identity, transform);
    else
    {
      goal = new GameObject("Goal");
      goal.transform.position = position;
      goal.transform.parent = transform;
    }

    goal.name = "Goal";
  }

  public void AddObstacle(Vector2 position, GameObject prefab)
  {
    var obstacle = Instantiate(prefab, position, Quaternion.identity, transform);
    obstacle.name = $"Obstacle_{obstacles.Count}";
    obstacles.Add(obstacle);
  }

  public void AddBlower(Vector2 position, GameObject prefab)
  {
    var go = Instantiate(prefab, position, Quaternion.identity, transform);
    go.name = $"Blower_{blowers.Count}";
    var blower = go.GetComponentInChildren<BlowerElement>();
    if (blower != null) blowers.Add(blower);
  }

  public List<BlowerElement> GetBlowers() => blowers;

  public void ClearAll()
  {
    if (birdStart != null) DestroyImmediate(birdStart);
    if (goal != null) DestroyImmediate(goal);

    foreach (var obs in obstacles)
      if (obs != null) DestroyImmediate(obs);
    obstacles.Clear();

    foreach (var blower in blowers)
      if (blower != null) DestroyImmediate(blower.gameObject);
    blowers.Clear();
  }

  public void ExportTo(LevelData data)
  {
    if (birdStart != null)
      data.birdStartPosition = birdStart.transform.position;

    if (goal != null)
      data.goalPosition = goal.transform.position;

    var obstacleList = new List<ObstacleData>();
    foreach (var obs in obstacles)
    {
      if (obs == null) continue;
      obstacleList.Add(new ObstacleData
      {
        type = ObstacleType.Platform,
        position = obs.transform.position,
        rotation = obs.transform.eulerAngles.z
      });
    }
    data.obstacles = obstacleList.ToArray();
  }

  public void LoadFrom(LevelData data, GameObject playerPrefab, GameObject goalPrefab,
      GameObject platformPrefab, GameObject blowerPrefab)
  {
    SetBirdStart(data.birdStartPosition, playerPrefab);
    SetGoal(data.goalPosition, goalPrefab);

    foreach (var obs in data.obstacles)
    {
      GameObject prefab = obs.type == ObstacleType.Platform ? platformPrefab : null;
      if (prefab != null)
      {
        var go = Instantiate(prefab, obs.position,
            Quaternion.Euler(0, 0, obs.rotation), transform);
        obstacles.Add(go);
      }
    }
  }
}
