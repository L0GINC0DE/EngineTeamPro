using System.Collections.Generic;
using UnityEngine;

public class KTH_GhostEffect : MonoBehaviour
{
    [SerializeField] private int _numberOfGhosts = 10;
    [SerializeField] private Color _ghostColor = Color.white;
    [SerializeField] private float _lifetime = 0.3f;
    [SerializeField] private float _spawnInterval = 0.02f;
    [SerializeField] private float _spawnDistance = 0.5f;

    private List<KTH_GhostRenderer> _ghostRenderers;
    private SpriteRenderer _objectSpriteRenderer;

    private Vector3 _lastPosition;
    private float _spawnTimer;

    void Start()
    {
        _objectSpriteRenderer = GetComponent<SpriteRenderer>();
        if( _objectSpriteRenderer == null)
        {
            enabled = false;
            return;
        }
        _ghostRenderers = new List<KTH_GhostRenderer>();
        for (int i = 0; i < _numberOfGhosts; i++)
        {

            GameObject ghostObject = new GameObject("GhostRenderer_" + i);
            ghostObject.transform.SetParent(transform, false);
            KTH_GhostRenderer ghostRenderer = ghostObject.AddComponent<KTH_GhostRenderer>();
            ghostRenderer.gameObject.SetActive(false);
            _ghostRenderers.Add(ghostRenderer);
        }

        _lastPosition = transform.position;
    }

    void LateUpdate()
    {
        _spawnTimer += Time.deltaTime;
        if (_spawnTimer < _spawnInterval)
        {
            return;
        }
        _spawnTimer = 0f;

        Vector3 currentPosition = transform.position;
        float distance = Vector3.Distance(currentPosition, _lastPosition);
        if (distance <= Mathf.Epsilon)
        {
            return;
        }
        int count = Mathf.Clamp(Mathf.CeilToInt(distance / _spawnDistance), 1, _numberOfGhosts);
        for (int i = 1; i <= count; i++)
        {
            SpawnGhost(Vector3.Lerp(_lastPosition, currentPosition, (float)i / count));
        }
        _lastPosition = currentPosition;
    }

    private void SpawnGhost(Vector3 position)
    {
        KTH_GhostRenderer leastLifeGhost = GetLeastLifeGhost();

        if (leastLifeGhost != null)
        {
            leastLifeGhost.Initialize(
                position,
                transform,
                _objectSpriteRenderer,
                _ghostColor,
                _lifetime
            );
        }
    }

    private KTH_GhostRenderer GetLeastLifeGhost()
    {
        KTH_GhostRenderer leastLifeGhost = null;
        float minAlpha = float.MaxValue;

        foreach (var ghost in _ghostRenderers)
        {
            if (!ghost.gameObject.activeSelf)
            {
                return ghost;
            }

            SpriteRenderer sr = ghost.ghostSpriteRenderer;
            float currentAlpha = sr.color.a;

            if (currentAlpha < minAlpha)
            {
                minAlpha = currentAlpha;
                leastLifeGhost = ghost;
            }
        }

        return leastLifeGhost;
    }

    void OnDestroy()
    {
        if (_ghostRenderers == null)
        {
            return;
        }
        foreach (var ghost in _ghostRenderers)
        {
            if (ghost != null)
            {
                Destroy(ghost.gameObject);
            }
        }
    }
}
