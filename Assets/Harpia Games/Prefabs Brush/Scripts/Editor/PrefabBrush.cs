//Available on the asset store at https://assetstore.unity.com/packages/tools/utilities/prefab-brush-easy-object-placement-tool-260560?aid=1100lACye&utm_campaign=unity_affiliate&utm_medium=affiliate&utm_source=partnerize-linkmaker
//Harpia Games Studio

using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEditor;
using UnityEditor.EditorTools;
using UnityEditor.SceneManagement;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Serialization;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;
using UnityEngine.Rendering;
using UnityEngine.UIElements;
using UnityEditorInternal;

// ReSharper disable ForeachCanBeConvertedToQueryUsingAnotherGetEnumerator

// ReSharper disable CheckNamespace
// ReSharper disable InconsistentNaming
// ReSharper disable CoVariantArrayConversion
// ReSharper disable LoopCanBeConvertedToQuery
// ReSharper disable PossibleNullReferenceException
// ReSharper disable Unity.InstantiateWithoutParent
// ReSharper disable CompareOfFloatsByEqualityOperator
// ReSharper disable ConvertIfStatementToReturnStatement
// ReSharper disable Unity.PerformanceCriticalCodeInvocation
// ReSharper disable ConvertIfStatementToConditionalTernaryExpression
// ReSharper disable ForeachCanBePartlyConvertedToQueryUsingAnotherGetEnumerator

namespace Harpia.PrefabBrush
{
    public class PrefabBrush : EditorWindow
    {
        #region Enums

        public enum PivotMode
        {
            MeshPivot,
            Bounds_Center,
            Bounds_Center_Bottom,
            Bounds_Center_Top,
        }

        public enum PaintMode
        {
            Multiple,
            Precision,
            Eraser,
        }

        public enum RaycastMode
        {
            Physical_Collider,
            Mesh,
        }

        public enum Vector3Mode
        {
            Fixed,
            Random,
        }

        public enum Vector3ModeUniform
        {
            Fixed,
            Random,
            Random_Uniform,
        }

        public enum ParentMode
        {
            No_Parent,
            Fixed_Transform,
            Hit_Surface_Object,
        }

        #endregion

        #region VisualElementsVariables

        private VisualElement _shortcutParent;
        private VisualElement _eraseModePanel;
        private VisualElement prefabsHolder;
        private VisualElement _statusBackground;
        private VisualElement _addSelectedPrefabsPanel;

        private EnumField pivotMode;
        private EnumField paintModeDropdown;
        private EnumField parentModeDropdown;
        private EnumField raycastModeDropdown;

        private Label _dragAndDropLabel;
        private Label _statusLabel;
        private Label _addSelectedPrefabsLabel;
        private Label _requiredParentWarningLabel;

        public Slider brushRadiusSlider;
        public Slider eraserRadiusSlider;
        public Slider sliderBrushStrength;
        public Slider clippingToleranceSlider;

        public Toggle gridSnapToggle;
        public Toggle useAngleLimitsToggle;
        public Toggle showBoundsToggle;

        public Toggle precisionModeAddMeshToBatch;

        public Toggle _showBrushGuideLines;
        public Toggle toggleAlignWithGround;
        public Toggle precisionModeChangePrefabToggle;

        private Foldout _brushFoldout;
        private Foldout _precisionModeFoldout;

        public FloatField gridSnapValueField;
        private FloatField angleMinField;
        private FloatField angleMaxField;
        public FloatField precisionModeRotationAngle;
        public FloatField brushGuideLinesDistance;

        public TextField parentNameInput;

        private Button _statusButton;
        private Button _paintModeButton;
        private Button _eraserModeButton;
        private Button createParentButton;
        private Button _addSelectedPrefabsButton;

        public MaskField tagMaskField;
        public ObjectField parentField;
        public ColorField boundsColorField;
        public MinMaxSlider angleLimitsField;
        public LayerMaskField layerMaskField;
        public DropdownField _templatesDropdown;

        public Camera sceneCamera;
        public bool isRaycastHit;
        public RaycastHit lastHitInfo;

        [FormerlySerializedAs("_randomPointInsideDisc")]
        public Vector3 randomPointInsideDisc;

        private string _dragAndDropOriginalText;

        public static PrefabBrush instance;

        public bool _isMouse0Down;
        public bool _isMouse1Down;
        private bool _isShiftDown;
        private bool _isAltDown;
        private bool _isCtrlDown;
        private bool _successPainted;

        private bool isXDown;
        private bool isYDown;
        private bool isZDown;
        private bool isChangeSizeDown;
        private bool isAdjustDistanceFromGroundDown;

        private Vector2 mouse0DownPosition;
        private Vector2 _lastMousePosCtrl;
        private Vector2 _lastMousePosShift;

        private List<string> _currentTagsSelected;
        private List<GameObject> _prefabsToAddFromSelection;
        public readonly List<PbPrefabUI> currentPrefabs = new();

        #endregion

        #region StaticVariables

        private const float brushSizeIncrement = 0.06f;
        private const string xmlFileName = "PrefabBrushXML";

        public static float deltaTime;
        private static Mesh _sphereMesh;
        private static Material _defaultMaterial;
        private static string _visualTreeGuid;
        private static double LastMouse0DownTime;
        private static double lastRepaintTime;
        private static readonly Color activeButtonColor = new Color(0f, 1f, 0.61f, 0.39f);
        public static readonly Color _styleBackgroundColorGreen = new Color(0f, 1f, 0f, 0.4f);
        public const string DebugLogStart = "[Prefab Brush]";
        //private static double mouse0DownTime => EditorApplication.timeSinceStartup - LastMouse0DownTime;

        private static PaintMode lastMode = PaintMode.Precision;
        private static bool isRaycastTagAllowed;
        private static bool _isTextureAllowed;
        public VisualElement prefabsPanel;
        public VisualElement paintModePanel;
        public Toggle makeObjectsStaticToggle;
        public Vector3ModeElement rotationField;
        public Vector3ModeElement scaleField;
        public Vector3ModeElement offsetField;
        public VisualElement spacer;
        public static PrefabBrushTemplate _lastLoadedTemplate;
        public Vector2Field gridOffsetField;
        private Label notAvailableInPlayModeLabel;
        public Toggle placeOnYZeroToggle;

        #endregion

        #region UnityMessages'

        [MenuItem("Tools/Prefab Brush/Open Prefab Brush", false, 1)]
        public static void ShowWindow()
        {
            if (instance != null)
            {
                instance.Focus();
                return;
            }

            Type inspectorType = Type.GetType("UnityEditor.InspectorWindow,UnityEditor.dll");
            PrefabBrush wnd = GetWindow<PrefabBrush>(new Type[] { inspectorType });

            wnd.titleContent = new GUIContent("Prefab Brush");
        }

        private void OnDisable()
        {
#if HARPIA_DEBUG
            Debug.Log("Prefab Brush Disabled");
#endif
            PrefabBrushTool.isUsingTool = false;
            PB_PhysicsSimulator.Dispose();
            ExitTool();
        }

        private void OnGUI()
        {
            if (Event.current.type == EventType.KeyDown)
            {
                PB_ModularShortcuts.UpdateAssignKey();
                UseCurrentEvent();
            }
        }

        private void OnDestroy()
        {
            instance = null;

            SceneView.duringSceneGui -= OnSceneGUI;

            Undo.undoRedoPerformed -= OnUndoRedo;

            ToolManager.activeContextChanged -= OnActiveContextChanged;
            ToolManager.activeToolChanged -= OnActiveToolChanged;
            ToolManager.activeContextChanging -= OnActiveContextChanging;
            ToolManager.activeToolChanging -= OnActiveToolChanging;

            PrefabStage.prefabStageClosing -= OnPrefabStageClosing;
            PrefabStage.prefabStageOpened -= OnPrefabStageOpened;
            PrefabStage.prefabStageDirtied -= OnPrefabStageDirtied;

            EditorSceneManager.sceneClosing -= OnSceneClosing;
            EditorSceneManager.sceneOpened -= OnSceneOpened;
            EditorSceneManager.sceneSaved -= OnSceneSaved;

            AssemblyReloadEvents.beforeAssemblyReload -= OnBeforeAssemblyReload;
            AssemblyReloadEvents.afterAssemblyReload -= OnAfterAssemblyReload;

            EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;

#if UNITY_2022_2_OR_NEWER
            EditorApplication.focusChanged -= OnEditorFocusChanged;
#endif

            EditorApplication.update -= OnEditorUpdate;
            Selection.selectionChanged -= OnSelectionChanged;
        }

        private void OnUndoRedo()
        {
            ExitTool();
            
            //Get the undo message
            string message = Undo.GetCurrentGroupName();
            PrefabBrushObject.Dispose();

            if (!PB_UndoManager.IsUndoMessage(message)) return;

#if HARPIA_DEBUG
            Debug.Log($"Undo/Redo {message}");
#endif
        }

        public void CreateGUI()
        {
            if (Application.isPlaying)
            {
                notAvailableInPlayModeLabel = new Label("Prefab Brush is not available in play mode")
                {
                    style =
                    {
                        fontSize = 20,
                        unityTextAlign = TextAnchor.MiddleCenter
                    }
                };
                rootVisualElement.Add(notAvailableInPlayModeLabel);

                //on state changed
                EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
                EditorApplication.playModeStateChanged += OnPlayModeStateChanged;

                return;
            }

            instance = this;
            if (notAvailableInPlayModeLabel != null) notAvailableInPlayModeLabel.RemoveFromHierarchy();
            VisualTreeAsset tree = LoadVisualTreeAsset(xmlFileName, ref _visualTreeGuid);
            tree.CloneTree(rootVisualElement);
            VisualElement root = rootVisualElement;

            //Top buttons

            paintModePanel = root.Q<VisualElement>("paint-mode");
            _eraserModeButton = root.Q<Button>("erase-mode-button");
            _paintModeButton = root.Q<Button>("paint-mode-button");
            root.Q<Button>("documentation-button").RegisterCallback<ClickEvent>(_ => OpenDocumentation());
            spacer = root.Q<VisualElement>("spacer");

            _paintModeButton.RegisterCallback<ClickEvent>(OnPaintModeButtonClick);
            _eraserModeButton.RegisterCallback<ClickEvent>(OnEraseModeButtonClick);
            root.Q<Button>("settings-button").RegisterCallback<ClickEvent>(OnSettingsModeButtonClick);

            //Custom props
            prefabsPanel = root.Q<VisualElement>("prefabs-section");
            CustomPrefabProps.Init(root);

            //Main Panels
            root.Q<VisualElement>("paint-mode");
            _eraseModePanel = root.Q<VisualElement>("erase-mode");
            root.Q<VisualElement>("settings-mode");

            //Prefabs selected section
            _addSelectedPrefabsPanel = root.Q("add-selected-objects");
            _addSelectedPrefabsButton = _addSelectedPrefabsPanel.Q<Button>("add-selected-objects-button");
            _addSelectedPrefabsLabel = _addSelectedPrefabsPanel.Q<Label>("add-selected-objects-label");
            _addSelectedPrefabsPanel.SetActive(false);
            _addSelectedPrefabsButton.RegisterCallback<ClickEvent>(OnAddSelectedPrefabsButton);

            //status
            _statusBackground = root.Q<VisualElement>("status-section");
            _statusLabel = root.Q<Label>("status");
            _statusButton = root.Q<Button>("start-brush");
            _statusButton.RegisterCallback<ClickEvent>(OnStartButton);

            Button revealTemplateButton = root.Q<Button>("reveal-template");
            revealTemplateButton.RegisterCallback<ClickEvent>(OnRevealTemplateButton);

            Button saveTemplateButton = root.Q<Button>("save-template");
            saveTemplateButton.RegisterCallback<ClickEvent>(OnSaveTemplateButton);

            //Prefabs
            prefabsHolder = root.Q<VisualElement>("prefabs-holder");
            prefabsHolder.Clear();

            Button clearListButton = root.Q<Button>("clear-list");
            clearListButton.RegisterCallback<ClickEvent>(OnClearListButton);

            //Parent
            parentModeDropdown = root.Q<EnumField>("parenting-mode");
            parentModeDropdown.Init(ParentMode.No_Parent);
            parentModeDropdown.RegisterValueChangedCallback(OnParentModeChanged);
            _requiredParentWarningLabel = root.Q<Label>("required-parent-label");

            parentField = root.Q<ObjectField>("parent");
            parentField.objectType = typeof(Transform);
            parentField.RegisterValueChangedCallback(OnParentChanged);

            parentNameInput = root.Q<TextField>("parent-name");
            createParentButton = root.Q<Button>("create-parent");
            createParentButton.RegisterCallback<ClickEvent>(OnCreateParentButton);

            //Shortcuts
            _shortcutParent = root.Q<Label>("shortcuts-label").parent;

            //Mode
            paintModeDropdown = root.Q<EnumField>("mode-dropdown");
            paintModeDropdown.Init(lastMode);
            paintModeDropdown.RegisterValueChangedCallback(OnModeChanged);

            //Texture Mask
            PB_TerrainHandler.ShowAlert();

            //Precision Mode
            _precisionModeFoldout = root.Q<Foldout>("single-section");
            precisionModeChangePrefabToggle = root.Q<Toggle>("single-mode-change-prefab");
            precisionModeAddMeshToBatch = root.Q<Toggle>("single-mode-add-mesh");
            precisionModeRotationAngle = root.Q<FloatField>("shortcut-rotation-angle");
            precisionModeRotationAngle.RegisterValueChangedCallback(OnPrecisionModeRotationAngleChanged);

            placeOnYZeroToggle = root.Q<Toggle>("no-hit-y-pos-toggle");

            PB_PhysicsSimulator.Init(root);

            //Drag and drop
            _dragAndDropLabel = root.Q<Label>("drag-drop-text");
            _dragAndDropOriginalText = _dragAndDropLabel.text;
            VisualElement dragAndDropSection = root.Q<VisualElement>("drag-drop-section");
            dragAndDropSection.RegisterCallback<DragEnterEvent>(OnDragAndDropPrefabsEnter);
            dragAndDropSection.RegisterCallback<DragExitedEvent>(OnDragPrefabsExit);
            dragAndDropSection.RegisterCallback<DragLeaveEvent>(OnDragPrefabsLeave);

            //Brush
            _brushFoldout = root.Q<Foldout>("brush-section");

            brushRadiusSlider = root.Q<Slider>("size");
            sliderBrushStrength = root.Q<Slider>("strength");
            sliderBrushStrength.lowValue = 0.01f;
            brushRadiusSlider.RegisterFocusEvents();

            eraserRadiusSlider = root.Q<Slider>("eraser-radius-slider");
            eraserRadiusSlider.RegisterFocusEvents();
            eraserRadiusSlider.SetActive(false);

            //Snapping
            gridSnapToggle = root.Q<Toggle>("grid-snapping-toggle");
            gridSnapToggle.RegisterValueChangedCallback(OnGridSnapToggleChanged);
            gridSnapValueField = root.Q<FloatField>("grid-snap-value");
            gridSnapValueField.RegisterFocusEvents();
            gridSnapValueField.RegisterValueChangedCallback(OnGridSnapFloatChanged);
            gridOffsetField = root.Q<Vector2Field>("grid-offset");
            FloatField offsetZField = gridOffsetField.Q<FloatField>("unity-y-input");
            offsetZField.Q<Label>().text = "Z";
            offsetZField.value = 0;
            gridOffsetField.RegisterFocusEvents();
            gridOffsetField.RegisterValueChangedCallback(OnGridOffsetChanged);

            //Raycast mode
            raycastModeDropdown = root.Q<EnumField>("raycast-mode");
            raycastModeDropdown.Init(RaycastMode.Mesh);
            raycastModeDropdown.RegisterValueChangedCallback(OnRaycastModeChanged);

            //Pivot mode
            pivotMode = root.Q<EnumField>("pivot-mode");
            pivotMode.Init(PivotMode.MeshPivot);

            offsetField = new Vector3ModeElement(root, "offset-type", "offset-fixed", "offset-max", "offset-min", "", false);
            offsetField.RegisterFocusEvents();
            offsetField.enumField.RegisterValueChangedCallback((_) => PB_PrecisionModeManager.UpdateCurrentOffset());
            offsetField.fixedField.RegisterValueChangedCallback((_) => PB_PrecisionModeManager.UpdateCurrentOffset());
            offsetField.maxField.RegisterValueChangedCallback((_) => PB_PrecisionModeManager.UpdateCurrentOffset());
            offsetField.minField.RegisterValueChangedCallback((_) => PB_PrecisionModeManager.UpdateCurrentOffset());

            toggleAlignWithGround = root.Q<Toggle>("align-with-ground");

            //Layer
            layerMaskField = root.Q<LayerMaskField>("layer");
            layerMaskField.RegisterValueChangedCallback(OnLayerMaskChanged);
            layerMaskField.SetValueWithoutNotify(-1);

            root.Q<Button>("physics-debbuger-btn").RegisterCallback<ClickEvent>(_ => PB_PhysicsSimulator.ShowDebugWindow());

            //Tag
            tagMaskField = root.Q<MaskField>("tags");
            tagMaskField.RegisterCallback<ChangeEvent<int>>(OnTagMaskChanged);
            tagMaskField.SetValueWithoutNotify(-1);

            string[] tags = InternalEditorUtility.tags;
            tagMaskField.choices = tags.ToList();

            useAngleLimitsToggle = root.Q<Toggle>("use-angle-limits");
            useAngleLimitsToggle.RegisterValueChangedCallback(OnUseAngleLimitsChanged);

            angleLimitsField = root.Q<MinMaxSlider>("slope-limits");
            angleMaxField = root.Q<FloatField>("max-slope");
            angleMinField = root.Q<FloatField>("min-slope");

            angleMaxField.value = angleLimitsField.highLimit;
            angleMinField.value = angleLimitsField.lowLimit;

            angleLimitsField.RegisterValueChangedCallback(OnAngleLimitsChanged);
            angleMinField.RegisterValueChangedCallback(OnAngleMinChanged);
            angleMaxField.RegisterValueChangedCallback(OnAngleMaxChanged);

            //Rotation
            rotationField = new Vector3ModeElement(root, "rotation-type", "rotation-fixed", "rotation-max", "rotation-min", "", false);
            rotationField.fixedField.RegisterFocusEvents();

            rotationField.enumField.RegisterValueChangedCallback((_) => PB_PrecisionModeManager.UpdateCurrentRotation());
            rotationField.fixedField.RegisterValueChangedCallback((_) => PB_PrecisionModeManager.UpdateCurrentRotation());
            rotationField.maxField.RegisterValueChangedCallback((_) => PB_PrecisionModeManager.UpdateCurrentRotation());
            rotationField.minField.RegisterValueChangedCallback((_) => PB_PrecisionModeManager.UpdateCurrentRotation());

            //scale
            scaleField = new Vector3ModeElement(root, "scale-type", "scale-fixed", "scale-max", "scale-min", "", true);
            scaleField.RegisterFocusEvents();
            scaleField.AddProportions("scale-proportions-toggle-fixed", "scale-proportions-toggle-max", "scale-proportions-toggle-min", root);

            scaleField.fixedField.value = Vector3.one;
            scaleField.minField.value = Vector3.one * 0.8f;
            scaleField.maxField.value = Vector3.one * 1.2f;

            scaleField.enumField.RegisterValueChangedCallback((_) => PB_PrecisionModeManager.UpdateCurrentScale());
            scaleField.fixedField.RegisterValueChangedCallback((_) => PB_PrecisionModeManager.UpdateCurrentScale());
            scaleField.maxField.RegisterValueChangedCallback((_) => PB_PrecisionModeManager.UpdateCurrentScale());
            scaleField.minField.RegisterValueChangedCallback((_) => PB_PrecisionModeManager.UpdateCurrentScale());

            //Advanced options
            makeObjectsStaticToggle = root.Q<Toggle>("objects-static-toggle");

            _showBrushGuideLines = root.Q<Toggle>("show-brush-world-lines");
            _showBrushGuideLines.RegisterValueChangedCallback(OnShowBrushGuideLinesChanged);
            _showBrushGuideLines.RegisterFocusEvents();

            brushGuideLinesDistance = root.Q<FloatField>("guide-lines-distance");
            brushGuideLinesDistance.RegisterFocusEvents();

            showBoundsToggle = root.Q<Toggle>("show-bounds");
            showBoundsToggle.RegisterValueChangedCallback(_ => UpdateUITool());
            boundsColorField = root.Q<ColorField>("clipping-bounds-color");

            //Clipping
            clippingToleranceSlider = root.Q<Slider>("clipping-strength");
            clippingToleranceSlider.RegisterFocusEvents();
            clippingToleranceSlider.RegisterValueChangedCallback(OnClippingToleranceChanged);

            //Template
            _templatesDropdown = root.Q<DropdownField>("select-template");
            _templatesDropdown.choices = PrefabBrushTemplate.GetAllPresetsNames();
            _templatesDropdown.RegisterValueChangedCallback(OnTemplateChanged);
            _templatesDropdown.value = (PrefabBrushTemplate.GetCurrentTemplateName());

            //Shortcuts
            PB_ModularShortcuts.Init(root);

            //Events
            SceneView.duringSceneGui -= OnSceneGUI;
            SceneView.duringSceneGui += OnSceneGUI;

            Undo.undoRedoPerformed -= OnUndoRedo;
            Undo.undoRedoPerformed += OnUndoRedo;

            ToolManager.activeContextChanged += OnActiveContextChanged;
            ToolManager.activeToolChanged += OnActiveToolChanged;
            ToolManager.activeContextChanging += OnActiveContextChanging;
            ToolManager.activeToolChanging += OnActiveToolChanging;

            Selection.selectionChanged += OnSelectionChanged;
            PrefabStage.prefabStageClosing += OnPrefabStageClosing;
            PrefabStage.prefabStageOpened += OnPrefabStageOpened;
            PrefabStage.prefabStageDirtied += OnPrefabStageDirtied;

            EditorApplication.update += OnEditorUpdate;

            //scene change event
            EditorSceneManager.sceneClosing += OnSceneClosing;
            EditorSceneManager.sceneOpened += OnSceneOpened;
            EditorSceneManager.sceneSaved += OnSceneSaved;

            //Editor lost focus
#if UNITY_2022_2_OR_NEWER
            EditorApplication.focusChanged += OnEditorFocusChanged;
#endif

            //Playmode
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;

            //Compilation
            AssemblyReloadEvents.beforeAssemblyReload += OnBeforeAssemblyReload;
            AssemblyReloadEvents.afterAssemblyReload += OnAfterAssemblyReload;

            PB_TextureMaskHandler.Init(root);
            PB_AdvancedSettings.Init(root);
            PB_PressurePen.Init(root);
            UpdateUITool();

            Selection.objects = Array.Empty<Object>();
            PrefabBrushTool.isUsingTool = false;
        }

        private void OnGridOffsetChanged(ChangeEvent<Vector2> evt)
        {
#if HARPIA_DEBUG
            Debug.Log($"Grid offset changed");
#endif
            float size = gridSnapValueField.value;
            Vector2 v = evt.newValue;
            v.x = Mathf.Clamp(v.x, 0, size);
            v.y = Mathf.Clamp(v.y, 0, size);
            gridOffsetField.SetValueWithoutNotify(v);
        }

        private void OnEditorFocusChanged(bool obj)
        {
#if HARPIA_DEBUG
            Debug.Log($"OnEditorFocusChanged {obj}", this);
#endif

            DisposeKeysVariables();

            if (!obj)
            {
                PB_PhysicsSimulator.Dispose();
            }
        }

        private void OnPlayModeStateChanged(PlayModeStateChange obj)
        {
#if HARPIA_DEBUG
            Debug.Log($"OnPlayModeStateChanged {obj}", this);
#endif

            Debug.Log($"OnPlayModeStateChanged {obj}", this);
            if (obj == PlayModeStateChange.EnteredEditMode && instance == null)
            {
                CreateGUI();
                return;
            }

            ResetStaticVariables();
            ExitTool();
        }

        private void OnEditorUpdate()
        {
            PB_PhysicsSimulator.Update();
        }

        private void OnFocus()
        {
            if (Application.isPlaying) return;
#if HARPIA_DEBUG
            Debug.Log($"{DebugLogStart} On Focus", this);
#endif
            PbPrefabUI.Reload(currentPrefabs);
            PrefabBrushObject.Dispose(PB_PhysicsSimulator.IsUsingPhysics());
            PB_PrecisionModeManager.HideObject();
            UpdateShortcuts();
        }

        private void OnLostFocus()
        {
            if (Application.isPlaying) return;
#if HARPIA_DEBUG
            Debug.Log($"{DebugLogStart} OnLostFocus", this);
#endif

            ResetStaticVariables();
            PB_AdvancedSettings.SaveValues();
            PB_ModularShortcuts.Dispose();
        }

        private void OnAddSelectedPrefabsButton(ClickEvent evt)
        {
            if (_prefabsToAddFromSelection == null) return;
            if (_prefabsToAddFromSelection.Count == 0) return;

#if HARPIA_DEBUG
            Debug.Log($"[{nameof(PrefabBrush)}] OnAddSelectedPrefabsButton {_prefabsToAddFromSelection.Count}", this);
#endif

            foreach (GameObject go in _prefabsToAddFromSelection)
            {
                AddPrefab(go);
            }
        }

        #endregion

        #region CompilationEvents

        private void OnAfterAssemblyReload()
        {
#if HARPIA_DEBUG
            Debug.Log($"On After Assembly Reload", this);
#endif

            PB_PrecisionModeManager.DisposePrecisionMode();
            PB_TextureMaskHandler.Init(rootVisualElement);
        }

        private void OnBeforeAssemblyReload()
        {
#if HARPIA_DEBUG
            Debug.Log($"On Before Assembly Reload", this);
#endif

            PB_PrecisionModeManager.DisposePrecisionMode();
            PB_PhysicsSimulator.Dispose();
        }

        #endregion

        #region ModeButtonsCallbacks

        private void OnSettingsModeButtonClick(ClickEvent evt)
        {
#if HARPIA_DEBUG
            Debug.Log($"On Settings Mode Button Click", this);
#endif
            PB_AdvancedSettings.SetActive(true);
            ExitEraserMode();
            paintModePanel.SetActive(false);
            _eraseModePanel.SetActive(false);
            PB_ShortcutManager.ClearShortcuts();
        }

        private void OnPaintModeButtonClick(ClickEvent evt)
        {
#if HARPIA_DEBUG
            Debug.Log($"On Paint Mode Button Click - Last Mode {lastMode}", this);
#endif
            Selection.activeObject = null;
            ExitEraserMode();

            PB_AdvancedSettings.SetActive(false);
            paintModePanel.SetActive(true);
            paintModeDropdown.SetValueWithoutNotify(lastMode);

            if (GetPaintMode() == PaintMode.Precision)
            {
                PB_PrecisionModeManager.SetPrefabToPaint();
            }

            OnModeChanged(null);

            UpdateUITool();
        }

        private void OnEraseModeButtonClick(ClickEvent evt)
        {
#if HARPIA_DEBUG
            Debug.Log($"On Erase Mode Button Click", this);
#endif

            EnterEraserMode();
        }

        #endregion

        #region SceneEvents

        private void OnSceneSaved(Scene scene)
        {
#if HARPIA_DEBUG
            Debug.Log($"OnSceneSaved - scene {scene.name}", this);
#endif
        }

        private void OnSceneOpened(Scene scene, OpenSceneMode mode)
        {
#if HARPIA_DEBUG
            Debug.Log($"OnSceneOpened - mode {mode}", this);
#endif
        }

        private void OnSceneClosing(Scene scene, bool removingScene)
        {
#if HARPIA_DEBUG
            Debug.Log($"On Scene Closing - removingScene {removingScene}", this);
#endif
        }

        #endregion

        #region PrefabStageEvents

        private void OnPrefabStageDirtied(PrefabStage obj)
        {
            ExitTool();
        }

        private void OnPrefabStageOpened(PrefabStage obj)
        {
            ExitTool();
        }

        private void OnPrefabStageClosing(PrefabStage obj)
        {
            UpdateUITool();
        }

        #endregion

        private void OnPrecisionModeRotationAngleChanged(ChangeEvent<float> evt)
        {
            float v = Mathf.Clamp(evt.newValue, 0.01f, 359.9f);
            precisionModeRotationAngle.SetValueWithoutNotify(v);
        }

        private void OnGridSnapFloatChanged(ChangeEvent<float> changeEvent)
        {
            float v = Mathf.Max(0.001f, gridSnapValueField.value);
            gridSnapValueField.SetValueWithoutNotify(v);
        }

        private void OnGridSnapToggleChanged(ChangeEvent<bool> evt)
        {
            UpdateUITool();
        }

        private void OnShowBrushGuideLinesChanged(ChangeEvent<bool> evt)
        {
            UpdateUITool();
        }

        private void OnUseAngleLimitsChanged(ChangeEvent<bool> evt)
        {
            UpdateUITool();
        }

        private void OnClippingToleranceChanged(ChangeEvent<float> evt)
        {
#if HARPIA_DEBUG
            Debug.Log($"OnClippingToleranceChanged {evt.newValue}", this);
#endif
            PrefabBrushObject.SetClippingSize(1 - evt.newValue);
        }

        private void OnParentModeChanged(ChangeEvent<Enum> evt)
        {
            ParentMode newValue = (ParentMode)evt.newValue;
            RaycastMode raycastMode = GetRaycastMode();
            if (newValue == ParentMode.Hit_Surface_Object && raycastMode == RaycastMode.Mesh)
            {
                ParentMode oldValue = (ParentMode)evt.previousValue;
                DisplayError($"Parenting to hit surface object is not supported when using Mesh Raycast Mode Yet." +
                             $"Please use the physical collider raycast mode in order to parent objects to hit surface" +
                             $"\n\nChanging back to {oldValue}");
                parentModeDropdown.SetValueWithoutNotify(oldValue);
            }

            UpdateUITool();
        }

        private void OnModeChanged(ChangeEvent<Enum> evt)
        {
            PaintMode newMode = evt == null ? GetPaintMode() : (PaintMode)evt.newValue;
#if HARPIA_DEBUG
            Debug.Log($"Mode Changed to {newMode}");
#endif

            PB_ModularShortcuts.Dispose();
            PB_PrecisionModeManager.DisposePrecisionMode();
            PrefabBrushObject.Dispose();

            //new mode
            if (newMode == PaintMode.Precision)
            {
                PbPrefabUI.SelectFirst();
                PB_PrecisionModeManager.SetPrefabToPaint();
            }

            if (newMode == PaintMode.Multiple)
            {
            }

            UpdateEraserModeUI();
            UpdateUITool();
            UpdateShortcuts();
        }

        private void OnAngleMaxChanged(ChangeEvent<float> evt)
        {
            Vector2 limits = angleLimitsField.value;
            limits.y = evt.newValue;
            angleLimitsField.SetValueWithoutNotify(limits);
        }

        private void OnAngleMinChanged(ChangeEvent<float> evt)
        {
            Vector2 limits = angleLimitsField.value;
            limits.x = evt.newValue;
            angleLimitsField.SetValueWithoutNotify(limits);
        }

        private void OnAngleLimitsChanged(ChangeEvent<Vector2> evt)
        {
            angleMinField.SetValueWithoutNotify(evt.newValue.x);
            angleMaxField.SetValueWithoutNotify(evt.newValue.y);
        }

        private static bool CanUseTool()
        {
            PrefabStage prefabStage = PrefabStageUtility.GetCurrentPrefabStage();
            bool isOnPrefabStage = prefabStage != null;
            return !isOnPrefabStage || PrefabBrushTool.isUsingTool;
        }

        //Start using tool
        public void OnStartButton(ClickEvent evt)
        {
            if (CanUseTool() == false) return;

#if HARPIA_DEBUG
            Debug.Log($"{PrefabBrush.DebugLogStart} Start button clicked - isUsingTool {PrefabBrushTool.isUsingTool} {GetPaintMode()}");
#endif

            DisposeKeysVariables();

            PrefabBrushTool.isUsingTool = !PrefabBrushTool.isUsingTool;

            if (PrefabBrushTool.isUsingTool)
            {
                if (Application.isPlaying)
                {
                    DisplayError("Cannot use Prefab Brush Tool while in Play Mode.");
                    PrefabBrushTool.isUsingTool = false;
                    return;
                }

                PrefabBrushTool.EnableAutoRefresh();
                Selection.objects = Array.Empty<Object>();
                _addSelectedPrefabsPanel.SetActive(false);
                PaintMode mode = GetPaintMode();

                if (mode == PaintMode.Eraser)
                {
                    UpdateUITool();
                    return;
                }

                if (currentPrefabs.Count == 0)
                {
                    PrefabBrushTool.isUsingTool = false;
                    DisplayError("No Prefabs Added. Please add at least one prefab to the list.");
                    return;
                }

                if (mode == PaintMode.Precision && PbPrefabUI.HasAnySelected() == false)
                {
                    PbPrefabUI.SelectFirst();
                }

                if (mode == PaintMode.Multiple && !PbPrefabUI.HasAnySelected())
                {
                    PbPrefabUI.SelectAll();
                }
            }
            else
            {
                PB_MeshBatcher.Dispose();
                PB_PrecisionModeManager.DisposePrecisionMode();
                PrefabBrushObject.Dispose();
                PB_PhysicsSimulator.Dispose();
            }

            UpdateUITool();
        }

        private static void OnRevealTemplateButton(ClickEvent evt)
        {
            //log
#if HARPIA_DEBUG
            Debug.Log("Reveal template button clicked");
#endif
            PrefabBrushTemplate.RevealCurrentTemplate();
        }

        private void OnSaveTemplateButton(ClickEvent evt)
        {
            //log
#if HARPIA_DEBUG
            Debug.Log("Save template button clicked");
#endif

            bool result = PrefabBrushTemplate.SaveTemplate(instance);

            if (!result)
            {
                return;
            }

            DisplayStatus("Template saved");

            //Update dropdown
            _templatesDropdown.choices = PrefabBrushTemplate.GetAllPresetsNames();
            _templatesDropdown.SetValueWithoutNotify(PrefabBrushTemplate.GetCurrentTemplateName());
        }

        private static void DisplayStatus(string msg)
        {
            EditorUtility.DisplayDialog("Prefab Brush", msg, "Ok");
        }

        private void OnTemplateChanged(ChangeEvent<string> evt)
        {
            if (evt.newValue == PrefabBrushTemplate.NoPresetsFound) return;

#if HARPIA_DEBUG
            Debug.Log("Template changed to " + evt.newValue);
#endif

            _lastLoadedTemplate = PrefabBrushTemplate.LoadTemplate(evt.newValue);
            if (_lastLoadedTemplate == null) return;

            if (_lastLoadedTemplate == null)
            {
                if (evt.newValue != "None") DisplayError($"Could not load template {evt.newValue}");
                return;
            }

            LoadTemplate(_lastLoadedTemplate);
        }

        #region DragAndDrop

        private void OnDragPrefabsLeave(DragLeaveEvent evt)
        {
            //log
#if HARPIA_DEBUG
            Debug.Log("Drag leave");
#endif

            _dragAndDropLabel.text = _dragAndDropOriginalText;
        }

        private void OnDragPrefabsExit(DragExitedEvent evt)
        {
#if HARPIA_DEBUG
            Debug.Log($"Drag exited - Trying to add prefabs - qtd {DragAndDrop.objectReferences.Length}");
#endif
            foreach (Object o in DragAndDrop.objectReferences)
            {
                GameObject originalGO = o as GameObject;
                if (originalGO == null)
                {
                    string path = AssetDatabase.GetAssetPath(o);
                    Debug.LogError($"{PrefabBrush.DebugLogStart} Not a game object at {path}");
                    continue;
                }

                GameObject assetGo = PrefabBrushTool.GetPrefabAsset(originalGO);
                if (assetGo == null)
                {
#if HARPIA_DEBUG
                    Debug.LogError($"Could not get prefab asset for {originalGO.gameObject.name}", originalGO);
#endif      
                    AddPrefab(originalGO);
                    continue;
                }

                AddPrefab(assetGo);
            }

            _dragAndDropLabel.text = _dragAndDropOriginalText;
        }

        private void OnDragAndDropPrefabsEnter(DragEnterEvent dragEnterEvent)
        {
            //log
#if HARPIA_DEBUG
            Debug.Log($"OnDragPerformedEvent {DragAndDrop.objectReferences.Length} {Event.current.type}");
#endif

            if (DragAndDrop.objectReferences.Length == 0) return;

            _dragAndDropLabel.text = $"Drop {DragAndDrop.objectReferences.Length} files here";
        }

        #endregion

        private void AddPrefab(PbPrefabUI objectList)
        {
            if (objectList == null) return;
            if (currentPrefabs.Contains(objectList)) return;
            currentPrefabs.Add(objectList);
            objectList.InstantiateUI(prefabsHolder);
        }

        private void AddPrefab(params Object[] objectList)
        {
#if HARPIA_DEBUG
            Debug.Log($"[{nameof(PrefabBrush)}] Adding Prefabs to List {objectList.Length}");
#endif
            foreach (Object o in objectList)
            {
                if (o == null) continue;

                GameObject gameObject = o as GameObject;
                if (gameObject == null) continue;
                currentPrefabs.Add(new PbPrefabUI(prefabsHolder, gameObject));
            }
        }

        #region MenuItem_TopMenu

        [MenuItem("Tools/Prefab Brush/Documentation", false, 2)]
        private static void OpenDocumentation()
        {
            Application.OpenURL("https://harpiagames.gitbook.io/prefab-brush-documentation/");
        }

        [MenuItem("Tools/Prefab Brush/Rate This asset", false, 12)]
        private static void OpenRatePage()
        {
            Application.OpenURL("https://u3d.as/37hF");
        }

        [MenuItem("Tools/Prefab Brush/Discord", false, 11)]
        private static void OpenOpenDiscord()
        {
            Application.OpenURL("https://discord.gg/Tr952uhsqb");
        }

        [MenuItem("Tools/Prefab Brush/More Tools/Low Poly Color Changer", false, 12)]
        private static void ToolsLowPoly()
        {
            Application.OpenURL("https://assetstore.unity.com/packages/tools/level-design/low-poly-color-changer-easy-color-changing-variations-248562?aid=1100lACye");
        }

        [MenuItem("Tools/Prefab Brush/More Tools/Prefab Icons - Icon Creator", false, 12)]
        private static void ToolsIconCreator()
        {
            Application.OpenURL("https://assetstore.unity.com/packages/tools/game-toolkits/icon-creator-generate-fast-easy-complete-icons-generator-198488?aid=1100lACye&utm_campaign=unity_affiliate&utm_medium=affiliate&utm_source=partnerize-linkmaker");
        }

        #endregion

        #region MenuItem_Assets

        [MenuItem("GameObject/Prefab Brush/Add Prefabs", false)]
        [MenuItem("Assets/Prefab Brush/Add Prefabs", false)]
        private static void AddPrefabs()
        {
            if (instance == null) ShowWindow();

            GameObject[] selectedGameObjects = Selection.gameObjects;
            instance.AddPrefab(selectedGameObjects);
        }

        [MenuItem("GameObject/Prefab Brush/Use These Prefabs", false)]
        [MenuItem("Assets/Prefab Brush/Use These Prefabs", false)]
        private static void UseThesePrefabs()
        {
            List<GameObject> selectedGameObjects = PrefabBrushTool.GetPrefabs(Selection.objects);

            if (instance == null)
            {
                ShowWindow();
            }

            instance.OnClearListButton(null);
            instance.AddPrefab(selectedGameObjects.ToArray());
            instance.parentField.value = null;
        }

        [MenuItem("Assets/Prefab Brush/Add Folder Prefabs", false)]
        private static void AddFolderPrefabs()
        {
            string selectedFolder = PB_FolderUtils.GetSelectedPathOrFallback();
#if HARPIA_DEBUG
            Debug.Log($"[{nameof(PrefabBrush)}] AddFolderPrefabs - selectedFolder: {selectedFolder}");
#endif
            if (instance == null) ShowWindow();
            List<GameObject> prefabs = PB_FolderUtils.GetPrefabs(selectedFolder);
            instance.AddPrefab(prefabs.ToArray());
        }

        [MenuItem("Assets/Prefab Brush/Add Folder Prefabs", true)]
        private static bool AddFolderPrefabsValidate()
        {
            string selectedFolder = PB_FolderUtils.GetSelectedPathOrFallback();
            bool hasAnyPrefab = PB_FolderUtils.HasAnyPrefab(selectedFolder);
            return !string.IsNullOrEmpty(selectedFolder) && hasAnyPrefab;
        }

        [MenuItem("GameObject/Prefab Brush/Use These Prefabs", true)]
        [MenuItem("Assets/Prefab Brush/Use These Prefabs", true)]
        private static bool ValidateUseThesePrefabs()
        {
            return PrefabBrushTool.HasAnyPrefab(Selection.objects);
        }

        [MenuItem("GameObject/Prefab Brush/Add Prefabs", true)]
        [MenuItem("Assets/Prefab Brush/Add Prefabs", true)]
        private static bool ValidateAddPrefabs()
        {
            return PrefabBrushTool.HasAnyPrefab(Selection.objects);
        }

        #endregion

        private void OnSelectionChanged()
        {
#if HARPIA_DEBUG
            Debug.Log($"[{nameof(PrefabBrush)}] OnSelectionChanged");
#endif

            PB_PhysicsSimulator.Dispose(!PrefabBrushTool.isUsingTool);
            PB_PhysicsSimulator.UpdateRigidbodiesObjects();
            PB_MeshBatcher.Dispose();
            _addSelectedPrefabsPanel.SetActive(false);

            if (Selection.objects.Length > 0)
            {
                ExitTool();
            }

            if (GetPaintMode() == PaintMode.Eraser) return;
            if (Selection.objects.Length == 0) return;
            if (!PrefabBrushTool.HasAnyPrefab(Selection.objects)) return;

            _prefabsToAddFromSelection = PrefabBrushTool.GetPrefabs(Selection.objects);
            if (_prefabsToAddFromSelection.Count == 0) return;

            _addSelectedPrefabsLabel.text = $"Add {_prefabsToAddFromSelection.Count} Selected Prefab(s) ";
            _addSelectedPrefabsPanel.SetActive(true);
        }

        private void OnActiveToolChanging()
        {
            //log the tool
#if HARPIA_DEBUG
            Debug.Log($"Active tool changing from {ToolManager.activeToolType}");
#endif
            ExitTool();
            UpdateUITool();
        }

        private void OnActiveContextChanging()
        {
            //log the tool
#if HARPIA_DEBUG
            Debug.Log($"Active context changing to {ToolManager.activeContextType}");
#endif
            UpdateUITool();
        }

        private void OnActiveToolChanged()
        {
            //log the tool
#if HARPIA_DEBUG
            Debug.Log($"Active tool changed to {ToolManager.activeToolType}");
#endif

            bool isUsingPrefabTool = ToolManager.activeToolType.ToString().Equals(nameof(PrefabBrushTool));
            PrefabBrushTool.isUsingTool = isUsingPrefabTool;

            if (isUsingPrefabTool)
            {
                Selection.activeGameObject = null;
            }

            UpdateUITool();
        }

        private void UpdateUITool()
        {
            bool isUsingTool = PrefabBrushTool.isUsingTool;

            if (!CanUseTool())
            {
                _statusLabel.text = "Cannot Use The Tool On Prefab Mode";
                _statusBackground.style.backgroundColor = new Color(1f, 0f, 0f, 0.4f);
                _statusButton.text = "Start";
                _statusButton.SetActive(false);
            }

            else if (isUsingTool)
            {
                _statusLabel.text = "Brush is running";
                _statusBackground.style.backgroundColor = _styleBackgroundColorGreen;
                _statusButton.text = "Stop";
                _statusButton.SetActive(true);
            }
            else
            {
                _statusLabel.text = "Brush is not running";
                _statusBackground.style.backgroundColor = new Color(1f, 0f, 0f, 0.4f);
                _statusButton.text = "Start";
                _statusButton.SetActive(true);
            }

            bool isInMultipleMode = GetPaintMode() == PaintMode.Multiple;
            bool isInPrecisionMode = GetPaintMode() == PaintMode.Precision;
            bool isInEraserMode = GetPaintMode() == PaintMode.Eraser;

            if (gridSnapToggle.value)
            {
                gridSnapToggle.SetValueWithoutNotify(isInPrecisionMode);
                if (gridSnapValueField.value <= 0) gridSnapValueField.SetValueWithoutNotify(0.1f);
            }

            _brushFoldout.SetActive(isInMultipleMode);
            _precisionModeFoldout.SetActive(isInPrecisionMode);
            gridSnapToggle.SetActive(!isInMultipleMode);

            ParentMode parentMode = GetParentMode();
            parentField.SetActive(parentMode == ParentMode.Fixed_Transform);
            parentNameInput.SetActive(parentMode == ParentMode.Fixed_Transform);
            createParentButton.SetActive(parentMode == ParentMode.Fixed_Transform);
            _requiredParentWarningLabel.SetActive(parentMode == ParentMode.Fixed_Transform && parentField.value == null);

            tagMaskField.SetActive(isInMultipleMode || isInPrecisionMode);
            precisionModeAddMeshToBatch.SetActive(GetRaycastMode() == RaycastMode.Mesh);

            bool angleLimits = useAngleLimitsToggle.value;
            angleLimitsField.parent.SetActive(angleLimits);
            angleMinField.parent.SetActive(angleLimits);

            bool snapToGrid = gridSnapToggle.value && gridSnapToggle.IsActive();
            gridSnapToggle.SetActive(isInPrecisionMode);
            gridSnapValueField.SetActive(snapToGrid && isInPrecisionMode);
            gridOffsetField.SetActive(snapToGrid && isInPrecisionMode);

            bool showGuideLines = _showBrushGuideLines.value;
            brushGuideLinesDistance.SetActive(showGuideLines);

            bool showBounds = showBoundsToggle.value;
            boundsColorField.SetActive(showBounds);

            PrefabBrushObject.SetClippingSize(isInEraserMode ? eraserRadiusSlider : clippingToleranceSlider);
            PB_TextureMaskHandler.UpdateUITerrain();

            _eraserModeButton.SetBackgroundColor(isInEraserMode ? activeButtonColor : Color.clear);
            _paintModeButton.SetBackgroundColor(!isInEraserMode ? activeButtonColor : Color.clear);

            angleLimitsField.value = new Vector2(angleMinField.value, angleMaxField.value);

            scaleField.UpdateUIVec3Element();
            rotationField.UpdateUIVec3Element();
            offsetField.UpdateUIVec3Element();

            UpdateShortcuts();

            PB_PhysicsSimulator.UpdateUIPhysics();
        }

        private void UpdateShortcuts()
        {
            PB_ShortcutManager.ClearShortcuts();
            PB_ModularShortcuts.Dispose();

            if (_shortcutParent == null) return;

            switch (GetPaintMode())
            {
                case PaintMode.Eraser:
                    PB_EraserManager.AddShortcuts();
                    break;
                case PaintMode.Precision:
                    PB_PrecisionModeManager.AddShortcuts();
                    break;
                default:
                    PB_MultipleModeManager.AddShortcuts();
                    break;
            }

            PB_ShortcutManager.ApplyTo(_shortcutParent);
        }

        public PaintMode GetPaintMode()
        {
            return (PaintMode)paintModeDropdown.value;
        }

        public ParentMode GetParentMode()
        {
            return (ParentMode)parentModeDropdown.value;
        }

        public PivotMode GetPivotMode()
        {
            return (PivotMode)pivotMode.value;
        }

        #region OnSceneGUI

        private void OnSceneGUI(SceneView obj)
        {
            sceneCamera = obj.camera;
            bool isMouseOverWindow = PrefabBrushTool.IsMouseOverAnySceneView();

            if (!isMouseOverWindow)
            {
                OnSceneGuiFocusInteraction();
                return;
            }

            if (!PrefabBrushTool.isUsingTool) return;

            EventHandlerBoth();

            if (_isShiftDown || _isAltDown || _isCtrlDown)
            {
#if HARPIA_DEBUG
                //Debug.Log($"Returning Here - shift down {_isShiftDown} = alt down {_isAltDown} - ctrl down {_isCtrlDown} - {Event.current.keyCode} - {Event.current.type}");
#endif

                if (_isCtrlDown && Event.current.keyCode == KeyCode.Z && Event.current.type == EventType.KeyUp)
                {
                    PB_UndoManager.PerformUndo();
                    isChangeSizeDown = false;
                }

                PB_HandlesExtension.Update(deltaTime);

                return;
            }

            switch (GetPaintMode())
            {
                case PaintMode.Precision:
                    if (currentPrefabs.Count == 0) return;
                    OnSceneGuiPrecisionMode();
                    PB_HandlesExtension.Update(deltaTime);
                    PB_PhysicsSimulator.DrawArrowHandles();
                    break;

                case PaintMode.Multiple:
                    if (currentPrefabs.Count == 0) return;
                    OnSceneGUIMultiple();
                    PB_HandlesExtension.Update(deltaTime);
                    PB_PhysicsSimulator.DrawArrowHandles();
                    break;

                case PaintMode.Eraser:
                    OnSceneGuiEraser();
                    PB_HandlesExtension.Update(deltaTime);
                    break;
            }
        }

        private void OnSceneGUIMultiple()
        {
            int mouseButton = Event.current.button;

            switch (Event.current.type)
            {
                case EventType.MouseDrag:

                    if (mouseButton is 1 or 2)
                    {
                        isRaycastHit = false;
                        return;
                    }

                    if (isRaycastTagAllowed && mouseButton == 0 && GetPaintMode() == PaintMode.Multiple)
                    {
                        PB_PressurePen.Update();
                        UpdateMouseRaycast();
                        PaintDrag();
                    }

                    break;

                case EventType.MouseDown:

                    if (mouseButton is 1 or 2)
                    {
                        isRaycastHit = false;
                        return;
                    }

                    UpdateMouseRaycast();

                    if (isRaycastTagAllowed && Event.current.button == 0)
                    {
                        PB_MultipleModeManager.OnPaintStart();
                        PB_PressurePen.OnMouseDown();
                    }

                    UseCurrentEvent();
                    break;

                case EventType.MouseUp:
                    if (mouseButton == 0)
                    {
                        PB_UndoManager.RegisterUndo();
                        PrefabBrushObject.Dispose(false);
                        PB_PressurePen.OnMouseUp();
                    }

                    break;
                case EventType.Used:
                    UseCurrentEvent();
                    break;

                case EventType.MouseMove:
                    UpdateMouseRaycast();

                    break;

                case EventType.Repaint:
                    DrawPrefabCircleMultiple();
                    if (showBoundsToggle.value)
                    {
                        PrefabBrushObject.DrawBoundsAll(lastHitInfo.point, boundsColorField.value);
                    }

                    if (!isRaycastHit)
                        PB_HandlesExtension.WriteTempTextAtMousePos("No Raycast Hit", Color.white, 1, deltaTime);

                    break;

                case EventType.KeyDown:
                    OnKeyDownBrushMode();
                    break;
            }
        }

        private void OnSceneGuiEraser()
        {
            int mouseButton = Event.current.button;

            switch (Event.current.type)
            {
                case EventType.Repaint:
                    if (!isRaycastHit)
                    {
                        PB_HandlesExtension.WriteTempTextAtMousePos("No Raycast Hit", Color.white, 1, deltaTime);
                        return;
                    }

                    PB_EraserManager.DrawCircleEraser(lastHitInfo, eraserRadiusSlider.value);
                    PrefabBrushTool.DrawGuideLines(lastHitInfo);
                    break;

                case EventType.MouseDrag:
                    UpdateMouseRaycast();
                    if (mouseButton is 1 or 2) isRaycastHit = false;
                    else if (mouseButton == 0)
                    {
                        PB_EraserManager.Eraser(lastHitInfo, eraserRadiusSlider.value);
                        UseCurrentEvent();
                    }

                    break;

                case EventType.MouseMove:
                    UpdateMouseRaycast();
                    PB_TerrainHandler.Init();
                    break;

                case EventType.MouseDown:
                    UpdateShortcuts();
                    if (mouseButton is 1 or 2) isRaycastHit = false;
                    else if (mouseButton == 0)
                    {
                        if (focusedWindow.GetType() != typeof(SceneView)) return;
                        mouse0DownPosition = Event.current.mousePosition;
                        UseCurrentEvent();
                    }

                    break;

                case EventType.MouseUp:
                    if (mouseButton == 0)
                    {
                        PB_UndoManager.RegisterUndo();
                    }

                    break;

                case EventType.KeyDown:
                    OnKeyDownEraserMode();
                    break;
            }
        }

        private void OnSceneGuiPrecisionMode()
        {
            int mouseButton = Event.current.button;
            switch (Event.current.type)
            {
                case EventType.MouseDrag:
                    if (mouseButton is 1 or 2)
                    {
                        isRaycastHit = false;
                    }

                    break;

                case EventType.MouseDown:

                    UpdateShortcuts();

                    if (mouseButton is 1 or 2)
                    {
                        isRaycastHit = false;
                        return;
                    }

                    UseCurrentEvent();
                    if (mouseButton == 0)
                    {
                        _successPainted = false;
                        if (focusedWindow != null && focusedWindow.GetType() != typeof(SceneView)) return;
                        if (!IsParentOk())
                        {
                            PB_HandlesExtension.WriteTextErrorTemp("Check Parent Settings", lastHitInfo, .8f);
                            return;
                        }

                        if (!isRaycastHit) return;
                        if (!IsAngleValid(lastHitInfo.normal))
                        {
                            PB_HandlesExtension.WriteTextErrorTemp("Invalid Angle", lastHitInfo, .8f);
                            UseCurrentEvent();
                            return;
                        }

                        _successPainted = PB_PrecisionModeManager.PaintPrefab(GetParent());
                        mouse0DownPosition = Event.current.mousePosition;
                    }

                    break;

                case EventType.MouseUp:
                    if (mouseButton == 0 && _successPainted)
                    {
                        PB_UndoManager.RegisterUndo();
                        PB_PrecisionModeManager.SetPrefabToPaint();
                        _isMouse0Down = false;
                    }

                    break;

                case EventType.Used:
                    UseCurrentEvent();
                    break;

                case EventType.MouseMove:
                    if (Event.current.control || Event.current.command || Event.current.shift) break;
                    UpdateMouseRaycast();
                    break;

                case EventType.Repaint:
                    if (!isRaycastHit)
                    {
                        PB_PrecisionModeManager.SetCurrentObjectActive(false);
                        if (mouseButton != 2 && mouseButton != 1 && !_isMouse1Down)
                            PB_HandlesExtension.WriteTempTextAtMousePos("No Raycast Hit", Color.white, 1, deltaTime);

                        return;
                    }

                    PrefabBrushTool.DrawGrid(lastHitInfo, false);
                    PB_PrecisionModeManager.SetCurrentObjectActive(true);
                    PB_PrecisionModeManager.DrawTemporaryPrefab(lastHitInfo);
                    PB_HandlesExtension.WriteAngle(lastHitInfo);
                    break;

                case EventType.KeyDown:
                    OnKeyDownPrecisionMode();
                    break;

                case EventType.MouseLeaveWindow:
                    PB_PrecisionModeManager.HideObject();
                    break;

                case EventType.MouseEnterWindow:
                    PB_PrecisionModeManager.ShowObject();
                    break;

                case EventType.ScrollWheel:

                    if (GetPaintMode() != PaintMode.Precision) return;

                    //Scrollwhell rotate object
                    float scrollValue = Event.current.delta.y;

                    if (isXDown)
                    {
                        PB_PrecisionModeManager.RotateCurrentObject(new Vector3(scrollValue, 0, 0));
                        UseCurrentEvent();
                    }
                    else if (isYDown)
                    {
                        PB_PrecisionModeManager.RotateCurrentObject(new Vector3(0, scrollValue, 0));
                        UseCurrentEvent();
                    }
                    else if (isZDown)
                    {
                        PB_PrecisionModeManager.RotateCurrentObject(new Vector3(0, 0, scrollValue));
                        UseCurrentEvent();
                    }
                    else if (isChangeSizeDown)
                    {
                        scrollValue *= .01f;
                        PB_PrecisionModeManager.AddScale(scrollValue);
                        UseCurrentEvent();
                    }
                    else if (isAdjustDistanceFromGroundDown)
                    {
                        PB_PrecisionModeManager.AdjustDistanceY(scrollValue);
                        UseCurrentEvent();
                    }

                    break;
            }
        }

        private void OnSceneGuiFocusInteraction()
        {
            if (VisualElementsExtension.focusElement == null) return;
            if (Event.current.type != EventType.Repaint) return;

            VisualElement focus = VisualElementsExtension.focusElement;
            if (focus == brushRadiusSlider || focus == PB_AdvancedSettings.brushBaseColor || focus == PB_AdvancedSettings.brushBorderColor)
            {
                RaycastHit hitInfo = PrefabBrushTool.GetCenterRay(sceneCamera, GetRaycastMode(), layerMaskField.value);
                PrefabBrushTool.DrawCircle(hitInfo, brushRadiusSlider.value, true);
                return;
            }

            if (focus == clippingToleranceSlider)
            {
                RaycastHit hitInfo = PrefabBrushTool.GetCenterRay(sceneCamera, GetRaycastMode(), layerMaskField.value);
                PrefabBrushObject.DrawBoundsAll(hitInfo.point, boundsColorField.value);
                return;
            }

            if (focus == scaleField.maxField)
            {
                RaycastHit hitInfo = PrefabBrushTool.GetCenterRay(sceneCamera, GetRaycastMode(), layerMaskField.value);
                PB_PrecisionModeManager.DrawTemporaryPrefab(hitInfo, scaleField.maxField.value);
                return;
            }

            if (focus == scaleField.minField)
            {
                RaycastHit hitInfo = PrefabBrushTool.GetCenterRay(sceneCamera, GetRaycastMode(), layerMaskField.value);
                PB_PrecisionModeManager.DrawTemporaryPrefab(hitInfo, scaleField.minField.value);
                return;
            }

            if (focus == offsetField.fixedField)
            {
                RaycastHit hitInfo = PrefabBrushTool.GetCenterRay(sceneCamera, GetRaycastMode(), layerMaskField.value);
                PrefabBrushTool.DrawDottedLines(hitInfo, offsetField.fixedField.value);
                PB_PrecisionModeManager.DrawTemporaryPrefab(hitInfo, offsetField.fixedField.value);
                return;
            }

            if (focus == _showBrushGuideLines || focus == brushGuideLinesDistance)
            {
                RaycastHit hitInfo = PrefabBrushTool.GetCenterRay(sceneCamera, GetRaycastMode(), layerMaskField.value);
                PrefabBrushTool.DrawGuideLines(hitInfo);
                return;
            }

            if (focus == eraserRadiusSlider || focus == PB_AdvancedSettings.eraserColor)
            {
                RaycastHit hitInfo = PrefabBrushTool.GetCenterRay(sceneCamera, GetRaycastMode(), layerMaskField.value);
                PB_EraserManager.DrawCircleEraser(hitInfo, eraserRadiusSlider.value);
                return;
            }

            if (focus == gridSnapToggle || focus == gridSnapValueField || focus == PB_AdvancedSettings.gridColor || focus == gridOffsetField)
            {
                RaycastHit hitInfo = PrefabBrushTool.GetCenterRay(sceneCamera, GetRaycastMode(), layerMaskField.value);
                PrefabBrushTool.DrawGrid(hitInfo, true);
                return;
            }

            if (focus == PB_PhysicsSimulator.impulseFieldVec3.fixedField || focus == PB_PhysicsSimulator.fixedButton)
            {
                RaycastHit hitInfo = PrefabBrushTool.GetCenterRay(sceneCamera, GetRaycastMode(), layerMaskField.value);
                PB_PhysicsSimulator.DrawArrowHandles(PB_PhysicsSimulator.impulseFieldVec3.fixedField.value, hitInfo.point);
                return;
            }

            if (focus == PB_PhysicsSimulator.impulseFieldVec3.minField || focus == PB_PhysicsSimulator.minButton)
            {
                RaycastHit hitInfo = PrefabBrushTool.GetCenterRay(sceneCamera, GetRaycastMode(), layerMaskField.value);
                PB_PhysicsSimulator.DrawArrowHandles(PB_PhysicsSimulator.impulseFieldVec3.minField.value, hitInfo.point);
                return;
            }

            if (focus == PB_PhysicsSimulator.impulseFieldVec3.maxField || focus == PB_PhysicsSimulator.maxButton)
            {
                RaycastHit hitInfo = PrefabBrushTool.GetCenterRay(sceneCamera, GetRaycastMode(), layerMaskField.value);
                PB_PhysicsSimulator.DrawArrowHandles(PB_PhysicsSimulator.impulseFieldVec3.maxField.value, hitInfo.point);
            }
        }

        #endregion

        #region KeyDownEvents

        private void OnKeyDownEraserMode()
        {
            if (PB_ModularShortcuts.increaseRadius.IsShortcut())
            {
                PB_EraserManager.IncreaseRadius(brushSizeIncrement);
            }
            else if (PB_ModularShortcuts.decreaseRadius.IsShortcut())
            {
                PB_EraserManager.IncreaseRadius(-brushSizeIncrement);
            }
        }

        private void OnKeyDownPrecisionMode()
        {
            if (PB_ModularShortcuts.rotateRight.IsShortcut())
            {
                Vector3 rotY = PB_PrecisionModeManager.AddToRotation(precisionModeRotationAngle.value);
                PB_HandlesExtension.WriteVector3(rotY, lastHitInfo, "Rotation", "°");
            }

            else if (PB_ModularShortcuts.rotateLeft.IsShortcut())
            {
                Vector3 rotY2 = PB_PrecisionModeManager.AddToRotation(-precisionModeRotationAngle.value);
                PB_HandlesExtension.WriteVector3(rotY2, lastHitInfo, "Rotation", "°");
            }

            else if (PB_ModularShortcuts.randomRotation.IsShortcut())
            {
                Vector3 randomRot = scaleField.GetValue();
                PB_PrecisionModeManager.SetRotation(randomRot);
                PB_HandlesExtension.WriteVector3(randomRot, lastHitInfo, "Rotation", "°");
            }

            else if (PB_ModularShortcuts.decreaseRadius.IsShortcut())
            {
                Vector3 newSize = PB_PrecisionModeManager.IncreaseSize(-.1f);
                PB_HandlesExtension.WriteTempText("Size: ", newSize, lastHitInfo);
            }

            else if (PB_ModularShortcuts.increaseRadius.IsShortcut())
            {
                Vector3 newSize2 = PB_PrecisionModeManager.IncreaseSize(.1f);
                PB_HandlesExtension.WriteTempText("Size: ", newSize2, lastHitInfo);
            }

            else if (PB_ModularShortcuts.normalizeSize.IsShortcut())
            {
                Vector3 normalizedSize = PB_PrecisionModeManager.NormalizeSize();
                PB_HandlesExtension.WriteTempText("Size: ", normalizedSize, lastHitInfo);
            }

            else if (PB_ModularShortcuts.nextPrefab.IsShortcut())
            {
                PB_PrecisionModeManager.SetPrefabToPaint(GetNextPrefab(PB_PrecisionModeManager.PrefabToPaint));
            }

            else if (PB_ModularShortcuts.previousPrefab.IsShortcut())
            {
                PB_PrecisionModeManager.SetPrefabToPaint(GetNextPrefab(PB_PrecisionModeManager.PrefabToPaint, true));
            }
        }

        private void OnKeyDownBrushMode()
        {
            if (PB_ModularShortcuts.exitTool.IsShortcut())
            {
                ExitTool();
            }
            else if (PB_ModularShortcuts.increaseRadius.IsShortcut())
            {
                IncreaseBrushSize(brushSizeIncrement);
            }
            else if (PB_ModularShortcuts.decreaseRadius.IsShortcut())
            {
                IncreaseBrushSize(-brushSizeIncrement);
            }
        }

        #endregion

        private PbPrefabUI GetNextPrefab(GameObject prefabToPaint, bool reverse = false)
        {
            int index = currentPrefabs.FindIndex(e => e.prefabToPaint == prefabToPaint);

            if (!reverse)
            {
                index++;
                if (index >= currentPrefabs.Count) index = 0;
            }
            else
            {
                index--;
                if (index < 0) index = currentPrefabs.Count - 1;
            }

            currentPrefabs[index].Select();
            return currentPrefabs[index];
        }

        private void EventHandlerBoth()
        {
            EventType evtType = Event.current.type;

            if (evtType == EventType.Repaint)
            {
                double time = EditorApplication.timeSinceStartup;
                deltaTime = (float)(time - lastRepaintTime);
                lastRepaintTime = time;

                if (_isMouse1Down) return;

                PrefabBrushTool.DrawBounds(new Bounds(), true);

                if (!isRaycastTagAllowed)
                {
                    if (lastHitInfo.transform != null)
                        PB_HandlesExtension.WriteTextErrorTemp($"Tag not allowed: {lastHitInfo.transform.tag}", lastHitInfo);
                }

                if (!_isTextureAllowed)
                    PB_HandlesExtension.WriteTextErrorTemp($"Texture Not Allowed", lastHitInfo);

                return;
            }

            if (evtType == EventType.KeyDown)
            {
                if (PB_ModularShortcuts.changeMode.IsShortcut(Event.current.keyCode))
                {
                    if (PB_EraserManager.IsOnEraseMode()) ExitEraserMode();
                    else EnterEraserMode();
                    return;
                }

                //ScrollWheel shortcuts
                if (PB_ModularShortcuts.rotationXShortcut.IsShortcut())
                {
                    isXDown = true;
                    if (GetPaintMode() == PaintMode.Precision) UseCurrentEvent();
                    return;
                }

                if (PB_ModularShortcuts.rotationYShortcut.IsShortcut())
                {
                    isYDown = true;
                    if (GetPaintMode() == PaintMode.Precision) UseCurrentEvent();
                    return;
                }

                if (PB_ModularShortcuts.rotationZShortcut.IsShortcut())
                {
                    isZDown = true;
                    if (GetPaintMode() == PaintMode.Precision) UseCurrentEvent();
                    return;
                }

                if (PB_ModularShortcuts.changeScaleShortcut.IsShortcut())
                {
                    isChangeSizeDown = true;
                    if (GetPaintMode() == PaintMode.Precision) UseCurrentEvent();
                    return;
                }

                if (PB_ModularShortcuts.yDisplacementShortcut.IsShortcut())
                {
                    isAdjustDistanceFromGroundDown = true;
                    if (GetPaintMode() == PaintMode.Precision) UseCurrentEvent();
                    return;
                }

                switch (Event.current.keyCode)
                {
                    case KeyCode.LeftShift or KeyCode.RightShift:
                        if (!_isShiftDown) _lastMousePosShift = Event.current.mousePosition;
                        _isShiftDown = true;
                        PB_PrecisionModeManager.HideObject();
                        break;

                    case KeyCode.LeftAlt or KeyCode.RightAlt or KeyCode.AltGr:
                        _isAltDown = true;
                        PB_PrecisionModeManager.HideObject();
                        break;

                    case KeyCode.RightControl or KeyCode.LeftControl or KeyCode.LeftCommand or KeyCode.RightCommand or KeyCode.LeftApple or KeyCode.RightApple:
                        if (!_isCtrlDown) _lastMousePosCtrl = Event.current.mousePosition;
                        _isCtrlDown = true;
                        PB_PrecisionModeManager.HideObject();
                        break;
                }

                return;
            }

            if (evtType == EventType.KeyUp)
            {
                if (PB_ModularShortcuts.rotationXShortcut.IsShortcut())
                {
                    isXDown = false;
                    return;
                }

                if (PB_ModularShortcuts.rotationYShortcut.IsShortcut())
                {
                    isYDown = false;
                    return;
                }

                if (PB_ModularShortcuts.rotationZShortcut.IsShortcut())
                {
                    isZDown = false;
                    return;
                }

                if (PB_ModularShortcuts.changeScaleShortcut.IsShortcut())
                {
                    isChangeSizeDown = false;
                    return;
                }

                if (PB_ModularShortcuts.yDisplacementShortcut.IsShortcut())
                {
                    isAdjustDistanceFromGroundDown = false;
                    return;
                }

                switch (Event.current.keyCode)
                {
                    case KeyCode.LeftShift or KeyCode.RightShift:
                        _isShiftDown = false;
                        break;
                    case KeyCode.RightControl or KeyCode.LeftControl or KeyCode.LeftCommand or KeyCode.RightCommand:
                        _isCtrlDown = false;
                        break;

                    case KeyCode.LeftAlt or KeyCode.RightAlt or KeyCode.AltGr:
                        _isAltDown = false;
                        break;
                }

                if (PB_ModularShortcuts.exitTool.IsShortcut()) ExitTool();

                return;
            }

            if (evtType == EventType.MouseDown)
            {
                PrefabBrushTool.EnableAutoRefresh();
                UpdateShortcuts();
                switch (Event.current.button)
                {
                    case 0:
                        mouse0DownPosition = Event.current.mousePosition;
                        LastMouse0DownTime = EditorApplication.timeSinceStartup;
                        _isMouse0Down = true;
                        UpdateShortcuts();
                        break;
                    case 1:
                        _isMouse1Down = true;
                        break;
                }

                return;
            }

            if (evtType == EventType.MouseUp)
            {
                switch (Event.current.button)
                {
                    case 0:
                        _isMouse0Down = false;
                        LastMouse0DownTime = double.PositiveInfinity;
                        break;
                    case 1:
                        _isMouse1Down = false;
                        break;
                }

                UpdateShortcuts();
            }

            if (evtType == EventType.MouseEnterWindow)
            {
                //get the scene view
                if (!PrefabBrushTool.IsMouseOverAnySceneView()) return;
                SceneView sceneView = SceneView.lastActiveSceneView;
                if (sceneView != null) sceneView.Focus();
                FocusWindowIfItsOpen<SceneView>();
            }
        }

        private void EnterEraserMode()
        {
#if HARPIA_DEBUG
            Debug.Log($"EnterEraserMode", this);
#endif
            Selection.activeObject = null;
            lastMode = GetPaintMode();
            paintModeDropdown.SetValueWithoutNotify(PaintMode.Eraser);
            gridSnapToggle.SetActive(false);

            PB_AdvancedSettings.SetActive(false);
            PB_PhysicsSimulator.impulseFieldVec3.SetActive(false);
            PB_PhysicsSimulator.impulseForceSlider.parent.SetActive(false);

            PB_PrecisionModeManager.SetCurrentObjectActive(false);
            UpdateUITool();
            UpdateEraserModeUI();
            PrefabBrushTool.DisposeBounds();
            PrefabBrushObject.ForceInit();
            CustomPrefabProps.Hide();
            PB_HandlesExtension.WriteTempTextAtMousePos($"Entered {GetPaintMode()} Mode", Color.white, 2, 1f);
        }

        private void ExitEraserMode()
        {
#if HARPIA_DEBUG
            Debug.Log($"ExitEraserMode", this);
#endif
            PB_PrecisionModeManager.SetCurrentObjectActive(true);
            PB_PrecisionModeManager.SetCurrentObjectPos(lastHitInfo.point);
            paintModeDropdown.SetValueWithoutNotify(lastMode);

            PB_PhysicsSimulator.impulseFieldVec3.SetActive(true);
            PB_PhysicsSimulator.impulseForceSlider.parent.SetActive(true);

            UpdateEraserModeUI();
            PB_HandlesExtension.WriteTempTextAtMousePos($"Entered {GetPaintMode()} Mode", Color.white, 2, 1f);
            PrefabBrushObject.Dispose(false);

            UpdateUITool();
        }

        private void UpdateEraserModeUI()
        {
            bool onEraserMode = GetPaintMode() == PaintMode.Eraser;
            _precisionModeFoldout.SetActive(!onEraserMode);
            parentField.parent.SetActive(!onEraserMode);
            pivotMode.parent.SetActive(!onEraserMode);
            scaleField.enumField.parent.SetActive(!onEraserMode);

            prefabsHolder.parent.SetActive(!onEraserMode);
            _templatesDropdown.parent.parent.SetActive(!onEraserMode);
            _dragAndDropLabel.parent.SetActive(!onEraserMode);
            _addSelectedPrefabsPanel.SetActive(false);

            eraserRadiusSlider.SetActive(onEraserMode);
            UpdateShortcuts();
        }

        private void IncreaseBrushSize(float p0)
        {
            float value = brushRadiusSlider.value;
            value += p0 * value;
            value = Mathf.Clamp(value, brushRadiusSlider.lowValue, brushRadiusSlider.highValue);
            brushRadiusSlider.value = value;

            string format = value < 1 ? "f2" : "f1";
            string t1 = "Radius: " + value.ToString(format);
            PB_HandlesExtension.WriteTempTextAtMousePos(t1, Color.white, 3);
        }

        private void OnRaycastModeChanged(ChangeEvent<Enum> evt)
        {
#if HARPIA_DEBUG
            Debug.Log($"On Raycast Mode Changed {evt.newValue}", this);
#endif

            RaycastMode newMode = (RaycastMode)evt.newValue;
            if (newMode == RaycastMode.Mesh)
            {
                if (!SystemInfo.supportsComputeShaders)
                {
                    DisplayError("Your system does not support compute shaders :(\n\nReverting mode to Physical Colliders");
                    raycastModeDropdown.SetValueWithoutNotify(RaycastMode.Physical_Collider);
                    return;
                }

                if (PB_TerrainHandler.HasAnyTerrain())
                {
                    DisplayStatus("Attention: Mesh Raycast mode is not compatible with Terrains. If you are facing issues, please switch to Physical Collider mode.");
                }
            }

            PB_TextureMaskHandler.UpdateUITerrain();
        }

        public void ExitTool()
        {
            if (PrefabBrushTool.isUsingTool == false) return;
#if HARPIA_DEBUG
            Debug.Log($"Exiting tool");
#endif

            PrefabBrushTool.isUsingTool = false;
            PrefabBrushObject.Dispose();
            PB_PrecisionModeManager.DisposePrecisionMode();
            PB_MeshRaycaster.Dispose();
            PB_TerrainHandler.Dispose();
            PB_TextureMaskHandler.Dispose();
            PB_PhysicsSimulator.Dispose();
            UpdateShortcuts();
            UpdateUITool();
        }

        private void DisposeKeysVariables()
        {
            isXDown = false;
            isYDown = false;
            isZDown = false;
            _isShiftDown = false;
            _isAltDown = false;
            _isCtrlDown = false;
            isAdjustDistanceFromGroundDown = false;
            isChangeSizeDown = false;
        }

        private void ResetStaticVariables()
        {
            mouse0DownPosition = Vector2.zero;
            _lastMousePosCtrl = Vector2.zero;
            _lastMousePosShift = Vector2.zero;
            LastMouse0DownTime = 0;

            _isMouse0Down = false;
            _isMouse1Down = false;
            isRaycastHit = false;
            lastHitInfo = new RaycastHit();

            PB_PhysicsSimulator.Dispose(false);
            PB_PhysicsSimulator.UpdateRigidbodiesObjects();
        }

        public bool IsParentOk()
        {
            return GetParentMode() != ParentMode.Fixed_Transform || parentField.value != null;
        }

        public Transform GetParent()
        {
            switch (GetParentMode())
            {
                case ParentMode.Fixed_Transform: return parentField.value as Transform;
                case ParentMode.Hit_Surface_Object: return lastHitInfo.transform;
                case ParentMode.No_Parent: return null;
            }

            return null;
        }

        private void PaintDrag()
        {
            if (GetParentMode() == ParentMode.Fixed_Transform && parentField.value == null)
            {
                Debug.LogError($"{DebugLogStart} No Parent Selected, please attach a parent or change the parent mode");
                return;
            }

            bool chance = Random.value <= sliderBrushStrength.value;
            if (!chance) return;

            randomPointInsideDisc = PrefabBrushTool.GetRandomPointInsideDisc(lastHitInfo, brushRadiusSlider.value);

            bool hitRandomPoint = PrefabBrushTool.RaycastToWorldPoint(sceneCamera, randomPointInsideDisc, layerMaskField.value,
                out RaycastHit hitInfoRandomPoint, GetRaycastMode());

            if (!hitRandomPoint) return;

            //get the angle
            float angle = Vector3.Angle(hitInfoRandomPoint.normal, Vector3.up);

            if (!IsAngleValid(angle)) return;
            if (!PB_TextureMaskHandler.IsTextureValid(hitInfoRandomPoint.point)) return;

            PB_MultipleModeManager.PaintPrefabMultiple(hitInfoRandomPoint, GetRandomPrefab(false));
        }

        public bool IsAngleValid(float a)
        {
            if (!useAngleLimitsToggle.value) return true;
            if (a > angleLimitsField.value.y) return false;
            if (a < angleLimitsField.value.x) return false;
            return true;
        }

        private bool IsAngleValid(Vector3 normal)
        {
            if (!useAngleLimitsToggle.value) return true;
            float a = Vector3.Angle(normal, Vector3.up);
            return IsAngleValid(a);
        }

        public static Quaternion GetAlignedRotation(RaycastHit hit)
        {
            return Quaternion.FromToRotation(Vector3.up, hit.normal);
        }

        public static void Rotate(RaycastHit hit, GameObject reference, bool allowRandom = true)
        {
            if (instance.toggleAlignWithGround.value)
            {
                reference.transform.rotation = GetAlignedRotation(hit);
            }

            if (allowRandom)
            {
                Vector3 rot = instance.scaleField.GetValue();
                reference.transform.Rotate(rot, Space.Self);
            }
        }

        public Vector3 GetLocalPosition(GameObject childObject, PivotMode pivotModeParam)
        {
            if (showBoundsToggle.value)
            {
                Bounds boundsToDraw = PB_BoundsManager.GetBounds(childObject);
                PrefabBrushTool.DrawBounds(boundsToDraw, false);
            }

            if (pivotModeParam == PivotMode.MeshPivot) return Vector3.zero;

            childObject.transform.position = Vector3.zero;

            Quaternion rotation = childObject.transform.rotation;
            Quaternion oldRot = rotation;

            //Bounds can only be calculated with identity rotation
            childObject.transform.rotation = Quaternion.identity;
            Bounds boundsToCalc = PB_BoundsManager.GetBounds(childObject);
            rotation = oldRot;

            childObject.transform.rotation = rotation;

            if (pivotModeParam == PivotMode.Bounds_Center)
            {
                Vector3 diff = childObject.transform.position - boundsToCalc.center;
                return diff;
            }

            if (pivotModeParam == PivotMode.Bounds_Center_Bottom)
            {
                Vector3 diff = childObject.transform.position - boundsToCalc.center + new Vector3(0, boundsToCalc.extents.y, 0);
                return diff;
            }

            if (pivotModeParam == PivotMode.Bounds_Center_Top)
            {
                Vector3 diff = childObject.transform.position - boundsToCalc.center - new Vector3(0, boundsToCalc.extents.y, 0);
                return diff;
            }

            return Vector3.zero;
        }

        private void UseCurrentEvent()
        {
            Event.current.Use();
            GUIUtility.hotControl = GUIUtility.GetControlID(FocusType.Passive);
        }

        private void DrawPrefabCircleMultiple()
        {
            if (!isRaycastHit) return;
            if (!isRaycastTagAllowed) return;

            bool validAngle = IsAngleValid(lastHitInfo.normal);
            PrefabBrushTool.DrawCircle(lastHitInfo, brushRadiusSlider.value, validAngle);

            if (!validAngle)
            {
                PB_HandlesExtension.WriteTextErrorTemp("Invalid Angle", lastHitInfo, deltaTime);
                return;
            }

            if (offsetField.GetMode() == Vector3Mode.Fixed)
            {
                if (offsetField.fixedField.value == Vector3.zero) return;
                PrefabBrushTool.DrawPoint(lastHitInfo.point + offsetField.fixedField.value);
                PrefabBrushTool.DrawDottedLines(lastHitInfo, offsetField.fixedField.value);
                return;
            }

            PrefabBrushTool.DrawPoint(lastHitInfo.point + offsetField.minField.value);
            PrefabBrushTool.DrawPoint(lastHitInfo.point + offsetField.maxField.value);
        }

        private void UpdateMouseRaycast()
        {
            void CalculateHitOnPlane(Ray ray)
            {
                if (instance.placeOnYZeroToggle.value && instance.GetPaintMode() == PaintMode.Precision)
                {
                    //get the raycast hit on Y 0 plane
                    Plane planeYZero = new Plane(Vector3.up, 0);
                    isRaycastHit = planeYZero.Raycast(ray, out float enter);
                    if (!isRaycastHit) return;
                    Vector3 point = ray.GetPoint(enter);
                    lastHitInfo.point = instance.GetSnapPosition(point);
                    lastHitInfo.normal = Vector3.up;
                }
            }

            isRaycastTagAllowed = true;
            _isTextureAllowed = true;

            if (GetRaycastMode() == RaycastMode.Mesh)
            {
                Mesh batch = PB_MeshBatcher.Batch();
                Ray ray = PrefabBrushTool.GetMouseGuiRay();
                isRaycastHit = PB_MeshRaycaster.Raycast(ray, batch, Matrix4x4.identity, out MeshRaycastResult meshHit);

                if (!isRaycastHit)
                {
                    CalculateHitOnPlane(ray);
                    return;
                }

                lastHitInfo = meshHit.ToHitInfo();
                lastHitInfo.point = instance.GetSnapPosition(lastHitInfo.point);
                return;
            }

            isRaycastHit = PrefabBrushTool.RaycastPhysicsGUI(sceneCamera.farClipPlane, layerMaskField.value, out lastHitInfo, out Ray r);
            lastHitInfo.point = instance.GetSnapPosition(lastHitInfo.point);

            if (!isRaycastHit)
            {
                CalculateHitOnPlane(r);
                return;
            }

            string tag = lastHitInfo.collider.gameObject.tag;
            isRaycastTagAllowed = IsTagAllowed(tag);
            _isTextureAllowed = PB_TextureMaskHandler.IsTextureValid(lastHitInfo.point);
        }

        private Vector3 GetSnapPosition(Vector3 point)
        {
            if (!CanUseGrid()) return point;

            Vector3 offset = PrefabBrush.instance.GetGridOffset();
            float snapValue = gridSnapValueField.value;
            return new Vector3(PrefabBrushTool.RoundTo(point.x, snapValue), point.y, PrefabBrushTool.RoundTo(point.z, snapValue)) + offset;
        }

        public RaycastMode GetRaycastMode()
        {
            return (RaycastMode)raycastModeDropdown.value;
        }

        private void OnActiveContextChanged()
        {
#if HARPIA_DEBUG
            Debug.Log($"Active context changed to {ToolManager.activeContextType}");
#endif
            UpdateUITool();
        }

        private void OnTagMaskChanged(ChangeEvent<int> evt)
        {
#if HARPIA_DEBUG
            if (evt != null)
                Debug.Log($"Tag mask changed to {evt.newValue} | {tagMaskField.choices.Count} ");
#endif

            _currentTagsSelected = new List<string>();
            int newValue = evt?.newValue ?? tagMaskField.value;

            if (newValue == 0)
            {
                //display error
                DisplayError("Not using any tag mask will make the brush not work");
                return;
            }

            //Everything
            if (newValue == -1)
            {
                _currentTagsSelected = tagMaskField.choices;
                return;
            }

            //Get the value in bits
            string binary = Convert.ToString(newValue, 2);

            for (int i = 0; i < binary.Length; i++)
            {
                char c = binary[i];
                if (c == '1') _currentTagsSelected.Add(tagMaskField.choices[binary.Length - i - 1]);
            }

#if HARPIA_DEBUG
            Debug.Log($"Tag selected {string.Join(", ", _currentTagsSelected)}");
#endif
            PB_MeshBatcher.Dispose();
        }

        private void OnLayerMaskChanged(ChangeEvent<int> changeEvent)
        {
#if HARPIA_DEBUG
            Debug.Log($"Layer mask changed to {changeEvent.newValue}");
#endif

            if (changeEvent.newValue == 0)
            {
                DisplayError("Not using any layer mask will make the brush not work");
            }

            PB_MeshBatcher.Dispose();
        }

        private void OnClearListButton(ClickEvent evt)
        {
#if HARPIA_DEBUG
            Debug.Log("Clear list button clicked");
#endif
            currentPrefabs.Clear();
            prefabsHolder.Clear();
        }

        private void OnParentChanged(ChangeEvent<Object> evt)
        {
#if HARPIA_DEBUG
            Debug.Log("Parent changed");
#endif
            UpdateUITool();
        }

        private void OnCreateParentButton(ClickEvent evt)
        {
            string parentBar = parentNameInput.value;

            if (string.IsNullOrEmpty(parentBar))
            {
                DisplayError("Parent name cannot be empty");
                return;
            }

            //Check if parent already exists
            GameObject parent = GameObject.Find(parentBar);

            if (parent != null)
            {
                //Ask user if they want to replace existing parent
                bool use = DisplayConfirmation(
                    $"There's already a object named {parentBar}.\nDo you want ot use it as parent?");
                if (!use) return;
            }
            else
            {
                parent = new GameObject(parentBar);
            }

            parentField.value = parent;
        }

        private bool DisplayConfirmation(string msg)
        {
            return EditorUtility.DisplayDialog("Prefab Brush", msg, "Yes", "Cancel");
        }

        public static void DisplayError(string msg)
        {
            EditorUtility.DisplayDialog("Prefab Brush - Error", msg, "Ok");
        }

        public static VisualTreeAsset LoadVisualTreeAsset(string xmlFileNameLocal, ref string visualTreeGuid)
        {
            string xmlFilePath = AssetDatabase.GUIDToAssetPath(visualTreeGuid);
            if (string.IsNullOrEmpty(xmlFilePath))
            {
                string[] foundedGUIDs = AssetDatabase.FindAssets(xmlFileNameLocal);

                if (foundedGUIDs.Length == 0)
                {
                    DisplayError($"Could not find the {xmlFileNameLocal}.uxml, did you renamed the file? If so rename it {xmlFileNameLocal}.uxml");
                    return null;
                }

                //get the first founded path
                visualTreeGuid = foundedGUIDs[0];
                xmlFilePath = AssetDatabase.GUIDToAssetPath(visualTreeGuid);
            }

            VisualTreeAsset loaded = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(xmlFilePath);
            if (loaded == null) Debug.LogError($"{DebugLogStart} Could not load file at {xmlFilePath}");

            return loaded;
        }

        public static void Remove(PbPrefabUI pbPrefabData)
        {
            instance.currentPrefabs.Remove(pbPrefabData);
            if (instance.currentPrefabs.Count == 0) instance.ExitTool();
        }

        public PbPrefabUI GetRandomPrefab(bool select, params GameObject[] exclude)
        {
            IEnumerable<PbPrefabUI> tempList;
            if (GetPaintMode() == PaintMode.Precision)
                tempList = currentPrefabs.Where(e => !exclude.Contains(e.prefabToPaint));
            else
                tempList = currentPrefabs.Where(e => !exclude.Contains(e.prefabToPaint) && e.selected);

            PbPrefabUI[] pbPrefabUis = tempList as PbPrefabUI[] ?? tempList.ToArray();
            if (!pbPrefabUis.Any()) return null;

            PbPrefabUI randomObj = pbPrefabUis.ElementAt(Random.Range(0, pbPrefabUis.Count()));

            if (select) randomObj.Select();

            return randomObj;
        }

        public List<string> GetPrefabGUIDs()
        {
            List<string> guids = new();

            foreach (PbPrefabUI prefab in currentPrefabs)
            {
                string guid = prefab.GetGuid();
                if (string.IsNullOrEmpty(guid)) continue;
                guids.Add(guid);
            }

            return guids;
        }

        public bool IsTagAllowed(string gameObjectTag)
        {
            if (_currentTagsSelected == null || _currentTagsSelected.Count == 0)
            {
                OnTagMaskChanged(null);
            }

            return _currentTagsSelected.Contains(gameObjectTag);
        }

        public bool IsLayerValid(int gameObjectLayer)
        {
            return layerMaskField.value == (layerMaskField.value | (1 << gameObjectLayer));
        }

        public static PrefabBrushObject RegisterObject(GameObject go)
        {
            if (instance.makeObjectsStaticToggle.value)
                go.isStatic = true;

            //Already has the component
            PrefabBrushObject brushObject = null;
            if (go.TryGetComponent(typeof(PrefabBrushObject), out Component com))
            {
                brushObject = com as PrefabBrushObject;
            }

            if (brushObject == null) brushObject = go.AddComponent<PrefabBrushObject>();

            brushObject.Init(PB_PhysicsSimulator.IsUsingPhysics());
            PB_PhysicsSimulator.ApplyImpulse(go);

            return brushObject;
        }

        public bool CanUseGrid()
        {
            return GetPaintMode() == PaintMode.Precision && gridSnapToggle.value;
        }

        public Vector3 GetGridOffset()
        {
            return new Vector3(gridOffsetField.value.x, 0, gridOffsetField.value.y);
        }

        public void MovePrefabCard(PbPrefabUI pbPrefabUI, bool isLeft)
        {
            //get the card index in the list
            int index = currentPrefabs.IndexOf(pbPrefabUI);
            int newIndex = isLeft ? index - 1 : index + 1;

#if HARPIA_DEBUG
            Debug.Log($"[{nameof(PrefabBrush)}] MovePrefabCard - index {index} - new index {newIndex}");
#endif

            if (newIndex < 0 || newIndex >= currentPrefabs.Count)
            {
                Debug.LogWarning($"[{DebugLogStart}] Could Not Move Prefab Card");
                return;
            }

            //swap the cards
            PbPrefabUI temp = currentPrefabs[newIndex];
            currentPrefabs[index] = temp;
            currentPrefabs[newIndex] = pbPrefabUI;

            //update the ui
            prefabsHolder.Clear();
            foreach (PbPrefabUI ui in currentPrefabs)
            {
                ui.InstantiateUI(prefabsHolder);
            }
        }

        public void LoadTemplate(PrefabBrushTemplate prefabBrushTemplate)
        {
            _lastLoadedTemplate = prefabBrushTemplate;

            PrefabBrushTemplate.SetLastTemplate(_lastLoadedTemplate.name);
            PB_PrecisionModeManager.DisposePrecisionMode();

            //Set values
            brushRadiusSlider.SetValueWithoutNotify(_lastLoadedTemplate.brushSize);
            sliderBrushStrength.SetValueWithoutNotify(_lastLoadedTemplate.brushStrength);
            clippingToleranceSlider.SetValueWithoutNotify(_lastLoadedTemplate.clippingStreght);
            pivotMode.SetValueWithoutNotify(_lastLoadedTemplate.pivotModeValue);
            toggleAlignWithGround.SetValueWithoutNotify(_lastLoadedTemplate.alignWithGround);
            layerMaskField.SetValueWithoutNotify(_lastLoadedTemplate.layerMask);
            tagMaskField.SetValueWithoutNotify(_lastLoadedTemplate.tagMask);
            showBoundsToggle.SetValueWithoutNotify(_lastLoadedTemplate.showClippingBounds);
            eraserRadiusSlider.SetValueWithoutNotify(_lastLoadedTemplate.eraserSize);
            gridSnapValueField.SetValueWithoutNotify(_lastLoadedTemplate.gridSnapValue);

            parentField.SetValueWithoutNotify(_lastLoadedTemplate.GetParent());
            parentNameInput.SetValueWithoutNotify(_lastLoadedTemplate.parentName);

            //precision mode
            precisionModeChangePrefabToggle.SetValueWithoutNotify(_lastLoadedTemplate.precisionModeChangePrefabAfterPlacing);
            precisionModeAddMeshToBatch.SetValueWithoutNotify(_lastLoadedTemplate.precisionModeAddMesh);
            paintModeDropdown.SetValueWithoutNotify(_lastLoadedTemplate.paintMode);
            precisionModeRotationAngle.SetValueWithoutNotify(_lastLoadedTemplate.precisionModeRotationAngle);
            placeOnYZeroToggle.SetValueWithoutNotify(_lastLoadedTemplate.placeOnYZero);

            //Physics
            PB_PhysicsSimulator.simulationModeEnum.SetValueWithoutNotify(_lastLoadedTemplate.physicsSimulationMode);
            PB_PhysicsSimulator.affectObjectsEnum.SetValueWithoutNotify(_lastLoadedTemplate.physicsAffectMode);
            PB_PhysicsSimulator.physicsStepSlider.SetValueWithoutNotify(_lastLoadedTemplate.physicsStepValue);
            PB_PhysicsSimulator.impulseForceSlider.SetValueWithoutNotify(_lastLoadedTemplate.physicsImpulseValue);
            PB_PhysicsSimulator.impulseFieldVec3.SetValues(_lastLoadedTemplate.useImpulse, (int)_lastLoadedTemplate.impulseMode, _lastLoadedTemplate.minPhysicsImpulse, _lastLoadedTemplate.maxPhysicsImpulse, _lastLoadedTemplate.fixedPhysicsImpulse, false);
            PB_PhysicsSimulator.UpdateUIPhysics();

            //Scale
            scaleField.fixedField.SetValueWithoutNotify(_lastLoadedTemplate.fixedScale);
            scaleField.minField.SetValueWithoutNotify(_lastLoadedTemplate.minScale);
            scaleField.maxField.SetValueWithoutNotify(_lastLoadedTemplate.maxScale);
            scaleField.enumField.SetValueWithoutNotify(_lastLoadedTemplate.scaleMode);
            scaleField.maxFieldUniform.SetValueWithoutNotify(_lastLoadedTemplate.scaleMaxUniform * Vector3.one);
            scaleField.minFieldUniform.SetValueWithoutNotify(_lastLoadedTemplate.scaleMinUniform * Vector3.one);

            scaleField.SetConstrainedProportions(_lastLoadedTemplate.constrainedScaleFixed, _lastLoadedTemplate.constrainedScaleMin, _lastLoadedTemplate.constrainedScaleMax);

            //Rotation
            rotationField.fixedField.SetValueWithoutNotify(_lastLoadedTemplate.fixedRotation);
            rotationField.minField.SetValueWithoutNotify(_lastLoadedTemplate.minRotation);
            rotationField.maxField.SetValueWithoutNotify(_lastLoadedTemplate.maxRotation);
            rotationField.enumField.SetValueWithoutNotify(_lastLoadedTemplate.rotationMode);

            //Texture Mask
            PB_TextureMaskHandler.useTextureMaskToggle.SetValueWithoutNotify(_lastLoadedTemplate.useTextureMask);
            PB_TextureMaskHandler.Dispose();
            PB_TextureMaskHandler.AddTextures(_lastLoadedTemplate.allowedTextures, true);

            //Offset
            offsetField.fixedField.SetValueWithoutNotify(_lastLoadedTemplate.fixedOffset);
            offsetField.minField.SetValueWithoutNotify(_lastLoadedTemplate.minOffset);
            offsetField.maxField.SetValueWithoutNotify(_lastLoadedTemplate.maxOffset);
            offsetField.enumField.SetValueWithoutNotify(_lastLoadedTemplate.offsetMode);

            parentModeDropdown.SetValueWithoutNotify(_lastLoadedTemplate.parentMode);

            raycastModeDropdown.SetValueWithoutNotify(_lastLoadedTemplate.raycastMode);

            useAngleLimitsToggle.SetValueWithoutNotify(_lastLoadedTemplate.useAngleLimits);
            makeObjectsStaticToggle.SetValueWithoutNotify(_lastLoadedTemplate.makeStatic);

            angleLimitsField.value = _lastLoadedTemplate.angleLimits;
            gridSnapToggle.value = _lastLoadedTemplate.snapToGrid;
            gridOffsetField.value = _lastLoadedTemplate.gridOffset;

            //Prefabs
            prefabsHolder.Clear();
            currentPrefabs.Clear();
            foreach (PbPrefabUI prefab in _lastLoadedTemplate.prefabs) AddPrefab(prefab);

            UpdateUITool();

            if (GetPaintMode() == PaintMode.Precision && currentPrefabs.Count > 0)
            {
                currentPrefabs[0].Select();
            }

            _templatesDropdown.SetValueWithoutNotify(_lastLoadedTemplate.name);
        }

        public bool HasAnyPrefabsWithPhysics()
        {
            foreach (PbPrefabUI prefabUI in currentPrefabs)
            {
                if (prefabUI == null) continue;
                if (!prefabUI.AllowsPhysicsPlacement()) continue;
                return true;
            }

            return false;
        }
    }

    public static class PrefabBrushTool
    {
        public static bool isUsingTool;
        private static Color BrushColor1 => PB_AdvancedSettings.brushBorderColor.value;
        private static Color BrushColor2 => PB_AdvancedSettings.brushBaseColor.value;
        private static Color BrushColorError => PB_AdvancedSettings.invalidLocationColor.value;

        private static readonly Color GridColor = new(1f, 0.06f, 0f, 0.1f);

        public static bool RaycastPhysicsGUI(float maxDistance, int mask, out RaycastHit hit, out Ray r)
        {
            //Raycast with GUI
            r = GetMouseGuiRay();

            return Physics.Raycast(r.origin, r.direction, out hit, maxDistance, mask,
                QueryTriggerInteraction.UseGlobal);
        }

        public static Ray GetMouseGuiRay()
        {
            return HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
        }

        public static bool RaycastToWorldPoint(Camera c, Vector3 refPoint, int layerMask, out RaycastHit hit, PrefabBrush.RaycastMode mode)
        {
            Transform transform = c.transform;
            Vector3 position = transform.position;
            Ray ray = new()
            {
                origin = position,
                direction = (refPoint - position).normalized
            };

            float maxDistance = Vector3.Distance(position, refPoint) + 0.2f;

            if (mode == PrefabBrush.RaycastMode.Mesh)
            {
                bool r = PB_MeshRaycaster.Raycast(ray, PB_MeshBatcher.Batch(), Matrix4x4.identity, out MeshRaycastResult result);
                hit = result.ToHitInfo();
                return r;
            }

            return Physics.Raycast(ray.origin, ray.direction, out hit, maxDistance, layerMask,
                QueryTriggerInteraction.UseGlobal);
        }

        public static void DrawGuideLines(RaycastHit hitInfo)
        {
            if (PrefabBrush.instance._showBrushGuideLines.value)
            {
                Handles.zTest = CompareFunction.LessEqual;
                float distance = PrefabBrush.instance.brushGuideLinesDistance.value;
                float size2 = 3f;
                Vector3 p = hitInfo.point + Vector3.up * 0.02f;

                Handles.color = Color.red;
                Handles.DrawDottedLine(p, p + Vector3.right * distance, size2);
                Handles.DrawDottedLine(p, p - Vector3.right * distance, size2);

                Handles.color = Color.green;
                Handles.DrawDottedLine(p, p + Vector3.up * distance, size2);
                Handles.DrawDottedLine(p, p - Vector3.up * distance, size2);

                Handles.color = Color.blue;
                Handles.DrawDottedLine(p, p + Vector3.forward * distance, size2);
                Handles.DrawDottedLine(p, p - Vector3.forward * distance, size2);
            }
        }

        public static void DrawCircle(RaycastHit hitInfo, float radius, bool validAngle)
        {
            //Draw a solid circle
            Handles.color = validAngle ? BrushColor2 : BrushColorError;
            Handles.DrawSolidDisc(hitInfo.point, hitInfo.normal, radius);

            Handles.color = BrushColor1;
            Handles.DrawWireDisc(hitInfo.point, hitInfo.normal, radius, 5f);
            Handles.DrawLine(hitInfo.point, hitInfo.point + hitInfo.normal * radius / 2f, 2f);

            DrawGuideLines(hitInfo);
        }

        public static bool IsMouseOverAnySceneView()
        {
            foreach (object sceneView in SceneView.sceneViews)
            {
                if (sceneView.GetType().ToString() == "UnityEditor.SceneView")
                {
                    SceneView sv = (SceneView)sceneView;
                    if (sv.rootVisualElement.worldBound.Contains(Event.current.mousePosition))
                        return true;
                }
            }

            return false;
        }

        public static Vector3 GetRandomPointInsideDisc(RaycastHit hit, float radius)
        {
            float randomAngleRad = Random.value * Mathf.PI * 2;
            float sin = Mathf.Sin(randomAngleRad);
            float cos = Mathf.Cos(randomAngleRad);
            float radiusRandom = Random.Range(0f, radius);
            Vector3 randomPoint = new Vector3(sin, 0, cos) * radiusRandom;

            //Rotate the point to match the normal
            Quaternion rotation = Quaternion.FromToRotation(Vector3.up, hit.normal);

            return hit.point + rotation * randomPoint;
        }

        public static void DrawPoint(Vector3 randomPointInsideDisc)
        {
            Handles.color = Color.yellow;
            Handles.DrawSolidDisc(randomPointInsideDisc, Vector3.up, 0.1f);
        }

        public static bool HasAnyPrefab(Object[] objects)
        {
            if (objects.Length == 0) return false;

            foreach (Object obj in objects)
            {
                if (obj is not GameObject go) continue;
                if (!IsPrefab(go)) continue;
                return true;
            }

            return false;
        }

        public static List<GameObject> GetPrefabs(Object[] objects)
        {
            if (objects.Length == 0)
            {
#if HARPIA_DEBUG
                Debug.LogError($"{PrefabBrush.DebugLogStart} objects array is null");
#endif
                return null;
            }

            List<GameObject> foundPrefabs = new();

            foreach (Object obj in objects)
            {
                //Check if object is GameObject
                if (obj is not GameObject go) continue;
                
                //Check if its prefab
                if (!IsPrefab(go)) continue;

               // GameObject source = PrefabUtility.GetCorrespondingObjectFromSource(go);
               GameObject source = GetPrefabAsset(obj as GameObject);
               
                //source
                if (source == null) source = go;
                if (foundPrefabs.Contains(source)) continue;

                
                foundPrefabs.Add(source);
            }

            if (foundPrefabs.Count == 0)
            {
                Debug.LogError($"{PrefabBrush.DebugLogStart} No prefabs selected");
            }

            return foundPrefabs;
        }

        public static bool IsPrefab(GameObject o)
        {
            if (PrefabUtility.GetPrefabAssetType(o) == PrefabAssetType.NotAPrefab) return false;
            //if (!PrefabUtility.IsAnyPrefabInstanceRoot(o)) return false;
            return true;
        }

        public static GameObject GetPrefabAsset(GameObject o)
        {
            if (!IsPrefab(o)) return o;

            bool isVariant = PrefabUtility.IsPartOfVariantPrefab(o);
            if (isVariant)
            {
                GameObject nearestPrefabInstanceRoot = PrefabUtility.GetNearestPrefabInstanceRoot(o);
                string variantPath = PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(nearestPrefabInstanceRoot);
                
#if HARPIA_DEBUG
                Debug.Log($"[{nameof(PrefabBrushTool)}] Getting Prefab Variant {nearestPrefabInstanceRoot.gameObject.name} at path {variantPath}", o);
#endif
                return PrefabUtility.GetCorrespondingObjectFromSourceAtPath(nearestPrefabInstanceRoot, variantPath);
            }
            
            GameObject obj = PrefabUtility.GetCorrespondingObjectFromOriginalSource(o);
            if (obj != null) return obj;

            GameObject asset = PrefabUtility.GetCorrespondingObjectFromSource(o);
            if (asset != null) return asset;

            return null;
        }

        public static RaycastHit GetCenterRay()
        {
            return GetCenterRay(PrefabBrush.instance.sceneCamera, PrefabBrush.instance.GetRaycastMode(), ~1);
        }

        public static RaycastHit GetCenterRay(Camera cam, PrefabBrush.RaycastMode mode, int mask)
        {
            Transform camTransform = cam.transform;
            Vector3 camPosition = camTransform.position;
            Vector3 camForward = camTransform.forward;

            Ray ray = new()
            {
                origin = camPosition,
                direction = camForward
            };

            RaycastHit hitInfo = new()
            {
                normal = Vector3.up,
                point = camPosition + camForward * 4f,
            };

            if (mode == PrefabBrush.RaycastMode.Mesh)
            {
                bool hit = PB_MeshRaycaster.Raycast(ray, PB_MeshBatcher.Batch(), Matrix4x4.identity, out MeshRaycastResult result);
                if (hit) hitInfo = result.ToHitInfo();
            }

            else if (Physics.Raycast(ray.origin, ray.direction, out RaycastHit hitInfo2, cam.farClipPlane, mask,
                         QueryTriggerInteraction.UseGlobal))
            {
                hitInfo = hitInfo2;
            }

            return hitInfo;
        }

        public static void DrawDottedLines(RaycastHit hitInfo, Vector3 positionOffsetValue)
        {
            float size = 3;

            Handles.zTest = CompareFunction.LessEqual;

            Handles.DrawDottedLine(hitInfo.point, hitInfo.point + positionOffsetValue, size);

            Handles.color = Color.red;
            Handles.DrawDottedLine(hitInfo.point, hitInfo.point + new Vector3(positionOffsetValue.x, 0, 0), size);

            Handles.color = Color.green;
            Handles.DrawDottedLine(hitInfo.point, hitInfo.point + new Vector3(0, positionOffsetValue.y, 0), size);

            Handles.color = Color.blue;
            Handles.DrawDottedLine(hitInfo.point, hitInfo.point + new Vector3(0, 0, positionOffsetValue.z), size);
        }

        public static float RoundTo(float value, float multipleOf)
        {
            return Mathf.Round(value / multipleOf) * multipleOf;
        }

        public static Vector3 RoundTo(Vector3 value, float multipleOf)
        {
            return new Vector3(RoundTo(value.x, multipleOf), RoundTo(value.y, multipleOf), RoundTo(value.z, multipleOf));
        }

        private static Vector3 gridPos;
        private static Bounds nextDrawBounds;

        public static void DrawGrid(RaycastHit hitInfo, bool forceDraw)
        {
            if (!forceDraw && !PrefabBrush.instance.CanUseGrid()) return;

            float snapValue = PrefabBrush.instance.gridSnapValueField.value;
            float radius = snapValue * 10;
            float arcRadius = snapValue / 10;
            Handles.color = GridColor;

            gridPos = new Vector3(RoundTo(hitInfo.point.x, snapValue), hitInfo.point.y, PrefabBrushTool.RoundTo(hitInfo.point.z, snapValue)) + PrefabBrush.instance.GetGridOffset();

            Vector3 start = new(gridPos.x - radius, gridPos.y, gridPos.z - radius);
            Vector3 end = new(gridPos.x + radius, gridPos.y, gridPos.z + radius);
            Vector3 center = new(gridPos.x, gridPos.y, gridPos.z);

            Vector3 offset = Vector3.up * 0.01f;

            Handles.zTest = CompareFunction.LessEqual;
            for (float x = start.x; x <= end.x; x += snapValue)
            {
                for (float z = start.z; z <= end.z; z += snapValue)
                {
                    Vector3 pos = new(x, gridPos.y, z);

                    Color c = Handles.color;
                    float distanceFromCenter = Vector3.Distance(pos, center) / radius;
                    c.a = (1 - distanceFromCenter) * 0.3f;

                    if (c.a < 0.1f) continue;

                    Handles.color = PB_AdvancedSettings.gridColor.value;
                    Handles.DrawSolidDisc(pos + offset, Vector3.up, arcRadius);
                }
            }
        }

        public static void DrawBounds(Bounds bounds, bool draw)
        {
            if (draw)
            {
                if (PrefabBrush.instance.showBoundsToggle.value)
                {
                    if (nextDrawBounds.size == Vector3.zero) return;
                    Handles.color = PrefabBrush.instance.boundsColorField.value;
                    Handles.DrawWireCube(nextDrawBounds.center, nextDrawBounds.size);
                }

                return;
            }

            nextDrawBounds = bounds;
        }

        public static void DisposeBounds()
        {
            nextDrawBounds = new Bounds();
        }

        public static string GetStringFormat(float newValue)
        {
            string format = newValue < 1 && newValue != 0 ? "f2" : "f1";
            return newValue.ToString(format);
        }

        public static void EnableAutoRefresh()
        {
            bool changed = false;
            foreach (object sceneView in SceneView.sceneViews)
            {
                SceneView sv = (SceneView)sceneView;
                if (!sv.hasFocus) continue;
                if (sv.sceneViewState.alwaysRefresh && sv.sceneViewState.alwaysRefreshEnabled) continue;

                if (!sv.sceneViewState.fxEnabled) sv.sceneViewState.fxEnabled = true;
                sv.sceneViewState.alwaysRefresh = true;

                changed = true;
            }

            if (changed)
            {
                Debug.Log($"{PrefabBrush.DebugLogStart} Changed Scene view alwaysRefresh to true");
            }
        }

        // ReSharper disable Unity.PerformanceAnalysis
        public static List<Rigidbody> GetSelectedRigidbodies()
        {
            List<Rigidbody> rigidbodies = new();
            foreach (Transform transform in Selection.transforms)
            {
                if (transform.TryGetComponent(out Rigidbody rb))
                {
                    if (!PB_PhysicsSimulator.HasPhysicalCollider(rb.gameObject)) continue;
                    rigidbodies.Add(rb);
                }
            }

            return rigidbodies;
        }

        public static Vector3 MultiplyVec3(Vector3 a, Vector3 b)
        {
            float x = a.x * b.x;
            float y = a.y * b.y;
            float z = a.z * b.z;

            return new Vector3(x, y, z);
        }
    }

    public static class PB_EraserManager
    {
        private static RaycastHit Hit => PrefabBrush.instance.lastHitInfo;

        public static void Eraser(RaycastHit point, float radius)
        {
            PrefabBrushObject[] list = PrefabBrushObject.GetBrushObjects();

            for (int index = 0; index < list.Length; index++)
            {
                PrefabBrushObject brushObject = list[index];
                if (brushObject == null) continue;
                if (brushObject.erasable == false) continue;

                //Check the distance
                PrefabBrushObject.RadiusBounds bounds = brushObject.GetBoundsSphere();
                if (!bounds.Intersects(radius, point.point)) continue;

                GameObject objectToEraser = brushObject.gameObject;
                PB_UndoManager.DestroyAndRegister(objectToEraser);
                index--;
            }
        }

        public static void DrawCircleEraser(RaycastHit hitInfo, float radius)
        {
            Handles.color = PB_AdvancedSettings.eraserColor.value;
            Handles.DrawSolidDisc(hitInfo.point, hitInfo.normal, radius);

            Handles.color = new Color(0f, 0f, 0f, 0.39f);
            Handles.DrawWireDisc(hitInfo.point, hitInfo.normal, radius, 5f);
            Handles.DrawLine(hitInfo.point, hitInfo.point + hitInfo.normal * radius / 2f, 2f);

            Handles.DrawWireArc(hitInfo.point, hitInfo.normal, Vector3.up, 360f, radius);

            if (PrefabBrush.instance.showBoundsToggle.value)
            {
                float d = PrefabBrush.instance.eraserRadiusSlider.value * 5;
                float eraserRadius = PrefabBrush.instance.eraserRadiusSlider.value;
                PrefabBrushObject.DrawBoundsEraser(hitInfo.point, PrefabBrush.instance.boundsColorField.value, d, eraserRadius);
            }
        }

        public static void AddShortcuts()
        {
            PB_ShortcutManager.ShowShortcuts(PB_ModularShortcuts.increaseRadius);
            PB_ShortcutManager.ShowShortcuts(PB_ModularShortcuts.decreaseRadius);
            PB_ShortcutManager.ShowShortcuts(PB_ModularShortcuts.changeMode);
        }

        public static void IncreaseRadius(float val)
        {
            float newValue = PrefabBrush.instance.eraserRadiusSlider.value;
            newValue += newValue * val;
            Slider s = PrefabBrush.instance.brushRadiusSlider;
            newValue = Mathf.Clamp(newValue, s.lowValue, s.highValue);
            PrefabBrush.instance.eraserRadiusSlider.value = newValue;
            PB_HandlesExtension.WriteTempText("Eraser Radius: " + PrefabBrushTool.GetStringFormat(newValue), Hit, Color.white);
        }

        public static bool IsOnEraseMode()
        {
            return PrefabBrush.instance.GetPaintMode() == PrefabBrush.PaintMode.Eraser;
        }
    }

    public static class PB_PrecisionModeManager
    {
        public static int prefabIndex;

        private static GameObject tempGo;
        private static GameObject prefabToPaint;
        private static GameObject lastPlacedObject;
        private static PbPrefabUI selectedPrefabUI;

        private static Vector3 currentScale = Vector3.one;
        private static float currentScaleProps = 1;
        private static Vector3 currentOffset = Vector3.zero;
        private static Vector3 currentRotation = Vector3.zero;

        private static RaycastHit hit => PrefabBrush.instance.lastHitInfo;
        private static float deltaTime => PrefabBrush.deltaTime;
        private static bool alignWithNormal => PrefabBrush.instance.toggleAlignWithGround.value;

        private static bool randomPrefabAfterPlace => PrefabBrush.instance.precisionModeChangePrefabToggle.value;

        public static GameObject PrefabToPaint
        {
            get
            {
                if (prefabToPaint != null) return prefabToPaint;
                SetPrefabToPaint(PrefabBrush.instance.GetRandomPrefab(true));
                return prefabToPaint;
            }
        }

        public static void DrawTemporaryPrefab(RaycastHit hitPoint, Vector3 size = default, bool lerp = true)
        {
            if (tempGo == null) return;

            PrefabAttributes props = selectedPrefabUI.customProps;
            Vector3 offset = props.useCustomOffset ? props.GetOffset() : currentOffset;

            Vector3 scale = props.useCustomScale ? props.GetScale() * currentScaleProps : PrefabBrushTool.MultiplyVec3(currentScale, prefabToPaint.transform.lossyScale);
            Vector3 rotation = props.useCustomRotationMode ? props.GetRotation() : currentRotation;

            Quaternion rot = alignWithNormal ? PrefabBrush.GetAlignedRotation(hitPoint) : Quaternion.Euler(rotation);
            rot *= Quaternion.AngleAxis(rotation.y * 2, Vector3.up);
            rot *= Quaternion.AngleAxis(rotation.x * 2, Vector3.right);
            rot *= Quaternion.AngleAxis(rotation.z * 2, Vector3.forward);

            if (props.useCustomOffset)
            {
                rot *= Quaternion.Euler(-props.fixedRotation);
            }

            rot = Quaternion.Lerp(tempGo.transform.rotation, rot, deltaTime * 20f);

            Vector3 pos;

            Assert.IsNotNull(selectedPrefabUI);
            Assert.IsNotNull(selectedPrefabUI.customProps);

            // ReSharper disable once ConvertIfStatementToConditionalTernaryExpression
            if (lerp)
                pos = Vector3.Lerp(tempGo.transform.position, hit.point + offset, deltaTime * 20f);
            else
                pos = hit.point + offset;

            tempGo.transform.position = pos;

            Transform meshObj = tempGo.transform.GetChild(0);

            PrefabBrush.PivotMode pivotMode = props.useCustomPivotMode ? props.pivotMode : PrefabBrush.instance.GetPivotMode();
            meshObj.localPosition = PrefabBrush.instance.GetLocalPosition(meshObj.gameObject, pivotMode);
            meshObj.transform.localScale = Vector3.Lerp(meshObj.transform.localScale, scale, deltaTime * 20f);
            tempGo.transform.rotation = rot;

            PrefabBrushTool.DrawDottedLines(hitPoint, offset);
            PrefabBrushTool.DrawGuideLines(hitPoint);

            tempGo.gameObject.SetActive(true);
        }

        public static void SetPrefabToPaint(PbPrefabUI ui)
        {
            if (ui == null)
            {
#if HARPIA_DEBUG
                Debug.LogError($"(SetPrefabToPaint) ui param is null");
#endif
                return;
            }

            Quaternion newObjectRotation = Quaternion.Euler(currentRotation);

            //destroy temp mesh
            if (tempGo != null)
            {
                newObjectRotation = tempGo.transform.rotation;
                Object.DestroyImmediate(tempGo);
            }

            if (lastPlacedObject != null) newObjectRotation = lastPlacedObject.transform.rotation;

            selectedPrefabUI = ui;
            selectedPrefabUI.Select();
            GameObject meshObj = Object.Instantiate(selectedPrefabUI.prefabToPaint);
            meshObj.hideFlags = HideFlags.HideAndDontSave;

            if (tempGo != null) Object.DestroyImmediate(tempGo);

            tempGo = new GameObject
            {
                hideFlags = HideFlags.HideAndDontSave
            };

            meshObj.transform.SetParent(tempGo.transform);
            meshObj.transform.localPosition = Vector3.zero;
            meshObj.transform.localRotation = Quaternion.identity;
            meshObj.transform.localScale = currentScale;

            tempGo.transform.position = hit.point + currentOffset;
            tempGo.transform.rotation = newObjectRotation;

            foreach (Collider col in tempGo.GetComponentsInChildren<Collider>()) col.enabled = false;
            foreach (Rigidbody rb in tempGo.GetComponentsInChildren<Rigidbody>()) rb.isKinematic = true;

            if (tempGo != null)
            {
                bool boolVal = false;
                if (EditorWindow.mouseOverWindow != null) boolVal = EditorWindow.mouseOverWindow.GetType() == typeof(SceneView);
                tempGo.SetActive(boolVal);
            }

            selectedPrefabUI.customProps.Dispose();
            prefabToPaint = selectedPrefabUI.prefabToPaint;

            UpdateTransformValues();
        }

        public static void UpdateTransformValues()
        {
            UpdateCurrentScale();
            UpdateCurrentRotation();
            UpdateCurrentOffset();

            if (tempGo != null)
            {
                tempGo.transform.localScale = currentScale;
            }
        }

        public static void SetPrefabToPaint()
        {
#if HARPIA_DEBUG
            Debug.Log($"[Precision Mode] Setting prefab to paint...");
#endif
            lastPlacedObject = null;

            selectedPrefabUI = PbPrefabUI.GetSelectedPrefab();
            if (selectedPrefabUI != null) selectedPrefabUI.Select();

            //Deselect all UI
            foreach (PbPrefabUI prefabs in PrefabBrush.instance.currentPrefabs)
            {
                if (prefabs == selectedPrefabUI) continue;
                prefabs.Deselect();
            }

            SetPrefabToPaint(selectedPrefabUI);
        }

        public static bool PaintPrefab(Transform parent)
        {
#if HARPIA_DEBUG
            Debug.Log($"[Precision Mode] Painting prefab... prefabToPaint is null {prefabToPaint == null}");
#endif

            //Find Prefab To Paint
            if (prefabToPaint == null)
            {
                selectedPrefabUI = PbPrefabUI.GetSelectedPrefab();
                if (selectedPrefabUI != null) prefabToPaint = selectedPrefabUI.prefabToPaint;
            }

            if (prefabToPaint == null)
            {
                Debug.LogError($"{PrefabBrush.DebugLogStart} No prefab to paint, please select one");
                return false;
            }

            //Instantiate
            lastPlacedObject = PrefabUtility.InstantiatePrefab(prefabToPaint) as GameObject;
            if (lastPlacedObject == null) lastPlacedObject = Object.Instantiate(prefabToPaint);
            if (lastPlacedObject == null)
            {
                Debug.LogError($"{PrefabBrush.DebugLogStart} [Code PMM01] Could not instantiate a new Object, Please contact support. harpiagamesstudio@gmail.com");
                return false;
            }

            //Get Reference Object
            if (tempGo == null) SetPrefabToPaint();
            if (tempGo == null)
            {
                Debug.LogError($"{PrefabBrush.DebugLogStart} [Code PMM02] Could Not Paint Prefab - Please contact support. harpiagamesstudio@gmail.com");
                return false;
            }

            //Transform
            Transform meshObject = tempGo.transform.GetChild(0);
            if (meshObject == null) meshObject = tempGo.transform;
            if (meshObject == null)
            {
                Debug.LogError($"{PrefabBrush.DebugLogStart} [Code PMM03] Could Not Paint Prefab - Please contact support. harpiagamesstudio@gmail.com");
                return false;
            }

            lastPlacedObject.transform.position = GetFinalPosition();
            lastPlacedObject.transform.rotation = meshObject.rotation;
            lastPlacedObject.transform.localScale = meshObject.lossyScale;

            //Parenting
            if (parent != null) lastPlacedObject.transform.SetParent(parent);

            //Register
            PrefabBrush.RegisterObject(lastPlacedObject);

            //Add mesh to batcher
            PB_MeshBatcher.AddMeshToBatch(lastPlacedObject);

            //Undo
            RegisterUndo();

            //Change prefab
            if (randomPrefabAfterPlace) SetPrefabToPaint(PrefabBrush.instance.GetRandomPrefab(true));

            Object.DestroyImmediate(tempGo);

#if HARPIA_DEBUG
            Debug.Log($"[Precision Mode] Prefab painted...", lastPlacedObject);
#endif

            return true;

            Vector3 GetFinalPosition()
            {
                if (!PrefabBrush.instance.gridSnapToggle.value) return meshObject.transform.position;
                return PrefabBrushTool.RoundTo(meshObject.transform.position, 0.0001f);
            }
        }

        public static Vector3 AddToRotation(float value)
        {
            PrefabAttributes customProps = selectedPrefabUI.customProps;
            bool useCustomRotation = customProps.useCustomRotationMode;

#if HARPIA_DEBUG
            Debug.Log($"[{nameof(PB_PrecisionModeManager)}] Add to rotation {currentRotation.y} | useCustomRotation {useCustomRotation}  | InputValue {value}");
#endif
            if (float.IsNaN(value) || value == 0) value = 15;
            if (float.IsNaN(currentRotation.y)) currentRotation.y = 0;

            if (useCustomRotation)
            {
                customProps.AddToRotation(value);
                return customProps.GetRotation();
            }

            currentRotation.y = PrefabBrushTool.RoundTo((currentRotation.y + value) % 360f, value);
            if (currentRotation.y < 0) currentRotation.y += 360;
            if (tempGo != null) DrawTemporaryPrefab(hit, default, false);

            //UI
            PrefabBrush.instance.rotationField.TrySetValue(PrefabBrush.Vector3Mode.Fixed, currentRotation);
            return currentRotation;
        }

        public static Vector3 IncreaseSize(float p0)
        {
            PrefabAttributes customProps = selectedPrefabUI.customProps;
            if (customProps.useCustomScale)
            {
                currentScaleProps += p0;
                currentScaleProps = Mathf.Clamp(currentScaleProps, 0.1f, 1000f);
                return tempGo.transform.localScale * currentScaleProps;
            }

            currentScale += currentScale * p0;

            const float min = 0.1f;
            const float max = 1000f;

            //clamp
            currentScale = new Vector3(
                Mathf.Clamp(currentScale.x, min, max),
                Mathf.Clamp(currentScale.y, min, max),
                Mathf.Clamp(currentScale.z, min, max)
            );

            PrefabBrush.instance.scaleField.TrySetValue(PrefabBrush.Vector3Mode.Fixed, currentScale);

            return currentScale;
        }

        public static void RotateCurrentObject(Vector3 worldRot)
        {
            currentRotation += worldRot;
            currentRotation = new Vector3(FixRotationValue(currentRotation.x), FixRotationValue(currentRotation.y), FixRotationValue(currentRotation.z));

            PrefabBrush brush = PrefabBrush.instance;

            PB_HandlesExtension.WriteVector3(currentRotation, brush.lastHitInfo, "Rotation", "°");
            return;

            float FixRotationValue(float v)
            {
                v %= 360f;
                v = PrefabBrushTool.RoundTo(v, 0.001f);
                return v;
            }
        }

        public static void AddScale(float p)
        {
#if HARPIA_DEBUG
            Debug.Log($"Add scale");
#endif

            if (tempGo == null) return;
            if (tempGo.transform.childCount == 0) return;

            Vector3 newScale = currentScale + currentScale * p;

            if (newScale.x <= 0.0001f) return;
            if (newScale.y <= 0.0001f) return;
            if (newScale.z <= 0.0001f) return;

            PrefabBrush brush = PrefabBrush.instance;
            currentScale = newScale;
            PB_HandlesExtension.WriteVector3(tempGo.transform.GetChild(0).lossyScale, brush.lastHitInfo, "Scale", "");
        }

        private static void RegisterUndo()
        {
            PB_UndoManager.AddToUndo(lastPlacedObject);
            PB_UndoManager.RegisterUndo();
        }

        public static Vector3 GetNewObjectPosition()
        {
            GameObject go = lastPlacedObject == null ? tempGo : lastPlacedObject;
            return go.transform.position;
        }

        public static float GetNewObjectRadius()
        {
            GameObject go = lastPlacedObject == null ? tempGo : lastPlacedObject;

            if (go == null)
            {
                SetPrefabToPaint();
                go = lastPlacedObject == null ? tempGo : lastPlacedObject;
            }

            if (go == null)
            {
                Debug.LogError($"null error");
                return 0;
            }

            Bounds bounds = PB_BoundsManager.GetBounds(go);
            return bounds.extents.magnitude / 2;
        }

        public static void DisposePrecisionMode()
        {
            if (tempGo != null)
                Object.DestroyImmediate(tempGo);

            lastPlacedObject = null;
            prefabToPaint = null;
            PbPrefabUI.Dispose();
            currentScaleProps = 1;
        }

        public static void HideObject()
        {
            if (tempGo != null) tempGo.gameObject.SetActive(false);
        }

        public static void ShowObject()
        {
            if (tempGo != null)
            {
                tempGo.gameObject.SetActive(true);
                return;
            }

            SetPrefabToPaint();
        }

        public static void SetRotation(Vector3 newRotation)
        {
            currentRotation = newRotation;
        }

        public static void AddShortcuts()
        {
            if (PrefabBrush.instance.GetPaintMode() != PrefabBrush.PaintMode.Precision) return;

            PB_ShortcutManager.ShowShortcuts(PB_ModularShortcuts.increaseRadius);
            PB_ShortcutManager.ShowShortcuts(PB_ModularShortcuts.decreaseRadius);

            PB_ShortcutManager.ShowShortcuts(PB_ModularShortcuts.rotateRight, true);
            PB_ShortcutManager.ShowShortcuts(PB_ModularShortcuts.rotateLeft);
            PB_ShortcutManager.ShowShortcuts(PB_ModularShortcuts.randomRotation);

            PB_ShortcutManager.ShowShortcuts(PB_ModularShortcuts.nextPrefab, true);
            PB_ShortcutManager.ShowShortcuts(PB_ModularShortcuts.previousPrefab);
            PB_ShortcutManager.ShowShortcuts(PB_ModularShortcuts.normalizeSize);

            PB_ShortcutManager.ShowShortcuts(PB_ModularShortcuts.rotationXShortcut, true, true);
            PB_ShortcutManager.ShowShortcuts(PB_ModularShortcuts.rotationYShortcut, false, true);
            PB_ShortcutManager.ShowShortcuts(PB_ModularShortcuts.rotationZShortcut, false, true);

            PB_ShortcutManager.ShowShortcuts(PB_ModularShortcuts.changeScaleShortcut, true, true);
            PB_ShortcutManager.ShowShortcuts(PB_ModularShortcuts.yDisplacementShortcut, false, true);

            PB_ShortcutManager.ShowShortcuts(PB_ModularShortcuts.changeMode, true);
            PB_ShortcutManager.ShowShortcuts(PB_ModularShortcuts.exitTool);
        }

        public static Vector3 NormalizeSize()
        {
            currentScale = Vector3.one;
            if (tempGo != null) tempGo.transform.GetChild(0).localScale = currentScale;
            return currentScale;
        }

        public static void SetCurrentObjectActive(bool b)
        {
            if (tempGo == null) return;
            tempGo.SetActive(b);
        }

        public static void SetCurrentObjectPos(Vector3 point)
        {
            if (tempGo == null) return;
            tempGo.transform.position = point;
        }

        public static void UpdateCurrentOffset() => currentOffset = PrefabBrush.instance.offsetField.GetValue();
        public static void UpdateCurrentScale() => currentScale = PrefabBrush.instance.scaleField.GetValue();
        public static void UpdateCurrentRotation() => currentRotation = PrefabBrush.instance.rotationField.GetValue();

        public static void AdjustDistanceY(float scrollValue)
        {
            scrollValue *= PB_AdvancedSettings.scrollRotationSpeed.value * .05f;
            currentOffset += new Vector3(0, scrollValue, 0);
            PB_HandlesExtension.WriteVector3(currentOffset, PrefabBrush.instance.lastHitInfo, "Offset", "");
        }
    }

    public static class PB_MultipleModeManager
    {
        private static RaycastHit Hit => PrefabBrush.instance.lastHitInfo;
        private static float Radius => PrefabBrush.instance.brushRadiusSlider.value;
        private static bool IsHit => PrefabBrush.instance.isRaycastHit;

        private static readonly List<PrefabBrushObject> _addedGameObjects = new List<PrefabBrushObject>();

        public static void PaintPrefabMultiple(RaycastHit raycastHit, PbPrefabUI prefabToSpawn)
        {
            if (prefabToSpawn == null)
            {
                Debug.LogError($"{PrefabBrush.DebugLogStart} No Prefab to spawn. Please Check your Prefabs list");
                PB_HandlesExtension.WriteTempText("Select at least 1 object on your prefab list", 2f, 5);
                return;
            }

            Vector3 hitPoint = raycastHit.point;

            //Lets check if any bounds intersects with the hit point

            if (PB_BoundsManager.BoundsIntersects(hitPoint))
                return;

            if (_addedGameObjects.Any(brushObject => brushObject != null && brushObject.meshClippingChecks && brushObject.BoundsIntersect(hitPoint)))
                return;

            //Instantiate prefab instance
            GameObject go = PrefabUtility.InstantiatePrefab(prefabToSpawn.prefabToPaint) as GameObject;

            if (go == null)
            {
                Debug.LogError($"{PrefabBrush.DebugLogStart} Could not instantiate Prefab");
                return;
            }

            PrefabAttributes props = prefabToSpawn.customProps;
            PrefabBrush pb = PrefabBrush.instance;
            Vector3 offset = props.useCustomOffset ? props.GetOffset() : pb.offsetField.GetValue();
            Vector3 scale = props.useCustomScale ? props.GetScale() : PrefabBrushTool.MultiplyVec3(pb.scaleField.GetValue(), prefabToSpawn.prefabToPaint.transform.lossyScale);
            Vector3 rotation = props.useCustomRotationMode ? props.GetRotation() : pb.rotationField.GetValue();
            PrefabBrush.PivotMode pivotMode = props.useCustomPivotMode ? props.pivotMode : pb.GetPivotMode();

            Quaternion rot = pb.toggleAlignWithGround.value ? PrefabBrush.GetAlignedRotation(raycastHit) : Quaternion.Euler(rotation);
            rot *= Quaternion.AngleAxis(rotation.y, Vector3.up);
            rot *= Quaternion.AngleAxis(rotation.x, Vector3.right);
            rot *= Quaternion.AngleAxis(rotation.z, Vector3.forward);

            go.transform.position = hitPoint + offset + PrefabBrush.instance.GetLocalPosition(go.gameObject, pivotMode);
            go.transform.localScale = scale;
            go.transform.rotation = rot;

            PrefabBrushObject.RadiusBounds radiusBounds = PB_BoundsManager.GetRadiusBound(go);

            //Lets check if the new bounds intersects with any other bounds
            if (_addedGameObjects.Any(brushObject => brushObject != null && brushObject.meshClippingChecks && brushObject.BoundsIntersect(radiusBounds)))
            {
                Object.DestroyImmediate(go);
                return;
            }

            if (PB_BoundsManager.BoundsIntersect(radiusBounds))
            {
                Object.DestroyImmediate(go);
                return;
            }

            //Finally
            go.transform.SetParent(pb.GetParent());
            PrefabBrushObject pbo = PrefabBrush.RegisterObject(go);
            PB_UndoManager.AddToUndo(go);
            _addedGameObjects.Add(pbo);
            props.Dispose();
        }

        public static void OnPaintStart()
        {
            _addedGameObjects.Clear();

            if (!IsHit) return;

            if (!PrefabBrush.instance.IsParentOk()) return;

            PrefabBrush.instance.randomPointInsideDisc = PrefabBrushTool.GetRandomPointInsideDisc(Hit, Radius);
        }

        public static void SelectAllPrefabs()
        {
            foreach (PbPrefabUI prefab in PrefabBrush.instance.currentPrefabs)
                prefab.Select();
        }

        public static void DeselectAllPrefabs()
        {
            foreach (PbPrefabUI prefab in PrefabBrush.instance.currentPrefabs)
                prefab.Deselect();
        }

        public static void AddShortcuts()
        {
            PB_ShortcutManager.ShowShortcuts(PB_ModularShortcuts.increaseRadius);
            PB_ShortcutManager.ShowShortcuts(PB_ModularShortcuts.decreaseRadius);
        }
    }

    public static class PB_HandlesExtension
    {
        private static float timer;
        private static bool useMousePos;
        private static string tempText = "";
        private static int currentPriority = -1;
        private static readonly Color errorColor = new Color(1f, 0.61f, 0.58f);

        private static Color tempColor;
        private static RaycastHit hit;
        private static GUIStyle mainStyle;

        public static void WriteAngle(RaycastHit raycastHit)
        {
            if (timer > 0) return;
            if (!PrefabBrush.instance.useAngleLimitsToggle.value) return;
            float angle = Vector3.Angle(raycastHit.normal, Vector3.up);
            bool isValid = PrefabBrush.instance.IsAngleValid(angle);
            Color color = isValid ? Color.white : errorColor;
            useMousePos = false;
            WriteText(angle.ToString("f0") + "°", color, raycastHit);
        }

        private static void WriteText(string t, Color color, RaycastHit raycastHit)
        {
            //Draw text

            if (mainStyle == null)
            {
                Texture2D transparentTexture = new Texture2D(1, 1);
                transparentTexture.SetPixel(1, 1, new Color(0f, 0f, 0f, .3f));
                transparentTexture.Apply();

                mainStyle = new GUIStyle
                {
                    normal =
                    {
                        textColor = color,
                        background = transparentTexture,
                    },
                    fontSize = 14,
                    padding = new RectOffset(3, 3, 1, 1),
                    fontStyle = FontStyle.Bold,
                    alignment = TextAnchor.MiddleLeft,
                };
            }

            mainStyle.normal.textColor = color;

            Vector3 pos;
            Camera cam = PrefabBrush.instance.sceneCamera;

            if (!useMousePos)
            {
                float distance = Vector3.Distance(cam.transform.position, raycastHit.point);

                Transform camTransform = cam.transform;
                Vector3 offset = -camTransform.up * distance * .05f;
                offset += camTransform.right * distance * .05f;
                pos = raycastHit.point + offset;
            }
            else
            {
                Vector2 mousePos = Event.current.mousePosition;
                mousePos.y = cam.pixelHeight - mousePos.y - 20;
                mousePos.x += 20;
                Ray r = cam.ScreenPointToRay(mousePos);
                pos = r.origin + r.direction * 1f;
            }

            Handles.Label(pos, t, mainStyle);
        }

        public static void WriteTempText(string t, Vector3 value, RaycastHit lastHitInfo)
        {
            float min = Mathf.Min(value.x, value.y, value.z);
            string format = min < 1 ? "f2" : "f1";
            string v = "X " + value.x.ToString(format);
            v += "    Y " + value.y.ToString(format);
            v += "    Z " + value.z.ToString(format);

            WriteTempText(t + " " + v, lastHitInfo, Color.white);
        }

        public static void WriteTempTextAtMousePos(string t, Color c, int priority, float time = .8f)
        {
            if (priority < currentPriority) return;

            tempText = t;
            timer = time;
            tempColor = c;
            useMousePos = true;
            currentPriority = priority;
        }

        public static void WriteTempText(string t, float time = 0.8f, int priority = 1)
        {
            WriteTempText(t, PrefabBrush.instance.lastHitInfo, Color.white, time, priority);
        }

        public static void WriteTempText(string t, RaycastHit lastHitInfo, Color c, float time = .8f, int currentPriorityParam = 1)
        {
            if (currentPriorityParam < PB_HandlesExtension.currentPriority) return;

            hit = lastHitInfo;
            tempText = t;
            timer = time;
            tempColor = c;
            useMousePos = false;
            PB_HandlesExtension.currentPriority = currentPriorityParam;
        }

        public static void Update(float deltaTime)
        {
            if (Event.current.type != EventType.Repaint) return;
            if (timer < 0)
            {
                currentPriority = -1;
                return;
            }

            timer -= deltaTime;

            WriteText(tempText, tempColor, hit);
        }

        public static void WriteVector3(Vector3 rot, RaycastHit lastHitInfo, string name, string complement)
        {
            if (currentPriority >= 0) return;

            string text = $"{name}\n" +
                          "X   " + PrefabBrushTool.GetStringFormat(rot.x) + $"{complement}     ";
            text += "Y   " + PrefabBrushTool.GetStringFormat(rot.y) + $"{complement}     ";
            text += "Z   " + PrefabBrushTool.GetStringFormat(rot.z) + $"{complement}";
            WriteTempText(text, lastHitInfo, Color.white);
            currentPriority = -1;
        }

        public static void WriteTextErrorTemp(string msg, RaycastHit raycastHit, float t = -1)
        {
            WriteTempText(msg, raycastHit, errorColor, t == -1 ? PrefabBrush.deltaTime : t);
        }
    }

    public static class VisualElementsExtension
    {
        public static VisualElement focusElement;

        public static void SetBackgroundColor(this VisualElement e, Color c)
        {
            if (c == Color.clear)
            {
                e.style.backgroundColor = new StyleColor(StyleKeyword.Null);
                return;
            }

            e.style.backgroundColor = c;
        }

        public static void ChangeColorOnHover(this VisualElement e, Color hoverColor)
        {
            e.RegisterCallback<MouseOverEvent>(_ => { e.SetBackgroundColor(hoverColor); });

            e.RegisterCallback<MouseOutEvent>(_ => { e.SetBackgroundColor(Color.clear); });
        }

        public static void RegisterEditorPrefs(this FloatField e, string keyStart, float defaultValue)
        {
            string key = keyStart + e.name;
            e.SetValueWithoutNotify(EditorPrefs.GetFloat(key, defaultValue));
            e.RegisterCallback<FocusOutEvent>(_ => EditorPrefs.SetFloat(key, e.value));
        }

        public static void SetBackgroundTexture(this VisualElement e, Texture n)
        {
#if HARPIA_DEBUG
            if (n == null) Debug.LogError($"Texture is null for {e.name}");
#endif
            e.style.backgroundImage = (StyleBackground)n;
        }

        public static void SetBackgroundTexture(this VisualElement e, Texture2D n)
        {
#if HARPIA_DEBUG
            if (e == null) Debug.LogError($"visual element is null");
            if (n == null) Debug.LogError($"Texture2D is null for {e.name}");
#endif
            e.style.backgroundImage = n;
        }

        public static void Destroy(this VisualElement e)
        {
            if (e == null) return;
            if (e.parent == null) return;
            e.parent.Remove(e);
        }

        public static Texture GetBackgroundTexture(this VisualElement e)
        {
            Background img = e.style.backgroundImage.value;
            if (img == null) return null;

            if (img.texture != null) return img.texture;
            if (img.sprite != null) return img.sprite.texture;
            if (img.renderTexture != null) return img.renderTexture;

            return null;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetActive(this VisualElement e, bool n)
        {
            e.style.display = n ? DisplayStyle.Flex : DisplayStyle.None;
        }

        public static void SetVisible(this VisualElement e, bool n)
        {
            e.style.visibility = new StyleEnum<Visibility>(n ? Visibility.Visible : Visibility.Hidden);
        }

        public static bool IsActive(this VisualElement e)
        {
            return e.style.display == DisplayStyle.Flex;
        }

        public static void SetBorderColor(this VisualElement element, Color c, float width = 2f)
        {
            element.style.borderBottomWidth = width;
            element.style.borderTopWidth = width;
            element.style.borderLeftWidth = width;
            element.style.borderRightWidth = width;

            element.style.borderBottomColor = c;
            element.style.borderTopColor = c;
            element.style.borderLeftColor = c;
            element.style.borderRightColor = c;
        }

        public static void SetBorderRadius(this VisualElement element, float r)
        {
            element.style.borderBottomLeftRadius = r;
            element.style.borderBottomRightRadius = r;
            element.style.borderTopLeftRadius = r;
            element.style.borderTopRightRadius = r;
        }

        public static void SetBorderPadding(this VisualElement element, float r)
        {
            element.style.paddingBottom = r;
            element.style.paddingTop = r;
            element.style.paddingLeft = r;
            element.style.paddingRight = r;
        }

        public static void BorderColorOnHover(this VisualElement e, Color c)
        {
            e.RegisterCallback<MouseEnterEvent>(_ => e.SetBorderColor(c));
            e.RegisterCallback<MouseOutEvent>(_ => e.SetBorderColor(Color.clear));
        }

        public static void RegisterFocusEvents(this VisualElement element)
        {
            element.RegisterCallback<FocusInEvent>(_ => { focusElement = element; });

            element.RegisterCallback<FocusOutEvent>(_ =>
            {
                if (focusElement == null) return;
                if (focusElement == element) focusElement = null;
            });
        }
    }

    public static class PB_ShortcutManager
    {
        private static List<VisualElement> toAdd;
        private static VisualElement lastParent;

        public static void ClearShortcuts()
        {
            toAdd?.Clear();
            if (lastParent != null) lastParent.Clear();
        }

        public static void ShowShortcuts(PB_ModularShortcuts.ShortcutData data, bool addSpace = false, bool useScrollWheel = false)
        {
            VisualElement shortCutElement = new VisualElement
            {
                style =
                {
                    flexDirection = FlexDirection.Row,
                    marginTop = addSpace ? 10 : 5
                },
                pickingMode = PickingMode.Ignore,
            };

            CreateShortcutLabel(data, shortCutElement);

            if (useScrollWheel) AddScrollWheel(shortCutElement);

            Label label = new Label
            {
                text = data.shortCutName,
                style =
                {
                    unityTextAlign = TextAnchor.MiddleLeft
                },
                pickingMode = PickingMode.Ignore,
            };

            shortCutElement.Add(label);

            toAdd ??= new List<VisualElement>();
            toAdd.Add(shortCutElement);
        }

        public static void ApplyTo(VisualElement parent)
        {
            parent.Clear();

            if (toAdd == null) return;
            if (toAdd.Count == 0) return;

            parent.Add(new Label("<b>Shortcuts</b>"));

            foreach (VisualElement element in toAdd)
            {
                parent.Add(element);
            }

            toAdd.Clear();
        }

        private static void AddScrollWheel(VisualElement parent)
        {
            Label plus = new Label()
            {
                text = "+",
                style =
                {
                    marginRight = 5,
                }
            };

            parent.Add(plus);

            Label shortcutLabel = new Label
            {
                text = "ScrollWheel",
                style =
                {
                    unityTextAlign = TextAnchor.MiddleCenter,
                    marginRight = 5
                },
                pickingMode = PickingMode.Ignore
            };

            shortcutLabel.SetBorderColor(Color.grey);
            shortcutLabel.SetBorderRadius(5);
            shortcutLabel.SetBorderPadding(2);
            shortcutLabel.style.minWidth = 20;
            shortcutLabel.style.height = 20;

            parent.Add(shortcutLabel);
            lastParent = parent;
        }

        private static void CreateShortcutLabel(PB_ModularShortcuts.ShortcutData data, VisualElement parent)
        {
            Label shortcutLabel = new Label
            {
                text = data.GetKeyText(),
                style =
                {
                    unityTextAlign = TextAnchor.MiddleCenter,
                    marginRight = 5
                },
                pickingMode = PickingMode.Ignore
            };

            shortcutLabel.SetBorderColor(Color.grey);
            shortcutLabel.SetBorderRadius(5);
            shortcutLabel.SetBorderPadding(2);
            shortcutLabel.style.minWidth = 20;
            shortcutLabel.style.height = 20;

            parent.Add(shortcutLabel);
            lastParent = parent;
        }
    }

    public static class PB_BoundsManager
    {
        public static Bounds GetBounds(GameObject go)
        {
            Transform obj = go.transform;

            if (obj.TryGetComponent(typeof(PrefabBrushObject), out Component com))
            {
                return ((PrefabBrushObject)com).GetBounds();
            }

            Renderer[] renderers = obj.GetComponentsInChildren<Renderer>();
            if (renderers.Length == 0) return new Bounds(obj.position, Vector3.zero);

            Bounds bounds = GetBounds(renderers);

            return bounds;
        }

        private static Bounds GetBounds(Renderer[] renderers)
        {
            Bounds bounds = renderers[0].bounds;
            for (int index = 1; index < renderers.Length; index++)
            {
                Renderer renderer = renderers[index];
                bounds.Encapsulate(renderer.bounds);
            }

            return bounds;
        }

        public static bool BoundsIntersect(PrefabBrushObject.RadiusBounds bounds)
        {
            if (PB_PhysicsSimulator.IsUsingPhysics()) return false;

            PrefabBrushObject[] allBrushObjects = PrefabBrushObject.GetBrushObjects();

            foreach (PrefabBrushObject brushObject in allBrushObjects)
            {
                if (brushObject == null) continue;
                if (brushObject.meshClippingChecks == false) continue;
                if (brushObject.BoundsIntersect(bounds)) return true;
            }

            return false;
        }

        public static bool BoundsIntersects(Vector3 raycastHitPoint)
        {
            if (PB_PhysicsSimulator.IsUsingPhysics()) return false;

            PrefabBrushObject[] brushObjects = PrefabBrushObject.GetBrushObjects();

            foreach (PrefabBrushObject brushObject in brushObjects)
            {
                if (brushObject == null) continue;
                if (!brushObject.meshClippingChecks) continue;
                if (brushObject.BoundsIntersect(raycastHitPoint)) return true;
            }

            return false;
        }

        public static PrefabBrushObject.RadiusBounds GetRadiusBound(GameObject go)
        {
            Bounds bounds = GetBounds(go);
            return new PrefabBrushObject.RadiusBounds(bounds, go.transform);
        }
    }

    public static class PB_TextureMaskHandler
    {
        private static VisualElement terrainPanel;
        public static Toggle useTextureMaskToggle;

        private static Dictionary<Texture, bool> allowedTextures;
        private static VisualElement texturesHolder;

        public static void Init(VisualElement root)
        {
            terrainPanel = root.Q<VisualElement>("allowed-textures-section");
            useTextureMaskToggle = root.Q<Toggle>("use-texture-mask-toggle");
            texturesHolder = root.Q<VisualElement>("allowed-textures-holder");
            texturesHolder.Clear();

            useTextureMaskToggle.RegisterValueChangedCallback(OnUseTextureMaskToggle);

            UpdateUITerrain();
        }

        private static void OnUseTextureMaskToggle(ChangeEvent<bool> evt)
        {
#if HARPIA_DEBUG
            Debug.Log($"toggle value changed {evt.newValue}");
#endif
            bool e = PrefabBrush.instance.GetRaycastMode() == PrefabBrush.RaycastMode.Physical_Collider;
            terrainPanel.SetActive(evt.newValue && e);
            UpdateUITerrain();
        }

        public static void UpdateUITerrain()
        {
            bool raycastMode = PrefabBrush.instance.GetRaycastMode() == PrefabBrush.RaycastMode.Physical_Collider;
            bool paintMode = PrefabBrush.instance.GetPaintMode() == PrefabBrush.PaintMode.Multiple;

            useTextureMaskToggle.SetActive(raycastMode && paintMode);
            terrainPanel.SetActive(raycastMode && useTextureMaskToggle.value && paintMode);

            AddAllowedTexture(PB_TerrainHandler.GetTerrainTextures(), texturesHolder.childCount == 0);
        }

        private static void AddAllowedTexture(IEnumerable<Texture> textures, bool selected)
        {
            textures ??= new List<Texture>();
            foreach (Texture texture in textures) AddAllowedTexture(texture, selected);
        }

        private static void AddAllowedTexture(Texture newTexture, bool selected)
        {
            if (newTexture == null) return;

            allowedTextures ??= new Dictionary<Texture, bool>();
            if (allowedTextures.ContainsKey(newTexture)) return;

            const float notSelectedOpacity = .3f;
            const float selectedOpacity = 1f;

            const int size = 35;
            VisualElement textureElement = new VisualElement
            {
                style =
                {
                    width = size,
                    height = size,
                    marginLeft = 2,
                    marginRight = 2,
                    opacity = selected ? selectedOpacity : notSelectedOpacity,
                }
            };
            textureElement.SetBorderRadius(1f);
            textureElement.SetBorderColor(selected ? Color.yellow : Color.clear);

            //hover
            textureElement.SetBackgroundTexture(newTexture);

            //Click event
            textureElement.RegisterCallback<ClickEvent>(e =>
            {
                VisualElement element = e.target as VisualElement;
                bool isSelected = element.style.opacity.value == 1f;
                isSelected = !isSelected;
                element.style.opacity = isSelected ? selectedOpacity : notSelectedOpacity;
                element.SetBorderRadius(1f);
                element.SetBorderColor(isSelected ? Color.yellow : Color.clear);

                //tooltip
                element.tooltip = newTexture.name + "\n";
                element.tooltip += isSelected ? "Selected" : "Not Selected";
                allowedTextures[newTexture] = isSelected;
            });

            RegisterContextMenu(textureElement);
            texturesHolder.Add(textureElement);
            allowedTextures.Add(newTexture, selected);
        }

        private static void RegisterContextMenu(VisualElement textureElement)
        {
            GenericMenu menu = new GenericMenu();

            menu.AddItem(new GUIContent("Reveal In Project"), false, () =>
            {
                Texture2D texture = textureElement.style.backgroundImage.value.texture;
                EditorUtility.FocusProjectWindow();
                Selection.activeObject = texture;
            });

            textureElement.RegisterCallback<ContextClickEvent>(_ => { menu.ShowAsContext(); });
        }

        public static bool IsTextureValid(Vector3 hitPoint)
        {
            if (useTextureMaskToggle.value == false) return true;
            if (PrefabBrush.instance.GetPaintMode() != PrefabBrush.PaintMode.Multiple) return true;
            if (allowedTextures == null) return true;
            if (PB_TerrainHandler.HasAnyTerrain() == false) return true;

            Texture t = PB_TerrainHandler.GetTextureAtPosition(hitPoint);
            if (t == null) return true;
            if (!allowedTextures.ContainsKey(t)) return true;
            return allowedTextures[t];
        }

        public static void Dispose()
        {
            texturesHolder.Clear();
            allowedTextures?.Clear();
        }

        public static void AddTextures(List<Texture> textures, bool selected)
        {
            if (textures == null) return;
            if (textures.Count == 0) return;
            foreach (Texture texture in textures) AddAllowedTexture(texture, selected);
        }

        public static List<Texture> GetAllowedTextures()
        {
            if (allowedTextures == null) return null;
            return allowedTextures.Where(x => x.Value).Select(x => x.Key).ToList();
        }
    }

    public static class PB_FileManager
    {
        public static bool IsInsideAssetFolder(string path)
        {
            //log path
#if HARPIA_DEBUG
            Debug.Log("IsInsideAssetFolder - path " + path);
#endif
            return path.Contains(Application.dataPath);
        }

        public static void RemoveFileNamesFromPath(ref string lastPath)
        {
            lastPath = lastPath.Replace(GetFileName(lastPath), "");
        }

        private static string GetFileName(string path)
        {
            string fileName = Path.GetFileName(path);
            return fileName;
        }
    }

    public static class PB_PhysicsSimulator
    {
        //https://forum.unity.com/threads/separating-physics-scenes.597697/

        private static bool shouldSimulate;
        public static Slider physicsStepSlider;
        public static EnumField affectObjectsEnum;
        public static EnumField simulationModeEnum;

        private static Toggle simulatePhysicsToggle;
#if UNITY_2022_2_OR_NEWER
        private static UnityEngine.SimulationMode defaultPhysicsSimulationMode;
#endif

        private static bool isPhysicsActive;
        private static VisualElement physicsStatus;
        public static Vector3ModeElement impulseFieldVec3;
        public static Slider impulseForceSlider;
        public static Button fixedButton;
        public static Button maxButton;
        public static Button minButton;
        private static double lastUpdateTime;
        private static float deltaTimeUpdate;
        private static Label noRigidbodyLabel;

        private static void OnSimulationModeChanged(ChangeEvent<Enum> evt)
        {
#if HARPIA_DEBUG
            Debug.Log($"Simulation mode changed {evt.newValue}");
#endif
            Dispose();
            UpdateUIPhysics();
        }

        public static AffectMode GetAffectMode() => affectObjectsEnum.value as AffectMode? ?? AffectMode.All;

        public static Vector3 GetImpulseForce() => impulseFieldVec3.useToggle.value ? impulseFieldVec3.GetValue() : Vector3.zero;

        public enum SimulationMode
        {
            Auto,
            Step,
        }

        public enum AffectMode
        {
            Only_New_Spawned_Objects,

            //Only_Selected,
            All,
        }

        public static void Init(VisualElement rootElement)
        {
            if (Application.isPlaying) return;

#if UNITY_2022_2_OR_NEWER
            defaultPhysicsSimulationMode = Physics.simulationMode;
#else
            isPhysicsActive = Physics.autoSimulation;
#endif

            physicsStatus = rootElement.Q<VisualElement>("status-physics-section");
            physicsStatus.SetActive(false);
            physicsStatus.SetBackgroundColor(PrefabBrush._styleBackgroundColorGreen);

            simulatePhysicsToggle = rootElement.Q<Toggle>("simulate-physics-toggle");
            simulatePhysicsToggle.text = "Simulate Physics";
            simulatePhysicsToggle.RegisterValueChangedCallback(OnSimulateToggle);

            rootElement.Q<Button>("physics-reset-bodies").RegisterCallback<ClickEvent>(OnResetBodiesClick);
            rootElement.Q<Button>("stop-physics-button").RegisterCallback<ClickEvent>(_ => Dispose());

            //Impulse
            fixedButton = rootElement.Q<Button>("align-with-scene-view-fixed-impulse");
            maxButton = rootElement.Q<Button>("align-with-scene-view-max-impulse");
            minButton = rootElement.Q<Button>("align-with-scene-view-min-impulse");

            impulseForceSlider = rootElement.Q<Slider>("impulse-force-slider");
            impulseFieldVec3 = new Vector3ModeElement(rootElement, "custom-props-impulse-type", "custom-props-impulse-fixed", "custom-props-impulse-max", "custom-props-impulse-min", "custom-props-impulse-toggle", false);
            impulseFieldVec3.useToggle.RegisterValueChangedCallback(OnImpulseChanged);

            impulseFieldVec3.RegisterFocusEvents();
            fixedButton.RegisterFocusEvents();
            maxButton.RegisterFocusEvents();
            minButton.RegisterFocusEvents();

            Toggle impulseToggle = rootElement.Q<Toggle>("custom-props-impulse-toggle");
            impulseToggle.RegisterValueChangedCallback(e => impulseForceSlider.SetActive(e.newValue));
            impulseForceSlider.SetActive(impulseToggle.value);

            impulseFieldVec3.SetButtons(fixedButton, maxButton, minButton);

            fixedButton.RegisterCallback<ClickEvent>(_ => OnAlignWithImpulse(PrefabBrush.instance.sceneCamera, impulseFieldVec3.fixedField));
            maxButton.RegisterCallback<ClickEvent>(_ => OnAlignWithImpulse(PrefabBrush.instance.sceneCamera, impulseFieldVec3.maxField));
            minButton.RegisterCallback<ClickEvent>(_ => OnAlignWithImpulse(PrefabBrush.instance.sceneCamera, impulseFieldVec3.minField));

            //step
            rootElement.Q<Button>("step-physics-button").RegisterCallback<ClickEvent>(OnStepPhysicsClick);

            //Physics settings
            rootElement.Q<Button>("btn-physics-settings").RegisterCallback<ClickEvent>(_ => SettingsService.OpenProjectSettings("Project/Physics"));

            //Info
            simulationModeEnum = rootElement.Q<EnumField>("physics-simulation-mode");
            simulationModeEnum.Init(SimulationMode.Auto);
            simulationModeEnum.RegisterValueChangedCallback(OnSimulationModeChanged);

            noRigidbodyLabel = rootElement.Q<Label>("no-rigidbody-selected");

            affectObjectsEnum = rootElement.Q<EnumField>("physics-affect-objects");
            affectObjectsEnum.Init(AffectMode.All);
            affectObjectsEnum.RegisterValueChangedCallback(OnAffectsObjectsChanged);
            OnAffectsObjectsChanged(null);

            physicsStepSlider = rootElement.Q<Slider>("physics-step-slider");
        }

        /// <summary>
        /// Checks if the prefab has a non trigger collider
        /// </summary>
        public static bool HasPhysicalCollider(GameObject o)
        {
            //Copy this method to prefabBrushObject
            foreach (Collider collider in o.GetComponentsInChildren<Collider>())
            {
                if (!collider.enabled) continue;
                if (collider.isTrigger) continue;
                return true;
            }

            return false;
        }

        /// <summary>
        /// Check If the Object has a non Kinematic rigidbody
        /// </summary>
        public static bool HasValidRigidbody(GameObject o)
        {
            //Copy this method to prefabBrushObject
            foreach (Rigidbody rigidbody in o.GetComponentsInChildren<Rigidbody>())
                if (rigidbody.isKinematic == false)
                    return true;

            return false;
        }

        private static void OnStepPhysicsClick(ClickEvent evt)
        {
#if HARPIA_DEBUG
            Debug.Log($"OnStepPhysicsClick");
#endif

            if (GetSimulationMode() != SimulationMode.Step) return;

#if UNITY_2022_2_OR_NEWER
            Physics.simulationMode = UnityEngine.SimulationMode.Script;
#else
            Physics.autoSimulation = false;
#endif
            Physics.Simulate(physicsStepSlider.value);
            shouldSimulate = false;
        }

        private static void OnImpulseChanged(ChangeEvent<bool> evt)
        {
            // if (evt.newValue == false) return;
            // if (PrefabBrush.instance.currentPrefabs.Count == 0) return;
            // if (PrefabBrush.instance.currentPrefabs.Any(e => PB_PhysicsSimulator.HasValidRigidbody(e.prefabToPaint))) return;
            // PrefabBrush.DisplayError("None of the selected prefabs have a Rigidbody component. You will not be able to use the impulse feature.");
        }

        private static void OnAlignWithImpulse(Camera cam, Vector3Field field)
        {
            if (cam == null)
            {
                PrefabBrush.DisplayError("No scene view found, please add a scene view to your layout.");
                return;
            }

            //round to 3 decimals
            const float round = 1000f;
            Vector3 forward = cam.transform.forward;
            forward.x = Mathf.Round(forward.x * round) / round;
            forward.y = Mathf.Round(forward.y * round) / round;
            forward.z = Mathf.Round(forward.z * round) / round;

            field.SetValueWithoutNotify(forward);
        }

        private static void OnAffectsObjectsChanged(ChangeEvent<Enum> evt)
        {
            AffectMode newValue = AffectMode.All;
            if (evt != null) newValue = evt.newValue as AffectMode? ?? AffectMode.All;

            UpdateRigidbodiesObjects();
            UpdateUIPhysics();
        }

        public static void UpdateRigidbodiesObjects()
        {
#if HARPIA_DEBUG
            Debug.Log($"[{nameof(PB_PhysicsSimulator)}] UpdateRigidbodiesObjects");
#endif

            //all rb in scene
            RigidbodyData.ClearList();

            if (!PrefabBrushTool.isUsingTool) return;

            AffectMode mode = GetAffectMode();
            if (mode == AffectMode.All)
            {
                UpdateUIPhysics();
                return;
            }

            if (mode == AffectMode.Only_New_Spawned_Objects)
            {
                //get all rigidbodies
#if UNITY_2022_2_OR_NEWER
                IEnumerable<Rigidbody> allRBs = Object.FindObjectsByType<Rigidbody>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
#else
                IEnumerable<Rigidbody> allRBs = Object.FindObjectsOfType<Rigidbody>();
#endif
                foreach (Rigidbody rB in allRBs) RigidbodyData.AddRigidbody(rB, true);
                UpdateUIPhysics();
                PB_UndoManager.RegisterUndoTransforms(allRBs);
                return;
            }

#if UNITY_2022_2_OR_NEWER
            IEnumerable<Rigidbody> kinematicRBs = Object.FindObjectsByType<Rigidbody>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
#else
            IEnumerable<Rigidbody> kinematicRBs = Object.FindObjectsOfType<Rigidbody>();
#endif

            foreach (Rigidbody rigidbody in kinematicRBs)
            {
                RigidbodyData.AddRigidbody(rigidbody, true);
            }

            UpdateUIPhysics();
        }

        private static void OnResetBodiesClick(ClickEvent evt)
        {
#if HARPIA_DEBUG
            Debug.Log("OnResetBodiesClick");
#endif

#if UNITY_2022_2_OR_NEWER
            Rigidbody[] list = Object.FindObjectsByType<Rigidbody>(FindObjectsInactive.Include, FindObjectsSortMode.None);
#else
            Rigidbody[] list = Object.FindObjectsOfType<Rigidbody>();
#endif

            foreach (Rigidbody rigidbody in list)
            {
                if (rigidbody.isKinematic) continue;
                rigidbody.velocity = Vector3.zero;
                rigidbody.angularVelocity = Vector3.zero;
            }
        }

        private static void OnSimulateToggle(ChangeEvent<bool> changeEvent)
        {
#if HARPIA_DEBUG
            Debug.Log($"[{nameof(PB_PhysicsSimulator)}] On simulate toggle {changeEvent.newValue}");
#endif

            if (Application.isPlaying)
            {
                PrefabBrush.DisplayError("Can't simulate physics while in play mode");
                return;
            }

            shouldSimulate = changeEvent.newValue;

            if (shouldSimulate)
            {
                bool anyPrefabWithPhysics = PrefabBrush.instance.HasAnyPrefabsWithPhysics();
                noRigidbodyLabel.SetActive(!anyPrefabWithPhysics);
                lastUpdateTime = EditorApplication.timeSinceStartup;
                deltaTimeUpdate = 0;
                UpdateRigidbodiesObjects();
            }
            else
            {
                noRigidbodyLabel.SetActive(false);
                RigidbodyData.ClearList();
            }

            UpdateUIPhysics();
        }

        public static void UpdateUIPhysics()
        {
            if (noRigidbodyLabel == null) return;

            physicsStepSlider?.parent.SetActive(GetSimulationMode() == SimulationMode.Step);
            physicsStatus?.SetActive(shouldSimulate);
            simulatePhysicsToggle.SetActive(GetSimulationMode() == SimulationMode.Auto);

            noRigidbodyLabel.SetActive(simulatePhysicsToggle.value && !PrefabBrush.instance.HasAnyPrefabsWithPhysics());

            impulseFieldVec3.SetActive(true);
            impulseForceSlider.SetActive(impulseFieldVec3.useToggle.value);

            PrefabBrush.instance.spacer.style.marginTop = !shouldSimulate ? 55 : 90;
        }

        public static void Dispose(bool stopSimulation = true)
        {
            if (Application.isPlaying) return;

#if HARPIA_DEBUG
            Debug.Log($"[Physics] Dispose  - stopSimulation {stopSimulation}");
#endif

            if (stopSimulation)
            {
                shouldSimulate = false;
                simulatePhysicsToggle?.SetValueWithoutNotify(false);
            }

            RigidbodyData.ClearList();
#if UNITY_2022_2_OR_NEWER
            Physics.simulationMode = defaultPhysicsSimulationMode;
#else
            Physics.autoSimulation = isPhysicsActive;
#endif

            if (Selection.transforms.Length > 0) UpdateRigidbodiesObjects();

            UpdateUIPhysics();
        }

        public static void Update()
        {
            if (!shouldSimulate) return;
            if (GetSimulationMode() != SimulationMode.Auto) return;

            deltaTimeUpdate = (float)(EditorApplication.timeSinceStartup - lastUpdateTime);
            lastUpdateTime = EditorApplication.timeSinceStartup;
            if (deltaTimeUpdate > .5f) return;

#if UNITY_2022_2_OR_NEWER
            Physics.simulationMode = UnityEngine.SimulationMode.Script;
#else
            Physics.autoSimulation = false;
#endif
            Physics.Simulate(deltaTimeUpdate);
        }

        [Serializable]
        public struct RigidbodyData
        {
            [SerializeField] private Rigidbody rb;

            [SerializeField] private bool UseGravity;

            [SerializeField] private bool DetectCollision;

            [SerializeField] private bool IsKinematic;

            [SerializeField] private float MaxAngularVelocity;

            [SerializeField] private float MaxDepenetrationVelocity;

            [SerializeField] private float AngularDrag;

            [SerializeField] private float Drag;

            [SerializeField] private int Layer;

            [SerializeField] private CollisionDetectionMode CollisionMode;

            [SerializeField] private RigidbodyInterpolation Interpolation;

            [SerializeField] private RigidbodyConstraints Constraints;

            public static List<RigidbodyData> allRigidbodies = new List<RigidbodyData>();
            public static int Count => allRigidbodies.Count;

            public static void AddRigidbody(Rigidbody body, bool kinematic)
            {
                if (body.isKinematic) return;

                bool hasPhysicalCollider = PB_PhysicsSimulator.HasPhysicalCollider(body.gameObject);

                allRigidbodies.Add(new RigidbodyData(body));

                body.isKinematic = kinematic || !hasPhysicalCollider;
                body.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
            }

            private RigidbodyData(Rigidbody body)
            {
                rb = body;
                Interpolation = body.interpolation;
                UseGravity = body.useGravity;
                IsKinematic = body.isKinematic;
                MaxAngularVelocity = body.maxAngularVelocity;
                MaxDepenetrationVelocity = body.maxDepenetrationVelocity;
                AngularDrag = body.angularDrag;
                Drag = body.drag;
                CollisionMode = body.collisionDetectionMode;
                Layer = body.gameObject.layer;
                DetectCollision = body.detectCollisions;
                Constraints = body.constraints;
            }

            public void RestoreRigidBody()
            {
                if (rb == null)
                {
                    allRigidbodies.Remove(this);
                    return;
                }

                rb.useGravity = UseGravity;
                rb.collisionDetectionMode = CollisionMode;
                rb.isKinematic = IsKinematic;
                rb.maxAngularVelocity = MaxAngularVelocity;
                rb.maxDepenetrationVelocity = MaxDepenetrationVelocity;
                rb.angularDrag = AngularDrag;
                rb.drag = Drag;
                rb.gameObject.layer = Layer;
                rb.detectCollisions = DetectCollision;
                rb.interpolation = Interpolation;
                rb.constraints = Constraints;

                EditorUtility.SetDirty(rb);
                allRigidbodies.Remove(this);
            }

            public static void ClearList()
            {
                while (allRigidbodies.Count > 0) allRigidbodies[0].RestoreRigidBody();
            }
        }

        public static SimulationMode GetSimulationMode()
        {
            return simulationModeEnum.value as SimulationMode? ?? SimulationMode.Auto;
        }

        public static void ApplyImpulse(GameObject go)
        {
            Rigidbody[] rbs = go.GetComponentsInChildren<Rigidbody>();
            if (rbs.Length == 0) return;

            bool addImpulse = impulseFieldVec3.useToggle.value;
            float force = impulseForceSlider.value;

            foreach (Rigidbody rb in rbs)
            {
                RigidbodyData.AddRigidbody(rb, false);
                if (addImpulse) rb.AddForce(impulseFieldVec3.GetValue().normalized * force, ForceMode.Impulse);
            }
        }

        public static void DrawArrowHandles()
        {
            if (!impulseFieldVec3.useToggle.value) return;
            if (!PrefabBrush.instance.isRaycastHit) return;

            Vector3 hit = PrefabBrush.instance.lastHitInfo.point + Vector3.up;

            Handles.color = Color.cyan;
            if (impulseFieldVec3.GetMode() == PrefabBrush.Vector3Mode.Fixed)
            {
                Quaternion rot = Quaternion.LookRotation(impulseFieldVec3.fixedField.value);
                Handles.ArrowHandleCap(-1, hit, rot, 1, EventType.Repaint);
                return;
            }

            Handles.color = Color.cyan;
            Handles.ArrowHandleCap(-1, hit, Quaternion.LookRotation(impulseFieldVec3.minField.value), 1, EventType.Repaint);
        }

        public static void DrawArrowHandles(Vector3 val, Vector3 hitInfoPoint)
        {
            Handles.color = Color.cyan;
            Vector3 hit = hitInfoPoint + Vector3.up;
            Handles.ArrowHandleCap(-1, hit, Quaternion.LookRotation(val), 1, EventType.Repaint);
        }

        public static bool IsUsingPhysics()
        {
            if (simulatePhysicsToggle == null) return false;
            return simulatePhysicsToggle.value || GetSimulationMode() == SimulationMode.Step;
        }

        public static void ShowDebugWindow()
        {
            EditorApplication.ExecuteMenuItem("Window/Analysis/Physics Debugger");
        }
    }

    public static class PB_MeshBatcher
    {
        private static Mesh combinedMesh;

        public static Mesh Batch()
        {
            if (combinedMesh != null) return combinedMesh;

            //get all mesh filter
#if UNITY_2022_2_OR_NEWER
            MeshFilter[] meshFilters = Object.FindObjectsByType<MeshFilter>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
#else
            MeshFilter[] meshFilters = Object.FindObjectsOfType<MeshFilter>(false);
#endif

            List<MeshFilter> validList = new List<MeshFilter>();

            foreach (MeshFilter meshFilter in meshFilters)
            {
                //Try to get renderer
                if (!IsValid(meshFilter)) continue;
                validList.Add(meshFilter);
            }

            //batch
            CombineInstance[] combine = new CombineInstance[validList.Count];

            for (int i = 0; i < validList.Count; i++)
            {
                combine[i].mesh = validList[i].sharedMesh;
                combine[i].transform = validList[i].transform.localToWorldMatrix;
            }

            //create new mesh
            combinedMesh = new Mesh
            {
                indexFormat = IndexFormat.UInt32
            };
            combinedMesh.CombineMeshes(combine);

#if HARPIA_DEBUG
            if (validList.Count == 0) Debug.LogError($"Error batcher list is empty");
            Debug.Log($"[PB Mesh Batcher] Generating new mesh {validList.Count}");
#endif

            return combinedMesh;
        }

        private static bool IsValid(MeshFilter meshFilter)
        {
            if (meshFilter.sharedMesh == null) return false;

            if (!meshFilter.gameObject.TryGetComponent(typeof(Renderer), out Component r))
                return false;

            MeshRenderer renderer = (MeshRenderer)r;
            if (renderer.enabled == false) return false;
            if (!PrefabBrush.instance.IsTagAllowed(renderer.gameObject.tag))
            {
                string tag = renderer.gameObject.tag;
                PB_HandlesExtension.WriteTextErrorTemp($"Tag not allowed: {tag}", PrefabBrush.instance.lastHitInfo);
                return false;
            }

            if (!PrefabBrush.instance.IsLayerValid(renderer.gameObject.layer))
            {
                return false;
            }

            return true;
        }

        public static void AddMeshToBatch(GameObject mesh)
        {
            if (PrefabBrush.instance.GetRaycastMode() != PrefabBrush.RaycastMode.Mesh) return;
            if (!PrefabBrush.instance.precisionModeAddMeshToBatch.value) return;

#if HARPIA_DEBUG
            int vertexCount = combinedMesh == null ? 0 : combinedMesh.vertexCount;
#endif

            MeshFilter[] allFilters = mesh.GetComponentsInChildren<MeshFilter>();
            List<MeshFilter> validFilters = new List<MeshFilter>();

            foreach (MeshFilter meshFilter in allFilters)
            {
                if (!IsValid(meshFilter)) continue;
                validFilters.Add(meshFilter);
            }

            if (validFilters.Count == 0) return;

            CombineInstance[] combineInstances = new CombineInstance[validFilters.Count + 1];
            combineInstances[0].mesh = combinedMesh == null ? new Mesh() : combinedMesh;
            combineInstances[0].transform = Matrix4x4.identity;

            for (int i = 1; i < combineInstances.Length; i++)
            {
                MeshFilter filter = validFilters[i - 1];
                combineInstances[i].mesh = filter.sharedMesh;
                combineInstances[i].transform = filter.transform.localToWorldMatrix;
#if HARPIA_DEBUG
                if (combineInstances[i].mesh == null) Debug.LogError($"Combine instance is null {i}");
#endif
            }

            Mesh newCombinedMesh = new Mesh
            {
                indexFormat = IndexFormat.UInt32
            };
            newCombinedMesh.CombineMeshes(combineInstances);

            combinedMesh = newCombinedMesh;

            PB_MeshRaycaster.Dispose();

#if HARPIA_DEBUG
            Debug.Log($"[PB Mesh Batcher] Adding new mesh {validFilters.Count}");
            Assert.IsFalse(vertexCount == combinedMesh.vertexCount, $"Vertex count is the same {vertexCount}");
#endif
        }

        public static void Dispose()
        {
            //destroy mesh
            Object.DestroyImmediate(combinedMesh);
            combinedMesh = null;
            PB_MeshRaycaster.Dispose();
        }

        public static Mesh BatchObject(GameObject gameObject)
        {
            //get all the mesh filter
            MeshFilter[] meshFilters = gameObject.GetComponentsInChildren<MeshFilter>(false);
            List<MeshFilter> validMeshFilters = new List<MeshFilter>();

            foreach (MeshFilter filter in meshFilters)
            {
                if (filter.sharedMesh == null) continue;

                if (!filter.gameObject.TryGetComponent(typeof(Renderer), out Component r)) continue;

                MeshRenderer renderer = (MeshRenderer)r;

                if (renderer.enabled == false) continue;

                validMeshFilters.Add(filter);
            }

            //batch
            CombineInstance[] combine = new CombineInstance[validMeshFilters.Count];

            for (int index = 0; index < validMeshFilters.Count; index++)
            {
                MeshFilter filter = validMeshFilters[index];
                combine[index].mesh = filter.sharedMesh;
                combine[index].transform = filter.transform.localToWorldMatrix;
            }

            //create new mesh
            Mesh newCombinedMesh = new Mesh
            {
                indexFormat = IndexFormat.UInt32
            };
            newCombinedMesh.CombineMeshes(combine);

            return newCombinedMesh;
        }
    }

    [Serializable]
    public class PbPrefabUI
    {
        private const string XMLFileName = "PrefabBrushPrefabXML";

        private static VisualTreeAsset _tree;
        private static string _visualTreeGuid;

        private static Texture2D _defaultThumbUnity;
        private static Texture2D _customPropsIconUnity;
        private static Texture2D _rigidbodyIconUnity;
        private static Texture2D _colliderIconUnity;

        private static PbPrefabUI _contextSelected;
        private static PbPrefabUI _lastSelectedCard;
        public static Color customPropsColor = new Color(0f, 1f, 0f, 0.27f);

        public VisualElement iconElement;
        private TemplateContainer _mainElement;

        public GameObject prefabToPaint;

        public bool selected;
        public PrefabAttributes customProps;

        [NonSerialized] public Texture2D loadedIcon;

        private VisualElement _parentElement;
        private VisualElement _customPropsIcon;
        private VisualElement _rigidbodyIconElement;
        private Button _deleteButton;
        private Task _loadIconTask;
        private VisualElement _colliderIconElement;
        private Label _nameLabel;
        private static PbPrefabUI _lastPrefabClickedPrecision;

        public PbPrefabUI(VisualElement parent, GameObject prefab)
        {
            prefabToPaint = prefab;
            InstantiateUI(parent);
            customProps = new PrefabAttributes();
        }

        private void LoadIcon()
        {
            if (loadedIcon == null)
            {
                _loadIconTask ??= LoadThumbnail(iconElement);
                return;
            }

            iconElement.SetBackgroundTexture(loadedIcon);
        }

        public void InstantiateUI(VisualElement parent)
        {
            _tree ??= PrefabBrush.LoadVisualTreeAsset(XMLFileName, ref _visualTreeGuid);
            _parentElement = parent;

            _mainElement = _tree.Instantiate();
            _mainElement.RegisterCallback<ClickEvent>(OnPrefabClick);

            _parentElement.Add(_mainElement);

            iconElement = _mainElement.Q<VisualElement>("icon");
#if UNITY_2022_2_OR_NEWER
            iconElement.style.backgroundPositionY = new StyleBackgroundPosition(new BackgroundPosition(BackgroundPositionKeyword.Top));
#endif

            if (_customPropsIconUnity == null) _customPropsIconUnity = EditorGUIUtility.IconContent("d_SceneViewTools").image as Texture2D;
            if (_rigidbodyIconUnity == null) _rigidbodyIconUnity = EditorGUIUtility.IconContent("d_Rigidbody Icon").image as Texture2D;
            if (_colliderIconUnity == null) _colliderIconUnity = EditorGUIUtility.IconContent("d_BoxCollider Icon").image as Texture2D;

            _customPropsIcon = _mainElement.Q<VisualElement>("custom-props-icon-pencil");
            _customPropsIcon.SetBackgroundTexture(_customPropsIconUnity);
            _customPropsIcon.SetActive(false);

            _colliderIconElement = _mainElement.Q<VisualElement>("icon-collider");
            _colliderIconElement.SetBackgroundTexture(_colliderIconUnity);
            _colliderIconElement.SetActive(false);

            LoadIcon();

            _nameLabel = _mainElement.Q<Label>("name");
            _nameLabel.text = GetName();

            _deleteButton = _mainElement.Q<Button>("remove");
            _deleteButton.RegisterCallback<ClickEvent>(OnDeleteButton);
            _deleteButton.SetActive(false);

            _mainElement.RegisterCallback<MouseEnterEvent>(OnMouseEnter);
            _mainElement.RegisterCallback<MouseLeaveEvent>(OnMouseLeave);
            _mainElement.SetBorderColor(Color.clear);

            _mainElement.RegisterCallback<ContextClickEvent>(OnContextClick);

            _rigidbodyIconElement = _mainElement.Q<VisualElement>("rigidbody-icon");
            _rigidbodyIconElement.SetBackgroundTexture(_rigidbodyIconUnity);

            if (selected) Select();

            UpdateUIPrefab();
        }

        public void UpdateUIPrefab()
        {
            bool hasCustomProps = customProps != null && customProps.HasAnyChange();
            _customPropsIcon.SetActive(hasCustomProps);

            //name
            _nameLabel.text = GetName();

            //Rb
            bool hasRigidbody = PB_PhysicsSimulator.HasValidRigidbody(prefabToPaint);
            _rigidbodyIconElement.SetActive(hasRigidbody);

            //Collider
            bool hasCollider = PB_PhysicsSimulator.HasPhysicalCollider(prefabToPaint);
            _colliderIconElement.SetActive(hasCollider);

            //Tooltip
            string tooltip = prefabToPaint.name + "\n";
            if (hasCustomProps) tooltip += "\nHas custom properties";
            if (hasRigidbody) tooltip += "\nHas a Non Kinematic Rigidbody";
            if (hasCollider) tooltip += "\nHas a Physical Collider";

            _mainElement.tooltip = tooltip;
            _mainElement.SetBorderColor(selected ? Color.yellow : Color.clear);
            LoadIcon();
        }

        #region ContextMenu

        private void OnContextClick(ContextClickEvent evt)
        {
#if HARPIA_DEBUG
            Debug.Log($"[{nameof(PbPrefabUI)}] OnContextClick ");
#endif

            _contextSelected = this;
            PrefabBrush.PaintMode mode = PrefabBrush.instance.GetPaintMode();

            //Create the menu
            GenericMenu menu = new GenericMenu();

            //Add the options
            menu.AddItem(new GUIContent($"{GetName()}"), false, null);
            menu.AddItem(new GUIContent("Custom Props"), false, OnCustomSpawnProps);

            if (!selected)
                menu.AddItem(new GUIContent("Select this"), false, Select);
            else
                menu.AddItem(new GUIContent("Deselect"), false, Deselect);

            if (mode == PrefabBrush.PaintMode.Multiple)
            {
                menu.AddSeparator("");
                menu.AddItem(new GUIContent("Select All"), false, PB_MultipleModeManager.SelectAllPrefabs);
                menu.AddItem(new GUIContent("Deselect All"), false, PB_MultipleModeManager.DeselectAllPrefabs);
            }

            menu.AddSeparator("");
            menu.AddItem(new GUIContent("Rename"), false, OnRename);
            menu.AddItem(new GUIContent("Show Prefab In Project"), false, OnShowPrefab);
            //menu.AddItem(new GUIContent("Show Prefab In Inspector"), false, OnShowInInspector);

            if (PrefabBrushTool.IsPrefab(prefabToPaint))
                menu.AddItem(new GUIContent("Open Prefab"), false, OnOpenPrefab);

            menu.AddItem(new GUIContent("Remove From List"), false, OnRemoveFromList);

            menu.AddSeparator("");
            menu.AddItem(new GUIContent("Move Left"), false, onMoveLeft);
            menu.AddItem(new GUIContent("Move Right"), false, OnMoveRight);
            
            menu.AddSeparator("");
            menu.AddItem(new GUIContent("Reload Thumbnail"), false, OnReloadThumbnail);

            //Show the menu
            menu.ShowAsContext();
        }

        private void OnReloadThumbnail()
        {
#if HARPIA_DEBUG
            Debug.Log($"[{nameof(PbPrefabUI)}] On Reload Thumbnail");
#endif
            LoadIcon();
        }

        private void onMoveLeft()
        {
            PrefabBrush.instance.MovePrefabCard(this, true);
        }

        private void OnMoveRight()
        {
            PrefabBrush.instance.MovePrefabCard(this, false);
        }

        private void OnRename()
        {
#if HARPIA_DEBUG
            Debug.Log($"[{nameof(PbPrefabUI)}] On Rename");
#endif

            //Show input dialog
            string newName = PB_EditorInputDialog.Show("Rename Prefab", "Enter the new name:", GetName(), "Rename");

            if (string.IsNullOrEmpty(newName)) return;

            if (string.IsNullOrWhiteSpace(newName))
            {
                PrefabBrush.DisplayError("Invalid Name! Try another name.");
                return;
            }

            customProps ??= new PrefabAttributes();
            customProps.customName = newName;

            UpdateUIPrefab();
#if HARPIA_DEBUG
            Debug.Log($"[{nameof(PbPrefabUI)}]PrefabBrushPrefab OnRename {prefabToPaint.name} to {newName}");
#endif
        }

        public string GetName()
        {
            if (customProps == null) return prefabToPaint.name;

            if (string.IsNullOrEmpty(customProps.customName)) return prefabToPaint.name;
            return customProps.customName;
        }

        private void OnOpenPrefab()
        {
            string path = PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(prefabToPaint);
            GameObject asset = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            AssetDatabase.OpenAsset(asset);
            EditorApplication.ExecuteMenuItem("Window/General/Inspector");
        }

        private void OnShowInInspector()
        {
            Selection.activeObject = prefabToPaint;
            EditorApplication.ExecuteMenuItem("Window/General/Inspector");
        }

        private void OnCustomSpawnProps()
        {
            CustomPrefabProps.Show(this);
        }

        private void OnRemoveFromList()
        {
            OnDeleteButton(null);
        }

        private void OnShowPrefab()
        {
#if HARPIA_DEBUG
            Debug.Log("[Prefab Brush] Clicked on show prefab in project");
#endif

            if (_contextSelected == null) return;
            if (_contextSelected.prefabToPaint == null) return;

            EditorGUIUtility.PingObject(_contextSelected.prefabToPaint);

            string path = AssetDatabase.GetAssetPath(_contextSelected.prefabToPaint);
            path = Path.GetDirectoryName(path);
            PB_FolderUtils.ShowFolder(path);
        }

        #endregion

        private void OnPrefabClick(ClickEvent evt)
        {
#if HARPIA_DEBUG
            Debug.Log($"[{nameof(PbPrefabUI)}] PrefabBrushPrefab OnPrefabClick is selected {selected} {PrefabBrush.instance.GetPaintMode()}");
#endif

            if (PrefabBrush.instance.GetPaintMode() == PrefabBrush.PaintMode.Precision) _lastPrefabClickedPrecision = this;
            UpdateUIPrefab();

            switch (PrefabBrush.instance.GetPaintMode())
            {
                case PrefabBrush.PaintMode.Precision:
                    SetSelectedCard(this);
                    PB_PrecisionModeManager.SetPrefabToPaint(this);
                    break;

                case PrefabBrush.PaintMode.Multiple:
                    if (selected) Deselect();
                    else Select();
                    break;
            }

            if (!PrefabBrushTool.isUsingTool) PrefabBrush.instance.OnStartButton(null);
        }

        private static void SetSelectedCard(PbPrefabUI newValue)
        {
            PrefabBrush.PaintMode mode = PrefabBrush.instance.GetPaintMode();

            if (mode == PrefabBrush.PaintMode.Multiple)
            {
                newValue.selected = true;
                newValue.UpdateUIPrefab();
                return;
            }

            //Precision Mode
            PB_MultipleModeManager.DeselectAllPrefabs();
            _lastSelectedCard = newValue;
            _lastSelectedCard.selected = true;
            _lastSelectedCard.UpdateUIPrefab();
        }

        private void OnMouseLeave(MouseLeaveEvent evt)
        {
            _deleteButton.SetActive(false);
            _mainElement.SetBackgroundColor(Color.clear);
        }

        private void OnMouseEnter(MouseEnterEvent evt)
        {
            _deleteButton.SetActive(true);
            _mainElement.SetBackgroundColor(new Color(0f, 0f, 0f, 0.39f));
        }

        private async Task LoadThumbnail(VisualElement target)
        {
            void SetThumb(Texture2D t)
            {
                if (t == null) return;
                target.SetBackgroundTexture(t);
            }

            int instanceID = prefabToPaint.GetInstanceID();

            if (_defaultThumbUnity == null)
            {
                AssetPreview.SetPreviewTextureCacheSize(100);
                _defaultThumbUnity = AssetPreview.GetMiniTypeThumbnail(typeof(GameObject));
            }

            SetThumb(_defaultThumbUnity);

            AssetPreview.GetAssetPreview(prefabToPaint);

            while (AssetPreview.IsLoadingAssetPreview(instanceID)) await Task.Delay(350);

            loadedIcon = AssetPreview.GetAssetPreview(prefabToPaint);
            SetThumb(loadedIcon);
        }

        private void OnDeleteButton(ClickEvent evt)
        {
            PrefabBrush.Remove(this);
            _parentElement.Remove(_mainElement);
        }

        public string GetGuid()
        {
            if (prefabToPaint == null) return "";

            string path = AssetDatabase.GetAssetPath(prefabToPaint);
            return AssetDatabase.AssetPathToGUID(path);
        }

        public static void Dispose()
        {
            if (_lastSelectedCard != null && PrefabBrush.instance.GetPaintMode() == PrefabBrush.PaintMode.Precision)
                _lastSelectedCard.Deselect();

            _lastSelectedCard = null;
        }

        public void Select()
        {
            SetSelectedCard(this);
        }

        public static PbPrefabUI GetSelectedPrefab()
        {
            if (_lastSelectedCard != null) return _lastSelectedCard;
            if (_lastPrefabClickedPrecision != null) return _lastPrefabClickedPrecision;
            return null;
        }

        public static void Reload(List<PbPrefabUI> prefabs)
        {
            foreach (PbPrefabUI prefab in prefabs)
            {
                prefab.UpdateUIPrefab();
            }
        }

        public static bool HasAnySelected()
        {
            return PrefabBrush.instance.currentPrefabs.Any(e => e.selected);
        }

        public static void SelectFirst()
        {
            if (_lastPrefabClickedPrecision != null)
            {
                _lastPrefabClickedPrecision.Select();
                return;
            }

            List<PbPrefabUI> list = PrefabBrush.instance.currentPrefabs;
            if (list.Count == 0)
            {
                Debug.LogError($"{PrefabBrush.DebugLogStart} Cannot select first because there's no prefab");
                return;
            }

            PB_MultipleModeManager.DeselectAllPrefabs();

            list[0].Select();
        }

        public void Deselect()
        {
            selected = false;
            UpdateUIPrefab();
        }

        public static void SelectAll()
        {
            foreach (PbPrefabUI prefab in PrefabBrush.instance.currentPrefabs)
                prefab.Select();
        }

        public void SetBackgroundColor(Color c)
        {
            _mainElement.SetBackgroundColor(c);
        }

        public bool AllowsPhysicsPlacement()
        {
            if (prefabToPaint == null) return false;
            if (!PB_PhysicsSimulator.HasValidRigidbody(prefabToPaint)) return false;
            if (!PB_PhysicsSimulator.HasPhysicalCollider(prefabToPaint)) return false;
            return true;
        }
    }

    [Serializable]
    public class PrefabAttributes
    {
        public string customName;
        public bool useCustomPivotMode;
        public PrefabBrush.PivotMode pivotMode;

        public bool useCustomOffset;

        public PrefabBrush.Vector3Mode offsetMode;
        public Vector3 offsetMinValue = Vector3.one * -.2f;
        public Vector3 offsetMaxValue = Vector3.one * 0.2f;
        public Vector3 offsetFixedValue = Vector3.zero;

        public bool useCustomRotationMode;
        public PrefabBrush.Vector3Mode rotationMode;
        public Vector3 rotationMinValue = Vector3.zero;
        public Vector3 rotationMaxValue = Vector3.up * 360f;
        public Vector3 fixedRotation = Vector3.zero;

        public bool useCustomScale;

        public PrefabBrush.Vector3ModeUniform scaleMode;
        public Vector3 scaleMinValue = Vector3.one * 0.8f;
        public Vector3 scaleMaxValue = Vector3.one * 1.2f;

        public float scaleMaxUniformValue = 1;
        public float scaleMinUniformValue = 1;

        public Vector3 fixedScale = Vector3.one;

        private Vector3 _currentRandomScale;
        private Vector3 _currentRandomRotation;
        private Vector3 _currentRandomOffset;

        public bool HasAnyChange()
        {
            return useCustomOffset || useCustomPivotMode || useCustomRotationMode || useCustomScale;
        }

        public void SetScale(Vector3ModeElement e)
        {
            useCustomScale = e.useToggle?.value ?? false;
            scaleMode = (PrefabBrush.Vector3ModeUniform)e.enumField.value;
            scaleMinValue = e.minField.value;
            scaleMaxValue = e.maxField.value;
            fixedScale = e.fixedField.value;

            scaleMaxUniformValue = e.maxFieldUniform.value.x;
            scaleMinUniformValue = e.minFieldUniform.value.x;
        }

        public void SetRotation(Vector3ModeElement e)
        {
            useCustomRotationMode = e.useToggle?.value ?? false;
            rotationMode = (PrefabBrush.Vector3Mode)e.enumField.value;
            rotationMinValue = e.minField.value;
            rotationMaxValue = e.maxField.value;
            fixedRotation = e.fixedField.value;
        }

        public void SetOffset(Vector3ModeElement e)
        {
            useCustomOffset = e.useToggle?.value ?? false;
            offsetMode = (PrefabBrush.Vector3Mode)e.enumField.value;
            offsetMinValue = e.minField.value;
            offsetMaxValue = e.maxField.value;
            offsetFixedValue = e.fixedField.value;
        }

        public void SetPivotMode(bool value, PrefabBrush.PivotMode p)
        {
            useCustomPivotMode = value;
            pivotMode = p;
        }

        public Vector3 GetOffset()
        {
            if (offsetMode == PrefabBrush.Vector3Mode.Fixed) return offsetFixedValue;

            if (_currentRandomOffset != Vector3.zero) return _currentRandomOffset;
            _currentRandomOffset = GetRandomVec3(offsetMinValue, offsetMaxValue);

            return _currentRandomOffset;
        }

        public Vector3 GetRotation()
        {
            if (rotationMode == PrefabBrush.Vector3Mode.Fixed) return fixedRotation;

            if (_currentRandomRotation != Vector3.zero) return _currentRandomRotation;
            _currentRandomRotation = GetRandomVec3(rotationMinValue, rotationMaxValue);

            return _currentRandomRotation;
        }

        public Vector3 GetScale()
        {
            if (scaleMode == PrefabBrush.Vector3ModeUniform.Fixed) return fixedScale;

            if (scaleMode == PrefabBrush.Vector3ModeUniform.Random_Uniform)
            {
                return Vector3.one * Random.Range(scaleMinUniformValue, scaleMaxUniformValue);
            }

            if (_currentRandomScale != Vector3.zero) return _currentRandomScale;
            _currentRandomScale = GetRandomVec3(scaleMinValue, scaleMaxValue);

            return _currentRandomScale;
        }

        private Vector3 GetRandomVec3(Vector3 min, Vector3 max)
        {
            float x = Random.Range(min.x, max.x);
            float y = Random.Range(min.y, max.y);
            float z = Random.Range(min.z, max.z);
            return new Vector3(x, y, z);
        }

        public void Dispose()
        {
            _currentRandomOffset = Vector3.zero;
            _currentRandomRotation = Vector3.zero;
            _currentRandomScale = Vector3.zero;
        }

        public void AddToRotation(float value)
        {
            if (rotationMode == PrefabBrush.Vector3Mode.Fixed)
            {
                fixedRotation += Vector3.up * value;
                return;
            }

            _currentRandomRotation += Vector3.up * value;
        }
    }

    public static class CustomPrefabProps
    {
        private static Label labelName;

        private static VisualElement _customPropsPanel;
        private static VisualElement _iconStatic;

        private static Vector3ModeElement _offsetField;
        private static Vector3ModeElement _rotationField;
        private static Vector3ModeElement _scaleField;

        private static PbPrefabUI _selectedPbPrefabObj;
        private static Toggle _useCustomPivotToggle;
        private static EnumField _customPivotEnum;
        private static Button _prefabScaleButton;

        public static void Init(VisualElement root)
        {
            _customPropsPanel = root.Q<VisualElement>("custom-props-section");

            _iconStatic = root.Q<VisualElement>("custom-props-icon");
#if UNITY_2022_2_OR_NEWER
            _iconStatic.style.backgroundPositionY = new StyleBackgroundPosition(new BackgroundPosition(BackgroundPositionKeyword.Top));
#endif
            _iconStatic.SetBackgroundTexture(Texture2D.blackTexture);

            labelName = root.Q<Label>("custom-props-prefab-name");

            _offsetField = new Vector3ModeElement(root, "custom-props-offset-type", "custom-props-offset-fixed",
                "custom-props-offset-max", "custom-props-offset-min", "custom-props-offset-toggle", false);

            _rotationField = new Vector3ModeElement(root, "custom-props-rotation-type", "custom-props-rotation-fixed",
                "custom-props-rotation-max", "custom-props-rotation-min", "custom-props-rotation-toggle", false);

            _scaleField = new Vector3ModeElement(root, "custom-props-scale-type", "custom-props-scale-fixed",
                "custom-props-scale-max", "custom-props-scale-min", "custom-props-scale-toggle", true);

            _prefabScaleButton = _customPropsPanel.Q<Button>("custom-prop-scale-use-prefab");
            _prefabScaleButton.RegisterCallback<ClickEvent>(OnUsePrefabScale);

            _customPropsPanel.Q<Button>("custom-props-scale-multiply-max").RegisterCallback<ClickEvent>(_ => OnMultiplyScale(_scaleField.maxField));
            _customPropsPanel.Q<Button>("custom-props-scale-multiply-min").RegisterCallback<ClickEvent>(_ => OnMultiplyScale(_scaleField.minField));

            Button prefabRotationButton = _customPropsPanel.Q<Button>("custom-prop-rot-use-prefab");
            prefabRotationButton.RegisterCallback<ClickEvent>(OnUsePrefabRotation);

            root.Q<Button>("custom-props-back").RegisterCallback<ClickEvent>(OnBackClick);

            _useCustomPivotToggle = root.Q<Toggle>("custom-props-pivot-toggle");
            _customPivotEnum = root.Q<EnumField>("custom-props-pivot-type");
            _customPivotEnum.Init(PrefabBrush.PivotMode.MeshPivot);

            _useCustomPivotToggle.RegisterValueChangedCallback(evt =>
            {
                _customPivotEnum.SetActive(evt.newValue);
                if (_selectedPbPrefabObj == null) return;
                OnAnyChange();
            });

            _customPivotEnum.RegisterValueChangedCallback(_ =>
            {
                if (_selectedPbPrefabObj == null) return;
                OnAnyChange();
            });

            _scaleField.RegisterEventAll(OnAnyChange);
            _rotationField.RegisterEventAll(OnAnyChange);
            _offsetField.RegisterEventAll(OnAnyChange);

            _customPropsPanel.SetActive(false);
        }

        private static void OnMultiplyScale(Vector3Field scaleInput)
        {
#if HARPIA_DEBUG
            Debug.Log($"Clicked multiply max");
#endif
            Vector3 scale = _selectedPbPrefabObj.prefabToPaint.transform.lossyScale;

            if (scale.magnitude == 0)
            {
                PrefabBrush.DisplayError("Prefab Scale is 0");
                return;
            }

            float x = scale.x * scaleInput.value.x;
            float y = scale.y * scaleInput.value.y;
            float z = scale.z * scaleInput.value.z;
            scaleInput.value = new Vector3(x, y, z);
        }

        private static void OnUsePrefabRotation(ClickEvent evt)
        {
#if HARPIA_DEBUG
            Debug.Log($"Clicked use prefab rotation");
#endif

            _rotationField.fixedField.value = _selectedPbPrefabObj.prefabToPaint.transform.rotation.eulerAngles;
        }

        private static void OnUsePrefabScale(ClickEvent evt)
        {
#if HARPIA_DEBUG
            Debug.Log($"Clicked use prefab scale");
#endif

            _scaleField.fixedField.value = _selectedPbPrefabObj.prefabToPaint.transform.lossyScale;
        }

        private static void OnAnyChange()
        {
            if (_selectedPbPrefabObj == null) return;
            PrefabAttributes props = _selectedPbPrefabObj.customProps;

            props.SetRotation(_rotationField);
            props.SetOffset(_offsetField);
            props.SetScale(_scaleField);
            props.Dispose();

            props.SetPivotMode(_useCustomPivotToggle.value, (PrefabBrush.PivotMode)_customPivotEnum.value);

            _selectedPbPrefabObj.UpdateUIPrefab();
        }

        private static void OnBackClick(ClickEvent evt)
        {
            _customPropsPanel.SetActive(false);
            PrefabBrush.instance.paintModePanel.SetActive(true);
        }

        public static void Show(PbPrefabUI obj)
        {
#if HARPIA_DEBUG
            Debug.Log($"Showing custom props for {obj.prefabToPaint.name}", obj.prefabToPaint);
#endif

            _selectedPbPrefabObj = obj;
            if (obj.loadedIcon != null)
            {
                _iconStatic.SetBackgroundTexture(obj.loadedIcon);
                _iconStatic.SetActive(true);
                _iconStatic.SetVisible(true);
                _iconStatic.SetEnabled(true);
            }
            else
            {
                _iconStatic.SetActive(false);
            }

            PrefabBrush.instance.paintModePanel.SetActive(false);
            _customPropsPanel.SetActive(true);

            labelName.text = "Custom Properties: " + obj.GetName();

            PrefabAttributes p = obj.customProps;
            _offsetField.SetValues(p.useCustomOffset, (int)p.offsetMode, p.offsetMinValue, p.offsetMaxValue, p.offsetFixedValue, false);
            _rotationField.SetValues(p.useCustomRotationMode, (int)p.rotationMode, p.rotationMinValue, p.rotationMaxValue, p.fixedRotation, false);
            _scaleField.SetValues(p.useCustomScale, (int)p.scaleMode, p.scaleMinValue, p.scaleMaxValue, p.fixedScale, true);

            _useCustomPivotToggle.value = p.useCustomPivotMode;
            _customPivotEnum.value = p.pivotMode;
            _customPivotEnum.SetActive(p.useCustomPivotMode);
            _prefabScaleButton.tooltip = "Prefab Scale: " + _selectedPbPrefabObj.prefabToPaint.transform.lossyScale;
        }

        public static void Hide()
        {
            _customPropsPanel.SetActive(false);
            PrefabBrush.instance.paintModePanel.SetActive(true);
        }
    }

    public static class PB_TerrainHandler
    {
        private static Terrain lastTerrain;
        private static TerrainData lastTerrainData;
        private static int mapWidth;
        private static int mapHeight;

        private static float[,,] splatMapData;
        private static int numTextures;
        private static Texture[] terrainTextures;

        public static void ShowAlert()
        {
            if (Terrain.activeTerrain == null) return;
            Debug.LogWarning($"{PrefabBrush.DebugLogStart} Attention: " +
                             $"Since you have a terrain in your scene, ensure you select the 'Physical Raycast' mode for object placement on 'Raycast mode' dropdown ");
        }

        public static void Init()
        {
            if (!PrefabBrush.instance.isRaycastHit) return;

            if (Terrain.activeTerrain == null) return;

            if (lastTerrain != Terrain.activeTerrain)
                GetTerrainProps();
        }

        public static Texture GetTextureAtPosition(Vector3 pos)
        {
            if (Terrain.activeTerrain == null) return null;

            if (lastTerrain != Terrain.activeTerrain)
                GetTerrainProps();

            int index = GetTerrainAtPosition(pos);
            return terrainTextures[index];
        }

        private static void GetTerrainProps()
        {
            lastTerrain = Terrain.activeTerrain;
            lastTerrainData = lastTerrain.terrainData;
            mapWidth = lastTerrainData.alphamapWidth;
            mapHeight = lastTerrainData.alphamapHeight;

            splatMapData = lastTerrainData.GetAlphamaps(0, 0, mapWidth, mapHeight);
            numTextures = splatMapData.Length / (mapWidth * mapHeight);

            //get the textures
            terrainTextures = new Texture[lastTerrainData.terrainLayers.Length];

            for (int i = 0; i < lastTerrainData.terrainLayers.Length; i++)
            {
                terrainTextures[i] = lastTerrainData.terrainLayers[i].diffuseTexture;
            }
        }

        public static Texture[] GetTerrainTextures()
        {
            if (Terrain.activeTerrain == null) return null;

            if (lastTerrain != Terrain.activeTerrain)
                GetTerrainProps();

            if (terrainTextures == null || terrainTextures.Length == 0)
            {
                Debug.LogError($"{PrefabBrush.DebugLogStart} Terrain textures are null or empty.");
            }

            return terrainTextures;
        }

        public static void Dispose()
        {
            lastTerrain = null;
            lastTerrainData = null;
            splatMapData = null;
            numTextures = 0;
            mapWidth = 0;
            mapHeight = 0;
        }

        private static int GetTerrainAtPosition(Vector3 pos)
        {
            Vector3 terrainCoordinate = ConvertToSplatMapCoordinate(pos);
            int ret = 0;
            const float comp = 0f;
            for (int i = 0; i < numTextures; i++)
            {
                if (comp < splatMapData[(int)terrainCoordinate.z, (int)terrainCoordinate.x, i])
                    ret = i;
            }

            return ret;
        }

        private static Vector3 ConvertToSplatMapCoordinate(Vector3 playerPos)
        {
            Vector3 vecRet = new();

            Vector3 terPosition = lastTerrain.transform.position;
            vecRet.x = (playerPos.x - terPosition.x) / lastTerrainData.size.x * lastTerrainData.alphamapWidth;
            vecRet.z = (playerPos.z - terPosition.z) / lastTerrainData.size.z * lastTerrainData.alphamapHeight;
            return vecRet;
        }

        public static bool HasAnyTerrain()
        {
            return Terrain.activeTerrain != null;
        }
    }

    public static class PB_UndoManager
    {
        private static List<GameObject> _currentUndoList;

        private const string baseUndoMsg = "Prefab Brush - ";
        private const string undoMessage = baseUndoMsg + "Painted Game Objects";
        //private const string undoMessageTransform = baseUndoMsg + "Saved Transforms";

        public static void AddToUndo(GameObject o)
        {
            _currentUndoList ??= new List<GameObject>();
            _currentUndoList.Add(o);
        }

        public static void RegisterUndo()
        {
            if (_currentUndoList == null) return;
            if (_currentUndoList.Count == 0) return;

            Undo.RecordObject(_currentUndoList[0], undoMessage);
            int undoID = Undo.GetCurrentGroup();

            int undoCount = 0;
            foreach (GameObject go in _currentUndoList)
            {
                if (go == null) continue;
                Undo.RegisterCreatedObjectUndo(go, undoMessage);
                Undo.CollapseUndoOperations(undoID);
                undoCount++;
            }

            Undo.SetCurrentGroupName($"Prefab Brush Undo - Object Count:{undoCount} - ID: {undoID}");

#if HARPIA_DEBUG
            Debug.Log($"[{nameof(PB_UndoManager)}] RegisterUndo - {undoCount} objects - Undo ID {undoID}");
#endif

            Undo.FlushUndoRecordObjects();
            _currentUndoList.Clear();
        }

        public static bool IsUndoMessage(string msg)
        {
            return msg.Contains(baseUndoMsg);
        }

        public static void DestroyAndRegister(GameObject objectToEraser)
        {
            int undoID = Undo.GetCurrentGroup();
            Undo.DestroyObjectImmediate(objectToEraser);
            Undo.CollapseUndoOperations(undoID);
        }

        public static void RegisterUndoTransforms(IEnumerable<Rigidbody> selectedRigidbodies)
        {
            return;
            // Debug.Log($"Saving multiple transforms {selectedRigidbodies.Count()}");
            // foreach (Rigidbody rb in selectedRigidbodies)
            // {
            //     Undo.RecordObject(rb.transform, undoMessage);
            // }
            // Undo.FlushUndoRecordObjects();
        }

        public static void PerformUndo()
        {
            if(PrefabBrush.instance != null) PrefabBrush.instance.ExitTool();
            
#if HARPIA_DEBUG
            Debug.Log($"[{nameof(PB_UndoManager)}] Perform undo | Name: {Undo.GetCurrentGroupName()}");
#endif

#if UNITY_2022_2_OR_NEWER
            bool isPrecessing = Undo.isProcessing;
            if (isPrecessing) return;
#endif
            Debug.LogWarning($"{PrefabBrush.DebugLogStart} If undo does not work, exit the tool [Esc] and Undo [Ctrl + Z]. Some Unity Versions undo through code does not works. We are working to fix this");
            PB_HandlesExtension.WriteTempTextAtMousePos($"Performed Undo - {Undo.GetCurrentGroup()}", Color.white, 1);
            Undo.PerformUndo();
        }
    }

    public static class EditorPrefsExtension
    {
        public static void SaveColor(string key, Color c)
        {
            EditorPrefs.SetFloat(key + "_r", c.r);
            EditorPrefs.SetFloat(key + "_g", c.g);
            EditorPrefs.SetFloat(key + "_b", c.b);
            EditorPrefs.SetFloat(key + "_a", c.a);
        }

        public static Color GetColor(string key, Color defaultValue)
        {
            if (EditorPrefs.HasKey(key + "_r") == false) return defaultValue;

            float r = EditorPrefs.GetFloat(key + "_r");
            float g = EditorPrefs.GetFloat(key + "_g");
            float b = EditorPrefs.GetFloat(key + "_b");
            float a = EditorPrefs.GetFloat(key + "_a");

            return new Color(r, g, b, a);
        }

        public static void DeleteColorKey(string pbBrushBaseColor)
        {
            EditorPrefs.DeleteKey(pbBrushBaseColor + "_r");
            EditorPrefs.DeleteKey(pbBrushBaseColor + "_g");
            EditorPrefs.DeleteKey(pbBrushBaseColor + "_b");
            EditorPrefs.DeleteKey(pbBrushBaseColor + "_a");
        }
    }

    public class Vector3ModeElement
    {
        public readonly Toggle useToggle;
        public readonly EnumField enumField;
        public readonly Vector3Field fixedField;
        public readonly Vector3Field maxField;
        public readonly Vector3Field minField;
        public readonly Vector3Field minFieldUniform;
        public readonly Vector3Field maxFieldUniform;
        public bool proportionsFixed;
        public bool proportionsMin;
        public bool proportionsMax;
        public readonly bool allowUniform;

        private Button buttonFixedField;
        private Button buttonMaxField;
        private Button buttonMinField;

        private static Texture linkedIcon;
        private static Texture unlinkedIcon;
        private VisualElement _constrainedElementFixed;
        private VisualElement _constrainedElementMax;
        private VisualElement _constrainedElementMin;

        public Vector3ModeElement(VisualElement root, string enumField, string fixedField, string maxField, string minField, string useToggle, bool allowUniform)
        {
            this.enumField = root.Q<EnumField>(enumField);
            this.fixedField = root.Q<Vector3Field>(fixedField);
            this.maxField = root.Q<Vector3Field>(maxField);
            this.minField = root.Q<Vector3Field>(minField);

            this.allowUniform = allowUniform;

            if (this.allowUniform)
            {
                string maxFieldName = maxField + "-uniform";
                string minFieldName = minField + "-uniform";

                maxFieldUniform = root.Q<Vector3Field>(maxFieldName);
                minFieldUniform = root.Q<Vector3Field>(minFieldName);

#if HARPIA_DEBUG
                if (maxFieldUniform == null) Debug.LogError($"Could not find field {maxFieldName}");
                if (minFieldUniform == null) Debug.LogError($"Could not find field {minFieldName}");
#endif

                minFieldUniform.RegisterValueChangedCallback(OnUniformFieldChanged);
                maxFieldUniform.RegisterValueChangedCallback(OnUniformFieldChanged);
            }

            if (!string.IsNullOrEmpty(useToggle))
                this.useToggle = root.Q<Toggle>(useToggle);

            this.enumField.Init(this.allowUniform ? PrefabBrush.Vector3ModeUniform.Fixed : PrefabBrush.Vector3Mode.Fixed);
            this.enumField.RegisterValueChangedCallback(OnEnumValueChanged);
            this.useToggle?.RegisterValueChangedCallback(OnUseToggleValueChanged);

            OnUseToggleValueChanged(null);
            UpdateUIVec3Element();

            AddManipulators(this.fixedField);
            AddManipulators(this.minField);
            AddManipulators(this.maxField);
            AddManipulators(this.maxFieldUniform);
            AddManipulators(this.minFieldUniform);
        }

        private void OnUniformFieldChanged(ChangeEvent<Vector3> evt)
        {
#if HARPIA_DEBUG
            Debug.Log($"{PrefabBrush.DebugLogStart} Changed uniform field");
#endif

            Vector3 old = evt.previousValue;
            Vector3 newValue = evt.newValue;
            Vector3Field field = (Vector3Field)evt.target;

            //Z
            if (old.x != newValue.x)
            {
                field.SetValueWithoutNotify(Vector3.one * newValue.x);
                PB_PrecisionModeManager.UpdateTransformValues();
                return;
            }

            //Y
            if (old.y != newValue.y)
            {
                field.SetValueWithoutNotify(Vector3.one * newValue.y);
                PB_PrecisionModeManager.UpdateTransformValues();
                return;
            }

            //z
            field.SetValueWithoutNotify(Vector3.one * newValue.z);
            PB_PrecisionModeManager.UpdateTransformValues();
        }

        private void AddManipulators(Vector3Field f)
        {
            if (f == null) return;
            f.AddManipulator(new ContextualMenuManipulator(evt => { evt.menu.AppendAction("Zero (0,0,0)", _ => f.value = Vector3.zero, DropdownMenuAction.AlwaysEnabled); }));
            f.AddManipulator(new ContextualMenuManipulator(evt => { evt.menu.AppendAction("One (1,1,1)", _ => f.value = Vector3.one, DropdownMenuAction.AlwaysEnabled); }));
        }

        void UpdateConstrainedUI(VisualElement e, bool value)
        {
            const string tooltipEnable = "Enable Constrained Proportions";
            const string tooltipDisable = "Disable Constrained Proportions";

            if (linkedIcon == null)
            {
                linkedIcon = EditorGUIUtility.IconContent("d_Linked").image;
                unlinkedIcon = EditorGUIUtility.IconContent("d_Unlinked").image;
            }

            e.SetBackgroundTexture(value ? linkedIcon : unlinkedIcon);
            e.tooltip = value ? tooltipDisable : tooltipEnable;
        }

        public void AddProportions(string elementNameFixed, string elementNameMax, string elementNameMin, VisualElement root)
        {
            _constrainedElementFixed = root.Q<VisualElement>(elementNameFixed);
            UpdateConstrainedUI(_constrainedElementFixed, proportionsFixed);

            _constrainedElementMax = root.Q<VisualElement>(elementNameMax);
            UpdateConstrainedUI(_constrainedElementMax, proportionsMax);

            _constrainedElementMin = root.Q<VisualElement>(elementNameMin);
            UpdateConstrainedUI(_constrainedElementMin, proportionsMin);

            _constrainedElementFixed.RegisterCallback<ClickEvent>(_ =>
            {
                proportionsFixed = !proportionsFixed;
                UpdateConstrainedUI(_constrainedElementFixed, proportionsFixed);
            });

            _constrainedElementMax.RegisterCallback<ClickEvent>(_ =>
            {
                proportionsMax = !proportionsMax;
                UpdateConstrainedUI(_constrainedElementMax, proportionsMax);
            });

            _constrainedElementMin.RegisterCallback<ClickEvent>(_ =>
            {
                proportionsMin = !proportionsMin;
                UpdateConstrainedUI(_constrainedElementMin, proportionsMin);
            });

            fixedField.RegisterValueChangedCallback(evt =>
            {
                if (!proportionsFixed) return;
                fixedField.SetValueWithoutNotify(GetConstrainedValue(evt.previousValue, evt.newValue));
            });

            maxField.RegisterValueChangedCallback(evt =>
            {
                if (!proportionsMax) return;
                maxField.SetValueWithoutNotify(GetConstrainedValue(evt.previousValue, evt.newValue));
            });

            minField.RegisterValueChangedCallback(evt =>
            {
                if (!proportionsMin) return;
                minField.SetValueWithoutNotify(GetConstrainedValue(evt.previousValue, evt.newValue));
            });

            Vector3 GetConstrainedValue(Vector3 oldValue, Vector3 newValue)
            {
                bool isX = oldValue.x != newValue.x;
                bool isY = oldValue.y != newValue.y;
                bool isZ = oldValue.z != newValue.z;
                if (isX && isY && isZ) return newValue;

                float increase = 1;

                if (isX) increase = newValue.x / oldValue.x;
                if (isY) increase = newValue.y / oldValue.y;
                if (isZ)
                {
                    increase = newValue.z / oldValue.z;
                }

                if (isX) newValue = new Vector3(newValue.x, newValue.y * increase, newValue.z * increase);
                if (isY) newValue = new Vector3(newValue.x * increase, newValue.y, newValue.z * increase);
                if (isZ) newValue = new Vector3(newValue.x * increase, newValue.y * increase, newValue.z);

                if (float.IsNaN(newValue.x) || float.IsInfinity(newValue.x)) return oldValue;
                if (float.IsNaN(newValue.y) || float.IsInfinity(newValue.y)) return oldValue;
                if (float.IsNaN(newValue.z) || float.IsInfinity(newValue.z)) return oldValue;

                const float round = 0.001f;
                newValue = new Vector3(PrefabBrushTool.RoundTo(newValue.x, round), PrefabBrushTool.RoundTo(newValue.y, round), PrefabBrushTool.RoundTo(newValue.z, round));

                return newValue;
            }
        }

        public void SetButtons(Button fixedButton, Button minButton, Button maxButton)
        {
            buttonFixedField = fixedButton;
            buttonMaxField = maxButton;
            buttonMinField = minButton;

            OnUseToggleValueChanged(null);
            UpdateUIVec3Element();
        }

        private void OnUseToggleValueChanged(ChangeEvent<bool> evt)
        {
            bool value = useToggle?.value ?? true;
            enumField.SetActive(value);
            UpdateUIVec3Element();
        }

        private void OnEnumValueChanged(ChangeEvent<Enum> evt)
        {
#if HARPIA_DEBUG
            // Debug.Log($"{PrefabBrush.DebugLogStart} {nameof(Vector3ModeElement)}.{nameof(OnEnumValueChanged)} ");
#endif

            bool toggleValue = useToggle?.value ?? true;

            int modeInt = Convert.ToInt32(enumField.value);

            enumField.SetActive(toggleValue);

            bool enableFixed = modeInt == (int)PrefabBrush.Vector3Mode.Fixed && toggleValue;
            fixedField.SetActive(enableFixed);
            buttonFixedField?.SetActive(enableFixed);

            bool enableMax = modeInt == (int)PrefabBrush.Vector3Mode.Random && toggleValue;
            maxField.SetActive(enableMax);
            buttonMaxField?.SetActive(enableMax);

            bool enableMin = modeInt == (int)PrefabBrush.Vector3Mode.Random && toggleValue;
            minField.SetActive(enableMin);
            buttonMinField?.SetActive(enableMin);

            if (allowUniform)
            {
                bool enableUniform = modeInt == 2 && toggleValue;
                maxFieldUniform.SetActive(enableUniform);
                minFieldUniform.SetActive(enableUniform);
            }
        }

        public Vector3 GetValue()
        {
            int mode = Convert.ToInt16(enumField.value);

            switch (mode)
            {
                //Fixed
                case (int)PrefabBrush.Vector3Mode.Fixed:
                    return fixedField.value;
                //Random
                case (int)PrefabBrush.Vector3ModeUniform.Random:
                {
                    Vector3 min = minField.value;
                    Vector3 max = maxField.value;
                    return new Vector3(Random.Range(min.x, max.x), Random.Range(min.y, max.y), Random.Range(min.z, max.z));
                }
                default:
                {
                    //Random Uniform
                    float randomValue = Random.Range(minFieldUniform.value.x, maxFieldUniform.value.x);
                    return new Vector3(randomValue, randomValue, randomValue);
                }
            }
        }

        public Vector3 GetValueFixed()
        {
            return fixedField.value;
        }

        public void RegisterFocusEvents()
        {
            minField.RegisterFocusEvents();
            maxField.RegisterFocusEvents();
            fixedField.RegisterFocusEvents();
        }

        public void TrySetValue(PrefabBrush.Vector3Mode desiredMode, Vector3 newVal)
        {
            PrefabBrush.Vector3Mode current = (PrefabBrush.Vector3Mode)enumField.value;
            if (current != desiredMode) return;
            fixedField.SetValueWithoutNotify(newVal);
        }

        public void SetValues(bool isUsing, int mode, Vector3 minValue, Vector3 maxValue, Vector3 fixedValue, bool useUniform)
        {
            minField.SetValueWithoutNotify(minValue);
            maxField.SetValueWithoutNotify(maxValue);
            fixedField.SetValueWithoutNotify(fixedValue);
            enumField.SetValueWithoutNotify(useUniform ? (PrefabBrush.Vector3ModeUniform)mode : (PrefabBrush.Vector3Mode)mode);
            if (useToggle != null) useToggle.SetValueWithoutNotify(isUsing);

            UpdateUIVec3Element();
        }

        public void SetValueFixed(Vector3 newValue, bool changeModeToFixed)
        {
            fixedField.SetValueWithoutNotify(newValue * PB_AdvancedSettings.scrollRotationSpeed.value);
            if (changeModeToFixed) enumField.value = PrefabBrush.Vector3Mode.Fixed;
        }

        public void RegisterEventAll(Action action)
        {
            enumField.RegisterValueChangedCallback((_) => action());
            minField.RegisterValueChangedCallback((_) => action());
            maxField.RegisterValueChangedCallback((_) => action());
            fixedField.RegisterValueChangedCallback((_) => action());
            useToggle?.RegisterValueChangedCallback((_) => action());
        }

        public PrefabBrush.Vector3Mode GetMode()
        {
            return (PrefabBrush.Vector3Mode)enumField.value;
        }

        public void SetActive(bool b)
        {
            if (b)
            {
                enumField.SetActive(true);
                useToggle?.SetActive(true);
                UpdateUIVec3Element();
                return;
            }

            minField.SetActive(false);
            maxField.SetActive(false);
            fixedField.SetActive(false);
            enumField.SetActive(false);
            useToggle?.SetActive(false);

            buttonFixedField?.SetActive(false);
            buttonMaxField?.SetActive(false);
            buttonMinField?.SetActive(false);
        }

        public void UpdateUIVec3Element()
        {
            OnEnumValueChanged(null);
        }

        public void SetConstrainedProportions(bool constrainedScaleFixed, bool constrainedScaleMin, bool constrainedScaleMax)
        {
            proportionsFixed = constrainedScaleFixed;
            proportionsMin = constrainedScaleMin;
            proportionsMax = constrainedScaleMax;

            UpdateConstrainedUI(_constrainedElementFixed, proportionsFixed);
            UpdateConstrainedUI(_constrainedElementMax, proportionsMax);
            UpdateConstrainedUI(_constrainedElementMin, proportionsMin);
        }
    }

    public static class PB_AdvancedSettings
    {
        public static ColorField brushBorderColor;
        public static ColorField brushBaseColor;
        public static ColorField invalidLocationColor;
        public static ColorField gridColor;
        public static ColorField eraserColor;

        private static VisualElement settingsPanel;
        private static Color originalInvalidLocationColor1;
        private static Color originalGridColor1;
        private static Color originalEraserColor1;
        private static Color originalBrushBorderColor1;
        private static Color originalBrushBaseColor1;
        public static FloatField scrollRotationSpeed;

        public static void Init(VisualElement root)
        {
            settingsPanel = root.Q("settings-section");

            brushBaseColor = root.Q<ColorField>("settings-brush-color");
            brushBorderColor = root.Q<ColorField>("settings-brush-border-color");
            eraserColor = root.Q<ColorField>("settings-eraser-color");
            gridColor = root.Q<ColorField>("settings-grid-color");
            invalidLocationColor = root.Q<ColorField>("settings-invalid-location-color");

            originalBrushBaseColor1 = brushBaseColor.value;
            originalBrushBorderColor1 = brushBorderColor.value;
            originalEraserColor1 = eraserColor.value;
            originalGridColor1 = gridColor.value;
            originalInvalidLocationColor1 = invalidLocationColor.value;

            brushBaseColor.RegisterFocusEvents();
            brushBorderColor.RegisterFocusEvents();
            eraserColor.RegisterFocusEvents();
            gridColor.RegisterFocusEvents();
            invalidLocationColor.RegisterFocusEvents();

            FloatField maxBrushRadius = root.Q<FloatField>("settings-max-brush-radius");
            FloatField maxEraserRadius = root.Q<FloatField>("settings-max-eraser-radius");
            FloatField maxImpulse = root.Q<FloatField>("settings-max-impulse");

            scrollRotationSpeed = root.Q<FloatField>("settings-scroll-rotation");
            scrollRotationSpeed.RegisterEditorPrefs("PB", 1);

            maxBrushRadius.value = PrefabBrush.instance.brushRadiusSlider.highValue;
            maxEraserRadius.value = PrefabBrush.instance.eraserRadiusSlider.highValue;
            maxImpulse.value = PB_PhysicsSimulator.impulseForceSlider.highValue;

            RegisterEvent(maxBrushRadius, PrefabBrush.instance.brushRadiusSlider);
            RegisterEvent(maxEraserRadius, PrefabBrush.instance.eraserRadiusSlider);
            RegisterEvent(maxImpulse, PB_PhysicsSimulator.impulseForceSlider);

            brushBaseColor.value = EditorPrefsExtension.GetColor("pb_brush_base_color", brushBaseColor.value);
            brushBorderColor.value = EditorPrefsExtension.GetColor("pb_brush_border_color", brushBorderColor.value);
            eraserColor.value = EditorPrefsExtension.GetColor("pb_eraser_color", eraserColor.value);
            gridColor.value = EditorPrefsExtension.GetColor("pb_grid_color", gridColor.value);
            invalidLocationColor.value = EditorPrefsExtension.GetColor("pb_invalid_location_color", invalidLocationColor.value);

            maxBrushRadius.value = EditorPrefs.GetFloat("pb_max_brush_radius", maxBrushRadius.value);
            maxEraserRadius.value = EditorPrefs.GetFloat("pb_max_eraser_radius", maxEraserRadius.value);
            maxImpulse.value = EditorPrefs.GetFloat("pb_max_impulse", maxImpulse.value);

            root.Q<Button>("settings-reset").RegisterCallback<ClickEvent>(OnResetButton);
            SetActive(false);
        }

        public static void SetActive(bool n)
        {
            settingsPanel.SetActive(n);
        }

        private static void OnResetButton(ClickEvent evt)
        {
#if HARPIA_DEBUG
            Debug.Log("Resetting PB settings");
#endif
            //show confirmation dialog
            bool r = EditorUtility.DisplayDialog("Reset Prefab Brush Settings", "Are you sure you want to reset all Prefab Brush settings to their default values?", "Yes", "No");

            if (!r) return;

            DeleteKeys();

            brushBaseColor.value = originalBrushBaseColor1;
            brushBorderColor.value = originalBrushBorderColor1;
            eraserColor.value = originalEraserColor1;
            gridColor.value = originalGridColor1;
            invalidLocationColor.value = originalInvalidLocationColor1;
        }

        public static void SaveValues()
        {
            EditorPrefsExtension.SaveColor("pb_brush_base_color", brushBaseColor.value);
            EditorPrefsExtension.SaveColor("pb_brush_border_color", brushBorderColor.value);
            EditorPrefsExtension.SaveColor("pb_eraser_color", eraserColor.value);
            EditorPrefsExtension.SaveColor("pb_grid_color", gridColor.value);
            EditorPrefsExtension.SaveColor("pb_invalid_location_color", invalidLocationColor.value);

            EditorPrefs.SetFloat("pb_max_brush_radius", PrefabBrush.instance.brushRadiusSlider.highValue);
            EditorPrefs.SetFloat("pb_max_eraser_radius", PrefabBrush.instance.eraserRadiusSlider.highValue);
            EditorPrefs.SetFloat("pb_max_impulse", PB_PhysicsSimulator.impulseForceSlider.highValue);
        }

        private static void DeleteKeys()
        {
            EditorPrefsExtension.DeleteColorKey("pb_brush_base_color");
            EditorPrefsExtension.DeleteColorKey("pb_brush_border_color");
            EditorPrefsExtension.DeleteColorKey("pb_eraser_color");
            EditorPrefsExtension.DeleteColorKey("pb_grid_color");
            EditorPrefsExtension.DeleteColorKey("pb_invalid_location_color");

            EditorPrefs.DeleteKey("pb_max_brush_radius");
            EditorPrefs.DeleteKey("pb_max_eraser_radius");
            EditorPrefs.DeleteKey("pb_max_impulse");
        }

        static void RegisterEvent(FloatField baseField, Slider target)
        {
            baseField.RegisterValueChangedCallback((evt) =>
            {
                if (evt.newValue < .1f)
                {
                    baseField.SetValueWithoutNotify(.1f);
                    return;
                }

                target.highValue = evt.newValue;
                float v = Mathf.Clamp(target.value, target.lowValue, target.highValue);
                target.SetValueWithoutNotify(v);
            });
        }
    }

    public class PB_EditorInputDialog : EditorWindow
    {
        private string okButton;
        private string inputText;
        private string description;
        private string cancelButton;
        private bool initializedPosition;
        private Action onOKButton;

        private bool shouldClose;

        private void OnGUI()
        {
            Event e = Event.current;
            if (e.type == EventType.KeyDown)
            {
                switch (e.keyCode)
                {
                    case KeyCode.Escape:
                        shouldClose = true;
                        break;

                    case KeyCode.Return:
                    case KeyCode.KeypadEnter:
                        onOKButton?.Invoke();
                        shouldClose = true;
                        break;
                }
            }

            if (shouldClose)
            {
                Close();
            }

            Rect rect = EditorGUILayout.BeginVertical();

            EditorGUILayout.Space(12);
            EditorGUILayout.LabelField(description);

            EditorGUILayout.Space(8);
            GUI.SetNextControlName("inText");
            inputText = EditorGUILayout.TextField("", inputText);
            GUI.FocusControl("inText");
            EditorGUILayout.Space(12);

            Rect r = EditorGUILayout.GetControlRect();
            r.width /= 2;
            if (GUI.Button(r, okButton))
            {
                onOKButton?.Invoke();
                shouldClose = true;
            }

            r.x += r.width;
            if (GUI.Button(r, cancelButton))
            {
                inputText = null; // Cancel - delete inputText
                shouldClose = true;
            }

            EditorGUILayout.Space(8);
            EditorGUILayout.EndVertical();

            // Force change size of the window
            if (rect.width != 0 && minSize != rect.size) minSize = maxSize = rect.size;

            if (!initializedPosition)
            {
                Vector2 mousePos = GUIUtility.GUIToScreenPoint(Event.current.mousePosition);
                position = new Rect(mousePos.x + 32, mousePos.y, position.width, position.height);
                initializedPosition = true;
            }
        }

        public static string Show(string title, string description, string inputText, string okButton = "OK", string cancelButton = "Cancel")
        {
            string ret = null;
            PB_EditorInputDialog wnd = CreateInstance<PB_EditorInputDialog>();
            wnd.titleContent = new GUIContent(title);
            wnd.description = description;
            wnd.inputText = inputText;
            wnd.okButton = okButton;
            wnd.cancelButton = cancelButton;
            wnd.onOKButton += () => ret = wnd.inputText;
            wnd.ShowModal();

            return ret;
        }
    }

    public static class PB_MeshRaycaster
    {
        private static int kernelIndex;
        private static uint threadGroupSizeX;
        private static uint threadGroupSizeY;
        private static uint threadGroupSizeZ;

        private static ComputeShader raycastShader;
        private const string RaycastShaderName = "PrefabBrush_MeshRaycaster";

        private static ComputeBuffer trianglesBuffer;
        private static ComputeBuffer vertexBuffer;
        private static ComputeBuffer resultBufferHits;
        private static ComputeBuffer resultNormalsBuffer;
        private static float[] hitDistances;
        private static Vector3[] resultNormals;
        private static int sizeX1;
        private static readonly int Epsilon = Shader.PropertyToID("epsilon");
        private static readonly int WorldToLocalMatrix = Shader.PropertyToID("worldToLocalMatrix");
        private static readonly int VertexBuffer = Shader.PropertyToID("vertexBuffer");
        private static readonly int TriangleBuffer = Shader.PropertyToID("triangleBuffer");
        private static readonly int ResultHits = Shader.PropertyToID("resultHits");
        private static readonly int RayOrigin = Shader.PropertyToID("rayOrigin");
        private static readonly int RayDirection = Shader.PropertyToID("rayDirection");
        private static readonly int ResultNormals = Shader.PropertyToID("resultNormals");

        public static bool Raycast(Ray r, Mesh m, Matrix4x4 matrix, out MeshRaycastResult result)
        {
            Init();

            if (m.vertices.Length == 0)
            {
                result = new MeshRaycastResult();
                return false;
            }

#if HARPIA_DEBUG
            UnityEngine.Profiling.Profiler.BeginSample("Prefab Brush - Mesh Raycaster");
#endif

            //Inputs
            if (vertexBuffer == null)
            {
#if HARPIA_DEBUG
                Debug.Log($"[PB Mesh Raycaster] Initializing raycastShader - {m.vertices.Length} vertices");
#endif

                raycastShader.SetFloat(Epsilon, Mathf.Epsilon);
                raycastShader.SetMatrix(WorldToLocalMatrix, matrix);

                vertexBuffer = new ComputeBuffer(m.vertices.Length, sizeof(float) * 3);
                vertexBuffer.SetData(m.vertices);
                raycastShader.SetBuffer(kernelIndex, VertexBuffer, vertexBuffer);

                trianglesBuffer = new ComputeBuffer(m.triangles.Length, sizeof(int));
                trianglesBuffer.SetData(m.triangles);
                raycastShader.SetBuffer(kernelIndex, TriangleBuffer, trianglesBuffer);

                hitDistances = new float[m.triangles.Length / 3];
                resultBufferHits = new ComputeBuffer(hitDistances.Length, sizeof(float));
                resultBufferHits.SetData(hitDistances);
                raycastShader.SetBuffer(kernelIndex, ResultHits, resultBufferHits);

                resultNormals = new Vector3[hitDistances.Length];
                resultNormalsBuffer = new ComputeBuffer(hitDistances.Length, sizeof(float) * 3);
                resultNormalsBuffer.SetData(resultNormals);
                raycastShader.SetBuffer(kernelIndex, ResultNormals, resultNormalsBuffer);

                sizeX1 = Mathf.CeilToInt((hitDistances.Length / threadGroupSizeX) + 1);
            }

            raycastShader.SetVector(RayOrigin, r.origin);
            raycastShader.SetVector(RayDirection, r.direction);

            raycastShader.Dispatch(kernelIndex, sizeX1, (int)threadGroupSizeY, (int)threadGroupSizeZ);

            resultBufferHits.GetData(hitDistances);
            resultNormalsBuffer.GetData(resultNormals);

            float maxDistance = Mathf.Infinity;
            int resultIndex = -1;
            for (int index = hitDistances.Length - 1; index >= 0; index--)
            {
                float distance = hitDistances[index];
                if (distance >= 1_000_000) continue;
                if (distance < maxDistance)
                {
                    maxDistance = distance;
                    resultIndex = index;
                }
            }

            //No collision ray
            if (resultIndex == -1)
            {
                result = new MeshRaycastResult();
                return false;
            }

            Vector3 pos = r.origin + r.direction * maxDistance;

            result = new MeshRaycastResult(pos, resultNormals[resultIndex], maxDistance);

#if HARPIA_DEBUG
            UnityEngine.Profiling.Profiler.EndSample();
#endif

            return true;
        }

        public static void Dispose()
        {
            trianglesBuffer?.Dispose();
            vertexBuffer?.Dispose();
            resultBufferHits?.Dispose();
            resultNormalsBuffer?.Dispose();

            hitDistances = null;
            trianglesBuffer = null;
            vertexBuffer = null;
            resultBufferHits = null;
            resultNormalsBuffer = null;
        }

        private static void Init()
        {
            if (raycastShader != null) return;

            FindShader();
            kernelIndex = raycastShader.FindKernel("MeshRaycastCS");
            raycastShader.GetKernelThreadGroupSizes(kernelIndex, out threadGroupSizeX, out threadGroupSizeY, out threadGroupSizeZ);
        }

        static void FindShader()
        {
            //find a asset called raycastShaderName
            string[] guids = AssetDatabase.FindAssets(RaycastShaderName);

            if (guids.Length == 0)
            {
                Debug.LogError($"{PrefabBrush.DebugLogStart} Mesh Raycaster shader not found");
                return;
            }

            string path = AssetDatabase.GUIDToAssetPath(guids[0]);
            raycastShader = AssetDatabase.LoadAssetAtPath<ComputeShader>(path);
        }
    }

    public readonly struct MeshRaycastResult
    {
        private readonly Vector3 position;
        private readonly Vector3 normal;
        private readonly float distance;

        //constructor
        public MeshRaycastResult(Vector3 position, Vector3 normal, float distance)
        {
            this.position = position;
            this.normal = normal;
            this.distance = distance;
        }

        public RaycastHit ToHitInfo()
        {
            RaycastHit hit = new RaycastHit
            {
                point = position,
                normal = normal,
                distance = distance
            };
            return hit;
        }
    }

    public static class PB_PressurePen
    {
        public enum PenPressureUseMode
        {
            Affect_Brush_Strength,
            Affect_Brush_Radius,
        }

        private static EnumField penPressureMode;
        private static Toggle usePressureToggle;

        private const string usePressureKey = "PB-use-pressure";
        private const string penPressureModeKey = "PB-pressure-mode";

        private static Vector2 radiusMinMax;
        private static Vector2 strengthMinMax;
        private static float oldStrength;
        private static float oldBrushRadius;

        public static void Init(VisualElement root)
        {
            usePressureToggle = root.Q<Toggle>("use-pressure-toggle");
            penPressureMode = root.Q<EnumField>("pen-pressure-mode");

            Label msg = root.Q<Label>("pen-pressure-message");

#if !UNITY_2022_1_OR_NEWER
            penPressureMode.SetActive(false);
            usePressureToggle.SetActive(false);
            return;
#else
            msg.SetActive(false);

            usePressureToggle.value = EditorPrefs.GetBool(usePressureKey, false);
            usePressureToggle.RegisterValueChangedCallback(OnToggleChanged);
            usePressureToggle.SetActive(true);

            penPressureMode.Init(PenPressureUseMode.Affect_Brush_Radius);
            penPressureMode.SetValueWithoutNotify((PenPressureUseMode)EditorPrefs.GetInt(penPressureModeKey, (int)PenPressureUseMode.Affect_Brush_Strength));
            penPressureMode.RegisterValueChangedCallback(OnDropdownChanged);

            Slider sliderRadius = PrefabBrush.instance.brushRadiusSlider;
            Slider strengthSlider = PrefabBrush.instance.sliderBrushStrength;

            radiusMinMax = new Vector2(sliderRadius.lowValue, sliderRadius.highValue);
            strengthMinMax = new Vector2(strengthSlider.lowValue, strengthSlider.highValue);
#endif
        }

        private static void OnDropdownChanged(ChangeEvent<Enum> changeEvent)
        {
#if HARPIA_DEBUG
            Debug.Log($"[{nameof(PB_PressurePen)}] Changed dropdown");
#endif

            EditorPrefs.SetInt(penPressureModeKey, (int)(PenPressureUseMode)changeEvent.newValue);
        }

        private static void OnToggleChanged(ChangeEvent<bool> evt)
        {
#if HARPIA_DEBUG
            Debug.Log($"Toggle changed to {evt.newValue}");
#endif
            EditorPrefs.SetBool(usePressureKey, usePressureToggle.value);
            penPressureMode.SetActive(usePressureToggle.value);
        }

        public static void Update()
        {
#if UNITY_2022_1_OR_NEWER
                 if (!usePressureToggle.value) return;

            float pressure = Event.current.pressure * Event.current.pressure;
            PenPressureUseMode mode = (PenPressureUseMode)penPressureMode.value;

            PB_HandlesExtension.WriteTempText("Pressure: " + pressure.ToString("f3"));

            switch (mode)
            {
                case PenPressureUseMode.Affect_Brush_Radius:
                    PrefabBrush.instance.brushRadiusSlider.value = Lerp(radiusMinMax, pressure);
                    break;
                case PenPressureUseMode.Affect_Brush_Strength:
                    PrefabBrush.instance.sliderBrushStrength.value = Lerp(strengthMinMax, pressure);
                    break;
            }
#endif
        }

        public static void OnMouseDown()
        {
#if !UNITY_2022_1_OR_NEWER
            return;
#else
            if (!usePressureToggle.value) return;
            oldBrushRadius = PrefabBrush.instance.brushRadiusSlider.value;
            oldStrength = PrefabBrush.instance.sliderBrushStrength.value;
#endif
        }

        public static void OnMouseUp()
        {
#if !UNITY_2022_1_OR_NEWER
            return;
#else
            if (!usePressureToggle.value) return;
            PrefabBrush.instance.brushRadiusSlider.value = oldBrushRadius;
            PrefabBrush.instance.sliderBrushStrength.value = oldStrength;
#endif
        }

        static float Lerp(Vector2 minMax, float value)
        {
            return Mathf.Lerp(minMax.x, minMax.y, value);
        }
    }

    public static class PB_ModularShortcuts
    {
        private const string KeyStart = "pb-shortcut-";
        private static VisualElement holder;

        public static ShortcutData increaseRadius;
        public static ShortcutData decreaseRadius;
        public static ShortcutData rotateRight;
        public static ShortcutData rotateLeft;
        public static ShortcutData randomRotation;
        public static ShortcutData nextPrefab;
        public static ShortcutData previousPrefab;
        public static ShortcutData normalizeSize;
        public static ShortcutData changeMode;
        public static ShortcutData exitTool;

        public static ShortcutData rotationXShortcut;
        public static ShortcutData rotationYShortcut;
        public static ShortcutData rotationZShortcut;
        public static ShortcutData changeScaleShortcut;
        public static ShortcutData yDisplacementShortcut;

        private static List<ShortcutData> allShortCuts;
        private static ShortcutData selectedData1;
        private static Label selectedButton;
        private const string TapKeyText = "Tap Desired Key";

        private static readonly Dictionary<KeyCode, string> cautionShortcuts = new Dictionary<KeyCode, string>()
        {
            { KeyCode.W, "This key is used on the SceneView to move the camera" },
            { KeyCode.A, "This key is used on the SceneView to move the camera" },
            { KeyCode.S, "This key is used on the SceneView to move the camera" },
            { KeyCode.D, "This key is used on the SceneView to move the camera" },

            { KeyCode.Alpha2, "This key toggle 3D /3 2D mode" },
            // { KeyCode.V, "This code is used on the scene to move objects" },
            // { KeyCode.Z, "This code is used on the scene to toggle gizmos on Pivot / Center" },
            // { KeyCode.X, "This code is used on the scene to toggle gizmos on Local / World view" },
        };

        public class ShortcutData
        {
            public EnumField shortCutField;
            public readonly string shortCutName;
            private readonly KeyCode _defaultShortcut;
            string EditorPrefsKey => KeyStart + shortCutName;

            public ShortcutData(string shortCutName, KeyCode shortCut)
            {
                _defaultShortcut = shortCut;

                this.shortCutName = shortCutName;
            }

            public KeyCode GetSavedShortcut()
            {
                return (KeyCode)EditorPrefs.GetInt(EditorPrefsKey, (int)_defaultShortcut);
            }

            public bool IsShortcut()
            {
                return IsShortcut(Event.current.keyCode);
            }

            public bool IsShortcut(KeyCode code)
            {
                return (KeyCode)shortCutField.value == code;
            }

            public void ResetShortcut()
            {
                EditorPrefs.DeleteKey(EditorPrefsKey);
                shortCutField.SetValueWithoutNotify(_defaultShortcut);
            }

            public string GetKeyText()
            {
                KeyCode value = (KeyCode)shortCutField.value;
                switch (value)
                {
                    case KeyCode.Escape: return "esc";
                    case KeyCode.Plus: return "+";
                    case KeyCode.Minus: return "-";
                    case KeyCode.Greater: return ">";
                    case KeyCode.Less: return "<";
                    case KeyCode.Backslash: return @"\";
                    case KeyCode.Dollar: return "$";
                    case KeyCode.Hash: return "#";
                    case KeyCode.DoubleQuote: return @"""";
                    case KeyCode.Question: return "?";
                    case KeyCode.Equals: return "=";
                    case KeyCode.Period: return ".";
                    case KeyCode.Comma: return ",";
                }

                string SplitCamelCase(string input)
                {
                    return System.Text.RegularExpressions.Regex.Replace(input, "([A-Z])", " $1", System.Text.RegularExpressions.RegexOptions.Compiled).Trim();
                }

                return SplitCamelCase(value.ToString());
            }
        }

        public static void Init(VisualElement root)
        {
            holder = root.Q<VisualElement>("modular-shortcuts-holder");

            increaseRadius = new("Increase Size", KeyCode.Equals);
            ShowShortcut(increaseRadius);
            decreaseRadius = new("Decrease Size", KeyCode.Minus);
            ShowShortcut(decreaseRadius);

            rotateRight = new("Rotate Right", KeyCode.Period);
            ShowShortcut(rotateRight, true);
            rotateLeft = new("Rotate Left", KeyCode.Comma);
            ShowShortcut(rotateLeft);
            randomRotation = new("Random Rotation", KeyCode.M);
            ShowShortcut(randomRotation);

            nextPrefab = new("Next Prefab", KeyCode.F);
            ShowShortcut(nextPrefab, true);
            previousPrefab = new("Previous Prefab", KeyCode.G);
            ShowShortcut(previousPrefab);

            normalizeSize = new("Normalize Size", KeyCode.L);
            ShowShortcut(normalizeSize);
            changeMode = new("Change Mode", KeyCode.B);
            ShowShortcut(changeMode);
            exitTool = new ShortcutData("Exit Tool", KeyCode.Escape);
            ShowShortcut(exitTool);

            rotationXShortcut = new ShortcutData("Rotate X with ScrollWheel", KeyCode.X);
            ShowShortcut(rotationXShortcut, true);

            rotationYShortcut = new ShortcutData("Rotate Y with ScrollWheel", KeyCode.Y);
            ShowShortcut(rotationYShortcut);

            rotationZShortcut = new ShortcutData("Rotate Z with ScrollWheel", KeyCode.Z);
            ShowShortcut(rotationZShortcut);

            changeScaleShortcut = new ShortcutData("Change Size with ScrollWheel", KeyCode.C);
            ShowShortcut(changeScaleShortcut, true);

            yDisplacementShortcut = new ShortcutData("Y Offset with ScrollWheel", KeyCode.V);
            ShowShortcut(yDisplacementShortcut);

            root.Q<Button>("reset-shortcuts").RegisterCallback<ClickEvent>(ResetShortcuts);
            root.Q<Button>("keycode-button").RegisterCallback<ClickEvent>(OpenKeycodeDoc);
        }

        private static void ShowShortcut(ShortcutData data, bool addSpace = false)
        {
            allShortCuts ??= new List<ShortcutData>();
            allShortCuts.Add(data);

            VisualElement newShortcutUI = new()
            {
                style =
                {
                    flexDirection = FlexDirection.Row,
                    marginTop = addSpace ? 5 : 0
                }
            };

            Label shortcutName = new(data.shortCutName)
            {
                style =
                {
                    width = new StyleLength(new Length(180, LengthUnit.Pixel)),
                    marginLeft = new StyleLength(new Length(3, LengthUnit.Pixel))
                }
            };
            newShortcutUI.Add(shortcutName);

            EnumField keycodeField = new()
            {
                style = { flexGrow = 1, }
            };

            data.shortCutField = keycodeField;
            keycodeField.Init(data.GetSavedShortcut());
            keycodeField.RegisterValueChangedCallback(e => OnShortcutEnumChanged(data, keycodeField, e));
            newShortcutUI.Add(keycodeField);

            //Button Get Key
            Label getKeyButton = new Label()
            {
                text = TapKeyText,
                style =
                {
                    backgroundColor = new StyleColor(new Color(0.4039216f, 0.4039216f, 0.4039216f, .5f)),
                    borderBottomLeftRadius = new StyleLength(new Length(3f, LengthUnit.Pixel)),
                    borderTopLeftRadius = new StyleLength(new Length(3f, LengthUnit.Pixel)),
                    borderBottomRightRadius = new StyleLength(new Length(3f, LengthUnit.Pixel)),
                    borderTopRightRadius = new StyleLength(new Length(3f, LengthUnit.Pixel)),
                    paddingRight = new StyleLength(new Length(3f, LengthUnit.Pixel)),
                    paddingLeft = new StyleLength(new Length(3f, LengthUnit.Pixel)),
                    fontSize = 12,
                    marginBottom = 1,
                    marginTop = 1
                }
            };

            getKeyButton.SetBorderColor(new Color(0.1294118f, 0.1294118f, 0.1294118f, 0.5f), 1f);

            getKeyButton.RegisterCallback<ClickEvent>(e => OnAssignKeyButton(data, e));

            newShortcutUI.Add(getKeyButton);
            newShortcutUI.ChangeColorOnHover(new Color(1, 1, 1, .3f));

            holder.Add(newShortcutUI);
        }

        private static void OnAssignKeyButton(ShortcutData data, ClickEvent evt)
        {
#if HARPIA_DEBUG
            Debug.Log($"On assign key button for {data.shortCutName}");
#endif
            Label pressedButton = ((Label)evt.target);

            if (selectedButton != null)
            {
                if (pressedButton == selectedButton)
                {
                    Dispose();
                    return;
                }

                selectedButton.text = TapKeyText;
            }

            selectedButton = pressedButton;
            selectedData1 = data;
            selectedButton.text = "Waiting For Input...";
        }

        private static void OnShortcutEnumChanged(ShortcutData data, EnumField keycodeField, ChangeEvent<Enum> changeEvent)
        {
            KeyCode keyCode = (KeyCode)keycodeField.value;
            string key = KeyStart + data.shortCutName;

#if HARPIA_DEBUG
            Debug.Log($"Changed shortcut {data.shortCutName} | new value {keyCode} | key = {key} ");
#endif
            Dispose();

            //Check if can use it
            if (cautionShortcuts.TryGetValue(keyCode, out string shortcut))
            {
                PrefabBrush.DisplayError($"Cannot use shortcut {keyCode.ToString()}: {shortcut}");
                Fallback();
                return;
            }

            //Find same shortcut
            foreach (ShortcutData allShortCut in allShortCuts)
            {
                if (allShortCut == data) continue;
                if ((KeyCode)allShortCut.shortCutField.value != keyCode) continue;

                PrefabBrush.DisplayError($"Shortcut Conflict! This shortcut ({keyCode}) is already being used by {allShortCut.shortCutName}. Please select another one");
                Fallback();
                return;
            }

            EditorPrefs.SetInt(key, (int)keyCode);

            void Fallback()
            {
                KeyCode oldValue = (KeyCode)changeEvent.previousValue;
                keycodeField.SetValueWithoutNotify(oldValue);
            }
        }

        public static void Dispose()
        {
            if (selectedButton != null) selectedButton.text = TapKeyText;
            selectedData1 = null;
            selectedButton = null;
        }

        private static void OpenKeycodeDoc(ClickEvent evt)
        {
#if HARPIA_DEBUG
            Debug.Log($"Clicked on open keycode docs");
#endif
            Application.OpenURL("https://docs.unity3d.com/ScriptReference/KeyCode.html");
        }

        private static void ResetShortcuts(ClickEvent evt)
        {
#if HARPIA_DEBUG
            Debug.Log($"Clicked to reset shortcuts");
#endif

            bool r = EditorUtility.DisplayDialog("Prefab Brush - Reset Shortcuts", "Are you sure you want to reset all shortcuts? This action cannot be undone.", "Ok", "Cancel");

            if (!r) return;

            Dispose();

            foreach (ShortcutData data in allShortCuts)
            {
                data.ResetShortcut();
            }
        }

        public static void UpdateAssignKey()
        {
            if (selectedData1 == null) return;
            KeyCode key = Event.current.keyCode;
            if (key == KeyCode.None) return;

            selectedData1.shortCutField.value = key;
            Dispose();
        }
    }

    public static class PB_FolderUtils
    {
        public static string GetSelectedPathOrFallback()
        {
            string path = "";
            foreach (Object obj in Selection.GetFiltered(typeof(Object), SelectionMode.Assets))
            {
                path = AssetDatabase.GetAssetPath(obj);
                bool fileExists = File.Exists(path);
                bool isNull = string.IsNullOrEmpty(path);
#if HARPIA_DEBUG
                Debug.Log($"[{nameof(PB_FolderUtils)}] Found path {path}| fileExists {fileExists} | isNull {isNull} | Obj {obj.name}");
#endif
                if (isNull || !fileExists) continue;
                path = Path.GetDirectoryName(path);
                break;
            }

            return path;
        }

        public static void ShowFolder(string path)
        {
            EditorUtility.FocusProjectWindow();

            Object folder = AssetDatabase.LoadAssetAtPath(path, typeof(object));

            if (folder == null)
            {
                Debug.Log($"{PrefabBrush.DebugLogStart} Could not find folder {path}");
                return;
            }

            Type pt = Type.GetType("UnityEditor.ProjectBrowser,UnityEditor");
            object ins = pt.GetField("s_LastInteractedProjectBrowser", BindingFlags.Static | BindingFlags.Public).GetValue(null);
            MethodInfo showDirMeth = pt.GetMethod("ShowFolderContents", BindingFlags.NonPublic | BindingFlags.Instance);
            Object obj = AssetDatabase.LoadAssetAtPath(path, typeof(Object));
            showDirMeth.Invoke(ins, new object[] { obj.GetInstanceID(), true });
        }

        public static bool HasAnyPrefab(string selectedFolder)
        {
            string[] assets = AssetDatabase.FindAssets("t:Prefab", new[] { selectedFolder });
            return assets.Length > 0;
        }

        public static List<GameObject> GetPrefabs(string selectedFolder)
        {
            if (string.IsNullOrEmpty(selectedFolder)) return new List<GameObject>();

            string[] assets = AssetDatabase.FindAssets("t:Prefab", new[] { selectedFolder });

            List<GameObject> objects = new List<GameObject>();

            foreach (string s in assets)
            {
                if (string.IsNullOrEmpty(s)) continue;
                string path = AssetDatabase.GUIDToAssetPath(s);
                GameObject obj = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (obj == null) continue;
                objects.Add(obj);
            }

            return objects;
        }
    }
}