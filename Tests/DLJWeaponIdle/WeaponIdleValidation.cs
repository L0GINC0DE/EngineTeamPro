using System;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.U2D.Sprites;
using UnityEngine;
using UnityEngine.U2D.Animation;

public static class DLJ_SwordIdleValidation
{
    const string Sheet = "Assets/02.Sprites/DLJ/SwordIdle_DLJ_RedParticles.png";
    const string Library = "Assets/02.Sprites/DLJ/PlayerBodyLibrary.asset";
    const string Work = ".utmp/DLJ_SwordIdleIntegration";
    static readonly BindingFlags Private = BindingFlags.Instance | BindingFlags.NonPublic;

    static void Check(bool value, string message) { if (!value) throw new Exception(message); }
    static void Field(object target, string name, object value)
    {
        var field = target.GetType().GetField(name, Private);
        Check(field != null, "Missing sword idle integration field: " + name);
        field.SetValue(target, value);
    }
    static void Tick(PlayerAppearance appearance, float delta)
    {
        var method = typeof(PlayerAppearance).GetMethod("AdvanceWeaponIdle", Private);
        Check(method != null, "Missing weapon idle playback");
        method.Invoke(appearance, new object[] { delta });
    }

    public static void Behavior()
    {
        try { VerifyBehavior(); Debug.Log("DLJ_SWORD_IDLE_BEHAVIOR_PASS"); }
        catch (Exception e) { Debug.LogException(e); EditorApplication.Exit(1); }
    }

    static void VerifyBehavior()
    {
        var root = new GameObject("Sword idle validation", typeof(SpriteRenderer), typeof(SpriteLibrary));
        var child = new GameObject("Weapon", typeof(SpriteRenderer));
        child.transform.SetParent(root.transform);
        var tex = new Texture2D(2, 2);
        var first = Sprite.Create(tex, new Rect(0, 0, 1, 1), Vector2.zero);
        var second = Sprite.Create(tex, new Rect(1, 0, 1, 1), Vector2.zero);
        var other = Sprite.Create(tex, new Rect(0, 1, 1, 1), Vector2.zero);
        var library = ScriptableObject.CreateInstance<SpriteLibraryAsset>();
        try
        {
            library.AddCategoryLabel(first, "Weapon", "Attack");
            library.AddCategoryLabel(other, "Weapon", "Heal");
            root.GetComponent<SpriteLibrary>().spriteLibraryAsset = library;
            var appearance = root.AddComponent<PlayerAppearance>();
            var weapon = child.GetComponent<SpriteRenderer>();
            Field(appearance, "weaponRenderer", weapon);
            Field(appearance, "swordIdleFrames", new[] { first, second });
            Field(appearance, "swordIdleFramesPerSecond", 30f);

            appearance.ShowWeapon(UIType.Attack);
            Check(weapon.sprite == first, "Selecting sword must start its idle");
            Tick(appearance, 1f / 30f);
            Check(weapon.sprite == second, "Sword particles must animate");
            Tick(appearance, 1f / 30f);
            Check(weapon.sprite == first, "Sword idle must loop");
            appearance.ShowWeapon(UIType.Attack);
            Check(weapon.sprite == first, "Reselecting sword restarts the idle");

            appearance.HideSelection();
            Tick(appearance, 1f / 30f);
            Check(weapon.sprite == null, "Hidden weapon must stay hidden");

            appearance.ShowWeapon(UIType.Attack);
            appearance.ShowWeapon(UIType.Heal);
            Tick(appearance, 1f / 30f);
            Check(weapon.sprite == other, "Another weapon must not receive sword frames");

            appearance.ShowWeapon(UIType.Attack);
            weapon.sprite = other;
            Tick(appearance, 1f / 30f);
            Check(weapon.sprite == other, "External action sprites must not be overwritten");

            Field(appearance, "swordIdleFrames", Array.Empty<Sprite>());
            appearance.ShowWeapon(UIType.Attack);
            Tick(appearance, 1f / 30f);
            Check(weapon.sprite == first, "Unconfigured idle must keep the existing static sword");
        }
        finally
        {
            UnityEngine.Object.DestroyImmediate(root);
            UnityEngine.Object.DestroyImmediate(library);
            UnityEngine.Object.DestroyImmediate(first);
            UnityEngine.Object.DestroyImmediate(second);
            UnityEngine.Object.DestroyImmediate(other);
            UnityEngine.Object.DestroyImmediate(tex);
        }
    }

    public static void ImportSheet()
    {
        try
        {
            VerifyBehavior();
            var library = AssetDatabase.LoadAssetAtPath<SpriteLibraryAsset>(Library);
            var source = library.GetSprite("Weapon", "Attack");
            Check(source != null && AssetDatabase.GetAssetPath(source).EndsWith("SwordAnimation.aseprite"), "Expected original sword reference");
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
            var so = new SerializedObject(appearance);
            var frames = so.FindProperty("swordIdleFrames");
            Check(frames.arraySize == 60, "Scene must contain 60 configured sword idle frames");
            Check(so.FindProperty("swordIdleFramesPerSecond").floatValue == 30f, "Scene must play at 30 fps");
            for (int i = 0; i < 60; i++) Check(frames.GetArrayElementAtIndex(i).objectReferenceValue != null, "Missing frame " + i);
            var weapon = (SpriteRenderer)so.FindProperty("weaponRenderer").objectReferenceValue;
            var library = appearance.GetComponent<SpriteLibrary>();
            // Loading a scene in Edit Mode does not run its play-mode initialization.
            typeof(SpriteLibrary).GetMethod("OnEnable", Private).Invoke(library, null);
            var first = (Sprite)frames.GetArrayElementAtIndex(0).objectReferenceValue;
            Check(library.GetSprite("Weapon", "Attack") == first, "Weapon library must use the idle first frame");
            var sprites = AssetDatabase.LoadAllAssetsAtPath(Sheet).OfType<Sprite>().OrderBy(s => s.name).ToArray();
            var original = AssetDatabase.LoadAllAssetsAtPath("Assets/02.Sprites/DLJ/SwordAnimation.aseprite").OfType<Sprite>().Single(s => Mathf.Approximately(s.rect.width, 30) && Mathf.Approximately(s.rect.height, 34));
            Check(first.pixelsPerUnit == original.pixelsPerUnit, "Sword scale changed");
            Check(Vector2.Distance(first.pivot - new Vector2(24,35), original.pivot) < .001f, "Sword handle alignment changed");
            appearance.ShowWeapon(UIType.Attack);
            Check(weapon.sprite == first, "Scene sword selection failed");
            Tick(appearance, 1f / 30f);
            Check(weapon.sprite == sprites[1], "Scene sword animation did not advance");
            Tick(appearance, 1f);
            Check(weapon.sprite != null && AssetDatabase.GetAssetPath(weapon.sprite) == Sheet, "Scene lost its idle animation");
            appearance.HideSelection(); Tick(appearance, 1f);
            Check(weapon.sprite == null, "Scene weapon did not stay hidden");
            Debug.Log("DLJ_SWORD_IDLE_INTEGRATION_PASS 60 frames, 30 fps, scale and handle alignment preserved");
        }
        catch (Exception e) { Debug.LogException(e); EditorApplication.Exit(1); }
    }
}
