using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public abstract class Liftable : MonoBehaviour
{
    [SerializeField, Tooltip("The weight of the item (in kilograms.)")] protected float itemWeight;
    [SerializeField, Tooltip("The material for the outlines.")] protected Material outlineMat;
    [SerializeField, Tooltip("Movement height.")] private float liftHeight = 5f;
    [SerializeField, Tooltip("The time it takes to lift the liftable.")] private float liftTime = 1f;
    [SerializeField, Tooltip("The time it takes to hold the liftable.")] private float holdTime = 0.25f;
    [SerializeField, Tooltip("The time it takes to lower the liftable.")] private float lowerTime = 1f;

    private bool isInteractable;
    private bool isHeld;
    private Renderer[] objectRenderers;

    public virtual void Awake()
    {
        objectRenderers = GetComponentsInChildren<Renderer>();
    }

    /// <summary>
    /// Called when the player is able to interact with the liftable.
    /// </summary>
    public void EnterInteractable()
    {
        if (!isInteractable)
        {
            foreach (Renderer r in objectRenderers)
            {
                List<Material> matList = new List<Material>(r.materials);
                matList.Add(outlineMat);
                r.materials = matList.ToArray();
            }
        }
        
        isInteractable = true;
    }

    /// <summary>
    /// Called when the player exits the range that allows them to interact with the liftable.
    /// </summary>
    public void ExitInteractable()
    {
        if (isInteractable)
        {
            foreach (Renderer r in objectRenderers)
            {
                List<Material> matList = new List<Material>(r.materials);
                matList.RemoveAt(1);
                r.materials = matList.ToArray();
            }
        }
        isInteractable = false;
    }

    /// <summary>
    /// Called when the player interacts with the object.
    /// </summary>
    public virtual void OnInteract()
    {
        if (isInteractable && !isHeld)
        {
            isHeld = true;
            LiftObject();
        }
    }

    private void LiftObject()
    {
        Debug.Log("Lifting object...");
        LTDescr liftableRep = LeanTween.delayedCall(liftTime + holdTime + lowerTime, () => LeanTween.moveLocalY(gameObject, transform.localPosition.y + liftHeight, liftTime).setOnComplete(() =>
     LeanTween.delayedCall(holdTime, () => LeanTween.moveLocalY(gameObject, transform.localPosition.y - liftHeight, lowerTime)))).setRepeat(-1);
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
