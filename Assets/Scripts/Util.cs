using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class Util : MonoBehaviour{

	public static void DebugComponents(MonoBehaviour behave)
	{
		behave.GetComponents(typeof(Component)).ToList().ForEach(x => Debug.Log(x));
	}

}
