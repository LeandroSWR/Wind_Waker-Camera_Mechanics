using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThirdPersonCamera : MonoBehaviour
{
    [SerializeField] private float distanceAway;
    [SerializeField] private float distanceUp;
    [SerializeField] private float smooth;
    [SerializeField] private Transform follow;

    [SerializeField] private float widescreen = 0.2f;
    [SerializeField] private float targetingTime = 0.5f;


    private Vector3 lookDir;
    private Vector3 targetPosition;
    private BarsEffect barEffect;

    // Smoothing and damping
    private Vector3 velocityCamSmooth = Vector3.zero;
    [SerializeField] private float camSmoothDampTime = 0.1f;

    // Start is called before the first frame update
    private void Start()
    {
        
    }

    // Update is called once per frame
    private void Update()
    {
        
    }

    private void LateUpdate()
    {
        Vector3 characterOffset = follow.position + new Vector3(0f, distanceUp, 0f);

        // Calculate direction from camera to player, kill Y, and
        // normalize to give a valid direction with unit magnitude.
        lookDir = characterOffset - transform.position;
        lookDir.y = 0;
        lookDir.Normalize();
        Debug.DrawRay(transform.position, lookDir, Color.green);


        // Setting the target position to be the correct offset from the follow
        targetPosition = characterOffset + follow.up * distanceUp - lookDir * distanceAway;

        Debug.DrawRay(follow.position, follow.up * distanceUp, Color.red);
        Debug.DrawRay(follow.position, -1f * follow.forward * distanceAway, Color.blue);
        Debug.DrawLine(follow.position, targetPosition, Color.magenta);

        // Making a smooth transition between it's current position and the position it wants to be in
        CompensateForWalls(characterOffset);
        SmoothPosition(transform.position, targetPosition);

        // Make sure the camera is looking the right way
        transform.LookAt(follow);
    }

    private void SmoothPosition(Vector3 fromPos, Vector3 toPos)
    {
        // Making a smooth transition between camera's current position and the position it wants to be in
        transform.position = Vector3.SmoothDamp(fromPos, toPos, ref velocityCamSmooth, camSmoothDampTime);
    }

    private void CompensateForWalls(Vector3 fromObject)
    {
        Debug.DrawLine(fromObject, targetPosition, Color.cyan);

        // Compensate for walls between camera
        RaycastHit wallHit;
        if (Physics.Linecast(fromObject, targetPosition, out wallHit))
        {
            Debug.DrawRay(wallHit.point, Vector3.left, Color.red);
            targetPosition = new Vector3(wallHit.point.x, targetPosition.y, wallHit.point.z);
        }
    }
}
