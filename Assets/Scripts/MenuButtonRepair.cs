using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

/// <summary>
/// Safety net for the shared menu prefabs (win, fail, pause). A prefab can't store a link to
/// the GameManager in a scene, so each level keeps those links as overrides, and reverting or
/// applying the prefab can silently drop them, leaving buttons that do nothing.
/// At level start, any menu button whose OnClick names a GameManager method but has lost its
/// target is reconnected to the current GameManager. Buttons that are wired correctly are untouched.
/// </summary>
public static class MenuButtonRepair
{
    /// <summary>Reconnects unwired buttons under <paramref name="menus"/>. Returns how many were fixed.</summary>
    public static int Repair(GameManager game, params GameObject[] menus)
    {
        int repaired = 0;
        var methodsOnButton = new HashSet<string>();

        foreach (GameObject menu in menus)
        {
            if (menu == null) continue;

            foreach (Button button in menu.GetComponentsInChildren<Button>(true))
            {
                methodsOnButton.Clear();
                Button.ButtonClickedEvent onClick = button.onClick;

                // Methods that are still wired correctly must not be added a second time.
                for (int i = 0; i < onClick.GetPersistentEventCount(); i++)
                {
                    if (onClick.GetPersistentTarget(i) != null) methodsOnButton.Add(onClick.GetPersistentMethodName(i));
                }

                for (int i = 0; i < onClick.GetPersistentEventCount(); i++)
                {
                    if (onClick.GetPersistentTarget(i) != null) continue;

                    string method = onClick.GetPersistentMethodName(i);
                    UnityAction action = ActionFor(game, method);

                    // One call per method, even if the prefab lists the same call twice.
                    if (action == null || !methodsOnButton.Add(method)) continue;

                    onClick.AddListener(action);
                    repaired++;
                }
            }
        }

        if (repaired > 0) Debug.Log($"MenuButtonRepair: reconnected {repaired} menu button call(s) to the GameManager.");
        return repaired;
    }

    private static UnityAction ActionFor(GameManager game, string method)
    {
        switch (method)
        {
            case nameof(GameManager.NextLevel): return () => game.NextLevel(0);
            case nameof(GameManager.GoHome): return game.GoHome;
            case nameof(GameManager.RestartGame):
            case "Restart": return game.RestartGame; // older name still in the menu prefabs
            case nameof(GameManager.ResumeGame):
            case "Resume": return game.ResumeGame;   // older name still in the pause prefab
            case nameof(GameManager.PauseGame): return game.PauseGame;
            default: return null;
        }
    }
}
