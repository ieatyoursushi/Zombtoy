﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class zombieCount : MonoBehaviour {
    public Text text;
    public int entityCount = 0;
    public int maximumEntities;
	// Use this for initialization
	void Start () {
        text = GetComponent<Text>();
	}
	
	// Update is called once per frame
	void Update () {
        text.text = "Zombies: " + entityCount.ToString();
    }
}
