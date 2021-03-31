using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using TMPro;

public class QualityDropdownManager : MonoBehaviour
{
    public RenderPipelineAsset[] rpAssets;
    public TMP_Dropdown dropdown;

    private void Start()
    {
        dropdown.value = QualitySettings.GetQualityLevel();
    }

    public void ChangeLevel(int value)
    {
        QualitySettings.SetQualityLevel(value);
        QualitySettings.renderPipeline = rpAssets[value];
    }
}
