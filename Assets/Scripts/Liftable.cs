using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

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

    /// <summary>
    /// Called when the player is able to interact with the liftable.
    /// </summary>
    protected void EnterInteractable()
    {
        isInteractable = true;
        materialList.Add(outlineMat);
        objectRenderer.materials = materialList.ToArray();
    }

    /// <summary>
    /// Called when the player exits the range that allows them to interact with the liftable.
    /// </summary>
    protected void ExitInteractable()
    {
        isInteractable = false;
        materialList.Remove(outlineMat);
        objectRenderer.materials = materialList.ToArray();
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
