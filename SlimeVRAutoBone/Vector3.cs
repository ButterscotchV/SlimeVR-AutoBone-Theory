using System;

namespace SlimeVRAutoBone
{
    public class Vector3 : ICloneable
    {
        public double X = 0;
        public double Y = 0;
        public double Z = 0;

        public double SqrMagnitude => (X * X) + (Y * Y) + (Z * Z);
        public double Magnitude => Math.Sqrt(SqrMagnitude);
        public Vector3 Normalized => this / Magnitude;

        public static Vector3 Randomized {
            get {
                var random = new Random();
                return new Vector3(random.NextDoubleInclusive(-1, 1), random.NextDoubleInclusive(-1, 1), random.NextDoubleInclusive(-1, 1));
            }
        }

        public Vector3()
        {
        }

        public Vector3(double x, double y, double z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public double Dist(Vector3 other)
        {
            var x = other.X - X;
            var y = other.Y - Y;
            var z = other.Z - Z;

            return Math.Sqrt((x * x) + (y * y) + (z * z));
        }

        public Vector3 Normalize()
        {
            var magnitude = Magnitude;

            X /= magnitude;
            Y /= magnitude;
            Z /= magnitude;

            return this;
        }

        public override string ToString()
        {
            return $"({X}, {Y}, {Z})";
        }

        public Vector3 Clone()
        {
            return new Vector3(X, Y, Z);
        }

        object ICloneable.Clone()
        {
            return Clone();
        }

        public static Vector3 operator +(Vector3 a, Vector3 b) => new Vector3(a.X + b.X, a.Y + b.Y, a.Z + b.Z);
        public static Vector3 operator -(Vector3 a, Vector3 b) => new Vector3(a.X - b.X, a.Y - b.Y, a.Z - b.Z);
        public static Vector3 operator *(Vector3 a, double b) => new Vector3(a.X * b, a.Y * b, a.Z * b);
        public static Vector3 operator /(Vector3 a, double b) => new Vector3(a.X / b, a.Y / b, a.Z / b);
    }
}
