using System.Collections.Generic;
using UnityEngine;

public class KTH_UiStorage : MonoBehaviour
{
    private readonly List<Sprite> absorbed = new();

    public int Count => absorbed.Count;

    public void Add(Sprite sprite)
    {
        if (sprite != null) absorbed.Add(sprite);
    }

    public List<Sprite> TakeAll()
    {
        List<Sprite> list = new(absorbed);
        absorbed.Clear();
        return list;
    }
}
