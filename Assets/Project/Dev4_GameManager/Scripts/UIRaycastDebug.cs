using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class UIRaycastDebug : MonoBehaviour
{
    private void Update()
    {
        if (!Input.GetMouseButtonDown(0)) return;

        if (EventSystem.current == null)
        {
            Debug.LogError("[UIRaycastDebug] EventSystem.current is NULL. Scene không có EventSystem hoạt động.");
            return;
        }

        PointerEventData pointerData = new PointerEventData(EventSystem.current)
        {
            position = Input.mousePosition
        };

        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(pointerData, results);

        Debug.Log($"[UIRaycastDebug] Mouse Position = {Input.mousePosition}");
        Debug.Log($"[UIRaycastDebug] Hit count = {results.Count}");

        foreach (RaycastResult result in results)
        {
            Debug.Log(
                $"[UIRaycastDebug] Hit: {result.gameObject.name} | " +
                $"Module: {result.module?.name} | " +
                $"Sorting: {result.sortingLayer}/{result.sortingOrder} | " +
                $"Depth: {result.depth}",
                result.gameObject
            );
        }
    }
}