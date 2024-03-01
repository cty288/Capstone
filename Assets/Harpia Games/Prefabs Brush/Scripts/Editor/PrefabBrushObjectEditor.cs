using UnityEditor;

namespace Harpia.PrefabBrush
{
    [CustomEditor(typeof(PrefabBrushObject))]
    public class PrefabBrushObjectEditor : UnityEditor.Editor
    {
        private bool _showAbout;

        public override void OnInspectorGUI()
        {
            
            //draw default inspector
            DrawDefaultInspector();
       
            EditorGUILayout.Space();
            
            _showAbout = EditorGUILayout.Foldout(_showAbout, "About");
            
            if (_showAbout)
            {
                EditorGUILayout.LabelField("Allows the game object to interact with the Prefab Brush.");
                EditorGUILayout.LabelField("It is automatically added by the Prefab Brush when you instantiate it");
                EditorGUILayout.LabelField("Not packed to the build");
            }
        }
        
        public void OnInspectorUpdate()
        {
            this.Repaint();
        }
    }
}