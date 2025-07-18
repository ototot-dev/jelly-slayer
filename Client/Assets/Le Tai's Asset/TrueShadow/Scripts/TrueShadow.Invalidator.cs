// Copyright (c) Le Loc Tai <leloctai.com> . All rights reserved. Do not redistribute.

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace LeTai.TrueShadow
{
interface IChangeTracker
{
    void Check();
}

class ChangeTracker<T> : IChangeTracker
{
    T                         previousValue;
    readonly Func<T>          getValue;
    readonly Func<T, T>       onChange;
    readonly Func<T, T, bool> compare;

    public ChangeTracker(
        Func<T>          getValue,
        Func<T, T>       onChange,
        Func<T, T, bool> compare = null
    )
    {
        this.getValue = getValue;
        this.onChange = onChange;
        this.compare  = compare ?? EqualityComparer<T>.Default.Equals;

        previousValue = this.getValue();
    }

    public void Forget()
    {
        previousValue = getValue();
    }

    public void Check()
    {
        T newValue = getValue();
        if (!compare(previousValue, newValue))
        {
            previousValue = onChange(newValue);
        }
    }
}

public partial class TrueShadow
{
    Action               checkHierarchyDirtiedDelegate;
    IChangeTracker[]     transformTrackers;
    ChangeTracker<int>[] hierarchyTrackers;

    void InitInvalidator()
    {
        checkHierarchyDirtiedDelegate = CheckHierarchyDirtied;
        hierarchyTrackers = new[] {
            new ChangeTracker<int>(
                () => RectTransform.GetSiblingIndex(),
                newValue =>
                {
                    SetHierachyDirty();
                    return newValue; // + 1;
                }
            ),
            new ChangeTracker<int>(
                () =>
                {
                    if (shadowRenderer)
                        return shadowRenderer.transform.GetSiblingIndex();
                    return -1;
                },
                newValue =>
                {
                    SetHierachyDirty();
                    return newValue; // + 1;
                }
            )
        };

        transformTrackers = new IChangeTracker[] {
            new ChangeTracker<Vector3>(
                () => RectTransform.position,
                newValue =>
                {
                    SetLayoutDirty();
                    return newValue;
                },
                (prev, curr) => prev == curr
            ),
            new ChangeTracker<Quaternion>(
                () => RectTransform.rotation,
                newValue =>
                {
                    SetLayoutDirty();
                    if (Cutout)
                        SetTextureDirty();
                    return newValue;
                },
                (prev, curr) => prev == curr
            ),
            new ChangeTracker<Color>(
                () => CanvasRenderer.GetColor(),
                newValue =>
                {
                    SetLayoutDirty();
                    return newValue;
                },
                (prev, curr) => prev == curr
            )
        };

#if TMP_PRESENT
        if (Graphic is TMPro.TextMeshProUGUI
         || Graphic is TMPro.TMP_SubMeshUI)
        {
            var old = transformTrackers;
            transformTrackers = new IChangeTracker[old.Length + 1];
            Array.Copy(old, transformTrackers, old.Length);

            transformTrackers[old.Length] = new ChangeTracker<Vector3>(
                () => RectTransform.lossyScale,
                newValue =>
                {
                    SetLayoutTextureDirty();
                    return newValue;
                },
                (prev, curr) =>
                {
                    if (prev == curr) // Early exit for most common path
                        return true;

                    if (prev.x * prev.y * prev.z < 1e-9f
                     && curr.x * curr.y * curr.z > 1e-9f)
                        return false;

                    var diff = curr - prev;
                    return Mathf.Abs(diff.x / prev.x) < .25f
                        && Mathf.Abs(diff.y / prev.y) < .25f
                        && Mathf.Abs(diff.z / prev.z) < .25f;
                }
            );
        }
#endif

        Graphic.RegisterDirtyLayoutCallback(SetLayoutTextureDirty);
        Graphic.RegisterDirtyVerticesCallback(SetLayoutTextureDirty);
        Graphic.RegisterDirtyMaterialCallback(OnGraphicMaterialDirty);

        CheckHierarchyDirtied();
        CheckTransformDirtied();
    }

    void TerminateInvalidator()
    {
        if (Graphic)
        {
            Graphic.UnregisterDirtyLayoutCallback(SetLayoutTextureDirty);
            Graphic.UnregisterDirtyVerticesCallback(SetLayoutTextureDirty);
            Graphic.UnregisterDirtyMaterialCallback(OnGraphicMaterialDirty);
        }
    }

    void OnGraphicMaterialDirty()
    {
        SetLayoutTextureDirty();
        shadowRenderer.UpdateMaterial();
    }

    internal void CheckTransformDirtied()
    {
        if (transformTrackers != null)
        {
            for (var i = 0; i < transformTrackers.Length; i++)
            {
                transformTrackers[i].Check();
            }
        }
    }

    internal void CheckHierarchyDirtied()
    {
        if (ShadowAsSibling && hierarchyTrackers != null)
        {
            for (var i = 0; i < hierarchyTrackers.Length; i++)
            {
                hierarchyTrackers[i].Check();
            }
        }
    }

    internal void ForgetSiblingIndexChanges()
    {
        for (var i = 0; i < hierarchyTrackers.Length; i++)
        {
            hierarchyTrackers[i].Forget();
        }
    }

#if UNITY_EDITOR
    protected override void Reset()
    {
        base.Reset();
        ApplySerializedData();

        if (ProjectSettings.Instance.UseGlobalAngleByDefault)
        {
            UseGlobalAngle = true;
        }
    }

    protected override void OnValidate()
    {
        SetLayoutTextureDirty();
    }
#endif

    protected override void OnTransformParentChanged()
    {
        base.OnTransformParentChanged();

        if (!isActiveAndEnabled) return;

        SetHierachyDirty();
        this.NextFrames(checkHierarchyDirtiedDelegate);
    }

    protected override void OnRectTransformDimensionsChange()
    {
        base.OnRectTransformDimensionsChange();

        if (!isActiveAndEnabled) return;

        SetLayoutTextureDirty();
    }


    protected override void OnDidApplyAnimationProperties()
    {
        if (!isActiveAndEnabled) return;

        SetLayoutTextureDirty();
    }

    public void ModifyMesh(Mesh mesh)
    {
        if (casterMeshProvider != null)
            return;

        if (!isActiveAndEnabled) return;

        if (SpriteMesh) Utility.SafeDestroy(SpriteMesh);
        SpriteMesh = Instantiate(mesh);
        ModifyShadowCastingMesh(SpriteMesh);

        SetLayoutTextureDirty();
    }

    public void ModifyMesh(VertexHelper verts)
    {
        if (casterMeshProvider != null)
            return;

        if (!isActiveAndEnabled) return;

#if TMP_PRESENT
        if (!(Graphic is TMPro.TextMeshProUGUI))
        {
#endif
            // For when pressing play while in prefab mode
            if (!SpriteMesh) SpriteMesh = new Mesh();
            verts.FillMesh(SpriteMesh);
            ModifyShadowCastingMesh(SpriteMesh);
#if TMP_PRESENT
        }
#endif

        SetLayoutTextureDirty();
    }

    void SetLayoutTextureDirty()
    {
#if TMP_PRESENT
        if (Graphic is TMPro.TextMeshProUGUI tmp)
        {
            SpriteMesh = string.IsNullOrEmpty(tmp.text) ? null : tmp.mesh;
        }
        else if (Graphic is TMPro.TMP_SubMeshUI stmp)
        {
            var isEmpty = string.IsNullOrEmpty(stmp.textComponent.text);
#if UNITY_2022_2 || UNITY_2023_2_OR_NEWER
            isEmpty |= !stmp.canvasRenderer.GetMesh(); // This is a different mesh than stmp.mesh
#endif
            SpriteMesh = isEmpty ? null : stmp.mesh;
        }
#endif
        SetLayoutDirty();
        SetTextureDirty();
    }
}
}
