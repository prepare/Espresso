//MIT, 2013, Federico Di Gregorio <fog@initd.org>

using System;
namespace Espresso
{
    class BoundWeakDelegate : WeakDelegate
    {
        public BoundWeakDelegate(object target, string name)
            : base(target, name)
        {
        }

        public BoundWeakDelegate(Type type, string name)
            : base(type, name)
        {
        }
    }
}
