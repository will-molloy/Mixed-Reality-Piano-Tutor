using System.Collections;
using System.Collections.Generic;

public class PianoKeys {
	public readonly static List<PianoKey> keysList;

	static PianoKeys() {
		keysList = new List<PianoKey>();

		keysList.Add(new PianoKey(36, BlackOrWhite.White));
		keysList.Add(new PianoKey(37, BlackOrWhite.Black));
		keysList.Add(new PianoKey(38, BlackOrWhite.White));
		keysList.Add(new PianoKey(39, BlackOrWhite.Black));
		keysList.Add(new PianoKey(40, BlackOrWhite.White));
		keysList.Add(new PianoKey(41, BlackOrWhite.White));
		keysList.Add(new PianoKey(42, BlackOrWhite.Black));
		keysList.Add(new PianoKey(43, BlackOrWhite.White));
		keysList.Add(new PianoKey(44, BlackOrWhite.Black));
		keysList.Add(new PianoKey(45, BlackOrWhite.White));
		keysList.Add(new PianoKey(46, BlackOrWhite.Black));
		keysList.Add(new PianoKey(47, BlackOrWhite.White));
		keysList.Add(new PianoKey(48, BlackOrWhite.White));
		keysList.Add(new PianoKey(49, BlackOrWhite.Black));
		keysList.Add(new PianoKey(50, BlackOrWhite.White));
		keysList.Add(new PianoKey(51, BlackOrWhite.Black));
		keysList.Add(new PianoKey(52, BlackOrWhite.White));
		keysList.Add(new PianoKey(53, BlackOrWhite.White));
		keysList.Add(new PianoKey(54, BlackOrWhite.Black));
		keysList.Add(new PianoKey(55, BlackOrWhite.White));
		keysList.Add(new PianoKey(56, BlackOrWhite.Black));
		keysList.Add(new PianoKey(57, BlackOrWhite.White));
		keysList.Add(new PianoKey(58, BlackOrWhite.Black));
		keysList.Add(new PianoKey(59, BlackOrWhite.White));
		keysList.Add(new PianoKey(60, BlackOrWhite.White));
		keysList.Add(new PianoKey(61, BlackOrWhite.Black));
		keysList.Add(new PianoKey(62, BlackOrWhite.White));
		keysList.Add(new PianoKey(63, BlackOrWhite.Black));
		keysList.Add(new PianoKey(64, BlackOrWhite.White));
		keysList.Add(new PianoKey(65, BlackOrWhite.White));
		keysList.Add(new PianoKey(66, BlackOrWhite.Black));
		keysList.Add(new PianoKey(67, BlackOrWhite.White));
		keysList.Add(new PianoKey(68, BlackOrWhite.Black));
		keysList.Add(new PianoKey(69, BlackOrWhite.White));
		keysList.Add(new PianoKey(70, BlackOrWhite.Black));
		keysList.Add(new PianoKey(71, BlackOrWhite.White));
		keysList.Add(new PianoKey(72, BlackOrWhite.White));
		keysList.Add(new PianoKey(73, BlackOrWhite.Black));
		keysList.Add(new PianoKey(74, BlackOrWhite.White));
		keysList.Add(new PianoKey(75, BlackOrWhite.Black));
		keysList.Add(new PianoKey(76, BlackOrWhite.White));
		keysList.Add(new PianoKey(77, BlackOrWhite.White));
		keysList.Add(new PianoKey(78, BlackOrWhite.Black));
		keysList.Add(new PianoKey(79, BlackOrWhite.White));
		keysList.Add(new PianoKey(80, BlackOrWhite.Black));
		keysList.Add(new PianoKey(81, BlackOrWhite.White));
		keysList.Add(new PianoKey(82, BlackOrWhite.Black));
		keysList.Add(new PianoKey(83, BlackOrWhite.White));
		keysList.Add(new PianoKey(84, BlackOrWhite.White));
		keysList.Add(new PianoKey(85, BlackOrWhite.Black));
		keysList.Add(new PianoKey(86, BlackOrWhite.White));
		keysList.Add(new PianoKey(87, BlackOrWhite.Black));
		keysList.Add(new PianoKey(88, BlackOrWhite.White));
		keysList.Add(new PianoKey(89, BlackOrWhite.White));
		keysList.Add(new PianoKey(90, BlackOrWhite.Black));
		keysList.Add(new PianoKey(91, BlackOrWhite.White));
		keysList.Add(new PianoKey(92, BlackOrWhite.Black));
		keysList.Add(new PianoKey(93, BlackOrWhite.White));
		keysList.Add(new PianoKey(94, BlackOrWhite.Black));
		keysList.Add(new PianoKey(95, BlackOrWhite.White));
		keysList.Add(new PianoKey(96, BlackOrWhite.White));
	}

	public static PianoKey GetKeyFor(int keyNum) {
		if(keyNum < 36 || keyNum > 96) return null;
		return keysList[keyNum-36];
	}
}
public enum BlackOrWhite {
	Black, White
}

public class PianoKey {
	public readonly int keyNum;
	public readonly BlackOrWhite blackOrWhite;

	public PianoKey(int keyNum, BlackOrWhite bow) {
		this.keyNum = keyNum;
		this.blackOrWhite = bow;
	}
}
