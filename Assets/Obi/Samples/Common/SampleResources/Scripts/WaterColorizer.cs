using UnityEngine;

public class WaterColorizer : MonoBehaviour
{
    public WaterContainer container;

    // ここに「Obi Fluid Renderer の 'Fluid material'」をドラッグで割り当てる
    public Material fluidMaterial;

    public Color cleanColor = new Color(0.2f, 0.6f, 1f, 1f);   // きれい
    public Color dirtyColor = new Color(0.4f, 0.3f, 0.2f, 1f); // 汚れ

    void LateUpdate()
    {
        if (container == null || fluidMaterial == null) return;

        float t = Mathf.InverseLerp(0f, 100f, container.quality);
        Color c = Color.Lerp(cleanColor, dirtyColor, t);

        // パイプライン差異に対応
        if (fluidMaterial.HasProperty("_BaseColor")) fluidMaterial.SetColor("_BaseColor", c);
        else if (fluidMaterial.HasProperty("_BaseColorMap")) fluidMaterial.SetColor("_BaseColorMap", c); // まれに
        else if (fluidMaterial.HasProperty("_Color")) fluidMaterial.SetColor("_Color", c);
    }
}
