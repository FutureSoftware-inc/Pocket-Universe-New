using System;
using UnityEngine.Serialization;

namespace Crystal.Common
{
    internal static class RefProvideHandler
    {
        internal static TRefType GetRef<TRefType>(UnityEngine.Object host, long id, ref WeakReference<TRefType> cache) where TRefType : class
        {
            if (host == null || id == 0)
            {
                return null;
            }
#if !DISABLE_REFTO_CACHE
            if (cache != null && cache.TryGetTarget(out var cachedTarget))
            {
                return cachedTarget;
            }
#endif
            var value = ManagedReferenceUtility.GetManagedReference(host, id);
            if (value is TRefType castObject)
            {
#if !DISABLE_REFTO_CACHE
                if (cache == null)
                {
                    cache = new WeakReference<TRefType>(castObject);
                }
                else
                {
                    cache.SetTarget(castObject);
                }
#endif
                return castObject;
            }
            return null;
        }
    }
}