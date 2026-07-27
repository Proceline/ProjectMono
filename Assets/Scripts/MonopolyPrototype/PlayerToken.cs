using System.Collections;
using UnityEngine;

namespace MonopolyPrototype
{
    public sealed class PlayerToken : MonoBehaviour
    {
        [SerializeField] private float moveDuration = 0.18f;
        [SerializeField] private float moveArcHeight = 0.28f;
        [SerializeField] private Vector3 tileOffset = new Vector3(0f, 1.05f, 0f);

        public void SnapTo(BoardTile tile)
        {
            transform.position = tile.transform.position + tileOffset;
        }

        public IEnumerator MoveTo(BoardTile tile)
        {
            var start = transform.position;
            var end = tile.transform.position + tileOffset;
            var elapsed = 0f;

            while (elapsed < moveDuration)
            {
                elapsed += Time.deltaTime;
                var t = Mathf.Clamp01(elapsed / moveDuration);
                var position = Vector3.Lerp(start, end, t);
                position.y += Mathf.Sin(t * Mathf.PI) * moveArcHeight;
                transform.position = position;
                yield return null;
            }

            transform.position = end;
        }
    }
}
