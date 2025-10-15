using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;
public class CamerPOV : MonoBehaviour {
    bool switching = false;
    public Text projText;
    [SerializeField]
    private float orthographicValue;
    
	// Use this for initialization
    void Start () {
		Camera.main.orthographicSize = orthographicValue;
	}
    public void CameraPerspective()
    {
        if (Input.GetKeyDown(KeyCode.V) && !switching)
        {
            switching = true;
            Camera.main.orthographic = false;
            projText.text = "POV: Perspective (V)";
        }
        else if (Input.GetKeyDown(KeyCode.V) && switching)
        {
            switching = false;
            Camera.main.orthographic = true;
            projText.text = "POV: Orthographic (V)";
        }
        if (Camera.main.orthographic)
        {
            float sizeMin = 5.0f;
            float sizeMax = 15.0f;
            float scroll = Input.GetAxis("Mouse ScrollWheel");
            orthographicValue -= scroll * 1.5f;
            orthographicValue = Mathf.Clamp(orthographicValue, sizeMin, sizeMax);
            Camera.main.orthographicSize = orthographicValue;
        } 
        
    }
 
	// Update is called once per frame
	void Update () {
        CameraPerspective();

    }
}
