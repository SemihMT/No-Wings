using UnityEngine;
using System.Collections.Generic;

public class TrailData
{
  public List<Vector2> Points { get; } = new List<Vector2>();
  public Vector2 DeathPosition { get; set; }
  public bool HasDied { get; set; }
}
