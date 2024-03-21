namespace Cat.Utilities
{
    using System.Collections.Generic;
    using UnityEngine;

    public delegate bool CollisionFilterDelegate3D(RaycastHit raycastHit);
    public delegate bool OverlapFilterDelegate3D(Collider collider);

    public static class PhysicsExtension
    {
        public static int OverlapSphere(Vector3 point, float radius, Collider[] unfilteredResults, List<Collider> filteredResults, OverlapFilterDelegate3D Filter = null)
        {
            int hits = Physics.OverlapSphereNonAlloc(point, radius, unfilteredResults);

            if (hits == 0)
                return 0;

            return FilterValidOverlaps(hits, unfilteredResults, filteredResults, Filter);
        }

        public static int OverlapCapsule(Vector3 bottom, Vector3 top, float radius, Collider[] unfilteredResults, List<Collider> filteredResults, OverlapFilterDelegate3D Filter = null)
        {
            int hits = Physics.OverlapCapsuleNonAlloc(bottom, top, radius, unfilteredResults);

            if (hits == 0)
                return 0;

            return FilterValidOverlaps(hits, unfilteredResults, filteredResults, Filter);
        }

        public static int OverlapBox(Vector3 center, Vector3 size, Quaternion rotation, Collider[] unfilteredResults, List<Collider> filteredResults, OverlapFilterDelegate3D Filter = null)
        {
            int hits = Physics.OverlapBoxNonAlloc(center, size / 2f, unfilteredResults, rotation);

            if (hits == 0)
                return 0;

            return FilterValidOverlaps(hits, unfilteredResults, filteredResults, Filter);
        }

        public static int FilterValidOverlaps(int hits, Collider[] unfilteredOverlaps, List<Collider> filteredOverlaps, OverlapFilterDelegate3D Filter)
        {
            filteredOverlaps.Clear();

            for (int i = 0; i < hits; i++)
            {
                Collider collider = unfilteredOverlaps[i];

                // User-defined filter
                if (Filter != null)
                {
                    bool validHit = Filter(collider);
                    if (!validHit)
                        continue;

                }

                filteredOverlaps.Add(collider);
            }

            return filteredOverlaps.Count;
        }

    }
}