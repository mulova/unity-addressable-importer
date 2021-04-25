using System.Collections.Generic;
using System.IO;
using UnityAddressableImporter.Helper;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.AddressableAssets.Settings.GroupSchemas;
using UnityEngine;
using static AddressableImporter;

[System.Serializable]
[CreateAssetMenu(fileName = "AddressableImportFolderRule", menuName = "Addressable Assets/Folder ImportRule", order = 51)]
public class AddressableImportFolderSetting : ScriptableObject
{
    public AddressableImportFolderRule rule;

    private string dir
    {
        get
        {
            var path = UnityEditor.AssetDatabase.GetAssetPath(this);
            return System.IO.Path.GetDirectoryName(path).Replace("\\", "/");
        }
    }

#if UNITY_EDITOR
    public void OnValidate()
    {
        ValidatePath();
        ApplyRule();

        void ValidatePath()
        {
            rule.path = $"^{dir}/.*";
            rule.matchType = AddressableImportRuleMatchType.Regex;
            if (string.IsNullOrWhiteSpace(rule.groupName))
            {
                rule.groupName = Path.GetFileName(dir);
            }
        }

        void ApplyRule()
        {
            var setting = AddressableImportSettings.Instance;
            if (setting == null)
            {
                setting = CreateInstance<AddressableImportSettings>();
                UnityEditor.AssetDatabase.CreateAsset(setting, AddressableImportSettings.kDefaultPath);
            }
            var match = AddressableImportSettings.Instance.rules.FindIndex(r => (r == rule || r.groupName == rule.groupName && r.path == rule.path));
            if (match >= 0)
            {
                setting.rules[match] = rule;
            } else
            {
                setting.rules.Add(rule);
            }
            UnityEditor.EditorUtility.SetDirty(setting);
        }
    }

    [ButtonMethod]
    public void CreateGroup()
    {
        if (!string.IsNullOrWhiteSpace(rule?.groupName))
        {
            if (!AddressableAssetSettingsDefaultObject.Settings.FindGroup(rule.groupName))
            {
                AddressableImporter.CreateAssetGroup<BundledAssetGroupSchema>(AddressableAssetSettingsDefaultObject.Settings, rule.groupName);
            } else
            {
                Debug.LogError($"<color=cyan>[Addressables]</color> {rule.groupName} already exists");
            }
        } else
        {
            Debug.LogError("<color=cyan>[Addressables]</color> Group name is empty");
        }
    }

    [ButtonMethod]
    public void ReimportFolder()
    {
        if (!string.IsNullOrWhiteSpace(rule?.groupName))
        {
            if (AddressableAssetSettingsDefaultObject.Settings.FindGroup(rule.groupName))
            {
                var overrided = new List<AddressableAssetEntry>();
                AddressableAssetSettingsDefaultObject.Settings.GetAllAssets(overrided, true, FilterCurrentGroup, FilterCurrentPath);
                if (overrided.Count == 0 || EditorUtility.DisplayDialog("Override Warning", string.Format("Following assets have already been assigned to another groups. {0}", string.Join(",", overrided.ConvertAll(o => o.address))), "Ok", "Cancel"))
                {
                    FolderImporter.ReimportFolders(new[] { dir });
                    Debug.Log("<color=cyan>[Addressables]</color> Reimport Complete");
                }

                bool FilterCurrentGroup(AddressableAssetGroup g)
                {
                    return g.Name != rule.groupName;
                }

                bool FilterCurrentPath(AddressableAssetEntry e)
                {
                    return rule.Match(e.AssetPath);
                }
            }
            else
            {
                Debug.LogError($"<color=cyan>[Addressables]</color> {rule.groupName} doesn't exists");
            }
        }
        else
        {
            Debug.LogError("<color=cyan>[Addressables]</color> Group name is empty");
        }
    }

#endif
}
