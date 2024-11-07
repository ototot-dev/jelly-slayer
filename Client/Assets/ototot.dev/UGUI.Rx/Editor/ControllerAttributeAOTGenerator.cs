#if UNITY_EDITOR
using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace UGUI.Rx
{
    /// <summary>
    /// ControllerAttributeAOT.cs Generator
    /// </summary>
    public class ControllerAttributeAOTGenerator
    {

        /// <summary>
        /// 'Generate' menu item
        /// </summary>
        [MenuItem("UGUI.Rx/Generate ControllerAttributeAOT.cs")]
        public static void Generate()
        {
            Debug.Log($"ControllerAttributeAOTGenerator => Generating {__outputPath}~");

            Reset();
            Init();

            var builder = new StringBuilder();

            Dump(builder);

            using (var file = File.CreateText(__outputPath))
            {
                file.Write(builder);
                file.Flush();
            }

            Debug.LogFormat("{0} generated :)", __outputPath);
        }

        static string __outputPath = Application.dataPath + "/UGUI.rx/ControllerAttributeAOT.cs";

        static HashSet<string> __ctrlerNamespaces = new HashSet<string>();

        static Dictionary<Type, ControllerAttribute> __ctrlAttrs = new Dictionary<Type, ControllerAttribute>();

        /// <summary>
        /// 
        /// </summary>
        struct ControllerAttribute
        {
            public TemplateAttribute templateAttr;
            public StyleSheetAttribute[] styleSheetAttrs;
            public Tuple<Type, string, ResourceAttribute>[] rscAttrs;
            public Tuple<Type, string, AssetBundleAttribute>[] assetBundleAttrs;

            public void Validate(Type type)
            {
                if (templateAttr != null)
                {
                    var gameObj = Resources.Load<GameObject>(templateAttr.Path);

                    if (gameObj == null || gameObj.GetComponent<Template>() == null)
                        Debug.LogWarningFormat("Validate error on `{0}`'s TemplateAttribute => `{1}` path is invalid.", type.ToString(), templateAttr.Path);
                }

                if (styleSheetAttrs.Length > 0)
                {
                    foreach (var s in styleSheetAttrs)
                    {
                        var gameObj = Resources.Load<GameObject>(s.Path);

                        if (gameObj == null || gameObj.GetComponent<StyleSheet>() == null)
                            Debug.LogWarningFormat("Validate error on `{0}`'s StyleSheetAttribute => `{1}` path is invalid.", type.ToString(), s.Path);
                    }
                }

                if (rscAttrs != null && rscAttrs.Length > 0)
                {
                    foreach (var r in rscAttrs)
                    {
                        if (r.Item3.Redirect)
                            return;

                        foreach (var p in r.Item3.Path)
                        {
                            var gameObj = Resources.Load<GameObject>(p);

                            if (gameObj == null)
                                Debug.LogWarningFormat("Validate error on `{0}.{1}`'s ResourceAttribute => `{2}` path is invalid.", type.ToString(), r.Item2, r.Item3.Path);
                        }
                    }
                }

                if (assetBundleAttrs != null && assetBundleAttrs.Length > 0)
                {
                    foreach (var a in assetBundleAttrs)
                    {
                        if (a.Item3.Redirect)
                            return;

                        foreach (var n in a.Item3.AssetName)
                        {
                            var assetPaths = UnityEditor.AssetDatabase.GetAssetPathsFromAssetBundleAndAssetName(a.Item3.BundleName, n);

                            if (assetPaths.Length == 0)
                                Debug.LogWarningFormat("Validate error on `{0}.{1}`'s AssetBundleAttribute => `{2}.{3}` path is invalid.", type.ToString(), a.Item2, a.Item3.BundleName, a.Item3.AssetName);
                        }
                    }
                }
            }
        }

        static void Reset()
        {
            _tapDepth = 0;
            __ctrlerNamespaces.Clear();
            __ctrlAttrs.Clear();
        }

        static void Init()
        {
            var query = Assembly.GetAssembly(typeof(Controller))
                    .GetTypes()
                    .Where(t => t != typeof(Controller) && typeof(Controller).IsAssignableFrom(t));

            foreach (var q in query)
            {
                var attr = new ControllerAttribute
                {
                    templateAttr = GetTemplateAttribute(q),
                    styleSheetAttrs = GetStyleSheetAttributes(q),
                    rscAttrs = GetResourceAttributes(q),
                    assetBundleAttrs = GetAssetBundleAttributes(q)
                };

                __ctrlAttrs.Add(q, attr);
                __ctrlerNamespaces.Add(q.Namespace);
            }

            foreach (var a in __ctrlAttrs)
                a.Value.Validate(a.Key);
        }

        static TemplateAttribute GetTemplateAttribute(Type type)
        {
            var query = type.GetCustomAttributes(typeof(TemplateAttribute), false);

            if (query.Any())
                return query.First() as TemplateAttribute;
            else
                return null;
        }

        static StyleSheetAttribute[] GetStyleSheetAttributes(Type type)
        {
            var query = type.GetCustomAttributes(typeof(StyleSheetAttribute), false);

            if (query.Any())
                return query.Select(q => q as StyleSheetAttribute).ToArray();
            else
                return new StyleSheetAttribute[] { };
        }

        static Tuple<Type, string, ResourceAttribute>[] GetResourceAttributes(Type type)
        {
            var ret = type.GetFields(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .Select(f =>
                {
                    var temps = f.GetCustomAttributes(typeof(ResourceAttribute), false);

                    if (temps != null && temps.Length > 0)
                        return new Tuple<Type, string, ResourceAttribute>(f.FieldType, f.Name, temps[0] as ResourceAttribute);
                    else
                        return new Tuple<Type, string, ResourceAttribute>(f.FieldType, f.Name, null);
                })
                .Where(t => t.Item3 != null)
                .ToArray();

            return ret;
        }

        static Tuple<Type, string, AssetBundleAttribute>[] GetAssetBundleAttributes(Type type)
        {
            var ret = type.GetFields(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .Select(f =>
                {
                    var temps = f.GetCustomAttributes(typeof(AssetBundleAttribute), false);

                    if (temps != null && temps.Length > 0)
                        return new Tuple<Type, string, AssetBundleAttribute>(f.FieldType, f.Name, temps[0] as AssetBundleAttribute);
                    else
                        return new Tuple<Type, string, AssetBundleAttribute>(f.FieldType, f.Name, null);
                })
                .Where(t => t.Item3 != null)
                .ToArray();

            return ret;
        }

        /// <summary>
        /// The current tap depth
        /// </summary>
        static int _tapDepth;

        static void AppendLine(StringBuilder builder, string line)
        {
            for (int i = 0; i < _tapDepth; ++i)
                builder.Append("    ");

            builder.AppendLine(line);
        }

        static void Dump(StringBuilder builder)
        {
            AppendLine(builder, "#if !UNITY_EDITOR && (UNITY_IOS || UNITY_ANDROID)");

            // Warning
            AppendLine(builder, "/* AUTO-GENERATED!!");
            AppendLine(builder, " * This code was generated by the UGUI.Rx");
            AppendLine(builder, " * Do not change this file manually!! (Changes will be lost after regenerated~)");
            AppendLine(builder, " * AUTO-GENERATED!!");
            AppendLine(builder, " */");

            // Namespace
            AppendLine(builder, "using System;");
            AppendLine(builder, "using System.Linq;");
            AppendLine(builder, "using System.Collections.Generic;");
            AppendLine(builder, "using UnityEngine;");
            AppendLine(builder, "using UniRx;");
            AppendLine(builder, "using AssetBundles.Rx;");

            // Body
            AppendLine(builder, "namespace UGUI.Rx {");
            ++_tapDepth;
            {
                AppendLine(builder, "public class ControllerAttributeAOT : Singleton<ControllerAttributeAOT> {");
                ++_tapDepth;
                {
                    AppendLine(builder, string.Empty);
                    DumpGetTemplatePath(builder);
                    AppendLine(builder, string.Empty);
                    DumpGetStyleSheetPath(builder);
                    AppendLine(builder, string.Empty);
                    DumpGetTemplateBundleAlias(builder);
                    AppendLine(builder, string.Empty);
                    DumpGetStyleSheetBundleAlias(builder);
                    builder.AppendLine(string.Empty);
                    DumpGetResourcePath(builder);
                    builder.AppendLine(string.Empty);
                    DumpSetResource(builder);
                    builder.AppendLine(string.Empty);
                    DumpGetAssetBundleAlias(builder);
                    builder.AppendLine(string.Empty);
                    DumpSetAssets(builder);
                }
                --_tapDepth;
                AppendLine(builder, "}");
            }
            --_tapDepth;
            AppendLine(builder, "}");

            AppendLine(builder, "#endif");
        }

        static void DumpGetTemplatePath(StringBuilder builder)
        {
            AppendLine(builder, "public string GetTemplatePath(Type type) {");
            ++_tapDepth;
            {
                AppendLine(builder, "if (_templatePaths == null) {");
                ++_tapDepth;
                {
                    AppendLine(builder, " _templatePaths = new Dictionary<Type, string> {");
                    ++_tapDepth;
                    {
                        foreach (var a in __ctrlAttrs.Where(v => v.Value.templateAttr != null && !string.IsNullOrEmpty(v.Value.templateAttr.Path)))
                            AppendLine(builder, string.Format("{{ typeof({0}), \"{1}\" }},", a.Key.ToString(), a.Value.templateAttr.Path));
                    }
                    --_tapDepth;
                    AppendLine(builder, "};");
                }
                --_tapDepth;
                AppendLine(builder, "}");
                AppendLine(builder, "return _templatePaths.ContainsKey(type) ? _templatePaths[type] : string.Empty;");
            }
            --_tapDepth;
            AppendLine(builder, "}");
            AppendLine(builder, "Dictionary<Type, string> _templatePaths;");
        }

        static void DumpGetStyleSheetPath(StringBuilder builder)
        {
            AppendLine(builder, "public string[] GetStyleSheetPath(Type type) {");
            ++_tapDepth;
            {
                AppendLine(builder, "if (_styleSheetPaths == null) {");
                ++_tapDepth;
                {
                    AppendLine(builder, " _styleSheetPaths = new Dictionary<Type, string[]> {");
                    ++_tapDepth;
                    {
                        foreach (var a in __ctrlAttrs.Where(v => v.Value.styleSheetAttrs != null && v.Value.styleSheetAttrs.Length > 0))
                        {
                            var path = "{ ";

                            for (int i = 0; i < a.Value.styleSheetAttrs.Length; ++i)
                            {
                                if (string.IsNullOrEmpty(a.Value.styleSheetAttrs[i].Path))
                                    continue;

                                path += string.Format("\"{0}\"", a.Value.styleSheetAttrs[i].Path);

                                if (i != a.Value.styleSheetAttrs.Length - 1)
                                    path += ", ";
                            }

                            path += " }";

                            AppendLine(builder, string.Format("{{ typeof({0}), new string[] {1} }},", a.Key.ToString(), path));
                        }
                    }
                    --_tapDepth;
                    AppendLine(builder, "};");
                }
                --_tapDepth;
                AppendLine(builder, "}");
                AppendLine(builder, "return _styleSheetPaths.ContainsKey(type) ? _styleSheetPaths[type] : new string[] {};");
            }
            --_tapDepth;
            AppendLine(builder, "}");
            AppendLine(builder, "Dictionary<Type, string[]> _styleSheetPaths;");
        }

        static void DumpGetTemplateBundleAlias(StringBuilder builder)
        {
            AppendLine(builder, "public BundleAlias GetTemplateBundleAlias(Type type) {");
            ++_tapDepth;
            {
                AppendLine(builder, "if (_templateBundleAliases == null) {");
                ++_tapDepth;
                {
                    AppendLine(builder, " _templateBundleAliases = new Dictionary<Type, BundleAlias> {");
                    ++_tapDepth;
                    {
                        foreach (var a in __ctrlAttrs.Where(v => v.Value.templateAttr != null && !string.IsNullOrEmpty(v.Value.templateAttr.BundleName)))
                            AppendLine(builder, string.Format("{{ typeof({0}), new BundleAlias(\"{1}\", \"{2}\") }},", a.Key.ToString(), a.Value.templateAttr.BundleName, a.Value.templateAttr.AssetName));
                    }
                    --_tapDepth;
                    AppendLine(builder, "};");
                }
                --_tapDepth;
                AppendLine(builder, "}");
                AppendLine(builder, "return _templateBundleAliases.ContainsKey(type) ? _templateBundleAliases[type] : BundleAlias.Empty;");
            }
            --_tapDepth;
            AppendLine(builder, "}");
            AppendLine(builder, "Dictionary<Type, BundleAlias> _templateBundleAliases;");
        }

        static void DumpGetStyleSheetBundleAlias(StringBuilder builder)
        {
            AppendLine(builder, "public BundleAlias[] GetStyleSheetBundleAlias(Type type) {");
            ++_tapDepth;
            {
                AppendLine(builder, "if (_styleSheetBundleAliases == null) {");
                ++_tapDepth;
                {
                    AppendLine(builder, " _styleSheetBundleAliases = new Dictionary<Type, BundleAlias[]> {");
                    ++_tapDepth;
                    {
                        foreach (var a in __ctrlAttrs.Where(v => v.Value.styleSheetAttrs != null && v.Value.styleSheetAttrs.Length > 0))
                        {
                            var bundleAliasAttrs = a.Value.styleSheetAttrs.Where(v => !string.IsNullOrEmpty(v.BundleName) && !string.IsNullOrEmpty(v.AssetName)).ToArray();

                            if (bundleAliasAttrs.Length == 0)
                                continue;

                            var temp = "{ ";

                            for (int i = 0; i < bundleAliasAttrs.Length; ++i)
                            {
                                var attr = bundleAliasAttrs[i];

                                temp += string.Format("new BundleAlias(\"{0}\", \"{1}\")", attr.BundleName, attr.AssetName);

                                if (i != bundleAliasAttrs.Length - 1)
                                    temp += ", ";
                            }

                            temp += " }";

                            AppendLine(builder, string.Format("{{ typeof({0}), new BundleAlias[] {1} }},", a.Key.ToString(), temp));
                        }
                    }
                    --_tapDepth;
                    AppendLine(builder, "};");
                }
                --_tapDepth;
                AppendLine(builder, "}");
                AppendLine(builder, "return _styleSheetBundleAliases.ContainsKey(type) ? _styleSheetBundleAliases[type] : new BundleAlias[] {};");
            }
            --_tapDepth;
            AppendLine(builder, "}");
            AppendLine(builder, "Dictionary<Type, BundleAlias[]> _styleSheetBundleAliases;");
        }

        static void DumpGetResourcePath(StringBuilder builder)
        {
            AppendLine(builder, "public string[] GetResourcePath<T>(T ctrl) where T : Controller {");
            ++_tapDepth;
            {
                AppendLine(builder, "if (_rscPathGetters == null) {");
                ++_tapDepth;
                {
                    AppendLine(builder, " _rscPathGetters = new Dictionary<Type, Func<Controller, string[]>> {");
                    ++_tapDepth;
                    {
                        foreach (var c in __ctrlAttrs.Where(v => v.Value.rscAttrs != null && v.Value.rscAttrs.Length > 0))
                            AppendLine(builder, string.Format("{{ typeof({0}), c => ResourcePathGetter(({0})c) }},", c.Key.ToString()));
                    }
                    --_tapDepth;
                    AppendLine(builder, "};");
                }
                --_tapDepth;
                AppendLine(builder, "}");
                AppendLine(builder, "return _rscPathGetters.ContainsKey(typeof(T)) ? _rscPathGetters[typeof(T)](ctrl) : new string[] {};");
            }
            --_tapDepth;
            AppendLine(builder, "}");
            AppendLine(builder, "Dictionary<Type, Func<Controller, string[]>> _rscPathGetters;");
            AppendLine(builder, string.Empty);

            foreach (var c in __ctrlAttrs.Where(v => v.Value.rscAttrs != null && v.Value.rscAttrs.Length > 0))
            {
                AppendLine(builder, string.Format("string[] ResourcePathGetter({0} ctrl) {{", c.Key.ToString()));
                ++_tapDepth;
                {
                    AppendLine(builder, "var ret = new List<string>();");
                    foreach (var r in c.Value.rscAttrs)
                    {
                        if (r.Item3.Redirect)
                        {
                            if (r.Item1.IsArray)
                                AppendLine(builder, string.Format("ret.AddRange(ctrl.{0});", r.Item3.Path[0]));
                            else
                                AppendLine(builder, string.Format("ret.Add(ctrl.{0});", r.Item3.Path[0]));
                        }
                        else
                        {
                            if (r.Item3.Path.Length > 1)
                            {
                                var temp = string.Empty;

                                foreach (var p in r.Item3.Path)
                                    temp += string.Format("\"{0}\", ", p);

                                AppendLine(builder, string.Format("ret.AddRange(new string[] {{ {0} }});", temp));
                            }
                            else
                            {
                                AppendLine(builder, string.Format("ret.Add(\"{0}\");", r.Item3.Path[0]));
                            }
                        }
                    }

                    AppendLine(builder, string.Format("return ret.ToArray();", c.Value.rscAttrs.Count()));
                }
                --_tapDepth;
                AppendLine(builder, "}");
            }
        }

        static void DumpSetResource(StringBuilder builder)
        {
            AppendLine(builder, "public void SetResource<T>(T ctrl)  where T : Controller {");
            ++_tapDepth;
            {
                AppendLine(builder, "if (_rscSetters == null) {");
                ++_tapDepth;
                {
                    AppendLine(builder, " _rscSetters = new Dictionary<Type, Func<Controller, int>> {");
                    ++_tapDepth;
                    {
                        foreach (var c in __ctrlAttrs.Where(v => v.Value.rscAttrs != null && v.Value.rscAttrs.Length > 0))
                            AppendLine(builder, string.Format("{{ typeof({0}), c => ResourceSetter(({0})c) }},", c.Key.ToString()));
                    }
                    --_tapDepth;
                    AppendLine(builder, "};");
                }
                --_tapDepth;
                AppendLine(builder, "}");
                AppendLine(builder, "if (_rscSetters.ContainsKey(typeof(T)))");
                ++_tapDepth;
                {
                    AppendLine(builder, "_rscSetters[typeof(T)](ctrl);");
                }
                --_tapDepth;
            }
            --_tapDepth;
            AppendLine(builder, "}");
            AppendLine(builder, "Dictionary<Type, Func<Controller, int>> _rscSetters;");
            AppendLine(builder, string.Empty);

            foreach (var c in __ctrlAttrs.Where(v => v.Value.rscAttrs != null && v.Value.rscAttrs.Length > 0))
            {
                AppendLine(builder, string.Format("int ResourceSetter({0} ctrl) {{", c.Key.ToString()));
                ++_tapDepth;
                {
                    foreach (var r in c.Value.rscAttrs)
                    {
                        if (r.Item3.Redirect)
                        {
                            if (r.Item1.IsArray)
                                AppendLine(builder, string.Format("ctrl.{0} = ctrl.{1}.Select(r => Resources.Load<UnityEngine.Object>(r) as {2}).ToArray();", r.Item2, r.Item3.Path[0], r.Item1.GetElementType()));
                            else
                                AppendLine(builder, string.Format("ctrl.{0} = Resources.Load<UnityEngine.Object>(ctrl.{1}) as {2};", r.Item2, r.Item3.Path[0], r.Item1));
                        }
                        else
                        {
                            if (r.Item3.Path.Length > 1)
                            {
                                AppendLine(builder, string.Format("ctrl.{0} = new {1} {{", r.Item2, r.Item1));
                                ++_tapDepth;
                                {
                                    foreach (var p in r.Item3.Path)
                                        AppendLine(builder, string.Format("Resources.Load<UnityEngine.Object>(\"{0}\") as {1},", p, r.Item1.GetElementType()));
                                }
                                --_tapDepth;
                                AppendLine(builder, "};");
                            }
                            else
                            {
                                AppendLine(builder, string.Format("ctrl.{0} = Resources.Load<UnityEngine.Object>(\"{1}\") as {2};", r.Item2, r.Item3.Path[0], r.Item1));
                            }
                        }
                    }

                    AppendLine(builder, string.Format("return {0};", c.Value.rscAttrs.Count()));
                }
                --_tapDepth;
                AppendLine(builder, "}");
            }
        }

        static void DumpGetAssetBundleAlias(StringBuilder builder)
        {
            AppendLine(builder, "public BundleAlias[] GetBundleAlias<T>(T ctrl) where T : Controller {");
            ++_tapDepth;
            {
                AppendLine(builder, "if (_bundleAliasGetters == null) {");
                ++_tapDepth;
                {
                    AppendLine(builder, " _bundleAliasGetters = new Dictionary<Type, Func<Controller, BundleAlias[]>> {");
                    ++_tapDepth;
                    {
                        foreach (var c in __ctrlAttrs.Where(v => v.Value.assetBundleAttrs != null && v.Value.assetBundleAttrs.Length > 0))
                            AppendLine(builder, string.Format("{{ typeof({0}), c => BundleAliasGetter(({0})c) }},", c.Key.ToString()));
                    }
                    --_tapDepth;
                    AppendLine(builder, "};");
                }
                --_tapDepth;
                AppendLine(builder, "}");
                AppendLine(builder, "return _bundleAliasGetters.ContainsKey(typeof(T)) ? _bundleAliasGetters[typeof(T)](ctrl) : new BundleAlias[] {};");
            }
            --_tapDepth;
            AppendLine(builder, "}");
            AppendLine(builder, "Dictionary<Type, Func<Controller, BundleAlias[]>> _bundleAliasGetters;");
            AppendLine(builder, string.Empty);

            foreach (var c in __ctrlAttrs.Where(v => v.Value.assetBundleAttrs != null && v.Value.assetBundleAttrs.Length > 0))
            {
                AppendLine(builder, string.Format("BundleAlias[] BundleAliasGetter({0} ctrl) {{", c.Key.ToString()));
                ++_tapDepth;
                {
                    AppendLine(builder, "var ret = new List<BundleAlias>();");
                    foreach (var r in c.Value.assetBundleAttrs)
                    {
                        if (r.Item3.Redirect)
                        {
                            if (r.Item1.IsArray)
                                AppendLine(builder, string.Format("ret.AddRange(ctrl.{0}.Select(v => new BundleAlias(ctrl.{1}, v)).ToArray());", r.Item3.AssetName[0], r.Item3.BundleName));
                            else
                                AppendLine(builder, string.Format("ret.Add(new BundleAlias(ctrl.{0}, ctrl.{1}));", r.Item3.BundleName, r.Item3.AssetName[0]));
                        }
                        else
                        {
                            if (r.Item3.AssetName.Length > 1)
                            {
                                var temp = string.Empty;
                                foreach (var n in r.Item3.AssetName)
                                    temp += string.Format("new BundleAlias(\"{0}\", \"{1}\"), ", r.Item3.BundleName, n);

                                AppendLine(builder, string.Format("ret.AddRange(new BundleAlias[] {{ {0} }});", temp));
                            }
                            else
                            {
                                AppendLine(builder, string.Format("ret.Add(new BundleAlias(\"{0}\", \"{1}\"));", r.Item3.BundleName, r.Item3.AssetName[0]));
                            }
                        }
                    }

                    AppendLine(builder, string.Format("return ret.ToArray();", c.Value.rscAttrs.Count()));
                }
                --_tapDepth;
                AppendLine(builder, "}");
            }
        }

        static void DumpSetAssets(StringBuilder builder)
        {
            AppendLine(builder, "public void SetAsset<T>(T ctrl)  where T : Controller {");
            ++_tapDepth;
            {
                AppendLine(builder, "if (_assetSetters == null) {");
                ++_tapDepth;
                {
                    AppendLine(builder, " _assetSetters = new Dictionary<Type, Func<Controller, int>> {");
                    ++_tapDepth;
                    {
                        foreach (var c in __ctrlAttrs.Where(v => v.Value.assetBundleAttrs != null && v.Value.assetBundleAttrs.Length > 0))
                            AppendLine(builder, string.Format("{{ typeof({0}), c => AssetSetter(({0})c) }},", c.Key.ToString()));
                    }
                    --_tapDepth;
                    AppendLine(builder, "};");
                }
                --_tapDepth;
                AppendLine(builder, "}");
                AppendLine(builder, "if (_assetSetters.ContainsKey(typeof(T)))");
                ++_tapDepth;
                {
                    AppendLine(builder, "_assetSetters[typeof(T)](ctrl);");
                }
                --_tapDepth;
            }
            --_tapDepth;
            AppendLine(builder, "}");
            AppendLine(builder, "Dictionary<Type, Func<Controller, int>> _assetSetters;");
            AppendLine(builder, string.Empty);

            foreach (var c in __ctrlAttrs.Where(v => v.Value.assetBundleAttrs != null && v.Value.assetBundleAttrs.Length > 0))
            {
                AppendLine(builder, string.Format("int AssetSetter({0} ctrl) {{", c.Key.ToString()));
                ++_tapDepth;
                {
                    foreach (var r in c.Value.assetBundleAttrs)
                    {
                        if (r.Item3.Redirect)
                        {
                            if (r.Item1.IsArray)
                                AppendLine(builder, string.Format("ctrl.{0} = ctrl.{1}.Select(v => BundlePool.Instance.GetLoaded<UnityEngine.Object>(ctrl.{2}, v) as {3}).ToArray();", r.Item2, r.Item3.AssetName[0], r.Item3.BundleName, r.Item1.GetElementType()));
                            else
                                AppendLine(builder, string.Format("ctrl.{0} = BundlePool.Instance.GetLoaded<UnityEngine.Object>(ctrl.{1}, ctrl.{2}) as {3};", r.Item2, r.Item3.BundleName, r.Item3.AssetName[0], r.Item1));
                        }
                        else
                        {
                            if (r.Item3.AssetName.Length > 1)
                            {
                                AppendLine(builder, string.Format("ctrl.{0} = new {1} {{", r.Item2, r.Item1));
                                ++_tapDepth;
                                {
                                    foreach (var n in r.Item3.AssetName)
                                        AppendLine(builder, string.Format("BundlePool.Instance.GetLoaded<UnityEngine.Object>(\"{0}\", \"{1}\") as {2},", r.Item3.BundleName, n, r.Item1.GetElementType()));
                                }
                                --_tapDepth;
                                AppendLine(builder, "};");
                            }
                            else
                            {
                                AppendLine(builder, string.Format("ctrl.{0} = BundlePool.Instance.GetLoaded<UnityEngine.Object>(\"{1}\", \"{2}\") as {3};", r.Item2, r.Item3.BundleName, r.Item3.AssetName[0], r.Item1));
                            }
                        }
                    }

                    AppendLine(builder, string.Format("return {0};", c.Value.assetBundleAttrs.Count()));
                }
                --_tapDepth;
                AppendLine(builder, "}");
            }
        }
    }
}
#endif