using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class block : MonoBehaviour
{
    public int index_Number;
    GameObject mindObj;
    mind_script mind_scr;
    // Start is called before the first frame update
    void Start()
    {
        mindObj = GameObject.Find("mind");
        mind_scr = mindObj.GetComponent<mind_script>();
    }

    // Update is called once per frame
    void Update()
    {
        if (mind_scr.confirmed && mind_scr.index == index_Number)
        {
            GetComponent<Image>().color =  Color.red;
            return;
        }
        GetComponent<Image>().color = 
            (mind_scr.index == index_Number) ? mind_scr.colors[1] : mind_scr.colors[0];
    }
}
