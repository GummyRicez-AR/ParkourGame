using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Zipline : MonoBehaviour
{
    public LineRenderer ropePath;
    public ZiplineWaypoints waypoints;
    public float thresholdUntilNextWaypoint = 0.2f;
    public float playerYOffset = 1.2f;
    public float naturalSpeedDecay = 0.5f;

    public IEnumerator StartRidingZipline(PlayerController plr)
    {
        print("begin");

        float currVelocity = Mathf.Max(plr.GetMovementVector().magnitude / 1.5f, 5);
        plr.usingZipline = true;
        plr.HaltMovement();
        Vector3 plrEuler = transform.rotation.eulerAngles;

        bool reachedEnd = false;
        Transform travellingWaypoint = waypoints.NextWaypoint(waypoints.FirstWaypoint());

        if (plr.sliding)
        {
            plr.sliding = false;
            plr.gameObject.GetComponent<CharacterController>().height = 2;
            plr.head.transform.localRotation = Quaternion.Euler(0, 0, 0);
            plr.transform.rotation = Quaternion.Euler(0, plrEuler.y, plrEuler.z);
        }

        plr.gameObject.GetComponent<CharacterController>().Move(waypoints.FirstWaypoint().position - plr.transform.position - new Vector3(0, playerYOffset, 0));
        while (!reachedEnd)
        {
            Vector3 origPos = plr.transform.position;
            plr.transform.position = Vector3.MoveTowards(plr.transform.position, travellingWaypoint.position - new Vector3(0, playerYOffset, 0), currVelocity * Time.deltaTime);
            Vector3 newPos = plr.transform.position;
            float diff = newPos.y - origPos.y;
            print(diff);

            if (currVelocity > 0)
            {
                currVelocity -= (naturalSpeedDecay + (diff * 175)) * Time.deltaTime;
            } else
            {
                currVelocity = 0;
            }

            if (Vector3.Distance(plr.transform.position, travellingWaypoint.position) < thresholdUntilNextWaypoint + playerYOffset)
            {
                travellingWaypoint = waypoints.NextWaypoint(travellingWaypoint);
                if (travellingWaypoint == waypoints.FirstWaypoint())
                {
                    print("end");
                    reachedEnd = true;
                    plr.usingZipline = false;
                }
            }
            yield return null;
        }
    }

    private void OnDrawGizmos()
    {
        DrawRope();
    }

    private void DrawRope()
    {
        ropePath.positionCount = waypoints.transform.childCount;
        for (int i = 0; i < waypoints.transform.childCount; i++)
        {
            Transform waypointTransform = waypoints.transform.GetChild(i);
            ropePath.SetPosition(i, waypointTransform.position);
        }
    }
}
