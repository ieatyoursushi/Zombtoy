using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class Sensitivity : MonoBehaviour {
    public static float sensitivityValue;
    public Text text;
    public Slider slider;
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
        sensitivityValue = slider.value;
        text.text = sensitivityValue.ToString();
	}
}
