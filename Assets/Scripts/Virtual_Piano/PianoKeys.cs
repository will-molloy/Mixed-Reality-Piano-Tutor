using System;
using System.Collections.Generic;
using System.Linq;

namespace Virtual_Piano
{
    /// <summary>
    ///     - Hardcoded keys for 61 key piano
    ///     - Also returns key number to match note number in MIDI apis
    /// </summary>
    public class PianoKeys
    {
        private const int startKeyNum = 36;

        private const int numKeys = 61;

        private const int keysRepeat = 12;

        private static readonly List<PianoKey> keysList = new List<PianoKey>();

        public static readonly HashSet<int> leftOffsetBlackKeyNums = new HashSet<int>();

        public static readonly HashSet<int> rightOffsetBlackKeyNums = new HashSet<int>();

        public static readonly HashSet<int> centerBlackKeyNums = new HashSet<int>();

        static PianoKeys()
        {
            var leftOffSetBlackSchema = new HashSet<int> {2, 7};
            var rightOffsetBlakSchema = new HashSet<int> {4, 11};
            var centerBlackSchema = new HashSet<int> {9};
            Enumerable.Range(0, numKeys / keysRepeat).ToList().ForEach(x =>
            {
                // Build keys, pattern of white/black order repeats every 12 keys
                Func<int, int> f = a => a + x * keysRepeat + startKeyNum - 1;
                leftOffsetBlackKeyNums.UnionWith(leftOffSetBlackSchema.Select(f));
                rightOffsetBlackKeyNums.UnionWith(rightOffsetBlakSchema.Select(f));
                centerBlackKeyNums.UnionWith(centerBlackSchema.Select(f));
            });

            var blackKeyNums = leftOffsetBlackKeyNums.Union(rightOffsetBlackKeyNums).Union(centerBlackKeyNums);
            Enumerable.Range(startKeyNum, numKeys).ToList().ForEach(keyNum =>
            {
                var color = blackKeyNums.Contains(keyNum) ? KeyColor.Black : KeyColor.White;
                keysList.Add(new PianoKey(keyNum, color));
            });
        }

        public static PianoKey GetKeyFor(int keyNum)
        {
            if (keyNum < startKeyNum || keyNum >= startKeyNum + numKeys) return null;
            return keysList[keyNum - startKeyNum];
        }

        public static PianoKey First()
        {
            return keysList.First();
        }

        public static PianoKey Last()
        {
            return keysList.Last();
        }

        public static List<PianoKey> GetAllKeys()
        {
            return keysList;
        }
    }

    public enum KeyColor
    {
        Black,
        White
    }

    public class PianoKey
    {
        public readonly KeyColor color;
        public readonly int keyNum;

        public PianoKey(int keyNum, KeyColor color)
        {
            this.keyNum = keyNum;
            this.color = color;
        }

        public override string ToString()
        {
            return keyNum + " " + color;
        }

        public override bool Equals(object obj)
        {
            var x = obj as PianoKey;
            if (x == null) return false;

            return keyNum == x.keyNum;
        }
    }
}