using UnityEngine;

namespace _01.Scripts.TAE.Combat
{
    public class InteractPoint : MonoBehaviour
    {
        private SpriteRenderer _spriteRenderer;

        public bool isSelect;

        public void Awake()
        {
            _spriteRenderer = GetComponent<SpriteRenderer>();
        }
        public void Select()
        {
            _spriteRenderer.color = isSelect ? Color.yellow : Color.white;
        }
    }
}