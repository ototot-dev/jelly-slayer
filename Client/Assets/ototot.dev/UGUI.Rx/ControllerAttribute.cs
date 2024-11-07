using System;

namespace UGUI.Rx {

/// <summary>
/// TemplateAttribute holds the information of Template resource.
/// </summary>
[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
public class TemplateAttribute : Attribute {
    
    public TemplateAttribute(string path) {
        Path = path;
    }

    public TemplateAttribute(string bundleName, string assetName) {
        BundleName = bundleName;
        AssetName = assetName;
    }

    public string Path { get; private set; }

    public string BundleName { get; private set; }

    public string AssetName { get; private set; }
}

/// <summary>
/// StyleSheetAttribute holds the information of Stylesheet resource.
/// </summary>
[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = true)]
public class StyleSheetAttribute : Attribute {

    public StyleSheetAttribute(string path) {
        Path = path;
    }

    public StyleSheetAttribute(string bundleName, string assetName) {
        BundleName = bundleName;
        AssetName = assetName;
    }

    public string Path { get; private set; }

    public string BundleName { get; private set; }

    public string AssetName { get; private set; }

}

/// <summary>
/// Marks a field in Controller as Resource object and will be loaded by Loader.
/// </summary>
[AttributeUsage(AttributeTargets.Field, Inherited = false, AllowMultiple = false)]
public class ResourceAttribute : Attribute {

    public ResourceAttribute(string path, bool redirect = false) {
        Path = new string[] { path };
        Redirect = redirect;
    }

    public ResourceAttribute(string[] path, bool redirect = false) {
        Path = new string[path.Length];
        path.CopyTo(Path, 0);
        Redirect = redirect;
    }

    public string[] Path { get; private set; }

    public bool Redirect { get; private set; }

}

/// <summary>
/// Marks a field in Controller as AssetBundle object and will be loaded by AssetBundle.Loader.
/// </summary>
[AttributeUsage(AttributeTargets.Field, Inherited = false, AllowMultiple = true)]
public class AssetBundleAttribute : Attribute {

    public AssetBundleAttribute(string bundleName, string assetName, bool redirect = false) {
        BundleName = bundleName;
        AssetName = new string[] { assetName };
        Redirect = redirect;
    }

    public AssetBundleAttribute(string bundleName, string[] assetName, bool redirect = false) {
        BundleName = bundleName;
        AssetName = new string[assetName.Length];
        assetName.CopyTo(AssetName, 0);
        Redirect = redirect;
    }

    public string BundleName { get; private set; }

    public string[] AssetName { get; private set; }

    public bool Redirect { get; private set; }

}

}
