using UnityEngine;

public class BirdTrailRenderer : MonoBehaviour
{
  [Header("References")]
  [SerializeField] private LineRenderer lineRenderer;
  [SerializeField] private SpriteRenderer deathMarker;

  public void Display(TrailData data, float alpha)
  {
    if (data.Points.Count < 2)
    {
      Hide();
      return;
    }

    // Set line positions
    lineRenderer.positionCount = data.Points.Count;
    for (int i = 0; i < data.Points.Count; i++)
      lineRenderer.SetPosition(i, new Vector3(data.Points[i].x, data.Points[i].y, 0f));

    // Set alpha
    Color lineColor = lineRenderer.startColor;
    lineColor.a = alpha;
    lineRenderer.startColor = lineColor;
    lineRenderer.endColor = lineColor;

    // Death marker
    if (data.HasDied)
    {
      deathMarker.gameObject.SetActive(true);
      deathMarker.transform.position = new Vector3(
          data.DeathPosition.x,
          data.DeathPosition.y,
          0f
      );
      Color markerColor = deathMarker.color;
      markerColor.a = alpha;
      deathMarker.color = markerColor;
    }
    else
    {
      deathMarker.gameObject.SetActive(false);
    }

    lineRenderer.enabled = true;
  }

  public void Hide()
  {
    lineRenderer.enabled = false;
    deathMarker.gameObject.SetActive(false);
  }
}
