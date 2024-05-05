using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEngine;
using UnityEngine.Serialization;
using Object = UnityEngine.Object;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Harpia.PrefabBrush
{
    public class PrefabBrushTemplate : ScriptableObject
    {
        [Space] public PbPrefabUI[] prefabs;
        [Space] public string parentName;
        public PrefabBrush.PivotMode pivotModeValue;
        public PrefabBrush.PaintMode paintMode;
        public PrefabBrush.RaycastMode raycastMode;
        public PrefabBrush.ParentMode parentMode;
        public PrefabBrush.Vector3Mode rotationMode;
        public PrefabBrush.Vector3ModeUniform scaleMode;
        public PrefabBrush.Vector3Mode offsetMode;
        public PrefabBrush.Vector3Mode impulseMode;
        public PB_PhysicsSimulator.AffectMode physicsAffectMode;
        public PB_PhysicsSimulator.SimulationMode physicsSimulationMode;

        [Space] public List<Texture> allowedTextures;

        [Space] public int tagMask;
        public int layerMask;
        public int parentInstanceID;

        [Space] public float brushSize;
        public float brushStrength;
        public float clippingStreght;
        [FormerlySerializedAs("singleModeRotationAngle")] public float precisionModeRotationAngle;
        public float eraserSize;
        public float physicsStepValue;
        public float gridSnapValue;
        public float physicsImpulseValue;

        [Space] public bool useAngleLimits;
        public bool alignWithGround;
        [FormerlySerializedAs("singleModeChangePrefabAfterPlacing")] public bool precisionModeChangePrefabAfterPlacing;
        [FormerlySerializedAs("singleModeAddMesh")] public bool precisionModeAddMesh;
        public bool snapToGrid;
        public bool showClippingBounds;
        public bool makeStatic;
        public bool useTextureMask;
        public bool useImpulse;
        public bool placeOnYZero;

        [Space] public Vector3 minScale;
        public Vector3 maxScale;
        public Vector3 fixedScale;
        [Space] public Vector3 maxRotation;
        public Vector3 minRotation;
        public Vector3 fixedRotation;
        [Space] public Vector3 fixedOffset;
        public Vector3 minOffset;
        public Vector3 maxOffset;

        [Space] public float scaleMaxUniform = 1;
        public float scaleMinUniform;
        
        [Space]
        public bool constrainedScaleFixed;
        public bool constrainedScaleMax;
        public bool constrainedScaleMin;
        
        [Space] public Vector3 fixedPhysicsImpulse;
        public Vector3 minPhysicsImpulse;
        public Vector3 maxPhysicsImpulse;
        public Vector2 angleLimits;
        public Vector2 gridOffset;

        private static List<PrefabBrushTemplate> _allPresets;
        private static PrefabBrushTemplate LoadedPreset => PrefabBrush._lastLoadedTemplate;

        public const string NoPresetsFound = "No presets found";
        private const string KeyLastTemplate = "PBLastTemplate";
        private const string KeyLastTemplatePath = "PBLastTemplatePath";

        private void Init(PrefabBrush model)
        {
            prefabs = model.currentPrefabs.ToArray();

            parentName = model.parentField.value == null ? model.parentNameInput.value : model.parentField.value.name;
            parentInstanceID = model.parentField.value == null ? 0 : model.parentField.value.GetInstanceID();

            brushSize = model.brushRadiusSlider.value;
            brushStrength = model.sliderBrushStrength.value;
            clippingStreght = model.clippingToleranceSlider.value;

            alignWithGround = model.toggleAlignWithGround.value;
            layerMask = model.layerMaskField.value;
            tagMask = model.tagMaskField.value;

            angleLimits = model.angleLimitsField.value;
            precisionModeChangePrefabAfterPlacing = model.precisionModeChangePrefabToggle.value;
            makeStatic = model.makeObjectsStaticToggle.value;
            useTextureMask = PB_TextureMaskHandler.useTextureMaskToggle.value;
            allowedTextures = PB_TextureMaskHandler.GetAllowedTextures();
            gridSnapValue = model.gridSnapValueField.value;
            placeOnYZero = model.placeOnYZeroToggle.value;

            useAngleLimits = model.useAngleLimitsToggle.value;
            precisionModeAddMesh = model.precisionModeAddMeshToBatch.value;
            precisionModeRotationAngle = model.precisionModeRotationAngle.value;
            snapToGrid = model.gridSnapToggle.value;
            showClippingBounds = model.showBoundsToggle.value;
            eraserSize = model.eraserRadiusSlider.value;

            raycastMode = model.GetRaycastMode();
            pivotModeValue = model.GetPivotMode();
            paintMode = model.GetPaintMode();
            parentMode = model.GetParentMode();

            rotationMode = (PrefabBrush.Vector3Mode)model.rotationField.enumField.value;
            fixedRotation = model.rotationField.fixedField.value;
            minRotation = model.rotationField.minField.value;
            maxRotation = model.rotationField.maxField.value;

            scaleMode = (PrefabBrush.Vector3ModeUniform)model.scaleField.enumField.value;
            fixedScale = model.scaleField.fixedField.value;
            minScale = model.scaleField.minField.value;
            maxScale = model.scaleField.maxField.value;
            scaleMaxUniform = model.scaleField.maxFieldUniform.value.x;
            scaleMinUniform = model.scaleField.minFieldUniform.value.x;

            offsetMode = (PrefabBrush.Vector3Mode)model.offsetField.enumField.value;
            fixedOffset = model.offsetField.fixedField.value;
            minOffset = model.offsetField.minField.value;
            maxOffset = model.offsetField.maxField.value;

            constrainedScaleFixed = model.scaleField.proportionsFixed;
            constrainedScaleMax = model.scaleField.proportionsMax;
            constrainedScaleMin = model.scaleField.proportionsMin;

            physicsStepValue = PB_PhysicsSimulator.physicsStepSlider.value;
            physicsAffectMode = PB_PhysicsSimulator.GetAffectMode();
            physicsSimulationMode = PB_PhysicsSimulator.GetSimulationMode();
            physicsImpulseValue = PB_PhysicsSimulator.impulseForceSlider.value;

            minPhysicsImpulse = PB_PhysicsSimulator.impulseFieldVec3.minField.value;
            maxPhysicsImpulse = PB_PhysicsSimulator.impulseFieldVec3.maxField.value;
            fixedPhysicsImpulse = PB_PhysicsSimulator.impulseFieldVec3.fixedField.value;
            useImpulse = PB_PhysicsSimulator.impulseFieldVec3.useToggle.value;
            impulseMode = (PrefabBrush.Vector3Mode)PB_PhysicsSimulator.impulseFieldVec3.enumField.value;

            gridOffset = model.gridOffsetField.value;

#if HARPIA_DEBUG
            Debug.Log($"[Prefab Brush] Preset cloned. If any variables were not saved please check here");
#endif
        }

        private bool SaveScriptableObject()
        {
            string openPath = GetLastPath().Replace("Assets", Application.dataPath) + "/";
            string path = EditorUtility.SaveFilePanelInProject("Save Brush Preset", GetFileName(), "asset",
                "Save Preset", openPath);

            if (string.IsNullOrEmpty(path)) return false;

            EditorPrefs.SetString(KeyLastTemplate, path);

            PrefabBrushTemplate asset = CreateInstance<PrefabBrushTemplate>();
            asset.CloneFrom(this);
            asset.hideFlags = HideFlags.DontSaveInBuild;
            AssetDatabase.CreateAsset(asset, path);


            //load asset at path
            PrefabBrushTemplate loadedAsset = AssetDatabase.LoadAssetAtPath<PrefabBrushTemplate>(path);
            PrefabBrush._lastLoadedTemplate = loadedAsset;
            PrefabBrush.instance._templatesDropdown.value = loadedAsset.name;

            Debug.Log($"Prefab Brush: Preset saved to {path}", loadedAsset);
            return true;

            string GetFileName()
            {
                if (PrefabBrush._lastLoadedTemplate != null)
                    return PrefabBrush._lastLoadedTemplate.name;

                return "Prefab Brush Preset - ";
            }

            string GetLastPath()
            {
                string path = EditorPrefs.GetString(KeyLastTemplatePath, "Assets");
                if (PrefabBrush._lastLoadedTemplate != null) path = AssetDatabase.GetAssetPath(PrefabBrush._lastLoadedTemplate);
                return Path.GetDirectoryName(path);
            }
        }

        private void CloneFrom(PrefabBrushTemplate other)
        {
            //copy data
            prefabs = other.prefabs;
            parentName = other.parentName;
            parentInstanceID = other.parentInstanceID;
            brushSize = other.brushSize;
            brushStrength = other.brushStrength;
            clippingStreght = other.clippingStreght;
            alignWithGround = other.alignWithGround;
            layerMask = other.layerMask;
            tagMask = other.tagMask;
            angleLimits = other.angleLimits;
            precisionModeChangePrefabAfterPlacing = other.precisionModeChangePrefabAfterPlacing;
            makeStatic = other.makeStatic;
            useTextureMask = other.useTextureMask;
            allowedTextures = other.allowedTextures;
            useAngleLimits = other.useAngleLimits;
            precisionModeAddMesh = other.precisionModeAddMesh;
            precisionModeRotationAngle = other.precisionModeRotationAngle;
            snapToGrid = other.snapToGrid;
            showClippingBounds = other.showClippingBounds;
            eraserSize = other.eraserSize;
            raycastMode = other.raycastMode;
            pivotModeValue = other.pivotModeValue;
            paintMode = other.paintMode;
            parentMode = other.parentMode;
            gridSnapValue = other.gridSnapValue;

            rotationMode = other.rotationMode;
            fixedRotation = other.fixedRotation;
            minRotation = other.minRotation;
            maxRotation = other.maxRotation;

            scaleMode = other.scaleMode;
            fixedScale = other.fixedScale;
            minScale = other.minScale;
            maxScale = other.maxScale;
            
            constrainedScaleFixed = other.constrainedScaleFixed;
            constrainedScaleMax = other.constrainedScaleMax;
            constrainedScaleMin = other.constrainedScaleMin;

            offsetMode = other.offsetMode;
            fixedOffset = other.fixedOffset;
            minOffset = other.minOffset;
            maxOffset = other.maxOffset;

            physicsAffectMode = other.physicsAffectMode;
            physicsSimulationMode = other.physicsSimulationMode;
            physicsStepValue = other.physicsStepValue;
            physicsImpulseValue = other.physicsImpulseValue;
            minPhysicsImpulse = other.minPhysicsImpulse;
            maxPhysicsImpulse = other.maxPhysicsImpulse;
            fixedPhysicsImpulse = other.fixedPhysicsImpulse;
            useImpulse = other.useImpulse;
            impulseMode = other.impulseMode;
            placeOnYZero = other.placeOnYZero;

            gridOffset = other.gridOffset;

#if HARPIA_DEBUG
            Debug.Log($"If some props are not saving check here");
#endif
        }

        public static List<string> GetAllPresetsNames()
        {
            //find all scriptable objects in the project of tye PrefabBrushTemplate
            string[] guids = AssetDatabase.FindAssets("t:PrefabBrushTemplate");
            List<string> allNames = new();
            _allPresets = new();

            if (guids.Length == 0)
            {
                allNames.Add(NoPresetsFound);
                return allNames;
            }

            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                if (string.IsNullOrEmpty(path)) continue;
                PrefabBrushTemplate template = AssetDatabase.LoadAssetAtPath<PrefabBrushTemplate>(path);
                if (template == null) continue;

                allNames.Add(template.name);
                _allPresets.Add(template);
            }

            return allNames;
        }

        public static bool SaveTemplate(PrefabBrush instance)
        {
            PrefabBrushTemplate newPreset = CreateInstance<PrefabBrushTemplate>();
            newPreset.Init(instance);
            return newPreset.SaveScriptableObject();
        }

        public static void DeleteTemplate(string templatesDropdownValue)
        {
            throw new NotImplementedException();
        }

        public Transform GetParent()
        {
            if (string.IsNullOrEmpty(parentName)) return null;

            //Instance ID
            GameObject possibleParent = EditorUtility.InstanceIDToObject(parentInstanceID) as GameObject;
            if (possibleParent != null) return possibleParent.transform;

            //Name
            possibleParent = GameObject.Find(parentName);
            if (possibleParent == null) return null;

            return possibleParent.transform;
        }

        public static void SetLastTemplate(string evtNewValue)
        {
            EditorPrefs.SetString(KeyLastTemplate, evtNewValue);
        }

        public static void RevealCurrentTemplate()
        {
            if (LoadedPreset == null) return;
            
            Debug.Log("[Prefab Brush] Make Sure a project tab is available");
            EditorUtility.FocusProjectWindow();
            Selection.activeObject = LoadedPreset;
        }

        public static PrefabBrushTemplate LoadTemplate(string presetName)
        {
            if (string.IsNullOrEmpty(presetName)) return null;
            if (presetName == "None") return null;

            PrefabBrushTemplate t = _allPresets.Find(x => x.name == presetName);

            if (t != null) return t;

            PrefabBrush.DisplayError($"Template {presetName} not found! Did you renamed or delete it?");
            return null;
        }

        public static string GetCurrentTemplateName()
        {
            if (LoadedPreset == null) return "None";
            return LoadedPreset.name;
        }

        [ContextMenu("Open Brush with this template")]
        public void OpenWithThisTemplate()
        {
            PrefabBrush.ShowWindow();
            PrefabBrush.instance.LoadTemplate(this);
        }
    }

#if UNITY_EDITOR

    [CustomEditor(typeof(PrefabBrushTemplate))]
    public class PrefabBrushTemplateEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            //button GUI
            if (GUILayout.Button("Open Prefab Brush With this template"))
            {
                PrefabBrushTemplate script = (PrefabBrushTemplate)target;
                script.OpenWithThisTemplate();
            }

            //space
            GUILayout.Space(20);

            DrawDefaultInspector();
        }
    }

#endif
}