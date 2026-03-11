#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityMeshDecimation.Internal;
using UnityMesh = UnityEngine.Mesh;

namespace UnityMeshDecimation.UI {
    public partial class MeshDecimationEditor : EditorWindow {

        private static MeshDecimationEditor Window { get; set; }
        [MenuItem("Window/Unity Mesh Decimation")]
        static void Init() {
            if (Window == null) {
                Window = EditorWindow.GetWindow<MeshDecimationEditor>("Unity Mesh Decimation", true);
                Window.minSize = new Vector2(550, 400);
                Window.Show();
            }
            Window.Focus();
        }

        private Vector2 _scroll;
        private UnityMesh mesh;
        private int triangleCount;
        private int vertexCount;
        private TargetConditions targetConditions;
        private MeshDecimationProfile profile;
        private Editor profileEditor;
        private string outputPath;
        //
        //* Next
        private bool profile_FoldOut = false;

        #region Editor Hooks
        private void OnEnable() {
            if (this.targetConditions == null) {
                targetConditions = new TargetConditions();
            }
            if (profile == null) {
                this.LoadDefaultProfile();
            }
        }

        private void OnGUI() {
            this._scroll = EditorGUILayout.BeginScrollView(this._scroll);

            EasyGUILayout.BeginRegion("Input");
            if(EasyGUILayout.ObjectField("Mesh", ref this.mesh)) {
                if (this.mesh) {
                    triangleCount = this.mesh.triangleCount();
                    vertexCount = this.mesh.vertexCount;
                }
                else {
                    triangleCount = 0;
                    vertexCount = 0;
                }
                if (targetConditions.faceCount > triangleCount) {
                    targetConditions.faceCount = triangleCount / 2;
                }
            }
            if (this.mesh) {
                EditorGUILayout.HelpBox($"triangles: {triangleCount}, vertices: {vertexCount}", MessageType.Info);
            }
            EasyGUILayout.EndRegion();

            EasyGUILayout.BeginRegion("End Condition");
            EasyGUILayout.IntField("Face Count", ref targetConditions.faceCount);
            EasyGUILayout.IntField("Vertex Count", ref targetConditions.vertexCount);
            EasyGUILayout.IntField("Max Operations", ref targetConditions.maxOperations);
            EasyGUILayout.FloatField("Max Error", ref targetConditions.maxMetrix);
            EasyGUILayout.FloatField("Max Time", ref targetConditions.maxTime);
            //
            //* Next
            OnGUI_EndCondition_ExcludeMask__Next();
            //
            EasyGUILayout.EndRegion();

            
            
            EasyGUILayout.BeginRegion("Profile");
            EasyGUILayout.ObjectField(string.Empty, ref this.profile);
            //
            profile_FoldOut = EditorGUILayout.Foldout(profile_FoldOut, "Settings:", true);
            if (profile_FoldOut)
            {
                if (this.profile) 
                {
                    Editor.CreateCachedEditor(this.profile, null, ref this.profileEditor);
                    this.profileEditor.OnInspectorGUI();
                }
            }
            EasyGUILayout.EndRegion();




            EasyGUILayout.BeginRegion("Output");
            if(EasyGUILayout.FilePathField("Path", ref this.outputPath, "asset")) {
                //* Next: why?
                //      Was: GUI.FocusControl(null);
            }
            //
            //* Next
            OnGUI_PathRegion_AfterSrc__Next();
            //
            EasyGUILayout.EndRegion();




            EditorGUILayout.EndScrollView();

            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Execute", GUILayout.Height(35))) {
                this.Execute();
            }

            GUILayout.Space(10);
        }
        #endregion

        private void Execute() {
            if (this.mesh == null) {
                Debug.LogError("Mesh cannot be null");
                return;
            }
            if (string.IsNullOrEmpty(this.outputPath)) {
                Debug.LogError("Output path cannot be empty");
                return;
            }
            if (this.profile == null) {
                this.LoadDefaultProfile();
            }

            //
			if (targetConditions.excludeMeshFilter)
			{
				//* Debug.Log("Safety checks for MeshFilter: " + targetConditions.excludeMeshFilter, targetConditions.excludeMeshFilter);
                //
				if (targetConditions.excludeMeshFilter.sharedMesh != mesh) { EditorUtility.DisplayDialog("", "excludeMeshFilter contain differetn mesh that in main mesh. Stop.", "OK"); return; }
                if (!targetConditions.excludeColliders_ChildrenCollect) { EditorUtility.DisplayDialog("", "No excludeColliders_ChildrenCollect setted. Stop.", "OK"); return; }
                //
                if (!targetConditions.excludeColliders_ChildrenCollect.parent) { EditorUtility.DisplayDialog("", "excludeColliders_ChildrenCollect must be child of excludeMeshFilter. No paretn at all. Stop.", "OK"); return; }
                if (targetConditions.excludeColliders_ChildrenCollect.parent != targetConditions.excludeMeshFilter.transform) { EditorUtility.DisplayDialog("", "excludeColliders_ChildrenCollect must be child of excludeMeshFilter. Stop.", "OK"); return; }
                //
                targetConditions.excludeColliders = targetConditions.excludeColliders_ChildrenCollect.GetComponentsInChildren<Collider>();
                if (targetConditions.excludeColliders.Length <= 0) { EditorUtility.DisplayDialog("", "No Colliders found in excludeColliders_ChildrenCollect. Stop.", "OK"); return; }
                foreach(Collider coll in targetConditions.excludeColliders)
                    if (!targetConditions.ExcludeCollider_Supported_Is(coll, false)) { EditorUtility.DisplayDialog("", "Colliders type is unsupported. See log. Stop.", "OK"); return; }

                if (targetConditions.excludeTolerance < 0.000001f)
                {
                    EditorUtility.DisplayDialog("", "excludeTolerance is too small. Fixing and continue.", "OK");
                    targetConditions.excludeTolerance = 0.000001f;
                }
			}


            //
            var oldMesh = AssetDatabase.LoadAssetAtPath<UnityMesh>(this.outputPath);
            var newMesh = this.profile.Optimize(this.mesh, this.targetConditions, oldMesh);
            if (oldMesh) {
                AssetDatabase.SaveAssets();
            }
            else {
                AssetDatabase.CreateAsset(newMesh, this.outputPath);
            }
            Selection.activeObject = AssetDatabase.LoadAssetAtPath<UnityMesh>(this.outputPath);
        }

        private void LoadDefaultProfile() {
            var profilePath = AssetDatabase.GUIDToAssetPath(AssetDatabase.FindAssets("DefaultMeshDecimation t:MeshDecimationProfile")[0]);
            this.profile = AssetDatabase.LoadAssetAtPath<MeshDecimationProfile>(profilePath);
        }
	}
}
#endif