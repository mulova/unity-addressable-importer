/// <summary>
/// ButtonMethodAttribute,
/// modified from https://github.com/Deadcows/MyBox/blob/master/Attributes/ButtonMethodAttribute.cs
/// </summary>

namespace UnityAddressableImporter.Helper.Internal
{
    using System.Collections.Generic;
    using System.Reflection;
    using Editor.Helper;
    using UnityEditor;

    [CustomEditor(typeof(AddressableImportFolderSetting), true), CanEditMultipleObjects]
    public class AddressableImportFolderSettingEditor : Editor
    {
        private List<MethodInfo> _methods;
        private AddressableImportFolderSetting _target;


        private void OnEnable()
        {
            _target = target as AddressableImportFolderSetting;
            _methods = AddressableImporterMethodHandler.CollectValidMembers(_target.GetType());

        }

        public override void OnInspectorGUI()
        {
            DrawBaseEditor();

#if !ODIN_INSPECTOR
            if (_methods == null)
                return;

            AddressableImporterMethodHandler.OnInspectorGUI(_target, _methods);
#endif

            serializedObject.ApplyModifiedProperties();

        }

        private void DrawBaseEditor()
        {
#if ODIN_INSPECTOR
			_drawer.Draw();
#else
            base.OnInspectorGUI();
#endif
        }
    }
}