using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

[System.Serializable]
public class Instruction
{
    public string instructionText;
    public float duration;
}

public class InstructionsManager : MonoBehaviour
{
    [SerializeField, Tooltip("The text instructions in order.")] private Instruction[] textInstructions;
    [SerializeField, Tooltip("The text object to write to.")] private TextMeshProUGUI instructionText;
    [SerializeField, Tooltip("Text flash speed.")] private float flashSpeed;
    [SerializeField, Tooltip("If true, the instructions flash on screen.")] private bool flashText;

    private bool instructionsComplete = false;
    private float currentTimer;
    private float currentFlashTimer;
    private int currentInstruction;
    private CanvasGroup textCanvasGroup;

    private void Start()
    {
        ProgressInstructions(currentInstruction);
        textCanvasGroup = instructionText.GetComponent<CanvasGroup>();
    }

    private void Update()
    {
        if (!instructionsComplete)
        {
            currentTimer += Time.deltaTime;

            if (currentTimer > textInstructions[currentInstruction].duration)
            {
                currentInstruction++;

                if (currentInstruction >= textInstructions.Length)
                {
                    instructionsComplete = true;
                    textCanvasGroup.alpha = 0;
                }

                else
                    ProgressInstructions(currentInstruction);

                currentTimer = 0;
            }

            if(flashText)
                FlashText();
        }
    }

    public void ProgressInstructions(int currentInstruction)
    {
        instructionText.text = textInstructions[currentInstruction].instructionText;
        currentTimer = 0;
    }

    public void FlashText()
    {
        currentFlashTimer += Time.deltaTime;

        if(currentFlashTimer > flashSpeed)
        {
            textCanvasGroup.alpha = textCanvasGroup.alpha == 0 ? 1 : 0;
            currentFlashTimer = 0;
        }
    }

}
