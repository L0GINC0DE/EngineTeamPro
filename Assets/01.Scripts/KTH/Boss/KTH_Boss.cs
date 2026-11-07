using System.Collections.Generic;
using UnityEngine;

public class KTH_Boss : KTH_Enemy
{
    // 패턴을 에셋 없이 보스에 직접 넣는다. 인스펙터에서 + 누른 뒤 드롭다운으로 패턴 종류를 고르면 된다
    [SerializeReference, KTH_SubclassSelector]
    private List<KTH_BossAction> patterns = new();
    [SerializeField]private Transform _target;
    public Transform target => _target;
    int index;
    public KTH_BossAction NextAction => patterns.Count == 0 ? null : patterns[index % patterns.Count];

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.S))
        {
            TakeTurn();
        }
    }

    public void TakeTurn()
    {
        if (patterns.Count == 0) return;

        index %= patterns.Count; // 플레이 중 리스트를 줄여도 범위를 벗어나지 않게
        patterns[index]?.Execute(this); // 타입을 아직 안 고른 빈 칸은 건너뛴다
        index = (index + 1) % patterns.Count;
    }

#if UNITY_EDITOR
    // 리스트의 + 버튼은 마지막 칸을 복제하는데, SerializeReference는 같은 인스턴스를 가리키게 될 수 있다
    // 그러면 한 칸을 고칠 때 다른 칸도 같이 바뀌므로 겹치는 칸은 복사본으로 바꿔준다
    private void OnValidate()
    {
        var seen = new HashSet<KTH_BossAction>();
        for (int i = 0; i < patterns.Count; i++)
        {
            KTH_BossAction p = patterns[i];
            if (p == null) continue;
            if (!seen.Add(p))
                patterns[i] = (KTH_BossAction)JsonUtility.FromJson(JsonUtility.ToJson(p), p.GetType());
        }
    }
#endif
}
