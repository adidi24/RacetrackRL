using UnityEngine;
using System.Collections;
using System;

public class AgentState
{
    private int fwdSteps, rightSteps, leftSteps;

    //Returns the agent state
    public Vector3Int GetState()
    {
        return new Vector3Int(fwdSteps, rightSteps, leftSteps);
    }

    public void SetState(GameObject agent, LayerMask WhatIsAGridLayer, int leftVelocity, int rightVelocity, int fwdVelocity)
    {
        Vector3 fwd = Vector3.up;
        Vector3 left = agent.transform.TransformDirection(Vector3.left);
        Vector3 right = agent.transform.TransformDirection(Vector3.right);

        float distFwd = GetDistance(agent, fwd, WhatIsAGridLayer);
        float distLeft = GetDistance(agent, left, WhatIsAGridLayer);
        float distRight = GetDistance(agent, right, WhatIsAGridLayer);


        fwdSteps = (fwdVelocity != 0 && distFwd > 0) ? (int)Mathf.Ceil(distFwd / fwdVelocity) : (int)distFwd;
        leftSteps = (leftVelocity != 0 && distLeft > 0) ? (int)Mathf.Ceil(distLeft / leftVelocity) : (int)distLeft;
        rightSteps = (rightVelocity != 0 && distRight > 0) ? (int)Mathf.Ceil(distRight / rightVelocity) : (int)distRight;

        //Debug.Log("distance fwd: " + distFwd + ", right: " + distRight + ", left: " + distLeft);
        //Debug.Log("steps fwd: " + fwdSteps + ", right: " + rightSteps + ", left: " + leftSteps);
    }

    private float GetDistance(GameObject agent, Vector3 direction, LayerMask WhatIsAGridLayer)
    {
        float distance;
        if (Physics.Raycast(agent.transform.position, direction, out RaycastHit HitInfo, 5f, WhatIsAGridLayer))
        {

            distance = Mathf.Ceil(HitInfo.distance);
        }
        else
            distance = -1;

        return distance;
    }
}

