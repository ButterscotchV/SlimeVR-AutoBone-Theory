using System;

namespace SlimeVRAutoBone
{
    class Program
    {
        static void Main(string[] args)
        {
            var random = new Random();

            var footPos = Vector3.Randomized;

            var lengths = new double[] {
                random.NextDouble(),
                random.NextDouble(),
                random.NextDouble(),
                random.NextDouble(), 
                random.NextDouble()
            };

            Console.WriteLine($"Lengths: {string.Join(", ", lengths)}");

            var fakeLengths = new double[] {
                random.NextDouble(),
                random.NextDouble(),
                random.NextDouble(),
                random.NextDouble(),
                random.NextDouble()
            };

            Console.WriteLine($"Fake lengths: {string.Join(", ", fakeLengths)}");
            Console.WriteLine();

            var rate = 0.01;

            for (var i = 0; i < 10; i++)
            {
                var rotations1 = new Vector3[] {
                    RandomRotation(random),
                    RandomRotation(random),
                    RandomRotation(random),
                    RandomRotation(random),
                    RandomRotation(random)
                };

                var rotations2 = new Vector3[] {
                    RandomRotation(random),
                    RandomRotation(random),
                    RandomRotation(random),
                    RandomRotation(random),
                    RandomRotation(random)
                };

                //Console.WriteLine($"Test {i + 1} rotations: {string.Join<Vector3>(", ", rotations)}");

                var origin1 = GetOriginPos(footPos, rotations1, lengths);
                var estimatedPos1 = GetEndPos(origin1, rotations1, fakeLengths);

                var origin2 = GetOriginPos(footPos, rotations2, lengths);
                var estimatedPos2 = GetEndPos(origin2, rotations2, fakeLengths);

                var dist = estimatedPos1.Dist(estimatedPos2);
                var error = (dist * dist) * rate;

                Console.WriteLine($"Test {i + 1} estimated position offset: {dist}");
            }
        }

        public static Vector3 RandomRotation(Random random)
        {
            return new Vector3(random.NextDoubleInclusive(-1, 1), random.NextDoubleInclusive(-1, -0.25), random.NextDoubleInclusive(-1, 1)).Normalize();
        }

        public static Vector3 GetOriginPos(Vector3 footPos, Vector3[] rotations, double[] lengths)
        {
            var origin = footPos.Clone();

            for (var i = 0; i < lengths.Length; i++)
            {
                var vector = rotations[i] * -lengths[i];

                origin += vector;
            }

            return origin;
        }

        public static Vector3 GetEndPos(Vector3 origin, Vector3[] rotations, double[] lengths)
        {
            var end = origin.Clone();

            for (var i = 0; i < lengths.Length; i++)
            {
                var vector = rotations[i] * lengths[i];

                end += vector;
            }

            return end;
        }
    }
}
