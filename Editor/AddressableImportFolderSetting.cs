#if UNITY_EDITOR
using UnityAddressableImporter.Helper;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings.GroupSchemas;
#endif
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
        UpdateRuleToSettings();

        void ValidatePath()
        {
            rule.path = $"^{dir}/.*";
            rule.matchType = AddressableImportRuleMatchType.Regex;
        }

        void UpdateRuleToSettings()
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
                Debug.LogError($"{rule.groupName} already exists");
            }
        } else
        {
            Debug.LogError("Group name is empty");
        }
    }

    [ButtonMethod]
    public void ReimportFolder()
    {
        if (!string.IsNullOrWhiteSpace(rule?.groupName))
        {
            if (AddressableAssetSettingsDefaultObject.Settings.FindGroup(rule.groupName))
            {
                FolderImporter.ReimportFolders(new[] { dir });
            }
            else
            {
                Debug.LogError($"{rule.groupName} doesn't exists");
            }
        }
        else
        {
            Debug.LogError("Group name is empty");
        }
    }

#endif
}
