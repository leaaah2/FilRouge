using UnityEngine;
using UnityEngine.InputSystem;

public class WeatherKeyboardScrubber : MonoBehaviour
{
    public WeatherSkyController skyController;
    public float minutesPerTap = 10f;
    public float hoursPerTap = 1f;

    private void Update()
    {
        if (skyController == null || !skyController.IsReady()) return;

        Keyboard kb = Keyboard.current;
        if (kb == null) return;

        if (kb.leftArrowKey.wasPressedThisFrame)
            skyController.NudgeMinutes(-hoursPerTap * 60f);

        if (kb.rightArrowKey.wasPressedThisFrame)
            skyController.NudgeMinutes(hoursPerTap * 60f);

        if (kb.downArrowKey.wasPressedThisFrame)
            skyController.NudgeMinutes(-minutesPerTap);

        if (kb.upArrowKey.wasPressedThisFrame)
            skyController.NudgeMinutes(minutesPerTap);
    }
}