using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class ProgressionSystem : MonoBehaviour
{
    [SerializeField] private Game_UI game_UI;
    [SerializeField] private PlaneController planeController;

    private bool floorComplete = false;
    private bool ceilingComplete = false;

    private void Start()
    {
        planeController.FloorSet.AddListener(OnFloorSet);
        StartCoroutine(InstructionProgression());
    }

    private void OnFloorSet()
    {
        floorComplete = true;
    }

    private void OnCeilingSet()
    {
        ceilingComplete = true;
    }

    private IEnumerator InstructionProgression()
    {
        game_UI.ShowFloorInstructions();
        yield return new WaitUntil(() => floorComplete);
        game_UI.ShowCeilingInstructions();
        yield return new WaitUntil(() => ceilingComplete);
        game_UI.ShowWallInstructions();
    }


}
