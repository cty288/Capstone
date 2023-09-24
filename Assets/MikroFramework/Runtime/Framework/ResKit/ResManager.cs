using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using MikroFramework.DataStructures;
using MikroFramework.Managers;
using MikroFramework.Pool;
using MikroFramework.Singletons;
using UnityEngine;

namespace MikroFramework.ResKit
{
   // [MonoSingletonPath("[FrameworkPersistent]/[ResKit]/ResManager")]
    public class ResManager : ISingleton {

        private ResTable SharedLoadedResources = new ResTable();

        public ResTable SharedLoadedResource => SharedLoadedResources;

        private const string SimulationModeKey = "simulation mode";

        private static int simulationMode = -1;

        public static ResManager singleton {
            get {
                return SingletonProperty<ResManager>.Singleton;
            }
        }

        private ResManager() {
            SafeObjectPool<ResourcesRes>.Singleton.Init(10,30);
            SafeObjectPool<AssetBundleRes>.Singleton.Init(20,50);
            SafeObjectPool<AssetRes>.Singleton.Init(30,100);
        }
        
        /// <summary>
        /// Get Global resource pool
        /// </summary>
        public static ResTable GetSharedLoadedRes() {
            return singleton.SharedLoadedResources;
        }

        public static void RemoveSharedRes(Res res) {
            singleton.SharedLoadedResources.Remove(res);

            ResRecycleHelper.RecycleToPool(res);
            
        }

        public static void AddRes(Res res) {
            singleton.SharedLoadedResources.Add(res);
        }

        /// <summary>
        /// Get a Res object from the manager by name
        /// </summary>
        /// <param name="resName"></param>
        /// <returns></returns>
        public static Res GetRes(string resName) {
            return singleton.SharedLoadedResources.NameIndex.Get(resName).FirstOrDefault();
        }

        public static Res GetRes(ResSearchKeys resSearchKeys) {
            return singleton.SharedLoadedResources.GetResWithSearchKeys(resSearchKeys);
        }

        void ISingleton.OnSingletonInit()
        {

        }

#if UNITY_EDITOR
        /// <summary>
        /// Get or set the current project's Simulation Mode
        /// Use IsSimultionModeLogic to determine whether the project is in Simulation Mode because this function will only run in the UnityEditor
        /// </summary>
        public static bool SimulationMode
        {
            get
            {
                if (simulationMode == -1)
                {
                    simulationMode = UnityEditor.EditorPrefs.GetBool(SimulationModeKey, true) ? 1 : 0;
                }

                return simulationMode != 0;
            }
            set
            {
                simulationMode = value ? 1 : 0;
                UnityEditor.EditorPrefs.SetBool(SimulationModeKey, value);
            }
        }
#endif


        /// <summary>
        /// get if the current project is in simulation mode
        /// it will always return false in RunTime build.
        /// SimulationMode makes you to "load" AssetBundles in UnityEditor without actually build AssetBundles
        /// </summary>
        public static bool IsSimulationModeLogic{
            get {
#if UNITY_EDITOR
                return SimulationMode;
#endif
                return false;
            }
        }

    }
}
