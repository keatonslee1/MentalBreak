using System.Collections;
using UnityEngine;

namespace MumbleGenerator
{
    public class MouthMover : MonoBehaviour
    {
        [SerializeField] Transform _mouth;
        [SerializeField] Vector2 _moveRange;

        IEnumerator _moveCoroutine;

        void Start()
        {
            GetComponent<MumblePlayer>().OnStartMumble += OnStartMumble;
            GetComponent<MumblePlayer>().OnEndMumble += OnEndMumble;
        }


        void OnStartMumble()
        {
            if (_moveCoroutine != null) StopCoroutine(_moveCoroutine);
            _moveCoroutine = MoveMouthCoroutine();
            StartCoroutine(_moveCoroutine);
        }

        void OnEndMumble()
        {
            if (_moveCoroutine != null) StopCoroutine(_moveCoroutine);
        }

        IEnumerator MoveMouthCoroutine()
        {
            while (true)
            {
                // move mouth up and down
                float targetY = Random.Range(_moveRange.x, _moveRange.y);
                float time = Random.Range(0.05f, 0.1f);
                float elapsedTime = 0f;
                Vector3 startPos = _mouth.localPosition;
                while (elapsedTime < time)
                {
                    _mouth.transform.localPosition = Vector3.Lerp(startPos, new(startPos.x, targetY, startPos.z),
                        elapsedTime / time);
                    elapsedTime += Time.deltaTime;
                    yield return null;
                }
            }
        }
    }
}