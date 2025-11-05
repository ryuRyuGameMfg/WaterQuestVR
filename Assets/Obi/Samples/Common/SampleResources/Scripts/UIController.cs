// UIController.cs
using UnityEngine;
using UnityEngine.UI;

public class UIController : MonoBehaviour
{
    [Header("参照")]
    public WaterContainer bucket;      // バケツ（WaterContainer付き）
    public Slider waterSlider;         // 残量
    public Slider qualitySlider;       // きれいさ

    [Header("色（きれい→汚れ）")]
    public Color cleanColor = new Color(0.2f, 0.6f, 1f, 1f);
    public Color dirtyColor = new Color(0.4f, 0.3f, 0.2f, 1f);

    // スライダーのFill(Image)を持っておくと色を変えられる
    Image waterFill;
    Image qualityFill;

    void Awake()
    {
        // Fill の Image を取得（Slider > Fill Area > Fill）
        if (waterSlider != null)
            waterFill = waterSlider.fillRect != null ? waterSlider.fillRect.GetComponent<Image>() : null;
        if (qualitySlider != null)
            qualityFill = qualitySlider.fillRect != null ? qualitySlider.fillRect.GetComponent<Image>() : null;
    }

    void Start()
    {
        if (bucket == null) { Debug.LogWarning("UIController: bucket 未割当"); return; }

        // 残量バーの最大値を容量に合わせる
        waterSlider.minValue = 0;
        waterSlider.maxValue = bucket.maxLiters;

        // きれいさバーは 0(きれい)〜100(汚れ)
        qualitySlider.minValue = 0;
        qualitySlider.maxValue = 100;
    }

    void Update()
    {
        if (bucket == null) return;

        // 値の反映
        waterSlider.value = bucket.currentLiters;
        qualitySlider.value = bucket.quality;

        // 品質に応じた色
        float t = Mathf.InverseLerp(0f, 100f, bucket.quality);
        Color c = Color.Lerp(cleanColor, dirtyColor, t);

        if (qualityFill != null) qualityFill.color = c;

        // 残量バーの色も連動させたい場合は以下を有効化
        if (waterFill != null) waterFill.color = c;
    }
}
