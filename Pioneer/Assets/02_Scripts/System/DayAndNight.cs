using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class DayAndNight : MonoBehaviour
{
    public Volume volume;
    private ColorAdjustments colorAdjustments;

    public Gradient dayToNight;
    public Gradient nightToDay;
    public AnimationCurve exposureByTime;
    public float dayDuration = 60f; // �Ϸ� �ð� (��)

    private float timer;

    void Start()
    {
        volume.profile.TryGet(out colorAdjustments);
    }

    void Update()
    {
        timer += Time.deltaTime;
        float t = (timer % dayDuration) / dayDuration;

        colorAdjustments.colorFilter.value = nightToDay.Evaluate(t);
        colorAdjustments.postExposure.value = exposureByTime.Evaluate(t);
    }
}
