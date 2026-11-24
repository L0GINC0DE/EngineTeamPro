using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "spriteList", menuName = "KTH/SpriteList")]
public class KTH_RandomSpriteListSo : ScriptableObject
{
   [field:SerializeField] public List<Sprite> SpriteList { get; private set; } = new List<Sprite>();

}
