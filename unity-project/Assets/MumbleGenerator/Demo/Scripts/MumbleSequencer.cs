using System.Collections;
using UnityEngine;

namespace MumbleGenerator
{
    public class MumbleSequencer : MonoBehaviour
    {
        [SerializeField] Mumble[] _mumbles;

        [SerializeField] Renderer _faceRenderer;
        [SerializeField] Renderer[] _eyeRenderers;
        [SerializeField] Renderer[] _eyebrowRenderers;
        [SerializeField] Renderer[] _mouthRenderers;

        [SerializeField] Transform _face;

        int _index;
        MumblePlayer _player;

        public void Start()
        {
            _player = GetComponent<MumblePlayer>();
        }

        public void StartSequence()
        {
            _index = 0;
            PlayNextMumble();
            _player.OnEndMumble += () => { StartCoroutine(SmallDelay()); };
        }

        public void PlayNextMumble()
        {
            _player.Mumble = _mumbles[_index];
            _index++;
            _player.PlayMumble();
            StartCoroutine(LerpEverythingCoroutine());

        }

        IEnumerator SmallDelay()
        {
            if (_index >= _mumbles.Length) yield break;

            StartCoroutine(LerpEverythingCoroutine());

            yield return new WaitForSeconds(1f);
            PlayNextMumble();
        }

        IEnumerator LerpEverythingCoroutine()
        {
            Vector3 originalScale = _face.transform.localScale;
            Vector3 newScale = new(Random.Range(1.3f, 1.7f), Random.Range(1.3f, 1.7f), Random.Range(1.3f, 1.7f));

            Color originalFaceColor = _faceRenderer.material.color;
            Color newFaceColor = Random.ColorHSV();

            Color originalEyeColor = _eyeRenderers[0].material.color;
            Color newEyeColor = Random.ColorHSV();

            Color originalEyebrowColor = _eyebrowRenderers[0].material.color;
            Color newEyebrowColor = Random.ColorHSV();

            Color originalMouthColor = _mouthRenderers[0].material.color;
            Color newMouthColor = Random.ColorHSV();

            float time = 0;
            while (time < 0.5f)
            {
                _faceRenderer.material.color = Color.Lerp(originalFaceColor, newFaceColor, time);
                foreach (Renderer r in _eyeRenderers)
                    r.material.color = Color.Lerp(originalEyeColor, newEyeColor, time);
                foreach (Renderer r in _eyebrowRenderers)
                    r.material.color = Color.Lerp(originalEyebrowColor, newEyebrowColor, time);
                foreach (Renderer r in _mouthRenderers)
                    r.material.color = Color.Lerp(originalMouthColor, newMouthColor, time);

                _face.localScale = Vector3.Lerp(originalScale, newScale, time);

                time += Time.deltaTime;
                yield return null;
            }
        }
    }
}