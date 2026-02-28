using UnityEngine;
using UnityEngine.Rendering.Universal.Internal;
using UnityEngine.UI;
using System.Collections;
public class ProgressBar : MonoBehaviour
{
    [SerializeField] private Image mask;
    [SerializeField] private GameObject progessBar;

    private float currentFill;
    private float maxfill;
    private float lastfill;
    private float fillAmount;

    [SerializeField] private float approxRangeCheck = 1f;

    [SerializeField] private Camera cam;

    private void Awake()
    {
        mask ??= GameObject.FindGameObjectWithTag("Mask").GetComponent<Image>();
        cam = Camera.main;
    }

    // Update is called once per frame
    void Update()
    {
        ProgressBarFaceMainCamera();
    }
    /// <summary>
    /// using the game object for progress bar to look at the main camera
    /// </summary>
    private void ProgressBarFaceMainCamera()
    {
        if (cam == null) cam = Camera.main;
        progessBar.transform.LookAt(cam.transform);
    }
    /// <summary>
    /// Setting the max capacity of the bar
    /// </summary>
    /// <param name="max"></param>
    public void SetFillMax(float max)
    {
        maxfill = max;
    }
    /// <summary>
    /// updating the bar based off approximately range check (default 1f) and updates mask fill amount
    /// </summary>
    /// <param name="current"></param>
    public void UpdateBar(float current)
    {
        currentFill = current;
        fillAmount = current / maxfill;

        if (fillAmount != lastfill || Mathf.Approximately(fillAmount, approxRangeCheck))
        {
            mask.fillAmount = fillAmount;
            lastfill = fillAmount;
        }
    }
}
