using System;
using System.Collections.Generic;
using Android.OS;

namespace Ambulance.Droid.TspImplementaion
{
    public static class RandomProvider
    {
        private static readonly Random _random = new Random();

        public static int GetRandomValue(int limit)
        {
            return _random.Next(limit);
        }

        public static Location[] GetRandomDestinations(int count)
        {
                return null;
            
        }

        public static void MutateRandomLocations(Location[] locations)
        {
            if (locations == null || locations.Length < 2)
                return;
            
            int mutationCount = GetRandomValue(locations.Length / 10) + 1;
            for (int mutationIndex = 0; mutationIndex < mutationCount; mutationIndex++)
            {
                int index1 = GetRandomValue(locations.Length);
                int index2 = GetRandomValue(locations.Length - 1);
                if (index2 >= index1)
                    index2++;

                switch (GetRandomValue(3))
                {
                    case 0: Location.SwapLocations(locations, index1, index2); break;
                    case 1: Location.MoveLocations(locations, index1, index2); break;
                    case 2: Location.ReverseRange(locations, index1, index2); break;
                    default: throw new InvalidOperationException();
                }
            }
        }

        public static void FullyRandomizeLocations(Location[] locations)
        {
            if (locations == null)
                return;
            
            int count = locations.Length;
            for (int i = count - 1; i > 0; i--)
            {
                int value = GetRandomValue(i + 1);
                if (value != i)
                    Location.SwapLocations(locations, i, value);
            }
        }

        internal static void _CrossOver(Location[] locations1, Location[] locations2, bool mutateFailedCrossovers)
        {
            var availableLocations = new HashSet<Location>(locations1);

            int startPosition = GetRandomValue(locations1.Length);
            int crossOverCount = GetRandomValue(locations1.Length - startPosition);

            if (mutateFailedCrossovers)
            {
                bool useMutation = true;
                int pastEndPosition = startPosition + crossOverCount;
                for (int i = startPosition; i < pastEndPosition; i++)
                {
                    if (locations1[i] != locations2[i])
                    {
                        useMutation = false;
                        break;
                    }
                }
                
                if (useMutation)
                {
                    MutateRandomLocations(locations1);
                    return;
                }
            }

            Array.Copy(locations2, startPosition, locations1, startPosition, crossOverCount);
            List<int> toReplaceIndexes = null;
            
            int index = 0;
            foreach (var value in locations1)
            {
                if (!availableLocations.Remove(value))
                {
                    if (toReplaceIndexes == null)
                        toReplaceIndexes = new List<int>();

                    toReplaceIndexes.Add(index);
                }

                index++;
            }

            // Finally we will replace duplicated items by those that are still available.
            // This is how we avoid having chromosomes that contain duplicated places to go.
            if (toReplaceIndexes != null)
            {
                // To do this, we enumerate two objects in parallel.
                // If we could use foreach(var indexToReplace, location from toReplaceIndexex, location1) it would be great.
                using (var enumeratorIndex = toReplaceIndexes.GetEnumerator())
                {
                    using (var enumeratorLocation = availableLocations.GetEnumerator())
                    {
                        while (true)
                        {
                            if (!enumeratorIndex.MoveNext())
                            {
                              
                                break;
                            }

                            if (!enumeratorLocation.MoveNext())
                                break;

                            locations1[enumeratorIndex.Current] = enumeratorLocation.Current;
                        }
                    }
                }
            }
        }
    }

}