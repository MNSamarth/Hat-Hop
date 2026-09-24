using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace HatHop.Editor
{
    public static class MovementCameraSetup
    {
        [MenuItem("Hat Hop/Apply Movement and Camera Update")]
        public static void Apply()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                Debug.LogWarning("Stop Play Mode before applying movement and camera setup.");
                return;
            }
            Scene scene = SceneManager.GetActiveScene();
            if (!scene.IsValid() || !scene.isLoaded) return;
            List<PlayerMotor2D> players = FindInScene<PlayerMotor2D>(scene);
            if (players.Count != 1)
            {
                Debug.LogError("Open a Hat Hop scene with exactly one PlayerMotor2D, such as GameplayTest.");
                return;
            }
            PlayerMotor2D motor = players[0];
            List<Camera> cameras = FindInScene<Camera>(scene);
            Camera camera = cameras.Find(candidate => candidate.CompareTag("MainCamera"));
            if (camera == null && cameras.Count == 1) camera = cameras[0];
            if (camera == null)
            {
                Debug.LogError("The active scene needs a Main Camera.");
                return;
            }
            List<RotationController> controllers = FindInScene<RotationController>(scene);
            if (controllers.Count > 1)
            {
                Debug.LogError("Use a scene with at most one RotationController.");
                return;
            }
            RotationController rotation = controllers.Count == 1 ? controllers[0] : null;
            SpriteRenderer original = motor.GetComponent<SpriteRenderer>();
            if (original == null || original.sprite == null)
            {
                Debug.LogError("This setup expects the starter's SpriteRenderer on the Player root.");
                return;
            }
            Transform visual = motor.transform.Find("Visual");
            if (visual != null && visual.GetComponent<SpriteRenderer>() == null)
            {
                Debug.LogError("Player already has a Visual child without a SpriteRenderer. Rename it before applying.");
                return;
            }
            if (!EditorUtility.DisplayDialog("Apply movement and camera setup?",
                "This adds a visual-only hop and a 2x follow camera to the open scene. " +
                "It preserves platforms and gameplay references. You can undo the scene changes or close without saving.",
                "Apply", "Cancel")) return;

            Undo.IncrementCurrentGroup();
            int group = Undo.GetCurrentGroup();
            Undo.SetCurrentGroupName("Apply Hat Hop movement and camera");
            if (visual == null)
            {
                GameObject child = new GameObject("Visual");
                Undo.RegisterCreatedObjectUndo(child, "Create player Visual");
                Undo.SetTransformParent(child.transform, motor.transform, "Parent player Visual");
                child.transform.localPosition = Vector3.zero;
                child.transform.localRotation = Quaternion.identity;
                child.transform.localScale = Vector3.one;
                visual = child.transform;
                Undo.AddComponent<SpriteRenderer>(child);
            }
            SpriteRenderer sprite = visual.GetComponent<SpriteRenderer>();
            Undo.RecordObject(sprite, "Copy player appearance");
            sprite.sprite = original.sprite;
            sprite.color = original.color;
            sprite.sharedMaterial = original.sharedMaterial;
            sprite.sortingLayerID = original.sortingLayerID;
            sprite.sortingOrder = original.sortingOrder;
            sprite.flipX = original.flipX;
            sprite.flipY = original.flipY;
            sprite.enabled = true;
            Undo.RecordObject(original, "Hide root renderer");
            original.enabled = false;

            PlayerHopVisual animator = motor.GetComponent<PlayerHopVisual>();
            if (animator == null) animator = Undo.AddComponent<PlayerHopVisual>(motor.gameObject);
            Undo.RecordObject(animator, "Connect visual hop");
            animator.Configure(motor, visual);
            SerializedObject motorSettings = new SerializedObject(motor);
            motorSettings.FindProperty("jumpBufferSeconds").floatValue = 0.12f;
            motorSettings.FindProperty("coyoteSeconds").floatValue = 0.08f;
            motorSettings.ApplyModifiedProperties();

            const string physicsFolder = "Assets/HatHop/Physics";
            const string materialPath = physicsFolder + "/GroundedPlayer.physicsMaterial2D";
            Directory.CreateDirectory(physicsFolder);
            AssetDatabase.Refresh();
            PhysicsMaterial2D material = AssetDatabase.LoadAssetAtPath<PhysicsMaterial2D>(materialPath);
            if (material == null)
            {
                material = new PhysicsMaterial2D("GroundedPlayer") { friction = 0, bounciness = 0 };
                AssetDatabase.CreateAsset(material, materialPath);
            }
            else
            {
                Undo.RecordObject(material, "Set player friction");
                material.friction = 0;
                material.bounciness = 0;
                EditorUtility.SetDirty(material);
            }
            Collider2D collider = motor.GetComponent<Collider2D>();
            Undo.RecordObject(collider, "Set player collision material");
            collider.sharedMaterial = material;

            PlayerFollowCamera follow = camera.GetComponent<PlayerFollowCamera>();
            float overview = follow == null ? camera.orthographicSize : follow.OverviewSize;
            if (follow == null) follow = Undo.AddComponent<PlayerFollowCamera>(camera.gameObject);
            Undo.RecordObject(follow, "Connect follow camera");
            follow.Configure(motor, rotation, overview);
            Undo.RecordObject(camera, "Set camera zoom");
            Undo.RecordObject(camera.transform, "Place camera at player");
            if (camera.transform.parent != null)
                Undo.SetTransformParent(camera.transform, null, "Keep camera outside rotating map");
            camera.orthographic = true;
            camera.orthographicSize = overview / 2f;
            Vector3 position = motor.transform.position;
            camera.transform.SetPositionAndRotation(new Vector3(position.x, position.y, -10), Quaternion.identity);
            Undo.CollapseUndoOperations(group);
            EditorSceneManager.MarkSceneDirty(scene);
            AssetDatabase.SaveAssets();
            Selection.activeGameObject = motor.gameObject;
            Debug.Log("Movement/camera setup applied. Save the scene, then test A/D + Space. " +
                "The root collider stays grounded; only Visual hops. Reapplying does not halve zoom again.");
        }

        private static List<T> FindInScene<T>(Scene scene) where T : Component
        {
            List<T> result = new List<T>();
            foreach (GameObject root in scene.GetRootGameObjects())
                result.AddRange(root.GetComponentsInChildren<T>(true));
            return result;
        }
    }
}
