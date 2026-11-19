using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>Preserves the source lobby outline shader's vertex contract.</summary>
[RequireComponent(typeof(Text))]
public class CustomTextOutline : BaseMeshEffect
{
    /// <summary>Source shader restored independently of the original project GUID.</summary>
    [SerializeField]
    private Shader outlineShader;

    /// <summary>Owned material with a resolved shader reference.</summary>
    private Material resolvedMaterial;

    /// <summary>Outline color encoded separately for each text mesh.</summary>
    [SerializeField]
    private Color outlineColor = Color.white;

    /// <summary>Outline width in local canvas units.</summary>
    [SerializeField, Range(0, 6)]
    private int outlineWidth = 2;

    /// <summary>Reusable triangle buffer owned by this component.</summary>
    private readonly List<UIVertex> vertices = new List<UIVertex>();

    /// <summary>Apply the serialized material when the text becomes visible.</summary>
    protected override void OnEnable()
    {
        base.OnEnable();
        Configure();
    }

    /// <summary>Restore shader channels after the widget joins its canvas.</summary>
    protected override void OnCanvasHierarchyChanged()
    {
        base.OnCanvasHierarchyChanged();
        Configure();
    }

    /// <summary>Release the material owned by this text instance.</summary>
    protected override void OnDestroy()
    {
        base.OnDestroy();

        if (resolvedMaterial)
        {
            if (Application.isPlaying)
                Destroy(resolvedMaterial);
            else
                DestroyImmediate(resolvedMaterial);
        }
    }

#if UNITY_EDITOR
    /// <summary>Refresh the preview after serialized settings change.</summary>
    protected override void OnValidate()
    {
        base.OnValidate();
        Configure();
    }
#endif

    /// <summary>Expand triangles while preserving each glyph's original UV bounds.</summary>
    /// <param name="helper">The text's generated triangle stream.</param>
    public override void ModifyMesh(VertexHelper helper)
    {
        if (IsActive() && outlineShader)
        {
            EnsureCanvasChannels();

            helper.GetUIVertexStream(vertices);

            for (int index = 0; index + 2 < vertices.Count; index += 3)
                ExpandTriangle(index);

            helper.Clear();
            helper.AddUIVertexTriangleStream(vertices);
        }
    }

    /// <summary>Bind the material and the extra channels required by its shader.</summary>
    /// <returns>True when the graphic has a valid source material.</returns>
    private bool Configure()
    {
        if (!graphic || !outlineShader)
            return false;

        if (!resolvedMaterial)
            resolvedMaterial = new Material(outlineShader) { hideFlags = HideFlags.HideAndDontSave };

        graphic.material = resolvedMaterial;
        EnsureCanvasChannels();

        graphic.SetVerticesDirty();

        return true;
    }

    /// <summary>Configure the final widget canvas before its text mesh is submitted.</summary>
    /// <returns>True when a rendering canvas is available.</returns>
    private bool EnsureCanvasChannels()
    {
        if (!graphic || !graphic.canvas)
            return false;

        graphic.canvas.additionalShaderChannels |= AdditionalCanvasShaderChannels.TexCoord1
            | AdditionalCanvasShaderChannels.TexCoord2 | AdditionalCanvasShaderChannels.TexCoord3
            | AdditionalCanvasShaderChannels.Tangent | AdditionalCanvasShaderChannels.Normal;

        return true;
    }

    /// <summary>Expand one glyph triangle using the original SV mesh algorithm.</summary>
    /// <param name="index">Index of the triangle's first vertex.</param>
    /// <returns>True when the triangle has nondegenerate edges.</returns>
    private bool ExpandTriangle(int index)
    {
        var first = vertices[index];
        var second = vertices[index + 1];
        var third = vertices[index + 2];
        Vector2 firstPosition = first.position;
        Vector2 secondPosition = second.position;
        Vector2 thirdPosition = third.position;
        var center = (Vector2.Min(firstPosition, Vector2.Min(secondPosition, thirdPosition))
            + Vector2.Max(firstPosition, Vector2.Max(secondPosition, thirdPosition))) * 0.5f;
        Vector2 edgeX = secondPosition - firstPosition;
        Vector2 edgeY = thirdPosition - secondPosition;
        Vector2 uvX = second.uv0 - first.uv0;
        Vector2 uvY = third.uv0 - second.uv0;

        if (Mathf.Abs(Vector2.Dot(edgeX.normalized, Vector2.right))
            <= Mathf.Abs(Vector2.Dot(edgeY.normalized, Vector2.right)))
        {
            var edge = edgeX;
            edgeX = edgeY;
            edgeY = edge;

            var uv = uvX;
            uvX = uvY;
            uvY = uv;
        }

        if (edgeX.sqrMagnitude < 0.000001f || edgeY.sqrMagnitude < 0.000001f)
            return false;

        var minimum = Vector2.Min(first.uv0, Vector2.Min(second.uv0, third.uv0));
        var maximum = Vector2.Max(first.uv0, Vector2.Max(second.uv0, third.uv0));

        for (int offset = 0; offset < 3; offset++)
        {
            var vertex = vertices[index + offset];
            var position = vertex.position;
            float x = position.x > center.x ? outlineWidth : -outlineWidth;
            float y = position.y > center.y ? outlineWidth : -outlineWidth;
            position.x += x;
            position.y += y;
            vertex.position = position;

            Vector2 shift = uvX / edgeX.magnitude * x * (Vector2.Dot(edgeX, Vector2.right) > 0 ? 1 : -1)
                + uvY / edgeY.magnitude * y * (Vector2.Dot(edgeY, Vector2.up) > 0 ? 1 : -1);
            vertex.uv0 += new Vector4(shift.x, shift.y, 0, 0);
            vertex.uv1 = minimum;
            vertex.uv2 = maximum;
            vertex.uv3 = new Vector2(outlineColor.r, outlineColor.g);
            vertex.tangent = new Vector4(0, 0, outlineColor.b, outlineColor.a);
            vertex.normal = new Vector3(0, 0, outlineWidth);
            vertices[index + offset] = vertex;
        }

        return true;
    }
}
