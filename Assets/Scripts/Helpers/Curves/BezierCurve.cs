using System;
using UnityEngine;

namespace Helpers
{
    public class BezierCurve
    {

        public BezierCurve((Vector2 p0, Vector2 p1, Vector2 p2, Vector2 p3) points)
        {
            Points = points;
        }

        public (Vector2 p0, Vector2 p1, Vector2 p2, Vector2 p3) Points { get; set; }

        public Vector2 GetSegment(float t)
        {
            t = Mathf.Clamp01(t);
            var oneMinusT = 1 - t;
            
            // Formula of Cubic Bézier Curve from Wikipedia
            return Mathf.Pow(oneMinusT, 3) * Points.p0 + 3 * Mathf.Pow(oneMinusT, 2) * t * Points.p1 +
                   3 * oneMinusT * Mathf.Pow(t, 2) * Points.p2 + Mathf.Pow(t, 3) * Points.p3;
        }

        public Vector2[] GetSegments(int subdivisions)
        {
            Vector2[] segments = new Vector2[subdivisions];

            for (int i = 0; i < subdivisions; i++)
            {
                var t = (float)i / subdivisions;
                segments[i] = GetSegment(t);
            }

            return segments;
        }
    }
}