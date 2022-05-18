using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerControls : MonoBehaviour
{
    public List<ControlsGroup> controlsGroups = new List<ControlsGroup>() { new ControlsGroup()};

    [System.Serializable]
    public class ControlsGroup 
    {
        public string name = "Default";
        public int id = 1;
        public List<Button> controls;
    }

    public void ActivateControls(int controlsGroupId)
    {
        DeactivateAllControls();
        for (int i = 0; i < controlsGroups.Count; i++)
        {
            if (controlsGroups[i].id == controlsGroupId)
            {
                for (int j = 0; j < controlsGroups[i].controls.Count; j++) { controlsGroups[i].controls[j].gameObject.SetActive(true); }
            }
        }
    }

    public void DeactivateAllControls()
    {
        for (int i = 0; i < controlsGroups.Count; i++)
        {
            for (int j = 0; j < controlsGroups[i].controls.Count; j++)
            {
                controlsGroups[i].controls[j].gameObject.SetActive(false);
            }
        }
    }

    public void EnableControls( int controlsGroupId )     
    {
        ActivateControls(controlsGroupId);
        DisableAllControls();
        for (int i = 0; i < controlsGroups.Count; i++)
        {
            if (controlsGroups[i].id == controlsGroupId)
            {
                for (int j = 0; j < controlsGroups[i].controls.Count; j++) { controlsGroups[i].controls[j].interactable = true; }
            }
        }
    }

    public void EnableControls(int controlsGroupId, float enableTime)
    {
        EnableControls(controlsGroupId);
        StartCoroutine("DisableControlsAutomatically", enableTime);
    }

    public void EnableControls(string controlsGroupName, float enableTime)
    {
        for (int i = 0; i < controlsGroups.Count; i++)
        {
            if (controlsGroups[i].name == controlsGroupName) { EnableControls(controlsGroups[i].id, enableTime); break; }
        }
    }

    public void DisableAllControls() 
    {
        for (int i = 0; i < controlsGroups.Count; i++)
        {
            for (int j = 0; j < controlsGroups[i].controls.Count; j++) 
            { 
                controlsGroups[i].controls[j].interactable = false; 
            }
        }
    }

    IEnumerator DisableControlsAutomatically(float wait)
    {
        yield return new WaitForSeconds(wait);
        DisableAllControls();
    }
}
