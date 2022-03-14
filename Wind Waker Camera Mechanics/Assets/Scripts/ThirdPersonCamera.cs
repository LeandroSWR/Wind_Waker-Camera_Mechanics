using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThirdPersonCamera : MonoBehaviour
{
    [SerializeField] private float distanceAway;
    [SerializeField] private float distanceUp;
    [SerializeField] private float smooth;
    [SerializeField] private Transform follow;
    [SerializeField] private Vector3 offset = new Vector3(0f, 1.5f, 0f);

    private Vector3 lookDir;
    private Vector3 targetPosition;

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
        Vector3 characterOffset = follow.position + offset;

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
        SmoothPosition(transform.position, targetPosition);

        // Make sure the camera is looking the right way
        transform.LookAt(follow);
    }

    private void SmoothPosition(Vector3 fromPos, Vector3 toPos)
    {
        // Making a smooth transition between camera's current position and the position it wants to be in
        transform.position = Vector3.SmoothDamp(fromPos, toPos, ref velocityCamSmooth, camSmoothDampTime);
    }
}
