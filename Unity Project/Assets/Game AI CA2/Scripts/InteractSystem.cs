// Author: Adam Kenny
// Student: Applied Computing (Game Development) 3rd Year (20102588)
// Date Created: 2025-07-16
// Description: Handles player interaction with objects in the scene using raycasting and an interaction icon.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// Interface for interactable objects.
public interface IInteractable
{
    void Interact();
}

public class InteractSystem : MonoBehaviour
{
    public static InteractSystem Instance;
    public Transform rayOrigin;
    public float interactRange = 5f;  // How far the ray can reach
    public Image interactIcon;

    public IInteractable currentInteractable;

    private void Awake()
    {
        if (!Instance)
            Instance = this;
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    public void Start()
    {
         // Pull the HUD icon if HUD already exists
        if (HUD.Instance != null)
        {
            interactIcon = HUD.Instance.interactIcon;
            interactIcon.enabled = false;
        }
    }

    // Checks for interactable objects every frame.
    void Update()
    {

        Ray ray = new Ray(rayOrigin.position, rayOrigin.forward);

        interactIcon.enabled = false;
        currentInteractable = null;

        if (Physics.Raycast(ray, out RaycastHit hit, interactRange))
        {
            IInteractable interactable = hit.collider.GetComponentInParent<IInteractable>();

            if (interactable != null)
            {
                interactIcon.enabled = true;
                currentInteractable = interactable;

                if (Input.GetKeyDown(KeyCode.E))
                {
                    interactable.Interact();
                }

                return;
            }
        }

        ClearCurrentInteractable();
    }

    void ClearCurrentInteractable()
    {
        interactIcon.enabled = false;
        currentInteractable = null;
    }
}
