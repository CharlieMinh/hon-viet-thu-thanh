using System;
using UnityEngine;

namespace HonVietThuThanh.Dev5
{
    public enum StoryPortraitSide
    {
        None,
        Left,
        Right
    }

    [Serializable]
    public class StoryLine
    {
        public string speakerName;

        [TextArea(3, 8)]
        public string text;

        public Sprite portrait;
        public StoryPortraitSide portraitSide = StoryPortraitSide.None;

        [Header("Audio")]
        public AudioClip audioClip;

        [Header("Music")]
        public bool changeMusic;
        public AudioClip musicClip;
    }
}
