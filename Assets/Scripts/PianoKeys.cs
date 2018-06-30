using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>  
/// - Hardcoded keys for 61 key piano
/// </summary>  
public class PianoKeys
{
    private readonly static List<PianoKey> keysList;
    public readonly static float pianoKeyGap = 0.001f; // 0.02 - white key width

    static PianoKeys()
    {
        keysList = new List<PianoKey>();
        keysList.Add(new PianoKey(36, KeyColor.White));
        keysList.Add(new PianoKey(37, KeyColor.Black));
        keysList.Add(new PianoKey(38, KeyColor.White));
        keysList.Add(new PianoKey(39, KeyColor.Black));
        keysList.Add(new PianoKey(40, KeyColor.White));
        keysList.Add(new PianoKey(41, KeyColor.White));
        keysList.Add(new PianoKey(42, KeyColor.Black));
        keysList.Add(new PianoKey(43, KeyColor.White));
        keysList.Add(new PianoKey(44, KeyColor.Black));
        keysList.Add(new PianoKey(45, KeyColor.White));
        keysList.Add(new PianoKey(46, KeyColor.Black));
        keysList.Add(new PianoKey(47, KeyColor.White));
        keysList.Add(new PianoKey(48, KeyColor.White));
        keysList.Add(new PianoKey(49, KeyColor.Black));
        keysList.Add(new PianoKey(50, KeyColor.White));
        keysList.Add(new PianoKey(51, KeyColor.Black));
        keysList.Add(new PianoKey(52, KeyColor.White));
        keysList.Add(new PianoKey(53, KeyColor.White));
        keysList.Add(new PianoKey(54, KeyColor.Black));
        keysList.Add(new PianoKey(55, KeyColor.White));
        keysList.Add(new PianoKey(56, KeyColor.Black));
        keysList.Add(new PianoKey(57, KeyColor.White));
        keysList.Add(new PianoKey(58, KeyColor.Black));
        keysList.Add(new PianoKey(59, KeyColor.White));
        keysList.Add(new PianoKey(60, KeyColor.White));
        keysList.Add(new PianoKey(61, KeyColor.Black));
        keysList.Add(new PianoKey(62, KeyColor.White));
        keysList.Add(new PianoKey(63, KeyColor.Black));
        keysList.Add(new PianoKey(64, KeyColor.White));
        keysList.Add(new PianoKey(65, KeyColor.White));
        keysList.Add(new PianoKey(66, KeyColor.Black));
        keysList.Add(new PianoKey(67, KeyColor.White));
        keysList.Add(new PianoKey(68, KeyColor.Black));
        keysList.Add(new PianoKey(69, KeyColor.White));
        keysList.Add(new PianoKey(70, KeyColor.Black));
        keysList.Add(new PianoKey(71, KeyColor.White));
        keysList.Add(new PianoKey(72, KeyColor.White));
        keysList.Add(new PianoKey(73, KeyColor.Black));
        keysList.Add(new PianoKey(74, KeyColor.White));
        keysList.Add(new PianoKey(75, KeyColor.Black));
        keysList.Add(new PianoKey(76, KeyColor.White));
        keysList.Add(new PianoKey(77, KeyColor.White));
        keysList.Add(new PianoKey(78, KeyColor.Black));
        keysList.Add(new PianoKey(79, KeyColor.White));
        keysList.Add(new PianoKey(80, KeyColor.Black));
        keysList.Add(new PianoKey(81, KeyColor.White));
        keysList.Add(new PianoKey(82, KeyColor.Black));
        keysList.Add(new PianoKey(83, KeyColor.White));
        keysList.Add(new PianoKey(84, KeyColor.White));
        keysList.Add(new PianoKey(85, KeyColor.Black));
        keysList.Add(new PianoKey(86, KeyColor.White));
        keysList.Add(new PianoKey(87, KeyColor.Black));
        keysList.Add(new PianoKey(88, KeyColor.White));
        keysList.Add(new PianoKey(89, KeyColor.White));
        keysList.Add(new PianoKey(90, KeyColor.Black));
        keysList.Add(new PianoKey(91, KeyColor.White));
        keysList.Add(new PianoKey(92, KeyColor.Black));
        keysList.Add(new PianoKey(93, KeyColor.White));
        keysList.Add(new PianoKey(94, KeyColor.Black));
        keysList.Add(new PianoKey(95, KeyColor.White));
        keysList.Add(new PianoKey(96, KeyColor.White));
    }

    public static PianoKey GetKeyFor(int keyNum)
    {
        if (keyNum < 36 || keyNum > 96) return null;
        return keysList[keyNum - 36];
    }

    public static PianoKey GetFirstKey()
    {
        return keysList[0];
    }
    public static PianoKey GetLastKey()
    {
        return keysList[keysList.Count - 1];
    }

    public static List<PianoKey> GetAllKeys() {
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

    override public string ToString() {
        return this.keyNum + " " + this.color.ToString();
    }

}
