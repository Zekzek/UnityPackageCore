using UnityEngine;

namespace Zekzek.HexWorld
{
    public static class FacingUtil
    {
        public static Vector2Int NE = new Vector2Int(0, 1);
        public static Vector2Int E = new Vector2Int(1, 0);
        public static Vector2Int SE = new Vector2Int(1, -1);
        public static Vector2Int SW = new Vector2Int(0, -1);
        public static Vector2Int W = new Vector2Int(-1, 0);
        public static Vector2Int NW = new Vector2Int(-1, 1);

        public static Vector2Int GetLeft(Vector2Int facing)
        {
            if (facing.Equals(NE)) { return NW; }
            if (facing.Equals(E)) { return NE; }
            if (facing.Equals(SE)) { return E; }
            if (facing.Equals(SW)) { return SE; }
            if (facing.Equals(W)) { return SW; }
            if (facing.Equals(NW)) { return W; }
            throw new MissingReferenceException();
        }

        public static Vector2Int GetRight(Vector2Int facing)
        {
            if (facing.Equals(NE)) { return E; }
            if (facing.Equals(E)) { return SE; }
            if (facing.Equals(SE)) { return SW; }
            if (facing.Equals(SW)) { return W; }
            if (facing.Equals(W)) { return NW; }
            if (facing.Equals(NW)) { return NE; }
            throw new MissingReferenceException();
        }

        public static Vector2Int GetFacing(float angle)
        {
            if (angle >= 0 && angle < 60) { return NE; }
            if (angle >= 60 && angle < 120) { return E; }
            if (angle >= 120 && angle < 180) { return SE; }
            if (angle >= -180 && angle < -120) { return SW; }
            if (angle >= -120 && angle < -60) { return W; }
            if (angle >= -60 && angle < 0) { return NW; }
            if (angle >= 180) { return GetFacing(angle - 360); }
            if (angle < -180) { return GetFacing(angle + 360); }
            throw new MissingReferenceException();
        }

        //TODO: Shouldn't these be the same? ↑↓

        public static Vector2Int GetFacing(Vector2Int fromGridPos, Vector2Int toGridPos)
        {
            float offsetAngle = -Vector2.SignedAngle(Vector2.up, (toGridPos - fromGridPos));
            if (offsetAngle >= -22.5 && offsetAngle < 45) { return NE; }
            if (offsetAngle >= 45 && offsetAngle < 112.5) { return E; }
            if (offsetAngle >= 112.5 && offsetAngle < 157.5) { return SE; }
            if (offsetAngle >= 157.5 || offsetAngle < -135) { return SW; }
            if (offsetAngle >= -135 && offsetAngle < -67.5) { return W; }
            if (offsetAngle >= -67.5 && offsetAngle < -22.5) { return NW; }
            throw new MissingReferenceException();
        }

        public static Quaternion GetRotation(Vector2Int facing)
        {
            return Quaternion.AngleAxis(GetRotationAroundUpAxis(facing), Vector3.up);
        }

        public static float GetRotationAroundUpAxis(Vector2Int facing)
        {
            if (facing.Equals(NE)) { return 30; }
            if (facing.Equals(E)) { return 90; }
            if (facing.Equals(SE)) { return 150; }
            if (facing.Equals(SW)) { return -150; }
            if (facing.Equals(W)) { return -90; }
            if (facing.Equals(NW)) { return -30; }
            throw new MissingReferenceException();
        }

        public static float LerpRotationAroundUpAxis(float first, float second, float percent)
        {
            float result;
            float delta = second - first;
            if (Mathf.Abs(delta) <= 180) {
                result = first + percent * delta;
            } else {
                result = first + (delta > 0 ? -percent : percent) * (360 - Mathf.Abs(delta));
            }
            while (result > 180) { result -= 360; }
            while (result <= -180) { result += 360; }
            return result;
        }

        public static float GetFacingRotationAroundUpAxis(Vector2Int fromGridIndex, Vector2Int toGridIndex)
        {
            float offsetAngle = -Vector2.SignedAngle(Vector2.up, (toGridIndex - fromGridIndex));
            if (offsetAngle >= -22.5 && offsetAngle < 45) { return 30; }
            if (offsetAngle >= 45 && offsetAngle < 112.5) { return 90; }
            if (offsetAngle >= 112.5 && offsetAngle < 157.5) { return 150; }
            if (offsetAngle >= 157.5 || offsetAngle < -135) { return -150; }
            if (offsetAngle >= -135 && offsetAngle < -67.5) { return -90; }
            if (offsetAngle >= -67.5 && offsetAngle < -22.5) { return -30; }
            throw new MissingReferenceException();
        }

        public static Vector2Int RotateAround(Vector2Int toRotate, Vector2Int center, float degree)
        {
            Vector3 toRotatePos = WorldUtil.GridIndexToPosition(toRotate, 0);
            Vector3 centerPos = WorldUtil.GridIndexToPosition(center, 0);

            Vector3 rotatedPos = centerPos + Quaternion.AngleAxis(degree, Vector3.up) * (toRotatePos - centerPos);
            return WorldUtil.PositionToGridIndex(rotatedPos);
        }

        public static int EstimateRotationCost(Vector2Int fromGridIndex, Vector2Int toGridIndex, Vector2Int fromFacing)
        {
            Vector2Int idealFacing = GetFacing(fromGridIndex, toGridIndex);
            if (idealFacing.Equals(fromFacing)) return 0;
            Vector2Int left = GetLeft(fromFacing);
            if (idealFacing.Equals(left)) return 1;
            Vector2Int right = GetRight(fromFacing);
            if (idealFacing.Equals(right)) return 1;
            if (idealFacing.Equals(GetLeft(left))) return 2;
            if (idealFacing.Equals(GetRight(right))) return 2;
            return 3;
        }
    }
}