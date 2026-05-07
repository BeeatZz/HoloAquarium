using UnityEditor;
using UnityEngine;

public class AnchorSizer
{
    [MenuItem("GameObject/UI/Anchors to Corners %l")] 
    static void AnchorsToCorners()
    {
        foreach (GameObject gameObject in Selection.gameObjects)
        {
            RectTransform t = gameObject.GetComponent<RectTransform>();
            RectTransform pt = gameObject.transform.parent as RectTransform;

            if (t == null || pt == null) continue;

            Vector2 newAnchorsMin = new Vector2(t.anchorMin.x + t.offsetMin.x / pt.rect.width,
                                                t.anchorMin.y + t.offsetMin.y / pt.rect.height);
            Vector2 newAnchorsMax = new Vector2(t.anchorMax.x + t.offsetMax.x / pt.rect.width,
                                                t.anchorMax.y + t.offsetMax.y / pt.rect.height);

            t.anchorMin = newAnchorsMin;
            t.anchorMax = newAnchorsMax;
            t.offsetMin = t.offsetMax = Vector2.zero;
        }
    }
}