using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UniRx;
using Assets.Rx;


namespace UGUI.Rx {

/// <summary>
/// ControllerAttributeAccessor provides access methods for Loader object.
/// </summary>
public class ControllerAttributeAccessor : ototot.dev.Singleton<ControllerAttributeAccessor> {

    public string GetTemplateResourcePath(Type type) {
#if !UNITY_EDITOR && (UNITY_IOS || UNITY_ANDROID)
        return ControllerAttributeAOT.Instance.GetTemplatePath(type);
#else
        var attrs = type.GetCustomAttributes(typeof(TemplateAttribute), false);

        if (attrs != null && attrs.Length > 0)
            return (attrs[0] as TemplateAttribute).Path;
        else
            return string.Empty;
#endif
    }

    public AssetAlias GetTemplateAssetAlias(Type type) {
#if !UNITY_EDITOR && (UNITY_IOS || UNITY_ANDROID)
        return ControllerAttributeAOT.Instance.GetTemplateBundleAlias(type);
#else
        var attrs = type.GetCustomAttributes(typeof(TemplateAttribute), false);

        if (attrs != null && attrs.Length > 0 && string.IsNullOrEmpty((attrs[0] as TemplateAttribute).Path))
            return new AssetAlias((attrs[0] as TemplateAttribute).BundleName, (attrs[0] as TemplateAttribute).AssetName);
        else
            return AssetAlias.Invalid;
#endif
    }

    public string[] GetStyleSheetResourcePath(Type type) {
#if !UNITY_EDITOR && (UNITY_IOS || UNITY_ANDROID)
        return ControllerAttributeAOT.Instance.GetStyleSheetPaths(type);
#else
        var attrs = type.GetCustomAttributes(typeof(StyleSheetAttribute), false);

        if (attrs != null && attrs.Length > 0)
            return attrs.Select(a => (a as StyleSheetAttribute).Path).Where(p => !string.IsNullOrEmpty(p)).ToArray();
        else
            return new string[] {};
#endif
    }

    public AssetAlias[] GetStyleSheetAssetAlias(Type type) {
#if !UNITY_EDITOR && (UNITY_IOS || UNITY_ANDROID)
        return ControllerAttributeAOT.Instance.GetStyleSheetBundleAlias(type);
#else
        var attrs = type.GetCustomAttributes(typeof(StyleSheetAttribute), false);

        if (attrs != null && attrs.Length > 0)
            return attrs.Where(a => string.IsNullOrEmpty((a as StyleSheetAttribute).Path)).Select(a => new AssetAlias((a as StyleSheetAttribute).BundleName, (a as StyleSheetAttribute).AssetName)).ToArray();
        else
            return new AssetAlias[] {};
#endif
    }

    public string[] GetMemberResourcePath<T>(T target) where T : Controller {
#if !UNITY_EDITOR && (UNITY_IOS || UNITY_ANDROID)
        return ControllerAttributeAOT.Instance.GetResourcePath<T>(target);
#else
        var attrs = typeof(T).GetFields(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            .Select(f => {
                var temps = f.GetCustomAttributes(typeof(ResourceAttribute), false);

                if (temps != null && temps.Length > 0)
                    return temps[0];
                else
                    return null;
            })
            .Where(a => a != null)
            .ToArray();

        if (attrs != null && attrs.Length > 0) {
            var ret = new List<string>();

            foreach (var a in attrs) {
                if ((a as ResourceAttribute).Redirect) {
                    var field = typeof(T).GetFields(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                        .First(f => f.Name == (a as ResourceAttribute).Path[0]);

                    if (field.FieldType.IsArray) {
                        foreach (var e in (field.GetValue(target) as string[]))
                            ret.Add(e);
                    }
                    else {
                        ret.Add(field.GetValue(target) as string);
                    }
                }
                else {
                    foreach (var p in (a as ResourceAttribute).Path)
                        ret.Add(p);
                }
            }

            return ret.ToArray();
        }
        else {
            return new string[] {};
        }
#endif
    }

    public void SetMemberResource<T>(T target) where T : Controller {
#if !UNITY_EDITOR && (UNITY_IOS || UNITY_ANDROID)
        ControllerAttributeAOT.Instance.SetResource<T>(target);
#else
        var tuples = typeof(T).GetFields(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            .Select(f => {
                var temps = f.GetCustomAttributes(typeof(ResourceAttribute), false);

                if (temps != null && temps.Length > 0)
                    return new Tuple<System.Reflection.FieldInfo, ResourceAttribute>(f, temps[0] as ResourceAttribute);
                else
                    return new Tuple<System.Reflection.FieldInfo, ResourceAttribute>(f, null);
            })
            .Where(t => t.Item2 != null)
            .ToArray();

        foreach (var t in tuples) {
            if (t.Item1.FieldType.IsArray) {
                List<UnityEngine.Object> gameObjs = new List<UnityEngine.Object>();

                if (t.Item2.Redirect) {
                    var field = typeof(T).GetFields(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                        .First(f => f.Name == t.Item2.Path[0]);

                    foreach (var e in (field.GetValue(target) as string[]))
                        gameObjs.Add(Resources.Load<UnityEngine.Object>(e));
                }
                else {
                    foreach (var p in t.Item2.Path)
                        gameObjs.Add(Resources.Load<UnityEngine.Object>(p));
                }

                var elemType = t.Item1.FieldType.GetElementType();
                var temp = Array.CreateInstance(elemType, gameObjs.Count);

                int i = -1;

                foreach (var o in gameObjs)
                    temp.SetValue(Convert.ChangeType(o, elemType), ++i);

                t.Item1.SetValue(target, temp);
            }
            else {
                if (t.Item2.Redirect) {
                    var path = typeof(T).GetFields(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                        .First(f => f.Name == t.Item2.Path[0])
                        .GetValue(target) as string;

                    t.Item1.SetValue(target, Convert.ChangeType(Resources.Load<UnityEngine.Object>(path), t.Item1.FieldType));
                }
                else {
                    t.Item1.SetValue(target, Convert.ChangeType(Resources.Load<UnityEngine.Object>(t.Item2.Path[0]), t.Item1.FieldType));
                }
            }
        }
#endif
    }

    public AssetAlias[] GetMemberAssetAlias<T>(T target) where T : Controller{
#if !UNITY_EDITOR && (UNITY_IOS || UNITY_ANDROID)
        return ControllerAttributeAOT.Instance.GetBundleAlias<T>(target);
#else
        var attrs = typeof(T).GetFields(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            .Select(f => {
                var temps = f.GetCustomAttributes(typeof(AssetBundleAttribute), false);

                if (temps != null && temps.Length > 0)
                    return temps[0];
                else
                    return null;
            })
            .Where(a => a != null)
            .ToArray();
        

        if (attrs != null && attrs.Length > 0) {
            var ret = new List<AssetAlias>();

            foreach (var a in attrs) {
                if ((a as AssetBundleAttribute).Redirect) {
                    var bundleName = typeof(T).GetFields(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                            .First(f => f.Name == (a as AssetBundleAttribute).BundleName).GetValue(target) as string;

                    var field = typeof(T).GetFields(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                            .First(f => f.Name == (a as AssetBundleAttribute).AssetName[0]);

                    if (field.FieldType.IsArray) {
                        foreach (var e in (field.GetValue(target) as string[]))
                            ret.Add(new AssetAlias(bundleName, e));
                    }
                    else {
                        ret.Add(new AssetAlias(bundleName, field.GetValue(target) as string));
                    }
                }
                else {
                    foreach (var e in (a as AssetBundleAttribute).AssetName)
                        ret.Add(new AssetAlias((a as AssetBundleAttribute).BundleName, e));
                }
            }

            return ret.ToArray();
        }
        else {
            return new AssetAlias[] {};
        }
#endif
    }

    public void SetMemberAsset<T>(T target) where T : Controller {
#if !UNITY_EDITOR && (UNITY_IOS || UNITY_ANDROID)
        ControllerAttributeAOT.Instance.SetAssets<T>(target);
#else
        var tuples = typeof(T).GetFields(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            .Select(f => {
                var temps = f.GetCustomAttributes(typeof(AssetBundleAttribute), false);

                if (temps != null && temps.Length > 0)
                    return new Tuple<System.Reflection.FieldInfo, AssetBundleAttribute>(f, temps[0] as AssetBundleAttribute);
                else
                    return new Tuple<System.Reflection.FieldInfo, AssetBundleAttribute>(f, null);
            })
            .Where(t => t.Item2 != null)
            .ToArray();

        foreach (var t in tuples) {
            if (t.Item1.FieldType.IsArray) {
                List<UnityEngine.Object> objs = new List<UnityEngine.Object>();

                if (t.Item2.Redirect) {
                    var bundleName = typeof(T).GetFields(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                        .First(f => f.Name == t.Item2.BundleName).GetValue(target) as string;

                    var field = typeof(T).GetFields(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                        .First(f => f.Name == t.Item2.AssetName[0]);

                    foreach (var e in (field.GetValue(target) as string[]))
                        objs.Add(AssetsManager.Instance.GetLoadedByAlias<UnityEngine.Object>(new AssetAlias(bundleName, e), "UGUI.Rx").Value);
                }
                else {
                    foreach (var n in t.Item2.AssetName)
                        objs.Add(AssetsManager.Instance.GetLoadedByAlias<UnityEngine.Object>(new AssetAlias(t.Item2.BundleName, n), "UGUI.Rx").Value);
                }

                var elemType = t.Item1.FieldType.GetElementType();
                var temp = Array.CreateInstance(elemType, objs.Count);

                int i = -1;

                foreach (var o in objs)
                    temp.SetValue(Convert.ChangeType(o, elemType), ++i);

                t.Item1.SetValue(target, temp);
            }
            else {
                if (t.Item2.Redirect) {
                    var bundleName = typeof(T).GetFields(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                        .First(f => f.Name == t.Item2.BundleName).GetValue(target) as string;

                    var assetName = typeof(T).GetFields(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                        .First(f => f.Name == t.Item2.AssetName[0]).GetValue(target) as string;

                    var asset = AssetsManager.Instance.GetLoadedByAlias<UnityEngine.Object>(new AssetAlias(bundleName, assetName), "UGUI.Rx");

                    t.Item1.SetValue(target, Convert.ChangeType(asset, t.Item1.FieldType));
                }
                else {
                    var asset = AssetsManager.Instance.GetLoadedByAlias<UnityEngine.Object>(new AssetAlias(t.Item2.BundleName, t.Item2.AssetName[0]), "UGUI.Rx");

                    t.Item1.SetValue(target, Convert.ChangeType(asset, t.Item1.FieldType));
                }
            }
        }
#endif
        
    }

}

}
