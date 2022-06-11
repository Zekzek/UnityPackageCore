namespace Zekzek.HexWorld
{
    public enum MoveType
    {
        NONE,
        WALK_FORWARD,
        WALK_BACKWARD,
        TURN_LEFT,
        TURN_RIGHT,
        CLIMB_UP,
        JUMP_UP,
        CLIMB_DOWN,
        DROP_DOWN
    }

    static class MoveTypeMethods
    {
        public static MoveType GetInverse(this MoveType type)
        {
            switch (type) {
                case MoveType.CLIMB_DOWN:
                    return MoveType.CLIMB_UP;
                case MoveType.CLIMB_UP:
                    return MoveType.CLIMB_DOWN;
                case MoveType.DROP_DOWN:
                    return MoveType.JUMP_UP;
                case MoveType.JUMP_UP:
                    return MoveType.DROP_DOWN;
                case MoveType.TURN_LEFT:
                    return MoveType.TURN_RIGHT;
                case MoveType.TURN_RIGHT:
                    return MoveType.TURN_LEFT;
                case MoveType.WALK_BACKWARD:
                    return MoveType.WALK_FORWARD;
                case MoveType.WALK_FORWARD:
                    return MoveType.WALK_BACKWARD;
                case MoveType.NONE:
                default:
                    return MoveType.NONE;
            }
        }
    }
}