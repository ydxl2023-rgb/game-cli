using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Oathx.Core;
using Oathx.Entity;
using Oathx.Expression;
using Oathx.Widget;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>Verifies facility entrances through actual graphics and production button handlers.</summary>
public static class MiniwowFacilityProbe
{
    /// <summary>Original logical position saved before the explicit placement fixture.</summary>
    private static Vector3 originalPosition;

    /// <summary>Player instance whose position belongs to the fixture.</summary>
    private static ActorEntity placedPlayer;

    /// <summary>Places only the test player at an authored entrance without altering saved progression.</summary>
    /// <param name="buildingId">Original building identity whose trigger is being inspected.</param>
    /// <returns>Fixture position and target used for the upcoming real UI click.</returns>
    public static string PlaceAtResident(int buildingId)
    {
        var scene = UnityEngine.Object.FindObjectOfType<HomeScene>();
        var binding = new SerializedObject(scene);
        var rows = binding.FindProperty("npcSelections");
        for (int index = 0; index < rows.arraySize; index++)
        {
            var row = rows.GetArrayElementAtIndex(index);
            if (row.FindPropertyRelative("buildingId").intValue != buildingId)
                continue;

            var entrance = (Transform)row.FindPropertyRelative("entrance").objectReferenceValue;
            Vector3 center = entrance.TransformPoint(row.FindPropertyRelative("center").vector3Value);
            if (!NavMesh.SamplePosition(center, out var ground, 3f, NavMesh.AllAreas))
                throw new InvalidOperationException("No reachable ground at resident: " + buildingId);

            var home = XEntityManager.GetSingleton().QueryEntity<HomeEntity>();
            var player = home.FindActor(4);
            if (!ReferenceEquals(placedPlayer, player))
            {
                placedPlayer = player;
                originalPosition = player.position;
            }

            player.SetMoveInput(Vector3.zero);
            typeof(ActorEntity).GetProperty(nameof(ActorEntity.position)).SetValue(player, ground.position);

            return JsonConvert.SerializeObject(new {
                fixture = "Editor-only logical placement; no task, inventory or progression changes",
                buildingId,
                position = ground.position.ToString(),
                entrance = center.ToString()
            });
        }

        throw new InvalidOperationException("Missing resident binding: " + buildingId);
    }

    /// <summary>Restores the logical player position after placement-based entry verification.</summary>
    public static void RestorePlayer()
    {
        var home = XEntityManager.GetSingleton().QueryEntity<HomeEntity>();
        var player = home.FindActor(4);
        if (!ReferenceEquals(placedPlayer, player))
            throw new InvalidOperationException("The placement fixture does not belong to this Home session.");

        player.SetMoveInput(Vector3.zero);
        typeof(ActorEntity).GetProperty(nameof(ActorEntity.position)).SetValue(player, originalPosition);
        placedPlayer = null;
    }

    /// <summary>Returns the precise raycast ordering before clicking a visible facility control.</summary>
    /// <param name="name">Unique authored button name.</param>
    /// <returns>Control state, screen position and every intersecting graphic.</returns>
    public static string InspectButton(string name)
    {
        var button = UnityEngine.Object.FindObjectsOfType<Button>(true).Single(item => item.name == name);
        var rect = (RectTransform)button.transform;
        var canvas = button.GetComponentInParent<Canvas>();
        var camera = canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera;
        Vector2 point = RectTransformUtility.WorldToScreenPoint(camera, rect.TransformPoint(rect.rect.center));

        var pointer = new PointerEventData(EventSystem.current) {
            position = point,
            button = PointerEventData.InputButton.Left
        };

        var hits = new List<RaycastResult>();
        EventSystem.current.RaycastAll(pointer, hits);

        return JsonConvert.SerializeObject(new {
            name,
            active = button.gameObject.activeInHierarchy,
            button.interactable,
            screen = point.ToString(),
            hits = hits.Select(hit => new {
                graphic = hit.gameObject.name,
                handler = ExecuteEvents.GetEventHandler<IPointerClickHandler>(hit.gameObject)?.name,
                hit.sortingOrder,
                hit.depth
            })
        });
    }

    /// <summary>Clicks a currently visible button after proving it is the foremost clickable graphic.</summary>
    /// <param name="name">Unique authored button name.</param>
    /// <returns>Whether the real Unity button received its pointer events.</returns>
    public static bool Click(string name)
    {
        return MiniwowHospitalProbe.Click(name);
    }

    /// <summary>Exercises canceled and replaced guide identities before deferred system activation resumes.</summary>
    /// <returns>Immediate ownership assertions; call AssertCancelledActivation after several frames.</returns>
    public static string BeginCancellationRace()
    {
        AssertCancelledActivation();
        var original = new TaskPresentationAction(TaskPresentation.Navigate, new ExpressionValue(60019));
        original.Start();

        XPluginUtility.FireEvent(new HomeResidentNavigationFinished {
            action = original,
            buildingId = 60019,
            arrived = true
        });

        original.Cancel();
        var replacement = new TaskPresentationAction(TaskPresentation.Navigate, new ExpressionValue(60019));
        replacement.Start();

        XPluginUtility.FireEvent(new HomeResidentNavigationFinished {
            action = original,
            buildingId = 60019,
            arrived = true
        });

        XPluginUtility.FireEvent(new HomeResidentSystemReady { action = original, buildingId = 60019 });
        if (replacement.status != TaskActionStatus.Running)
            throw new InvalidOperationException("An old completion consumed its replacement guide.");

        replacement.Cancel();
        AssertCancelledActivation();

        return "Canceled original and replacement; old arrival and activation were ignored synchronously.";
    }

    /// <summary>Checks that a canceled deferred request did not later open a facility.</summary>
    /// <returns>True when both pages remain closed.</returns>
    public static bool AssertCancelledActivation()
    {
        var widgets = UnityEngine.Object.FindObjectsOfType<IWidget>();
        if (widgets.Any(widget => widget is UIShop || widget.GetType().Name == "UIInn"))
            throw new InvalidOperationException("A facility opened during canceled navigation verification.");

        return true;
    }

    /// <summary>Exercises same-frame cancellation and replacement against an already installed facility.</summary>
    /// <param name="buildingId">Installed shop or inn building identity.</param>
    /// <returns>Whether cancellation happened before the page appeared and the old handle's final state.</returns>
    public static string BeginPageReplacementRace(int buildingId)
    {
        var original = new TaskPresentationAction(TaskPresentation.Navigate, new ExpressionValue(buildingId));
        if (buildingId == 60019)
        {
            XPluginUtility.FireEvent(new ShopOpenRequested { guide = original });
            XPluginUtility.FireEvent(new ShopOpenRequested { guide = original });
        }
        else if (buildingId == 60017)
        {
            XPluginUtility.FireEvent(new InnOpenRequested { guide = original });
            XPluginUtility.FireEvent(new InnOpenRequested { guide = original });
        }
        else
            throw new ArgumentOutOfRangeException(nameof(buildingId));

        string widgetName = buildingId == 60019 ? nameof(UIShop) : "UIInn";
        bool appeared = UnityEngine.Object.FindObjectsOfType<IWidget>()
            .Any(widget => widget.GetType().Name == widgetName);
        original.Cancel();
        if (buildingId == 60019)
        {
            XPluginUtility.FireEvent(new ShopOpenRequested());
            XPluginUtility.FireEvent(new ShopOpenRequested());
        }
        else
        {
            XPluginUtility.FireEvent(new InnOpenRequested());
            XPluginUtility.FireEvent(new InnOpenRequested());
        }

        return JsonConvert.SerializeObject(new {
            buildingId,
            pendingWindowExercised = !appeared,
            originalStatus = original.status.ToString(),
            expectation = "After several frames, one initialized manual page remains open."
        });
    }

    /// <summary>Reports facility visibility, live scene input and committed economy after each operation.</summary>
    /// <returns>A detached snapshot without invoking any business operation.</returns>
    public static string Snapshot()
    {
        var scene = UnityEngine.Object.FindObjectOfType<HomeScene>();
        var home = XEntityManager.GetSingleton().QueryEntity<HomeEntity>();
        var ui = UISystem.GetSingleton();
        var opening = UnityEngine.Object.FindObjectOfType<HomeOpeningScene>();
        var sceneBinding = new SerializedObject(opening);
        var hotel = (GameObject)sceneBinding.FindProperty("hotelBuilt").objectReferenceValue;
        var models = sceneBinding.FindProperty("hotelModels");
        var hotelModels = new List<object>();
        for (int index = 0; index < models.arraySize; index++)
        {
            var model = (GameObject)models.GetArrayElementAtIndex(index).objectReferenceValue;
            hotelModels.Add(new { model.name, active = model.activeInHierarchy });
        }

        var widgets = UnityEngine.Object.FindObjectsOfType<IWidget>()
            .Select(widget => widget.GetType().Name).OrderBy(name => name).ToArray();

        return JsonConvert.SerializeObject(new {
            acceptsInput = scene.AcceptsInput(),
            player = home.FindActor(4).position.ToString(),
            merchantVisible = scene.IsNpcEntryVisible(60019),
            hotelVisible = scene.IsNpcEntryVisible(60017),
            shopOpen = ui.TryGetWidget<UIShop>(out _),
            hotelBuilt = hotel && hotel.activeInHierarchy,
            hotelModels,
            widgets,
            village = home.village.Snapshot()
        });
    }
}
