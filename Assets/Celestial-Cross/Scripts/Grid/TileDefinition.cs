using UnityEngine;
using Sirenix.OdinInspector;

[CreateAssetMenu(menuName = "Celestial Cross/Grid/Tile Definition")]
public class TileDefinition : ScriptableObject
{
    [HorizontalGroup("Identity", Width = 60), PreviewField(55, ObjectFieldAlignment.Left), HideLabel]
    public Sprite defaultSprite;

    [VerticalGroup("Identity/Info")]
    [LabelWidth(110)]
    public int id;

    [VerticalGroup("Identity/Info")]
    [LabelWidth(110)]
    public string displayName = "New Tile";

    [VerticalGroup("Identity/Info")]
    [LabelWidth(110)]
    public Color editorTint = new Color(0.6f, 0.6f, 0.6f, 1f);

    [Space]
    [Title("Gameplay")]
    [LabelWidth(110)]
    public bool isWalkable = true;

    [LabelWidth(110)]
    public bool isPlayerSpawnZone = false;

    [LabelWidth(110)]
    public GameObject prefab;
}