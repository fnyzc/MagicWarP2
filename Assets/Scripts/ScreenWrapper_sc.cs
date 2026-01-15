using UnityEngine;

public class ScreenWrapper_sc : MonoBehaviour
{ public float padding = 0.4f;

    [Header("Optional: keep sprite fully inside")]
    public bool useRendererBounds = true;

    private Camera cam;
    private float extX, extY;

    private void Awake()
    {
        cam = Camera.main;
    }

    private void LateUpdate()
    {
        if (cam == null) return;
        if (useRendererBounds)
        {
            var r = GetComponent<Renderer>();
            if (r != null)
            {
                extX = r.bounds.extents.x;
                extY = r.bounds.extents.y;
            }
            else
            {
                extX = extY = 0f;
            }
        }

        float halfH = cam.orthographicSize;
        float halfW = halfH * cam.aspect;

        float minX = cam.transform.position.x - halfW + padding + extX;
        float maxX = cam.transform.position.x + halfW - padding - extX;
        float minY = cam.transform.position.y - halfH + padding + extY;
        float maxY = cam.transform.position.y + halfH - padding - extY;

        Vector3 pos = transform.position;
        pos.x = Mathf.Clamp(pos.x, minX, maxX);
        pos.y = Mathf.Clamp(pos.y, minY, maxY);
        transform.position = pos;
    }
}
