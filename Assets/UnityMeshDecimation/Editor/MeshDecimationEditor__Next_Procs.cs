/*
 * * Fork at: https://github.com/santadev/UnityMeshDecimation
 *
 */



#if UNITY_EDITOR
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


        //============================================================================================================================================

        //-----------------------------------------------------------------------------------------------------------------------
        //Получить Ассет префаба по компоненту.
        //  есть Component comp.
        //      Если он часть префаба - получить этот префаб. Компонент может быть частью сцены, а может ыбть частью уже префаба сразу.
        //
        //      возвращает резульатт AssetDataBase.GetAssetPath. или "".
        //
        /// Возвращает путь к ассету префаба, из которого был создан GameObject, содержащий указанный компонент.
        /// Если компонент не является частью префаба (например, живёт на сцене напрямую) — возвращает пустую строку.
        //
        //Ret: Путь к .prefab файлу или ""
        //
        public static string Prefab_By_GameObject_Get_AssetPath(GameObject obj, bool no_Error_If_Not_Prefab = false)
        {
            if (obj == null)
            {
                if (!no_Error_If_Not_Prefab)
                    Debug.LogError("[PrefabUtils] GameObject is null.");
                return string.Empty;
            }

            // Получаем путь к ассету префаба, из которого был создан этот инстанс
            string prefabPath = PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(obj);

            if (string.IsNullOrEmpty(prefabPath))
            {
                if (!no_Error_If_Not_Prefab)
                    Debug.LogError($"[PrefabUtils] GameObject '{obj.name}' is not part of a prefab (likely scene-only).", obj);
                return string.Empty;
            }

            return prefabPath;
        }
        //---
        public static string Prefab_By_Component_Get_AssetPath(Component comp, bool no_Error_If_Not_Prefab = false)
        {
            if (comp == null || comp.gameObject == null)
            {
                if (!no_Error_If_Not_Prefab)
                    Debug.LogError("[PrefabUtils] Component or its GameObject is null.");
                return string.Empty;
            }

            return Prefab_By_GameObject_Get_AssetPath(comp.gameObject, no_Error_If_Not_Prefab);
        }
        //-----------------------------------------------------------------------------------------------------------------------


        //============================================================================================================================================


	}
    //==========================================================
}
#endif