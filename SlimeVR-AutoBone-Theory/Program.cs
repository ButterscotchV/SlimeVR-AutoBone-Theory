using System;

namespace SlimeVR.AutoBone.Theory
{
    class Program
    {
        static void Main(string[] args)
        {
            var random = new Random();

            var footPos = Vector3.Randomized;

            var lengths = new double[] {
                random.NextDoubleInclusive(0.5, 2),
                random.NextDoubleInclusive(0.5, 2),
                random.NextDoubleInclusive(0.5, 2),
                random.NextDoubleInclusive(0.5, 2),
                random.NextDoubleInclusive(0.5, 2)
            };

            Console.WriteLine($"Lengths: {string.Join(", ", lengths)}");

            var fakeLengths = new double[] {
                random.NextDoubleInclusive(0.5, 2),
                random.NextDoubleInclusive(0.5, 2),
                random.NextDoubleInclusive(0.5, 2),
                random.NextDoubleInclusive(0.5, 2),
                random.NextDoubleInclusive(0.5, 2)
            };

            var originalFakeLengths = (double[])fakeLengths.Clone();

            Console.WriteLine($"Fake lengths: {string.Join(", ", fakeLengths)}");
            Console.WriteLine();

            var rate = 0.2;

            for (var i = 0; i < 500; i++)
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
                var origin2 = GetOriginPos(footPos, rotations2, lengths);

                for (var k = 0; k < 100; k++)
                {
                    var dist = CalcDist(origin1, origin2, rotations1, rotations2, fakeLengths);
                    var error = dist * dist;
                    var adjust = error * rate;

                    Console.WriteLine($"Test {i + 1} estimated position offset: {dist}");

                    for (var j = 0; j < fakeLengths.Length; j++)
                    {
                        var fakeLengthsCopy = (double[])fakeLengths.Clone();
                        fakeLengthsCopy[j] = fakeLengths[j] + adjust;

                        var dist2 = CalcDist(origin1, origin2, rotations1, rotations2, fakeLengthsCopy);

                        if (dist2 > dist)
                        {
                            fakeLengthsCopy[j] = fakeLengths[j] - adjust;

                            var dist3 = CalcDist(origin1, origin2, rotations1, rotations2, fakeLengthsCopy);

                            if (dist3 > dist)
                            {
                                continue;
                            }
                            else
                            {
                                fakeLengths[j] -= adjust;
                            }
                        }
                        else
                        {
                            fakeLengths[j] += adjust;
                        }

                        dist = CalcDist(origin1, origin2, rotations1, rotations2, fakeLengths);
                        error = dist * dist;
                        adjust = error * rate;
                    }
                }
            }

            Console.WriteLine();
            Console.WriteLine($"Lengths: {string.Join(", ", lengths)}");
            Console.WriteLine($"Fake lengths: {string.Join(", ", originalFakeLengths)}");
            Console.WriteLine($"Estimated lengths: {string.Join(", ", fakeLengths)}");

            var origAccuracy = CalcAccuracy(lengths, originalFakeLengths);
            var finalAccuracy = CalcAccuracy(lengths, fakeLengths);

            Console.WriteLine($"Start accuracy: {origAccuracy * 100d} Final accuracy: {finalAccuracy * 100d}");
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

        public static double CalcDist(Vector3 origin1, Vector3 origin2, Vector3[] rotations1, Vector3[] rotations2, double[] fakeLengths)
        {
            var estimatedPos1 = GetEndPos(origin1, rotations1, fakeLengths);
            var estimatedPos2 = GetEndPos(origin2, rotations2, fakeLengths);

            return estimatedPos1.Dist(estimatedPos2);
        }

        public static double CalcAccuracy(double[] actual, double[] estimate)
        {
            var accuracy = 0d;

            for (var i = 0; i < actual.Length; i++)
            {
                accuracy += Math.Abs((actual[i] - estimate[i]) / actual[i]);
            }

            return Math.Max(0, 1d - accuracy);
        }
    }
}
