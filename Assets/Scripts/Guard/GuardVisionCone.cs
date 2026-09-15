using UnityEngine;

namespace AriadnesThread.Guard
{
    /// <summary>
    /// A flat fan mesh in local space along +Z — parented to the guard so its own transform
    /// rotation (already kept in sync with movement facing) orients the cone for free.
    /// Opaque, not alpha-blended: scripting URP Lit's transparent surface type needs more
    /// than SetFloat/EnableKeyword to actually take effect (it rendered as a solid blob in
    /// testing), so this uses a low-contrast muted color instead of fighting that shader.
    /// </summary>
    public class GuardVisionCone : MonoBehaviour
    {
        private static readonly Color HazeColor = new Color(0.18f, 0.16f, 0.09f);

        public void Build(float rangeWorldUnits, float angleDegrees, int segments = 16)
        {
            var mesh = new Mesh { name = "GuardVisionCone" };
            var vertices = new Vector3[segments + 2];
            var triangles = new int[segments * 3];

            vertices[0] = Vector3.zero;
            float halfAngle = angleDegrees * 0.5f;
            for (int i = 0; i <= segments; i++)
            {
                float t = (float)i / segments;
                float angle = Mathf.Deg2Rad * Mathf.Lerp(-halfAngle, halfAngle, t);
                vertices[i + 1] = new Vector3(Mathf.Sin(angle) * rangeWorldUnits, 0f, Mathf.Cos(angle) * rangeWorldUnits);
            }

            for (int i = 0; i < segments; i++)
            {
                triangles[i * 3] = 0;
                triangles[i * 3 + 1] = i + 1;
                triangles[i * 3 + 2] = i + 2;
            }

            mesh.vertices = vertices;
            mesh.triangles = triangles;
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();

            var meshFilter = gameObject.AddComponent<MeshFilter>();
            meshFilter.mesh = mesh;

            var meshRenderer = gameObject.AddComponent<MeshRenderer>();
            meshRenderer.material = new Material(Shader.Find("Universal Render Pipeline/Lit")) { color = HazeColor };
            meshRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            meshRenderer.receiveShadows = false;

            transform.localPosition = Vector3.up * 0.02f; // just above the floor, avoids z-fighting
        }
    }
}
