using UnityEngine;
using UnityEngine.UI;

namespace LinkModule.Scripts.Helper
{
    [RequireComponent(typeof(CanvasRenderer))]
    public class CircleGraphic : MaskableGraphic
    {
        [Range(3, 128)]      public int   segments  = 64;
        [Range(0.01f, 1f)]   public float arc       = 0.25f;  // 0.25 = 90° arc
        [Range(1f, 100f)]    public float thickness = 8f;

        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();

            var r   = rectTransform.rect;
            float halfOuter = Mathf.Min(r.width, r.height) * 0.5f;
            float halfInner = halfOuter - thickness;
            float twoPi     = Mathf.PI * 2f;
            float sweep     = twoPi * Mathf.Clamp01(arc);
            float delta     = sweep / segments;
            float start     = -Mathf.PI / 2f; // start at top (12 o'clock)

            Vector2 center = r.center;

            for (int i = 0; i < segments; i++)
            {
                float a0 = start + i * delta;
                float a1 = a0 + delta;

                Vector2 outer0 = center + new Vector2(Mathf.Cos(a0), Mathf.Sin(a0)) * halfOuter;
                Vector2 outer1 = center + new Vector2(Mathf.Cos(a1), Mathf.Sin(a1)) * halfOuter;
                Vector2 inner1 = center + new Vector2(Mathf.Cos(a1), Mathf.Sin(a1)) * halfInner;
                Vector2 inner0 = center + new Vector2(Mathf.Cos(a0), Mathf.Sin(a0)) * halfInner;

                int idx = vh.currentVertCount;

                vh.AddVert(inner0, color, Vector2.zero);
                vh.AddVert(outer0, color, Vector2.zero);
                vh.AddVert(outer1, color, Vector2.zero);
                vh.AddVert(inner1, color, Vector2.zero);

                vh.AddTriangle(idx + 0, idx + 1, idx + 2);
                vh.AddTriangle(idx + 2, idx + 3, idx + 0);
            }
        }
    }
}