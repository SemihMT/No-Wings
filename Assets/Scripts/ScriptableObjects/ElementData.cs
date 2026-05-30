using UnityEngine;

[CreateAssetMenu(fileName = "ElementData", menuName = "No Wings/Element Data")]
public class ElementData : ScriptableObject
{
  public ElementType type;
  public Sprite icon;
  public GameObject prefab;
}
