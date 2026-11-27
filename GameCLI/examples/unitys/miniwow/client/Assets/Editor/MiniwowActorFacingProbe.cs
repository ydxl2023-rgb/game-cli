using System.Linq;
using Newtonsoft.Json;
using Spine.Unity;
using Oathx.Entity;
using UnityEditor;
using UnityEngine;

/// <summary>Reports the authored visual axes used by actor facing.</summary>
public static class MiniwowActorFacingProbe
{
    /// <summary>Exercises both horizontal directions on the real hero and player views using isolated actor data.</summary>
    /// <returns>Whether both models face the same screen direction as their movement.</returns>
    public static bool VerifyDirections()
    {
        var home = XEntityManager.GetSingleton().QueryEntity<HomeEntity>();
        foreach (int id in new[] { 1, 4 })
        {
            string name = id == 1 ? "TownVisitor" : "TownPlayer";
            var shape = Object.FindObjectsOfType<ActorShape>(true).Single(item => item.name == name);
            var fields = new SerializedObject(shape);
            var camera = (Camera)fields.FindProperty("sceneCamera").objectReferenceValue;
            var animation = (SkeletonAnimation)fields.FindProperty("animationView").objectReferenceValue;
            var original = home.FindActor(id);
            try
            {
                foreach (int sign in new[] { -1, 1 })
                {
                    Vector3 start = original.position;

                    var spawn = new ActorSpawn {
                        actorId = 99,
                        configId = original.configId,
                        destinationId = 0,
                        moveSpeed = 1f,
                        route = new[] { start }
                    };

                    var sample = new ActorEntity();
                    sample.Initialize(spawn);
                    sample.FollowRoute(new[] { start, start + camera.transform.right * sign });
                    sample.Update(0.1f);
                    shape.Bind(sample);
                    float screenDirection = Vector3.Dot(camera.transform.right,
                        animation.transform.TransformVector(Vector3.right * animation.Skeleton.ScaleX));
                    sample.Destroy();
                    if (screenDirection * sign <= 0f)
                        throw new System.InvalidOperationException(name + " faces opposite its movement.");
                }
            }
            finally
            {
                shape.Bind(original);
            }
        }

        return true;
    }

    /// <summary>Inspects actual world and screen axes without changing any actor.</summary>
    /// <returns>Facing evidence for each live actor presentation.</returns>
    public static string Inspect()
    {
        var shapes = Object.FindObjectsOfType<ActorShape>(true);

        return JsonConvert.SerializeObject(shapes.Select(shape => {
            var fields = new SerializedObject(shape);
            var camera = (Camera)fields.FindProperty("sceneCamera").objectReferenceValue;
            var animation = (SkeletonAnimation)fields.FindProperty("animationView").objectReferenceValue;

            return new {
                name = shape.name,
                rotation = animation.transform.eulerAngles.ToString(),
                scale = animation.transform.lossyScale.ToString(),
                cameraRotation = camera.transform.eulerAngles.ToString(),
                screenRightAlignment = Vector3.Dot(camera.transform.right, animation.transform.right),
                skeletonScaleX = animation.Skeleton.ScaleX
            };
        }));
    }
}
