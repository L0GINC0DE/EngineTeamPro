using System.Collections.Generic;
using _01.Scripts.TAE.Combat;
using _01.Scripts.TAE.CoreLib;
using _01.Scripts.TAE.Utils;
using UnityEngine;

 namespace _01.Scripts.TAE.Managers
 {
     public class GimmickManager : MonoSingleton<GimmickManager>
     {
         public GimmickList list;
         [SerializeField] private Sprite[] sprites;

         private readonly List<InteractPoint> _points = new();
         private int _currentIndex = -1;

         public InteractPoint CurrentPoint => _currentIndex >= 0 && _currentIndex < _points.Count
             ? _points[_currentIndex] : null;
         
         public void StartSelect()
         {
             ClearSelection();

             if (list.gimmickList.Count <= 0) return;

             // 화면에 보이는 포인트만 모으기
             foreach (var gimmick in list.gimmickList)
             {
                 if (gimmick == null) continue;
                 gimmick.ResetPoint();

                 bool visible = IsVisible(gimmick.point);
                 gimmick.CheckSelect(visible);

                 if (visible) _points.Add(gimmick.point);
             }

             if (_points.Count > 0) Select(0);
         }

         private void Select(int index)
         {
             CurrentPoint?.SetSelected(false);
             _currentIndex = index;
             CurrentPoint?.SetSelected(true);
         }
         
         public void Next() => MoveSelection(1);
         public void Prev() => MoveSelection(-1);

         private void MoveSelection(int direction)
         {
             if (_points.Count == 0) return;

             int nextIndex = (_currentIndex + direction + _points.Count) % _points.Count;

             Select(nextIndex);
         }
         
         public void ClearSelection()
         {
             CurrentPoint?.SetSelected(false);
             _currentIndex = -1;
             _points.Clear();
         }
         
         public Sprite GetSprite(InteractPointType type)
         {
             int index = (int)type;

             if (sprites == null || index < 0 || index >= sprites.Length)
                 return null;

             return sprites[index];
         }
         
         private bool IsVisible(InteractPoint point)
         {
             if (point == null ||
                 !point.isActiveAndEnabled)
                 return false;

             Vector3 pos = Utility.GetViewPort(point.transform.position);
             return pos is { z: > 0f, x: >= 0f and <= 1f, y: >= 0f and <= 1f };
         }

         public Vector3 CurrentPointPos()
         {
             return CurrentPoint.transform.position;
         }
     }
 }