using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using _02._Scripts.Runtime.Utilities;
using Framework;
using MikroFramework.Architecture;
using MikroFramework.Utilities;
using UnityEngine;
using UnityEngine.AI;


public class RefPointsRecorder : AbstractMikroController<MainGame> {
   [SerializeField] private bool enable = false;
   [SerializeField] private TriggerCheck playerGoundCheck;
   [SerializeField] private GameObject player;
   [SerializeField] private float recordTimeInteval = 5f;
   [SerializeField] private GameObject tempRefPointPrefab;
   private float recordTimer = 0;
   
   private List<Vector3> refPoints = new List<Vector3>();

   private void Awake(){
      playerGoundCheck.OnEnter += OnPlayerEnter;
   }

   private void Update() {
      if (!Application.isEditor) {
         return;
      }
      if (!enable) return;
      recordTimer += Time.deltaTime;
      if (recordTimer >= recordTimeInteval) {
         recordTimer = 0;
         RecordPointAtPlayerPosition();
      }
   }

   private void OnPlayerEnter(Collider e) {
      if (!Application.isEditor) {
         return;
      }
      RecordPointAtPlayerPosition();
   }

   private void OnDestroy() {
      if (!Application.isEditor) {
         return;
      }
      //for each ref point, use Navmesh.CalculatePath to calculate the path to each other ref point
      //if there is a path, then we remove the ref point
      //so that after the iteration, we will have a list of ref points that are not connected to each other
      //which means they are not in the same arena

      if (!enable) return;
      for (int i = 0; i < refPoints.Count; i++) {
         for (int j = 0; j < refPoints.Count; j++) {
            if (i == j) continue;
            NavMeshPath path = new NavMeshPath();
            if (NavMesh.CalculatePath(refPoints[i], refPoints[j], NavMeshHelper.GetSpawnableAreaMask(), path)) {
               if (path.status == NavMeshPathStatus.PathComplete) {
                  refPoints.RemoveAt(i);
                  i--;
                  break;
               }
            }
         }
      }
      
      CreateOnGameEnds();
   }

   private async void CreateOnGameEnds() {
      await Task.Delay(1000);
      
      
      
      //we instantiate the ref points at OnDestory to forcefully save the ref points in scene mode
      //we create a parent to group all the ref points
      GameObject refPointsParent = new GameObject($"RefPoints_{DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss")}");
      foreach (Vector3 refPoint in refPoints) {
         GameObject p = new GameObject();
         p.transform.SetParent(refPointsParent.transform);
         p.transform.position = refPoint;
         p.name = "RefPoint_" + refPoints.Count;
         p.gameObject.tag = "ArenaRefPoint";
      }
   }


   private void RecordPointAtPlayerPosition() {
      if (!enable) return;
      Vector3 playerPosition = player.transform.position;
      Vector3 refPoint = RecordRefPointPos(playerPosition);
      if (!float.IsInfinity(refPoint.magnitude)) {
         refPoints.Add(refPoint);
      }
   }
   
   private Vector3 RecordRefPointPos(Vector3 position) {
     
      if (NavMesh.SamplePosition(position, out NavMeshHit hit, 5f, NavMeshHelper.GetSpawnableAreaMask())) {
         GameObject refPoint = Instantiate(tempRefPointPrefab);
         refPoint.transform.position = hit.position;
         refPoint.name = "RefPoint_TempRecord_" + refPoints.Count;
         refPoint.gameObject.tag = "ArenaRefPoint";
         return hit.position;
      }

      return Vector3.negativeInfinity;
   }
}
