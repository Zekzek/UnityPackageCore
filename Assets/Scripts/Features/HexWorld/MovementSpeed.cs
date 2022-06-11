using UnityEngine;

namespace Zekzek.HexWorld
{
    public class MovementSpeed
    {
        public float Walk { get; private set; }
        public float Backstep { get; private set; }
        public float Rotate { get; private set; }
        public float Climb { get; private set; }
        public float Jump { get; private set; }
        public float Drop { get; private set; }
        public float Wait { get; private set; }
        public float FastestHorizontal { get; private set; }

        public int MaxJump { get; private set; }
        public int MaxDrop { get; private set; }

        public float MaxJumpHeight => MaxJump * WorldUtil.HEIGHT;
        public float MaxDropHeight => MaxDrop * WorldUtil.HEIGHT;


        public MovementSpeed(float walk, float backstep, float rotate, float climb, float jump, int maxJump, float drop, int maxDrop, float wait)
        {
            Walk = walk;
            Backstep = backstep;
            Rotate = rotate;
            Climb = climb;
            Jump = jump;
            Drop = drop;
            MaxJump = maxJump;
            MaxDrop = maxDrop;
            Wait = wait;
            FastestHorizontal = Mathf.Max(walk, backstep);
        }

        public float Get(MoveType movementType)
        {
            switch (movementType) {
                case MoveType.WALK_FORWARD:
                    return Walk;
                case MoveType.WALK_BACKWARD:
                    return Backstep;
                case MoveType.TURN_LEFT:
                case MoveType.TURN_RIGHT:
                    return Rotate;
                case MoveType.CLIMB_DOWN:
                case MoveType.CLIMB_UP:
                    return Climb;
                case MoveType.JUMP_UP:
                    return Jump;
                case MoveType.DROP_DOWN:
                    return Drop;
                case MoveType.NONE:
                    return Wait;
                default:
                    Debug.LogError(string.Format("Unable to find movement speed for {0}", movementType));
                    return 0;
            }
        }

        public override string ToString()
        {
            return $"Walk={Walk}";
        }
    }
}