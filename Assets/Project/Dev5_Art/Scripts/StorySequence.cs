using System.Collections.Generic;
using UnityEngine;

namespace HonVietThuThanh.Dev5
{
    public enum StorySequenceMode
    {
        BlackScreen,
        Dialogue,
        Unlock
    }

    [CreateAssetMenu(fileName = "StorySequence", menuName = "Hon Viet/Story/Story Sequence")]
    public class StorySequence : ScriptableObject
    {
        [Header("Sequence")]
        public string sequenceId;
        public StorySequenceMode mode = StorySequenceMode.Dialogue;
        public List<StoryLine> lines = new List<StoryLine>();

        [Header("Unlock Panel")]
        public Sprite unlockPortrait;
        public string unlockTitle = "ĐÃ MỞ KHÓA";
        public string unlockHeroName;
        public string unlockRole;

        [TextArea(2, 6)]
        public string unlockDescription;
    }
}
