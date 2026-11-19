using System;
using System.Linq;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.U2D.Animation;

public class PlayerAppearanceTests
{
    private GameObject player;
    private SpriteLibraryAsset library;

    [OneTimeSetUp]
    public void BuildLibrary()
    {
        library = ScriptableObject.CreateInstance<SpriteLibraryAsset>();
        AddBody("Attack", "Player_01_Red_DLJ_Idle");
        AddBody("Defend", "Player_06_Blue_DLJ_Idle");
        AddBody("Act", "Player_04_Orange_DLJ_Idle");
        AddBody("Heal", "Player_03_Green_DLJ_Idle");
        AddBody("HealthBar", "Player_05_Yellow_DLJ_Idle");
        AddBody("Setting", "Player_02_Gray_DLJ_Idle");
        AddBody("TextBar", "Player_07_White_DLJ_Idle");
        AssetDatabase.CreateAsset(library, "Assets/PlayerBodyLibrary.asset");
        AssetDatabase.SaveAssets();
    }

    private void AddBody(string label, string file)
    {
        Sprite sprite = AssetDatabase.LoadAllAssetsAtPath("Assets/Sprites/" + file + ".aseprite")
            .OfType<Sprite>().OrderBy(s => s.name).First();
        library.AddCategoryLabel(sprite, "Body", label);
    }

    [TearDown]
    public void TearDown()
    {
        if (player != null) UnityEngine.Object.DestroyImmediate(player);
    }

    [TestCase(UIType.Attack, "Player_01_Red_DLJ_Idle")]
    [TestCase(UIType.Defend, "Player_06_Blue_DLJ_Idle")]
    [TestCase(UIType.Act, "Player_04_Orange_DLJ_Idle")]
    [TestCase(UIType.Heal, "Player_03_Green_DLJ_Idle")]
    [TestCase(UIType.HealthBar, "Player_05_Yellow_DLJ_Idle")]
    [TestCase(UIType.Setting, "Player_02_Gray_DLJ_Idle")]
    [TestCase(UIType.TextBar, "Player_07_White_DLJ_Idle")]
    public void SelectingBodyShowsTheMatchingCharacter(UIType type, string spriteFile)
    {
        player = new GameObject("Player", typeof(SpriteLibrary), typeof(SpriteRenderer));
        player.GetComponent<SpriteLibrary>().spriteLibraryAsset = library;
        player.GetComponent<SpriteRenderer>().enabled = false;
        Type appearanceType = Type.GetType("DLJ_PlayerAppearance, Appearance.Runtime");
        Assert.That(appearanceType, Is.Not.Null, "Body selection needs a Player appearance component.");
        Component appearance = player.AddComponent(appearanceType);
        Assert.That(appearanceType.GetMethod("ShowBody").Invoke(appearance, new object[] { type }), Is.EqualTo(true));
        Sprite expected = AssetDatabase.LoadAllAssetsAtPath("Assets/Sprites/" + spriteFile + ".aseprite")
            .OfType<Sprite>().OrderBy(s => s.name).First();
        Assert.That(player.GetComponent<SpriteRenderer>().sprite, Is.EqualTo(expected));
        Assert.That(player.GetComponent<SpriteRenderer>().enabled, Is.True);

        Assert.That(appearanceType.GetMethod("ShowBody").Invoke(appearance, new object[] { (UIType)999 }), Is.EqualTo(false));
        Assert.That(player.GetComponent<SpriteRenderer>().sprite, Is.EqualTo(expected), "Missing entries must preserve the displayed character.");
    }
}
