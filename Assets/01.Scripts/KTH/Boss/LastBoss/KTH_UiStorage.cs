using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 보스가 흡수한 UI(스프라이트)를 보관한다. 흡수 패턴에서 Add, 방출 패턴에서 TakeAll.
/// </summary>
public class KTH_UiStorage : MonoBehaviour
{
    private readonly List<Sprite> absorbed = new();

    public int Count => absorbed.Count;

    public void Add(Sprite sprite)
    {
        if (sprite != null) absorbed.Add(sprite);
    }

    /// <summary>흡수한 순서대로 전부 꺼내고 비운다.</summary>
    public List<Sprite> TakeAll()
    {
        var list = new List<Sprite>(absorbed);
        absorbed.Clear();
        return list;
    }
}
