using System.Linq;
using System;
using System.Collections.Generic;

namespace Ambulance.Droid.TspImplementaion
{
    public sealed class Location
    {
        public Dictionary<int, double> Distance;
        public Dictionary<int, double> Time;
        public int Id;
        public long? OrderId;

        public Location()
        {
        }

        // We could add other properties, like the name, the description
        // or anything similar that we consider useful.

        public long X { get; private set; }
        public long Y { get; private set; }


        public double GetDistance(Location other)
        {
            return Time[other.Id];
        }




        public static double GetTotalDistance(Location startLocation, Location[] locations)
        {
            if (startLocation == null || locations == null || locations.Length == 0)
                return 0.0;

            foreach (var location in locations)
                if (location == null)
                    return 0.0;


            var resultedList = locations.ToList();
            resultedList.Insert(0, startLocation);
            double totalTime = 0;
            double totalLenght = 0;
            for (int i = 0; i < resultedList.Count() - 1; i++)
            {
                totalLenght += resultedList[i].Distance[resultedList[i + 1].Id];
                totalTime += resultedList[i].Time[resultedList[i + 1].Id];
            }

          /*  double result = startLocation.GetDistance(locations[0]);
            int countLess1 = locations.Length - 1;
            for (int i = 0; i < countLess1; i++)
            {
                var actual = locations[i];
                var next = locations[i + 1];

                var distance = actual.GetDistance(next);
                result += distance;
            }*/

            //result += locations[locations.Length - 1].GetDistance(startLocation);

            return totalTime;
        }

        public static void SwapLocations(Location[] locations, int index1, int index2)
        {
            if (locations == null || index1 < 0 || index1 >= locations.Length || index2 < 0 || index2 >= locations.Length)
                return;

            var location1 = locations[index1];
            var location2 = locations[index2];
            locations[index1] = location2;
            locations[index2] = location1;
        }
        
        public static void MoveLocations(Location[] locations, int fromIndex, int toIndex)
        {
            if (locations == null || fromIndex < 0 || fromIndex >= locations.Length || toIndex < 0 || toIndex >= locations.Length)
                return;

            var temp = locations[fromIndex];

            if (fromIndex < toIndex)
            {
                for (int i = fromIndex + 1; i <= toIndex; i++)
                    locations[i - 1] = locations[i];
            }
            else
            {
                for (int i = fromIndex; i > toIndex; i--)
                    locations[i] = locations[i - 1];
            }

            locations[toIndex] = temp;
        }

        public static void ReverseRange(Location[] locations, int startIndex, int endIndex)
        {
            if (locations == null || startIndex < 0 || startIndex >= locations.Length || endIndex < 0 || endIndex >= locations.Length)
                   return;

            if (endIndex < startIndex)
            {
                int temp = endIndex;
                endIndex = startIndex;
                startIndex = temp;
            }

            while (startIndex < endIndex)
            {
                Location temp = locations[endIndex];
                locations[endIndex] = locations[startIndex];
                locations[startIndex] = temp;

                startIndex++;
                endIndex--;
            }
        }

        public override string ToString()
        {
            return X + ", " + Y;
        }
    }

}