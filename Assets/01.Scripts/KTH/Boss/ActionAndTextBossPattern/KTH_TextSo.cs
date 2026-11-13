using UnityEngine;

[CreateAssetMenu(fileName = "KTH_TextSo", menuName = "KTH/KTH_TextSo")]
public class KTH_TextSo : ScriptableObject
{
    [TextArea] public string text;
    public string[] choices = new string[3];
    [Range(0, 2)] public int answer;
}
