using UnityEngine;

public class BlockFlasher : MonoBehaviour
{
    private Renderer blockRenderer;
    private Color originalColor;
    private bool isFlashing = false;
    private Coroutine flashCoroutine;

    public void StartFlashing(Color flashColor, float interval)
    {
        if (isFlashing) return;

        blockRenderer = GetComponent<Renderer>();
        if (blockRenderer == null) return;

        originalColor = blockRenderer.material.color;
        isFlashing = true;
        flashCoroutine = StartCoroutine(Flash(flashColor, interval));
    }

    public void StopFlashing()
    {
        if (flashCoroutine != null)
        {
            StopCoroutine(flashCoroutine);
            flashCoroutine = null;
        }

        if (blockRenderer != null)
        {
            blockRenderer.material.color = originalColor;
        }

        isFlashing = false;
    }

    private System.Collections.IEnumerator Flash(Color flashColor, float interval)
    {
        while (true)
        {
            blockRenderer.material.color = flashColor;
            yield return new WaitForSeconds(interval);
            blockRenderer.material.color = originalColor;
            yield return new WaitForSeconds(interval);
        }
    }
}
