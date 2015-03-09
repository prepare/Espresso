//2015, MIT WinterDev
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;

namespace VroomJs
{
    //create more if you want


    abstract class DelegateHolder
    {
        public JsFunction jsFunc;
        /// <summary>
        /// create new black delegate holder with the same kind
        /// </summary>
        /// <returns></returns>
        public abstract DelegateHolder New();
    }

    class FuncDelegateHolder<TResult> : DelegateHolder
    {
        public TResult Invoke()
        {
            return (TResult)this.jsFunc.Invoke(new object[0]);
        }
        public override DelegateHolder New()
        {
            return new FuncDelegateHolder<TResult>();
        }
    }
    class FuncDelegateHolder<T, TResult> : DelegateHolder
    {
        public TResult Invoke(T arg)
        {
            return (TResult)this.jsFunc.Invoke(arg);
        }
        public override DelegateHolder New()
        {
            return new FuncDelegateHolder<T, TResult>();
        }
    }
    class FuncDelegateHolder<T1, T2, TResult> : DelegateHolder
    {
        public TResult Invoke(T1 arg1, T2 arg2)
        {
            return (TResult)this.jsFunc.Invoke(arg1, arg2);
        }
        public override DelegateHolder New()
        {
            return new FuncDelegateHolder<T1, T2, TResult>();
        }
    }
    class FuncDelegateHolder<T1, T2, T3, TResult> : DelegateHolder
    {
        public TResult Invoke(T1 arg1, T2 arg2, T3 arg3)
        {
            return (TResult)this.jsFunc.Invoke(arg1, arg2, arg3);
        }
        public override DelegateHolder New()
        {
            return new FuncDelegateHolder<T1, T2, T3, TResult>();
        }
    }
    class FuncDelegateHolder<T1, T2, T3, T4, TResult> : DelegateHolder
    {
        public TResult Invoke(T1 arg1, T2 arg2, T3 arg3, T4 arg4)
        {
            return (TResult)this.jsFunc.Invoke(arg1, arg2, arg3, arg4);
        }
        public override DelegateHolder New()
        {
            return new FuncDelegateHolder<T1, T2, T3, T4, TResult>();
        }
    }


    class ActionDelegateHolder : DelegateHolder
    {
        public void Invoke()
        {
            this.jsFunc.Invoke(new object[0]);
        }
        public override DelegateHolder New()
        {
            return new ActionDelegateHolder();
        }
    }
    class ActionDelegateHolder<T> : DelegateHolder
    {
        public void Invoke(T arg)
        {
            this.jsFunc.Invoke(arg);
        }
        public override DelegateHolder New()
        {
            return new ActionDelegateHolder<T>();
        }
    }
    class ActionDelegateHolder<T1, T2> : DelegateHolder
    {
        public void Invoke(T1 arg1, T2 arg2)
        {
            this.jsFunc.Invoke(arg1, arg2);
        }
        public override DelegateHolder New()
        {
            return new ActionDelegateHolder<T1, T2>();
        }
    }

    class ActionDelegateHolder<T1, T2, T3> : DelegateHolder
    {
        public void Invoke(T1 arg1, T2 arg2, T3 arg3)
        {
            this.jsFunc.Invoke(arg1, arg2, arg3);
        }
        public override DelegateHolder New()
        {
            return new ActionDelegateHolder<T1, T2, T3>();
        }
    }
    class ActionDelegateHolder<T1, T2, T3, T4> : DelegateHolder
    {
        public void Invoke(T1 arg1, T2 arg2, T3 arg3, T4 arg4)
        {
            this.jsFunc.Invoke(arg1, arg2, arg3, arg4);
        }
        public override DelegateHolder New()
        {
            return new ActionDelegateHolder<T1, T2, T3, T4>();
        }
    }


}