using UnityEngine;
using System.Collections;
using RTS;

public class wintext : MonoBehaviour {
	public TextMesh text;

	// Use this for initialization
	void Start () {
		if(ResourceManager.previousLevel == 11)
			text.text = "Victory!";
	}
}
