using System;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.U2D.Sprites;
using UnityEngine;
using UnityEngine.U2D.Animation;

public static class HeadParticlesValidation
{
    const BindingFlags Private = BindingFlags.Instance | BindingFlags.NonPublic;
    static void Check(bool value, string message) { if (!value) throw new Exception(message); }

    public static void RepairMetadata()
    {
        try
        {
            Directory.CreateDirectory("Assets/FreshImport");
            Directory.CreateDirectory("metadata-repair");
            foreach (string source in Directory.GetFiles("Assets/Sprites/Player", "*.aseprite"))
            {
                string fresh = "Assets/FreshImport/" + Path.GetFileName(source);
                File.Copy(source, fresh, true);
                AssetDatabase.ImportAsset(fresh, ImportAssetOptions.ForceSynchronousImport | ImportAssetOptions.ForceUpdate);
                var originalImporter = AssetImporter.GetAtPath(source);
                var original = (ISpriteEditorDataProvider)originalImporter;
                var regenerated = (ISpriteEditorDataProvider)AssetImporter.GetAtPath(fresh);
                original.InitSpriteEditorDataProvider();
                regenerated.InitSpriteEditorDataProvider();
                var rects = original.GetSpriteRects();
                var corrected = regenerated.GetSpriteRects().ToDictionary(r => r.name);
                foreach (var rect in rects)
                {
                    Check(corrected.ContainsKey(rect.name), "Missing regenerated frame: " + rect.name);
                    var geometry = corrected[rect.name];
                    rect.rect = geometry.rect;
                    rect.pivot = geometry.pivot;
                    rect.alignment = geometry.alignment;
                }
                original.SetSpriteRects(rects);
                original.Apply();
                originalImporter.SaveAndReimport();
                File.Copy(source + ".meta", "metadata-repair/" + Path.GetFileName(source) + ".meta", true);
            }
            Debug.Log("DLJ_METADATA_REPAIR_PASS: regenerated frame rectangles and pivots; original sprite IDs preserved");
        }
        catch (Exception e) { Debug.LogException(e); EditorApplication.Exit(1); }
    }

    static Sprite[] ReadableFrames(string path)
    {
        var importer = AssetImporter.GetAtPath(path);
        var field = importer.GetType().GetField("m_TextureImporterSettings", Private);
        Check(field != null, "Aseprite texture settings are unavailable");
        var settings = (TextureImporterSettings)field.GetValue(importer);
        settings.readable = true;
        EditorUtility.SetDirty(importer);
        importer.SaveAndReimport();
        return AssetDatabase.LoadAllAssetsAtPath(path).OfType<Sprite>().OrderBy(s => s.name).ToArray();
    }

    static Color ColorAtCanvasPixel(Sprite sprite, int x, int y)
    {
        int tx = Mathf.RoundToInt(sprite.rect.x + sprite.pivot.x + x - 32);
        int ty = Mathf.RoundToInt(sprite.rect.y + sprite.pivot.y + 76 - y);
        if (tx < sprite.rect.xMin || tx >= sprite.rect.xMax || ty < sprite.rect.yMin || ty >= sprite.rect.yMax) return Color.clear;
        return sprite.texture.GetPixel(tx, ty);
    }

    public static void Run()
    {
        try
        {
            var tick = typeof(PlayerAppearance).GetMethod("AdvanceBodyIdle", Private);
            Check(tick != null, "Original body animation playback is missing");
            string[] names = { "01_Red", "06_Blue", "04_Orange", "03_Green", "05_Yellow", "02_Gray", "07_White" };
            var allFrames = names.Select(name => ReadableFrames("Assets/Sprites/Player/Player_" + name + "_DLJ_Idle.aseprite")).ToArray();
            var baselineFrames = names.Select(name => ReadableFrames("Assets/Baselines/Player_" + name + "_DLJ_Idle.aseprite")).ToArray();
            int visibleSamples = 0;
            for (int i = 0; i < names.Length; i++)
            for (int f = 0; f < 8; f++)
            {
                int addedPixels = 0;
                for (int y = 0; y < 28; y++)
                for (int x = 0; x < 64; x++)
                    if (ColorAtCanvasPixel(baselineFrames[i][f], x, y).a == 0
                        && ColorAtCanvasPixel(allFrames[i][f], x, y).a >= .25f) addedPixels++;
                Check(addedPixels > 0, "Imported particle pixels are invisible: " + names[i] + " frame " + f);
                visibleSamples += addedPixels;
            }
            Debug.Log("DLJ_IMPORTED_PARTICLES_VISIBLE_PASS: " + visibleSamples + " visible pixels across 56 imported frames");
            var preview = new Texture2D(64 * 7 * 3, 77 * 2 * 3, TextureFormat.RGBA32, false);
            for (int row = 0; row < 2; row++)
            for (int i = 0; i < names.Length; i++)
            for (int y = 0; y < 77; y++)
            for (int x = 0; x < 64; x++)
            {
                var color = ColorAtCanvasPixel(row == 0 ? baselineFrames[i][0] : allFrames[i][0], x, y);
                var background = new Color(24f / 255f, 21f / 255f, 30f / 255f, 1);
                var pixel = Color.Lerp(background, new Color(color.r, color.g, color.b, 1), color.a);
                for (int dy = 0; dy < 3; dy++) for (int dx = 0; dx < 3; dx++)
                    preview.SetPixel((i * 64 + x) * 3 + dx, ((1 - row) * 77 + 76 - y) * 3 + dy, pixel);
            }
            File.WriteAllBytes("imported-preview.png", preview.EncodeToPNG());
            UnityEngine.Object.DestroyImmediate(preview);
            for (int i = 0; i < names.Length; i++) for (int f = 0; f < 8; f++)
            for (int y = 28; y < 77; y++) for (int x = 0; x < 64; x++)
            {
                Color before = ColorAtCanvasPixel(baselineFrames[i][f], x, y), after = ColorAtCanvasPixel(allFrames[i][f], x, y);
                // Unity bleeds edge RGB into fully transparent texels when packing.
                Check((before.a == 0 && after.a == 0) || before == after,
                    "Imported visible body pixels changed: " + names[i] + " frame " + f + " at " + x + "," + y
                    + " before=" + before + " after=" + after + " before pivot=" + baselineFrames[i][f].pivot + " after pivot=" + allFrames[i][f].pivot
                    + " before rect=" + baselineFrames[i][f].rect + " after rect=" + allFrames[i][f].rect);
            }
            var library = AssetDatabase.LoadAssetAtPath<SpriteLibraryAsset>("Assets/Sprites/PlayerBodyLibrary.asset");
            File.WriteAllLines("frame-references.txt", allFrames.SelectMany(f => f).Select(sprite => {
                AssetDatabase.TryGetGUIDAndLocalFileIdentifier(sprite, out string guid, out long id);
                return "  - {fileID: " + id + ", guid: " + guid + ", type: 3}";
            }));
            var root = new GameObject("Baked body validation", typeof(SpriteLibrary), typeof(SpriteRenderer));
            try
            {
                root.GetComponent<SpriteLibrary>().spriteLibraryAsset = library;
                var body = root.GetComponent<SpriteRenderer>();
                var appearance = root.AddComponent<PlayerAppearance>();
                var so = new SerializedObject(appearance);
                so.FindProperty("bodyRenderer").objectReferenceValue = body;
                var frameList = so.FindProperty("bodyIdleFrames");
                Check(frameList != null, "Original body frames must be configurable");
                var flat = allFrames.SelectMany(f => f).ToArray();
                frameList.arraySize = flat.Length;
                for (int i = 0; i < flat.Length; i++) frameList.GetArrayElementAtIndex(i).objectReferenceValue = flat[i];
                so.FindProperty("bodyIdleFramesPerSecond").floatValue = 6.25f;
                so.ApplyModifiedPropertiesWithoutUndo();

                for (int i = 0; i < names.Length; i++)
                {
                    Check(allFrames[i].Length == 8, "Keep the original eight frames: " + names[i]);
                    var type = (UIType)i;
                    Check(library.GetSprite("Body", type.ToString()) == allFrames[i][0], "Existing body reference changed: " + type);
                    var bytes = File.ReadAllBytes("Assets/Sprites/Player/Player_" + names[i] + "_DLJ_Idle.aseprite");
                    Check(System.Text.Encoding.UTF8.GetString(bytes).Contains("DLJ_Head_Particles"), "Particles must be inside the original Aseprite: " + type);
                    Check(BitConverter.ToUInt16(bytes, 8) == 64 && BitConverter.ToUInt16(bytes, 10) == 77, "Original canvas changed");
                    int offset = 128;
                    for (int f = 0; f < 8; f++)
                    {
                        Check(BitConverter.ToUInt16(bytes, offset + 8) == 160, "Original breathing timing changed");
                        offset += BitConverter.ToInt32(bytes, offset);
                    }
                    appearance.ShowBody(type);
                    Check(body.sprite == allFrames[i][0], "Selection must start the original animation: " + type);
                    tick.Invoke(appearance, new object[] { .1601f });
                    Check(body.sprite == allFrames[i][1], "Baked animation must advance: " + type);
                    tick.Invoke(appearance, new object[] { 1.12f });
                    Check(body.sprite == allFrames[i][0], "Baked animation must loop: " + type);
                    Check(root.GetComponentsInChildren<SpriteRenderer>(true).Length == 1, "No extra particle renderer may be created");
                    appearance.HideSelection();
                    tick.Invoke(appearance, new object[] { .16f });
                    Check(body.sprite == null, "Hidden body must stay hidden");
                }

                appearance.ShowBody(UIType.Heal);
                body.sprite = allFrames[0][0];
                tick.Invoke(appearance, new object[] { .16f });
                Check(body.sprite == allFrames[0][0], "External body animation must not be overwritten");
                Debug.Log("DLJ_BAKED_HEAD_PARTICLES_PASS: seven original Aseprites, eight frames, 160ms timing, selection, loop, hiding and single renderer");
            }
            finally { UnityEngine.Object.DestroyImmediate(root); }
            DLJ_SwordIdleValidation.Behavior();
        }
        catch (Exception e) { Debug.LogException(e); EditorApplication.Exit(1); }
    }
}
