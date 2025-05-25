using UnityEngine;
using System;
using System.Collections.Generic;
//from https://www.theappguruz.com/blog/display-projectile-trajectory-path-in-unity
public class ForwardIndicator : MonoBehaviour
{
    public GameObject TrajectoryPointPrefeb;
    public GameObject BallPrefb;
    private float power = 25;
    private int numOfTrajectoryPoints = 30;
    private List<GameObject> trajectoryPoints;
    public GameObject StartPoint;
    public GameObject bow;
    
    public Shoot_arrow sh;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        trajectoryPoints = new List<GameObject>();
        
        //   TrajectoryPoints are instatiated
        for (int i = 0; i < numOfTrajectoryPoints; i++)
        {
            GameObject dot = (GameObject)Instantiate(TrajectoryPointPrefeb);
            dot.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
            dot.GetComponent<Renderer>().enabled = false;
            trajectoryPoints.Insert(i, dot);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (sh.sh != Vector3.zero) {
            Debug.Log(sh.sh);
            setTrajectoryPoints(StartPoint.transform.localPosition, sh.sh);
        }
        
    }
        void setTrajectoryPoints(Vector3 pStartPosition , Vector3 pDirectionVect )
    {
        float c = 0f;
        
        
        for (int i = 0 ; i < numOfTrajectoryPoints ; i++)
        {
            
            Vector3 pos =Vector3.Lerp(bow.transform.position ,  bow.transform.position +Quaternion.Euler(0, 90, 90)*pDirectionVect*30, c);
            trajectoryPoints[i].transform.parent = bow.transform;
            trajectoryPoints[i].transform.localPosition = pos;
            //trajectoryPoints[i].transform.localScale = new Vector3(0.1f,0.1f,0.1f);

            trajectoryPoints[i].GetComponent<Renderer>().enabled = true;
            
            c+= 1/30f;
        }
    }
}
