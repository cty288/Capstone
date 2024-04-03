using System;
using System.Collections;
using System.Collections.Generic;
using _02._Scripts.Runtime.Utilities;
using UnityEngine;
using Random = UnityEngine.Random;

[ExecuteInEditMode]
public class CatmullRomSpline : MonoBehaviour
{
    [SerializeField] List<GameObject> allPoints;

    [SerializeField] List<GameObject> points;
    [SerializeField] float sphereSize = 1.0f;
    [SerializeField] int division = 100;
    [SerializeField] float speed = 3.0f;
    [SerializeField] float moveSphereSize = 1.0f;
    [SerializeField] float moveOffset = 0.1f;
    [SerializeField] int sphereNum = 5;
    
    public int amountOfPoints = 0;
    public bool pickRandomPoints = false;

    // private void OnValidate()
    // {
    //     if(pickRandomPoints)
    //     {
    //         points.Clear();
    //         if (amountOfPoints >= 2)
    //         {
    //             HashSet<int> selectedIndices = new HashSet<int>();
    //             while (selectedIndices.Count < amountOfPoints)
    //             {
    //                 int randomIndex = Random.Range(0, allPoints.Count -1);
    //                 if (selectedIndices.Add(randomIndex))
    //                 {
    //                     points.Add(allPoints[randomIndex]);
    //                 }
    //
    //                 if (selectedIndices.Count == amountOfPoints)
    //                 {
    //                     points.Insert(0, points[0]);
    //                     points.Add(points[^1]);
    //                 }
    //             }
    //         }
    //         pickRandomPoints = false;
    //     }
    // }
    //
    // // void OnDrawGizmos()
    // // {
    // //     Gizmos.color = Color.blue;
    // //     foreach (var point in points)
    // //     {
    // //         Gizmos.DrawSphere(point.transform.position, sphereSize);
    // //     }
    // //
    // //     Gizmos.color = Color.red;
    // //     for(int i=0; i< points.Count-3; i++)
    // //     {
    // //         Vector3 prevPos = points[i+1].transform.position;
    // //         for (int j = 1; j <= division; j++)
    // //         {
    // //             float t = j * 1.0f / division;
    // //
    // //             Vector3 pos = MathFunctions.CatmullRomSplineInterp(points[i].transform.position, points[i + 1].transform.position, points[i + 2].transform.position, points[i + 3].transform.position, t);
    // //             // Vector3 position = CatmullRomSplineInterp(points[idx - 1].transform.position, points[idx].transform.position, points[idx + 1].transform.position, points[idx + 2].transform.position, currentTime % 1.0f);
    // //
    // //             Gizmos.DrawLine(pos, prevPos);
    // //             prevPos = pos;
    // //         }
    // //     }
    // // }
    //
    // void OnDrawGizmos()
    // {
    //     Gizmos.color = Color.blue;
    //     foreach (var point in points)
    //     {
    //         Gizmos.DrawSphere(point.transform.position, sphereSize);
    //     }
    //
    //     Gizmos.color = Color.red;
    //     for(int i=0; i< points.Count-3; i++)
    //     {
    //         Vector3 prevPos = points[i+1].transform.position;
    //         for (int j = 1; j <= division; j++)
    //         {
    //             float t = j * 1.0f / division;
    //
    //             Vector3 pos = MathFunctions.CatmullRomSplineInterp(points[i].transform.position, points[i + 1].transform.position, points[i + 2].transform.position, points[i + 3].transform.position, t);
    //
    //             Gizmos.DrawLine(pos, prevPos);
    //             prevPos = pos;
    //         }
    //     }
    //
    //
    //     for(int i = 0; i < sphereNum; i++)
    //     {
    //         int pointsCnt = points.Count - 3;
    //         float currentTime = (Time.realtimeSinceStartup * speed + i * moveOffset) % pointsCnt;
    //         int idx = Mathf.FloorToInt(currentTime) + 1;
    //         Vector3 position = MathFunctions.CatmullRomSplineInterp(points[idx - 1].transform.position, points[idx].transform.position, points[idx + 1].transform.position, points[idx + 2].transform.position, currentTime % 1.0f);
    //         Gizmos.DrawSphere(position, moveSphereSize);
    //     }
    //
    // }
}