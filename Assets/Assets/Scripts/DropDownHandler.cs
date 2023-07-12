using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DropDownHandler : MonoBehaviour
{
    public int destination;
    TMP_Dropdown dropdown;

    private void Start()
    {
        dropdown = GetComponent<TMP_Dropdown>();
        HandleDropdownData(dropdown);
        dropdown.onValueChanged.AddListener(delegate { HandleDropdownData(dropdown); } );
    }
    private void HandleDropdownData(TMP_Dropdown dropdown)
    {
        destination = dropdown.options.Count - dropdown.value - 1;
    }
}
