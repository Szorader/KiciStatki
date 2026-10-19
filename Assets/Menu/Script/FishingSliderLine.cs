using UnityEngine;
using UnityEngine.UI;

public class SliderScrollSync : MonoBehaviour
{
    [SerializeField] private Slider slider;
    [SerializeField] private ScrollRect scrollRect;

    private void Start()
    {
        slider.minValue = 0f;
        slider.maxValue = 1f;

        slider.onValueChanged.AddListener(OnSliderChanged);
        scrollRect.onValueChanged.AddListener(OnScrollChanged);

        slider.SetValueWithoutNotify(1f - scrollRect.verticalNormalizedPosition);
    }

    
    private void OnSliderChanged(float value)
    {
        scrollRect.verticalNormalizedPosition = 1f - value;
    }

    private void OnScrollChanged(Vector2 position)
    {
        slider.SetValueWithoutNotify(1f - position.y);
    }


    private void OnDestroy()
    {
        slider.onValueChanged.RemoveListener(OnSliderChanged);
        scrollRect.onValueChanged.RemoveListener(OnScrollChanged);
    }
}