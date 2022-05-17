using System;
using System.Linq;

namespace SlimeVR.AutoBone.Theory
{
    public class Program
    {
        public static void Main(string[] args)
        {
            RunTestSet((i) => {
                return RunTest(3d, false);
            }, "Not using contribution");

            RunTestSet((i) => {
                return RunTest(6.5d, true);
            }, "Using contribution");
        }

        public static void RunTestSet(Func<int, TestReport> testFunction, string? testName = null)
        {
            var numTests = 100;

            var iters = 0;
            var accuracy = 0d;
            var improvement = 0d;

            for (var i = 0; i < numTests; i++)
            {
                var report = testFunction(i);
                iters += report.Iters;
                accuracy += report.Accuracy;
                improvement += report.Improvement;
            }

            iters /= numTests;
            accuracy /= numTests;
            improvement /= numTests;

            Console.WriteLine(testName != null ? $"===== RESULTS ({testName}) =====" : "===== RESULTS =====");
            Console.WriteLine($"Avg iters: {iters}");
            Console.WriteLine($"Avg accuracy: {accuracy:0.0000}%");
            Console.WriteLine($"Avg improvement: {improvement:0.0000}%");
        }

        public struct TestReport
        {
            public int Iters;

            public double OrigAccuracy;
            public double Accuracy;

            public double Improvement => Accuracy - OrigAccuracy;
        }

        public static TestReport RunTest(double initRate = 200d, bool useContribution = true, bool randomlyOffset = true)
        {
            var targetError = 0d;
            var targetAccuracy = 99.9999d;

            var numSegments = 16;
            var random = new Random();

            var footPos = new Vector3(random) * 5d;

            var lengths = new double[numSegments];
            for (var j = 0; j < lengths.Length; j++)
            {
                lengths[j] = random.NextDoubleInclusive(10d, 50d);
            }

            //Console.WriteLine($"Lengths: {string.Join(", ", lengths)}");

            var fakeLengths = new double[numSegments];
            for (var j = 0; j < fakeLengths.Length; j++)
            {
                fakeLengths[j] = random.NextDoubleInclusive(10d, 50d);
            }
            
            var originalFakeLengths = (double[])fakeLengths.Clone();

            //Console.WriteLine($"Fake lengths: {string.Join(", ", fakeLengths)}");
            //Console.WriteLine();

            var rate = initRate;
            var i = 0;
            while (true)
            {
                if (i > 0)
                {
                    rate *= 0.9998d;
                }
                
                i++;

                if (rate <= 0.0000001d)
                {
                    Console.WriteLine("Rate is too low... Giving up.");
                    break;
                }

                var rotations1 = new Vector3[numSegments];
                for (var j = 0; j < rotations1.Length; j++)
                {
                    rotations1[j] = random.NextVectorRotation();
                }

                var rotations2 = new Vector3[numSegments];
                for (var j = 0; j < rotations2.Length; j++)
                {
                    rotations2[j] = random.NextVectorRotation();
                }

                //Console.WriteLine($"Test {i} rotations1: {string.Join<Vector3>(", ", rotations1)}");

                var origin1 = GetOriginPos(footPos, rotations1, lengths);
                var origin2 = GetOriginPos(footPos, rotations2, lengths);
                //var originOffset = origin2 - origin1;
                var originDist = origin1.Dist(origin2);

                //Console.WriteLine($"Test {i} origin dist: {originDist}");

                var estimatedPos1 = GetEndPos(origin1, rotations1, fakeLengths);
                var estimatedPos2 = GetEndPos(origin2, rotations2, fakeLengths);
                var estimatedPosOffset = estimatedPos2 - estimatedPos1;

                var dist = estimatedPos2.Dist(estimatedPos1);
                var errorDeriv = CalcErrorDeriv(originDist, dist);
                //var error = CalcError(errorDeriv);

                var adjust = errorDeriv * rate;

                if (adjust <= 0)
                {
                    continue;
                }

                //Console.WriteLine($"Test {i} estimated position offset: {dist}");
                //Console.WriteLine($"Test {i} error deriv: {errorDeriv}");
                //Console.WriteLine($"Test {i} error: {error}");
                //Console.WriteLine($"Test {i} adjust: {adjust}");

                var accuracy = CalcAccuracy(lengths, fakeLengths) * 100d;
                //Console.WriteLine($"Test {i} accuracy: {accuracy}");

                if (accuracy >= targetAccuracy)
                {
                    break;
                }

                if (errorDeriv <= targetError)
                {
                    break;
                }

                var lengthSum = fakeLengths.Sum();

                var fakeLengthsCopy = (double[])fakeLengths.Clone();
                for (var j = 0; j < fakeLengthsCopy.Length; j++)
                {
                    double adjust2;
                    if (useContribution)
                    {
                        adjust2 = (adjust * CalcRotationContribution(estimatedPosOffset, rotations1[j], rotations2[j], fakeLengthsCopy[j])) / lengthSum;
                    }
                    else
                    {
                        adjust2 = (adjust * fakeLengthsCopy[j]) / lengthSum;
                    }

                    //Console.WriteLine($"Test {i} adjust2: {adjust2}");
                    //Console.WriteLine($"Test {i} rotation contrib: {CalcRotationContribution(estimatedPosOffset, rotations1[j], rotations2[j], fakeLengthsCopy[j])}");

                    var fakeLengthsCopy2 = (double[])fakeLengthsCopy.Clone();
                    fakeLengthsCopy2[j] = fakeLengthsCopy[j] + adjust2;

                    var dist2 = CalcDist(origin1, origin2, rotations1, rotations2, fakeLengthsCopy2);
                    var errorDeriv2 = CalcErrorDeriv(originDist, dist2);

                    if (errorDeriv2 > errorDeriv)
                    {
                        fakeLengthsCopy2[j] = fakeLengthsCopy[j] - adjust2;

                        var dist3 = CalcDist(origin1, origin2, rotations1, rotations2, fakeLengthsCopy2);
                        var errorDeriv3 = CalcErrorDeriv(originDist, dist3);

                        if (fakeLengthsCopy2[j] < 0 || errorDeriv3 > errorDeriv)
                        {
                            continue;
                        }
                        else
                        {
                            fakeLengths[j] -= adjust2;
                        }
                    }
                    else
                    {
                        fakeLengths[j] += adjust2;
                    }
                }
            }

            //Console.WriteLine();
            //Console.WriteLine($"Lengths: {string.Join(", ", lengths)}");
            //Console.WriteLine($"Fake lengths: {string.Join(", ", originalFakeLengths)}");
            //Console.WriteLine($"Estimated lengths: {string.Join(", ", fakeLengths)}");

            var origAccuracy = CalcAccuracy(lengths, originalFakeLengths) * 100d;
            var finalAccuracy = CalcAccuracy(lengths, fakeLengths) * 100d;

            //Console.WriteLine($"Start accuracy: {origAccuracy:0.0000}% Final accuracy: {finalAccuracy:0.0000}%");
            return new TestReport()
            {
                Iters = i,
                OrigAccuracy = origAccuracy,
                Accuracy = finalAccuracy
            };
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

        public static double CalcErrorDeriv(double rootDist, double endDist)
        {
            //return rootDist > 0 ? (endDist / 2d) / rootDist : 0;
            return endDist / 2d;
        }

        public static double CalcError(double errorDeriv)
        {
            return 0.5d * (errorDeriv * errorDeriv);
        }

        public static double CalcRotationContribution(Vector3 offset, Vector3 rotation1, Vector3 rotation2, double length)
        {
            Vector3 normalizedOffset = offset.Normalized;

            var dot1 = Math.Abs(normalizedOffset.Dot(rotation1));
            var dot2 = Math.Abs(normalizedOffset.Dot(rotation2));

            return length * Math.Abs(dot2 - dot1);
        }
    }
}
