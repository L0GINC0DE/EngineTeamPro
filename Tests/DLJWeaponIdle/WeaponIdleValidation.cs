using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.SceneManagement;
using UnityEditor.U2D.Sprites;
using UnityEngine;
using UnityEngine.U2D.Animation;

public static class DLJ_SwordIdleValidation
{
    const string Sheet = "Assets/02.Sprites/DLJ/SwordIdle_DLJ_RedParticles.png";
    const string Library = "Assets/02.Sprites/DLJ/PlayerBodyLibrary.asset";
    const string Work = ".utmp/DLJ_SwordIdleIntegration";
    const string IdleClip = "Assets/02.Sprites/DLJ/Weapon/SwordIdle_DLJ.anim";
    const string OriginalSwordGuid = "fb2360a96ab93e14f9a8b62ad114b226";

    static void Check(bool value, string message) { if (!value) throw new Exception(message); }
    public static void Behavior()
    {
        try { VerifyBehavior(); Debug.Log("DLJ_SWORD_IDLE_BEHAVIOR_PASS"); }
        catch (Exception e) { Debug.LogException(e); EditorApplication.Exit(1); }
    }

    static void VerifyBehavior()
    {
        var clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(IdleClip);
        var sprites = AssetDatabase.LoadAllAssetsAtPath(Sheet).OfType<Sprite>().OrderBy(sprite => sprite.name).ToArray();
        Check(clip != null && clip.isLooping && clip.frameRate == 30f, "Sword idle must loop at 30 fps");
        Check(sprites.Length == 60 && Mathf.Abs(clip.length - 2f) < .001f, "Expected a two-second sword idle");
        var binding = EditorCurveBinding.PPtrCurve("", typeof(SpriteRenderer), "m_Sprite");
        var keys = AnimationUtility.GetObjectReferenceCurve(clip, binding);
        Check(keys != null && keys.Length == sprites.Length, "Missing sword idle keys");
        for (int index = 0; index < sprites.Length; index++)
        {
            Check(keys[index].value == sprites[index] && Mathf.Abs(keys[index].time - index / 30f) < .001f,
                "Incorrect sprite or duration at frame " + index);
        }
    }

    public static void ImportSheet()
    {
        try
        {
            var originalSword = AssetDatabase.GUIDToAssetPath(OriginalSwordGuid);
            var source = AssetDatabase.LoadAllAssetsAtPath(originalSword).OfType<Sprite>()
                .Single(sprite => sprite.name == "Frame_0");
            Check(source.rect.width == 30 && source.rect.height == 34, "Expected sword first pose trim");
            var pivot = new Vector2((source.pivot.x + 16 + 8) / 80, (source.pivot.y + 27 + 8) / 88);
            var importer = (TextureImporter)AssetImporter.GetAtPath(Sheet);
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Multiple;
            importer.spritePixelsPerUnit = source.pixelsPerUnit;
            importer.filterMode = FilterMode.Point;
            importer.mipmapEnabled = false;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.alphaIsTransparency = true;
            importer.npotScale = TextureImporterNPOTScale.None;
            var settings = new TextureImporterSettings();
            importer.ReadTextureSettings(settings);
            settings.spriteMeshType = SpriteMeshType.FullRect;
            settings.spriteGenerateFallbackPhysicsShape = false;
            importer.SetTextureSettings(settings);
            var factories = new SpriteDataProviderFactories(); factories.Init();
            var provider = factories.GetSpriteEditorDataProviderFromObject(importer);
            provider.InitSpriteEditorDataProvider();
            var rects = Enumerable.Range(0, 60).Select(i => new SpriteRect {
                name = "SwordIdle_DLJ_" + i.ToString("D3"),
                rect = new Rect(i % 10 * 80, (5 - i / 10) * 88, 80, 88),
                pivot = pivot, alignment = SpriteAlignment.Custom, spriteID = GUID.Generate()
            }).ToArray();
            provider.SetSpriteRects(rects);
            provider.GetDataProvider<ISpriteNameFileIdDataProvider>().SetNameFileIdPairs(rects.Select(r => new SpriteNameFileIdPair(r.name, r.spriteID)));
            provider.Apply(); importer.SaveAndReimport();
            var sprites = AssetDatabase.LoadAllAssetsAtPath(Sheet).OfType<Sprite>().OrderBy(s => s.name).ToArray();
            Check(sprites.Length == 60, "Expected 60 sword idle sprites");
            Directory.CreateDirectory(Work);
            var references = sprites.Select(s => {
                AssetDatabase.TryGetGUIDAndLocalFileIdentifier(s, out string guid, out long id);
                return "{fileID: " + id + ", guid: " + guid + ", type: 3}";
            });
            File.WriteAllLines(Work + "/sprite-references.txt", references);
            File.WriteAllText(Work + "/import-summary.txt", "PPU=" + source.pixelsPerUnit + "; old pivot=" + source.pivot + "; padded pivot=" + sprites[0].pivot);
            Debug.Log("DLJ_SWORD_IDLE_IMPORT_PASS " + File.ReadAllText(Work + "/import-summary.txt"));
        }
        catch (Exception e) { Debug.LogException(e); EditorApplication.Exit(1); }
    }

    public static void Integration()
    {
        try
        {
            VerifyBehavior();
            var scene = EditorSceneManager.OpenScene("Assets/00.Scenes/DLJ/PlayerTest.unity", OpenSceneMode.Single);
            var appearance = scene.GetRootGameObjects().SelectMany(g => g.GetComponentsInChildren<PlayerAppearance>(true)).Single();
            var weapon = (SpriteRenderer)new SerializedObject(appearance).FindProperty("weaponRenderer").objectReferenceValue;
            var controller = weapon.GetComponent<Animator>().runtimeAnimatorController as AnimatorController;
            Check(controller != null, "Weapon Animator Controller is missing");
            var idle = controller.layers[0].stateMachine.states.Single(child => child.state.name == "SwordIdle").state;
            Check(idle.motion == AssetDatabase.LoadAssetAtPath<AnimationClip>(IdleClip), "SwordIdle state is not using the sword idle clip");
            var library = AssetDatabase.LoadAssetAtPath<SpriteLibraryAsset>(Library);
            var first = AssetDatabase.LoadAllAssetsAtPath(Sheet).OfType<Sprite>().Single(sprite => sprite.name == "SwordIdle_DLJ_000");
            Check(library.GetSprite("Weapon", "Attack") == first, "Weapon library and idle clip must use the same first frame");
            Debug.Log("DLJ_SWORD_IDLE_INTEGRATION_PASS SwordIdle state, 60 frames, 30 fps");
        }
        catch (Exception e) { Debug.LogException(e); EditorApplication.Exit(1); }
    }
}
