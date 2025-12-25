using System.Numerics;

namespace Nl2TrackGen.Models
{
    public struct TrackPoint
    {
        public Vector3 Position;
        public Vector3 Front;
        public Vector3 Left;
        public Vector3 Up;

        public TrackPoint(Vector3 pos, Vector3 front, Vector3 up)
        {
            Position = pos;
            Front = Vector3.Normalize(front);
            Up = Vector3.Normalize(up);
            Left = Vector3.Normalize(Vector3.Cross(Up, Front));
            // Re-orthogonalize Up to be sure
            Up = Vector3.Normalize(Vector3.Cross(Front, Left));
        }
    }
}
