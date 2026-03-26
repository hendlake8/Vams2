using UnityEngine;
using Vams2.Input;

namespace Vams2.InGame.Player
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class PlayerMovement : MonoBehaviour
    {
        [SerializeField] private FloatingJoystick mJoystick;
        [SerializeField] private float mBaseMoveSpeed = 5.0f;

        private Rigidbody2D mRigidbody;
        private PlayerStats mStats;
        private SpriteRenderer mSpriteRenderer;

        private void Awake()
        {
            mRigidbody = GetComponent<Rigidbody2D>();
            mStats = GetComponent<PlayerStats>();
            mSpriteRenderer = GetComponent<SpriteRenderer>();
        }

        public void SetJoystick(FloatingJoystick joystick)
        {
            mJoystick = joystick;
        }

        private void FixedUpdate()
        {
            if (mJoystick == null)
            {
                return;
            }

            Vector2 direction = mJoystick.Direction;
            float magnitude = mJoystick.Magnitude;

            float speedBonus = 0f;
            if (mStats != null)
            {
                speedBonus = mStats.MoveSpeedBonus;
            }

            float finalSpeed = mBaseMoveSpeed * (1f + speedBonus);
            mRigidbody.linearVelocity = direction * magnitude * finalSpeed;

            // X축 이동 방향에 따라 좌우 반전
            if (mSpriteRenderer != null && direction.x != 0f)
            {
                mSpriteRenderer.flipX = direction.x < 0f;
            }
        }
    }
}
