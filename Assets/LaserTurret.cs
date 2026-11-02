using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LaserTurret : MonoBehaviour
{
    [SerializeField] LayerMask targetLayer;
    [SerializeField] GameObject crosshair;
    [SerializeField] float baseTurnSpeed = 3;
    [SerializeField] GameObject gun;
    [SerializeField] Transform turretBase;
    [SerializeField] Transform barrelEnd;
    [SerializeField] LineRenderer line;
    [SerializeField] int maxBounces = 5;
    [SerializeField] float maxDistance = 1000.0f;

    List<Vector3> laserPoints = new List<Vector3>();

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        TrackMouse();
        TurnBase();

        laserPoints.Clear();
        laserPoints.Add(barrelEnd.position);

        CastLaser(barrelEnd.position, barrelEnd.forward, maxBounces);

        line.positionCount = laserPoints.Count;
        for (int i = 0; i < line.positionCount; i++)
        {
            line.SetPosition(i, laserPoints[i]);
        }
    }

    // Recursively casts the laser, adding a point at each hit and bouncing until out of bounces
    void CastLaser(Vector3 origin, Vector3 direction, int bouncesLeft)
    {
        if (Physics.Raycast(origin, direction, out RaycastHit hit, maxDistance, targetLayer))
        {
            laserPoints.Add(hit.point);

            if (bouncesLeft > 0)
            {
                Vector3 reflected = ReflectVector(direction, hit.normal);

                // Nudge the start off the surface so the next ray doesn't hit the same spot
                CastLaser(hit.point + hit.normal * 0.01f, reflected, bouncesLeft - 1);
            }
        }
        else
        {
            // Nothing hit - draw the laser out to max distance
            laserPoints.Add(origin + direction * maxDistance);
        }
    }

    // Reflection: r = d - 2(d . n)n
    // (d . n)n is the projection of d onto the normal - the part of d going into the surface.
    // Subtracting it twice flips that part, so the laser bounces back out.
    Vector3 ReflectVector(Vector3 direction, Vector3 normal)
    {
        normal = normal.normalized;
        float dot = Vector3.Dot(direction, normal);
        return direction - 2 * dot * normal;
    }

    void TrackMouse()
    {
        Ray cameraRay = Camera.main.ScreenPointToRay(Input.mousePosition);

        RaycastHit hit;
        if (Physics.Raycast(cameraRay, out hit, 1000, targetLayer))
        {
            crosshair.transform.forward = hit.normal;
            crosshair.transform.position = hit.point + hit.normal * 0.1f;
        }
    }

    void TurnBase()
    {
        Vector3 directionToTarget = (crosshair.transform.position - turretBase.transform.position).normalized;
        Quaternion lookRotation = Quaternion.LookRotation(new Vector3(directionToTarget.x, directionToTarget.y, directionToTarget.z));
        turretBase.transform.rotation = Quaternion.Slerp(turretBase.transform.rotation, lookRotation, Time.deltaTime * baseTurnSpeed);
    }
}