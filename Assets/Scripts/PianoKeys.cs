using System.Collections.Generic;
using System.Linq;
using System;

/// <summary>  
/// - Hardcoded keys for 61 key piano
/// </summary>  
public class PianoKeys
{
    private const int startKeyNum = 36;

    private const int numKeys = 61;

    private const int keysRepeat = 12;

    private readonly static List<PianoKey> keysList = new List<PianoKey>();

    public readonly static HashSet<int> leftOffsetBlackKeyNums = new HashSet<int>();

    public readonly static HashSet<int> rightOffsetBlackKeyNums = new HashSet<int>();

    public readonly static HashSet<int> centerBlackKeyNums = new HashSet<int>();

    static PianoKeys()
    {
        var leftOffSetBlackSchema = new HashSet<int> { 2, 7 };
        var rightOffsetBlakSchema = new HashSet<int> { 4, 11 };
        var centerBlackSchema = new HashSet<int> { 9 };
        Enumerable.Range(0, numKeys / keysRepeat).ToList().ForEach(x =>
        {
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
    Black, White
}

public class PianoKey
{
    public readonly int keyNum;
    public readonly KeyColor color;

    public PianoKey(int keyNum, KeyColor color)
    {
        this.keyNum = keyNum;
        this.color = color;
    }

    override public string ToString()
    {
        return this.keyNum + " " + this.color.ToString();
    }

    override public bool Equals(object obj)
    {
        var x = obj as PianoKey;
        if (x == null)
        {
            return false;
        }

        return this.keyNum == x.keyNum;
    }

}
