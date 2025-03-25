using System.Collections.Generic;
using UnityEngine;

namespace Helpers
{
    public class Smoother
    {
        public Smoother(List<Vector2> dataPointPositions, float smoothingLength = 2f, int smoothingSegmentsCount = 10)
        {
            DataPointPositions = dataPointPositions;
            SmoothingLength = smoothingLength;
            SmoothingSegmentsCount = smoothingSegmentsCount;
        }


        public List<Vector2> DataPointPositions { get; set; }
        public float SmoothingLength { get; set; }
        public int SmoothingSegmentsCount { get; set; }
        public BezierCurve[] LineSectionCurves { get; set; }

        private void PopulateBezierCurvePoints()
        {
            LineSectionCurves = new BezierCurve[DataPointPositions.Count - 1];

            for (int i = 0; i < LineSectionCurves.Length; i++)
            {
                var previousPoint = i == 0 ? DataPointPositions[0] : DataPointPositions[i - 1];
                var currentPoint = DataPointPositions[i];
                var nextPoint = DataPointPositions[i + 1];

                var previousDirection = (currentPoint - previousPoint).normalized;
                var nextDirection = (nextPoint - currentPoint).normalized;

                var startTangent = (previousDirection + nextDirection) * SmoothingLength;
                var endTangent = startTangent * -1;

                LineSectionCurves[i] = new BezierCurve((currentPoint, currentPoint + startTangent, nextPoint + endTangent,nextPoint));
            }
        }

        public List<Vector2> SmoothenPath()
        {
            // TODO double check condition for this
            // Smoothing can not be applied, return original dataset
            if (DataPointPositions.Count < 3)
            {
                return DataPointPositions;
            }
            
            PopulateBezierCurvePoints();
            
            // Data points of the new, smooth curve
            var newDataPointPositions = new List<Vector2>();

            foreach (var lineSectionCurve in LineSectionCurves)
            {
                var smoothSegments = lineSectionCurve.GetSegments(SmoothingSegmentsCount);
                newDataPointPositions.AddRange(smoothSegments);
            }

            return newDataPointPositions;
        }
        
    }
}