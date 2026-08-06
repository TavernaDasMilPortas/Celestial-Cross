using System;

namespace CombatSimulator.Core.Models
{
    public struct SimpleVector2Int : IEquatable<SimpleVector2Int>
    {
        public int X { get; set; }
        public int Y { get; set; }

        public SimpleVector2Int(int x, int y)
        {
            X = x;
            Y = y;
        }

        public bool Equals(SimpleVector2Int other)
        {
            return X == other.X && Y == other.Y;
        }

        public override bool Equals(object obj)
        {
            return obj is SimpleVector2Int other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(X, Y);
        }

        public static bool operator ==(SimpleVector2Int left, SimpleVector2Int right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(SimpleVector2Int left, SimpleVector2Int right)
        {
            return !left.Equals(right);
        }
    }
}
