using System.Collections.Generic;
using UnityEngine;
using CelestialCross.Dialogue.Graph;

namespace CelestialCross.Dialogue.Data
{
    [CreateAssetMenu(fileName = "NewDiaryCatalog", menuName = "Celestial Cross/Dialogue/Diary Catalog")]
    public class DiaryCatalog : ScriptableObject
    {
        public List<DiaryEntry> entries = new List<DiaryEntry>();
    }

    [global::System.Serializable]
    public class DiaryEntry
    {
        public string diaryName;
        [TextArea(2, 5)]
        public string description;
        public DialogueGraph dialogueGraph;
        public Sprite coverImage;
    }
}
