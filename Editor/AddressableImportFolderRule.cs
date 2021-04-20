using UnityEditor;

[System.Serializable]
public class AddressableImportFolderRule : AddressableImportRule
{
    public override bool Match(string assetPath)
    {
        if (AssetDatabase.LoadAssetAtPath<AddressableImportFolderSetting>(assetPath) != null)
        {
            return false;
        }
        return base.Match(assetPath);
    }
}