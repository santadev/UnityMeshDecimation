/*
 * * Fork at: https://github.com/santadev/UnityMeshDecimation
 *
 */



#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityMeshDecimation.Internal;
using UnityMesh = UnityEngine.Mesh;

namespace UnityMeshDecimation.UI 
{
    //==========================================================
    public partial class MeshDecimationEditor : EditorWindow 
    {

        //==========================================================

        public const string Name_PostFix_Decimated = " - decimated";


        //==========================================================

        //----------------------------------------------------------
        [MenuItem("Custom/Editor/Windows/Mesh Decimation")]
        static void Init_CustomEditorWindows() 
        {
            Init();
        }
        //----------------------------------------------------------
        [MenuItem("Custom/Graphics/Mesh/Decimation/Mesh Decimation")]
        static void Init_MeshTools() 
        {
            Init();
        }
        //----------------------------------------------------------


        //----------------------------------------------------------
        static void Open_For_MeshFilter(MeshFilter mf) 
        {
           //...
        }
        //----------------------------------------------------------



        //==========================================================

        //----------------------------------------------------------
        //Ret:  как в Src - EasyGUILayout
        //          true если менялось
        private bool OnGUI_EndCondition_ExcludeMask__Next(Object target = null) 
        {
            
            EditorGUILayout.LabelField("Exclude mask", EditorStyles.toolbarButton);

            EditorGUILayout.HelpBox("* Remeber! Target 'Face Count' and 'Vertex Count' must be more than frozen ones." +
                                     "\n   Real target Count is 'Face - Frozen_Faces' etc." +
                                     "\n   To know amount of frozen 'Execute' once and they will be printed below.", MessageType.Info);

            if ((targetConditions.unsafe_Last_Optimize_Frozen_Faces > 0) || (targetConditions.unsafe_Last_Optimize_Frozen_Verts > 0))
            {
                bool error_Info = false;
                string err_Msg_Add = "";

				if (targetConditions.faceCount > 0)
					if (targetConditions.faceCount < targetConditions.unsafe_Last_Optimize_Frozen_Faces)
					{
                        error_Info = true;
						err_Msg_Add += "\n targetConditions.faceCount ("+targetConditions.faceCount+") < ("+targetConditions.unsafe_Last_Optimize_Frozen_Faces+") Frozen_Faces. Increase targetConditions.faceCount!";
					}
				if (targetConditions.vertexCount > 0)
					if (targetConditions.vertexCount < targetConditions.unsafe_Last_Optimize_Frozen_Verts)
					{
                        error_Info = true;
						err_Msg_Add += "\n targetConditions.vertexCount ("+targetConditions.vertexCount+") < ("+targetConditions.unsafe_Last_Optimize_Frozen_Verts+") Frozen_Verts. Increase targetConditions.vertexCount";
					}

                if (error_Info)
                {
                    EditorGUILayout.HelpBox("Frozen count. Faces: " + targetConditions.unsafe_Last_Optimize_Frozen_Faces + ". Vertices: " + targetConditions.unsafe_Last_Optimize_Frozen_Verts +
                                            "\n And current Target is invalide for them! Increase limits!" + 
                                            err_Msg_Add
                                            , MessageType.Error);
                }
                else
                {
                    EditorGUILayout.HelpBox("Frozen count. Faces: " + targetConditions.unsafe_Last_Optimize_Frozen_Faces + ". Vertices: " + targetConditions.unsafe_Last_Optimize_Frozen_Verts, MessageType.Info);
                }
            }
            else
            {
                EditorGUILayout.HelpBox("Frozen count: unknown (Execute once to calculate).", MessageType.Info);
            }


            //
            EasyGUILayout.ObjectField<MeshFilter>("Exclude MeshFilter (same mesh as main)", ref targetConditions.excludeMeshFilter);
            EasyGUILayout.ObjectField<Transform>("Exclude Colliders from All Children", ref targetConditions.excludeColliders_ChildrenCollect);
            EasyGUILayout.FloatField("          Tolerance", ref targetConditions.excludeTolerance);
            

            //Тут уместней
            OnGUI_Swap__Next();


            //* как в Src - EasyGUILayout
            //      true если менялось
            return false;
        }
        //----------------------------------------------------------

        //----------------------------------------------------------
        //Ret:  как в Src - EasyGUILayout
        //          true если менялось
        private bool OnGUI_PathRegion_AfterSrc__Next(Object target = null) 
        {
            string was__outputPath = this.outputPath;


            //~~~

			EditorGUILayout.BeginHorizontal();
            {
                GUILayout.Space(150);
                

    			if(GUILayout.Button("Near Mesh (+ postfix)", GUILayout.Width(250)))
                {
	    			if (mesh) 
                    {
                        string assetPathName = AssetDatabase.GetAssetPath(mesh);
                        if (string.IsNullOrEmpty(assetPathName))
                        {
                            EditorUtility.DisplayDialog("", "Mesh is not saved file asset - can get path.", "OK");
                        }
                        else
                        {
                            outputPath = Path.ChangeExtension(assetPathName, null) + Name_PostFix_Decimated + ".asset";
                        }
                    }
                    else
                    {
                        EditorUtility.DisplayDialog("", "No mesh selected", "OK");
                    }
		    	}


                GUILayout.Space(20);

    			if(GUILayout.Button("Near Exclude Prefab (+ postfix)", GUILayout.Width(250)))
                {
	    			if (targetConditions.excludeMeshFilter) 
                    {
                        string assetPathName = Prefab_By_Component_Get_AssetPath(targetConditions.excludeMeshFilter, false);
                        if (string.IsNullOrEmpty(assetPathName))
                        {
                            EditorUtility.DisplayDialog("", "targetConditions.excludeMeshFilter is not saved prefab - can get path.", "OK");
                        }
                        else
                        {
                            outputPath = Path.ChangeExtension(assetPathName, null) + Name_PostFix_Decimated + ".asset";
                        }
                    }
                    else
                    {
                        EditorUtility.DisplayDialog("", "No targetConditions.excludeMeshFilter selected", "OK");
                    }
		    	}

            }
			EditorGUILayout.EndHorizontal();


            //~~~

            //
			if (was__outputPath != this.outputPath) 
            {
				if (target) EditorUtility.SetDirty(target);
				return true;
			}

            //* как в Src - EasyGUILayout
            //      true если менялось
            return false;
        }
        //----------------------------------------------------------

        //----------------------------------------------------------
        //Ret:  как в Src - EasyGUILayout
        //          true если менялось
        private bool OnGUI_Swap__Next(Object target = null) 
        {
            

			EditorGUILayout.BeginHorizontal();
            {
                GUILayout.Space(150);
                
    			if(GUILayout.Button("Swap with Exclude MeshFilter", GUILayout.Width(250)))
                {
                    if (targetConditions.excludeMeshFilter)
                    {
                        if (string.IsNullOrEmpty(outputPath))
                        {
                            EditorUtility.DisplayDialog("", "No asset yet at outputPath: '"+outputPath+"'", "OK");
                        }
                        else
                        {
                            UnityMesh outputMesh = AssetDatabase.LoadAssetAtPath<UnityMesh>(outputPath);
                            if (outputMesh)
                            {
                                Undo.RecordObject(targetConditions.excludeMeshFilter, "Decimeted mesh swap");
                                //
                                if (targetConditions.excludeMeshFilter.sharedMesh == mesh)
                                {
                                    targetConditions.excludeMeshFilter.sharedMesh = outputMesh;
                                }
                                else
                                {
                                    targetConditions.excludeMeshFilter.sharedMesh = mesh;
                                }
                            }
                            else
                            {
                                EditorUtility.DisplayDialog("", "Not a mesh asset at outputPath: '"+outputPath+"'", "OK");
                            }
                        }
                        
                    }
                    else
                    {
                        EditorUtility.DisplayDialog("", "No targetConditions.excludeMeshFilter selected", "OK");
                    }
                }

            }
			EditorGUILayout.EndHorizontal();



            //* как в Src - EasyGUILayout
            //      true если менялось
            return false;
        }
        //----------------------------------------------------------

        //==========================================================


	}
    //==========================================================
}
#endif