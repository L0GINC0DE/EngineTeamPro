using System;
using _01.Scripts.TAE.Combat;
 using CoreLib;
 using UnityEngine;

 namespace _01.Scripts.TAE.Managers
 {
     public class GimmickManager : CSILib.SoundManager.RunTime.MonoSingleton<GimmickManager>
     {
         public GimmickList list;

         // 현재 파일 전체에 걸린 블록 주석을 해제한 뒤 아래 멤버를 적용하세요.
         private readonly NotifyValue<int> CurrentIndex = new(-1);

         private Camera mainCam;

         private void Awake()
         {
             mainCam = Camera.main; 
         }

         private void OnEnable() => CurrentIndex.OnValueChanged += HandleGimmickSelectChanged;
         private void OnDisable() => CurrentIndex.OnValueChanged -= HandleGimmickSelectChanged;

         public void StartSelect()
         {
             int index = 0;
             int firstIndex = -1;
             
             foreach (var gimmick in list.gimmickList)
             {
                 gimmick.CheckSelect(false);

                 bool canSelect = false;

                 if (gimmick.point != null && gimmick.point.isActiveAndEnabled)
                 {
                     var p = mainCam.WorldToViewportPoint(gimmick.point.transform.position);

                     canSelect = p.x is >= 0f and <= 1f &&
                                      p.y is >= 0f and <= 1f;
                 }
                 
                 gimmick.CheckSelect(canSelect);
                 
                 if (firstIndex < 0 && canSelect)
                     firstIndex = index;

                 index++;
             }
             Select(firstIndex);
         }

         private void Select(int index)
         {
             var gimmicks = list.gimmickList;
             if (index < 0 || index >= gimmicks.Count || gimmicks[index].point == null)
                 return;

             CurrentIndex.Value = index;
         }
         private void HandleGimmickSelectChanged(int prev, int next)
         {
             var gimmicks = list.gimmickList;

             if (prev >= 0 && prev < gimmicks.Count)
             {
                 var previousPoint = gimmicks[prev].point;
                 if (previousPoint != null)
                 {
                     previousPoint.isSelect = false;
                     previousPoint.Select();
                 }
             }

             var nextPoint = gimmicks[next].point;
             nextPoint.isSelect = true;
             nextPoint.Select();
         }
         
         public void Next() => MoveSelection(1);
         public void Prev() => MoveSelection(-1);

         private void MoveSelection(int direction)
         {
             var gimmicks = list.gimmickList;
             int index = CurrentIndex.Value;

             if (index < 0 || index >= gimmicks.Count)
                 return;

             for (int i = 0; i < gimmicks.Count - 1; i++)
             {
                 index += direction;

                 index = index >= gimmicks.Count ? 0 : 
                         index < 0 ? gimmicks.Count - 1 : index;

                 if (gimmicks[index].canSelect)
                 {
                     Select(index);
                     return;
                 }
             }
         }
     }
 }