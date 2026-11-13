using UnityEngine;
using TMPro;
using System.Collections;
using Systems;
using IPoolable = Systems.IPoolable;

namespace _01.Scripts.TAE.UI
{
    public class DamageTMP : MonoBehaviour, IPoolable
    {
        [SerializeField] private PoolItemSO poolItem;
        GameObject Critical;
        private TextMeshProUGUI m_Text;
        private RectTransform rectTransform;

        private Vector2 velocity;
        private float gravity = 300.0f;
        private float lifetime = 0.5f;
    
        private Color textColor;

        private Camera mainCam;
        
        private void Awake()
        {
            m_Text = GetComponentInChildren<TextMeshProUGUI>();
            rectTransform = GetComponent<RectTransform>();
            mainCam = Camera.main;
        }

        public void Initialize(Transform parent, Vector3 pos, string temp,  Color color, bool critical = false)
        {
            //Critical.SetActive(critical);
            transform.SetParent(parent);

            m_Text.text = temp;
            m_Text.color = color;

            Vector2 screenPosition = mainCam.WorldToScreenPoint(pos);
            rectTransform.position = screenPosition;

            velocity = new Vector2(Random.Range(-50.0f, 50.0f), Random.Range(150.0f, 250.0f));

            textColor = m_Text.color;

            StartCoroutine(MoveAndFade());
        }

        IEnumerator MoveAndFade()
        {
            float elapsedTime = 0.0f;
        
            while(elapsedTime < lifetime)
            {
                velocity.y += gravity * Time.deltaTime;

                rectTransform.anchoredPosition += velocity * Time.deltaTime;

                textColor.a = Mathf.Lerp(1.0f, 0.0f, elapsedTime / lifetime);
                m_Text.color = textColor;

                elapsedTime += Time.deltaTime;
                yield return null;
            }
            
            PoolManager.Instance.Push(this);
        }

        public PoolItemSO Item => poolItem;
        public GameObject GameObject => gameObject;
        public void ResetItem()
        {
            
        }
    }

}