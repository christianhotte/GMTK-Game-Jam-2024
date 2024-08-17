using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

[RequireComponent(typeof(SphereCollider))]
public abstract class Liftable : MonoBehaviour
{
    [SerializeField, Tooltip("The weight of the item (in kilograms.)")] protected float itemWeight;
    [SerializeField, Tooltip("The material for the outlines.")] protected Material outlineMat;

    private bool isInteractable;
    private bool isHeld;
    private Renderer objectRenderer;

    private List<Material> materialList;

    public virtual void Awake()
    {
        objectRenderer = GetComponent<Renderer>();
        materialList = new List<Material>();
        materialList = objectRenderer.materials.OfType<Material>().ToList();
    }

    protected virtual void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isInteractable = true;
            materialList.Add(outlineMat);
            objectRenderer.materials = materialList.ToArray();
        }    
    }

    protected virtual void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isInteractable = false;
            materialList.Remove(outlineMat);
            objectRenderer.materials = materialList.ToArray();
        }
    }

    /// <summary>
    /// Called when the player interacts with the object.
    /// </summary>
    public virtual void OnInteract()
    {
        if (isInteractable && !isHeld)
        {
            isHeld = true;
        }
    }

    /// <summary>
    /// Checks to see if the player can lift the object.
    /// </summary>
    /// <param name="playerWeightCapacity">The weight that the player can currently lift (in kilograms).</param>
    /// <returns>Returns true if the player can lift the item. Returns false if they cannot.</returns>
    public bool CanBeLifted(float playerWeightCapacity)
    {
        return playerWeightCapacity > itemWeight;
    }
}
