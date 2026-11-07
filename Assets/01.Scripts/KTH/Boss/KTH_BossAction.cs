using System;

// 보스 패턴의 기반 클래스. 에셋이 아니라 보스 인스펙터의 리스트 안에 직접 들어간다
// (KTH_Boss의 [SerializeReference] 리스트 + KTH_SubclassSelector 드롭다운으로 추가)
// 보스마다 따로 인스턴스가 생기므로 패턴 안에 상태를 둬도 다른 보스와 섞이지 않고,
// 씬 오브젝트(보스 자식 등)도 인스펙터로 바로 연결할 수 있다
[Serializable]
public abstract class KTH_BossAction
{
    public abstract void Execute(KTH_Boss kthBoss);
}
