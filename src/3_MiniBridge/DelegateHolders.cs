//MIT, 2015-2016, WinterDev
using System;
using System.Reflection;
namespace Espresso
{
    //create more if you want

    class DelegateTemplate
    {
        public readonly Type delHolderType;
        public readonly DelegateHolder holder;

        public DelegateTemplate(Type delHolderType, DelegateHolder holder)
        {
            this.delHolderType = delHolderType;
            this.holder = holder;
        }

        public Delegate CreateNewDelegate(Type targetDelegateType, JsFunction jsfunc)
        {
            DelegateHolder newHolder = this.holder.New();
            newHolder._jsFunc = jsfunc;
#if NET20
            return Delegate.CreateDelegate(targetDelegateType,
                newHolder,
                this.holder.InvokeMethodInfo);
#else
            return this.holder.InvokeMethodInfo.CreateDelegate(targetDelegateType, newHolder);
#endif

        }
    }

    static class Helper
    {
        public static MethodInfo GetInvokeMethod<T>()
        {
#if NET20
            return typeof(T).GetMethod("Invoke");
#else
            return typeof(T).GetRuntimeMethod("Invoke", null);//.GetMethod("Invoke");
#endif
        }
    }

    abstract class DelegateHolder
    {
        public JsFunction _jsFunc;
        /// <summary>
        /// create new black delegate holder with the same kind
        /// </summary>
        /// <returns></returns>
        public abstract DelegateHolder New();
        public abstract MethodInfo InvokeMethodInfo { get; }
    }

    class FuncDelegateHolder<TResult> : DelegateHolder
    {
        static MethodInfo s_invokeMethodInfo = Helper.GetInvokeMethod<FuncDelegateHolder<TResult>>();

        public TResult Invoke() => (TResult)_jsFunc.Invoke(new object[0]);

        public override DelegateHolder New() => new FuncDelegateHolder<TResult>();

        public override MethodInfo InvokeMethodInfo => s_invokeMethodInfo;
    }
    class FuncDelegateHolder<T, TResult> : DelegateHolder
    {
        static MethodInfo s_invokeMethodInfo = Helper.GetInvokeMethod<FuncDelegateHolder<T, TResult>>();

        public TResult Invoke(T arg) => (TResult)_jsFunc.Invoke(arg);

        public override DelegateHolder New() => new FuncDelegateHolder<T, TResult>();

        public override MethodInfo InvokeMethodInfo => s_invokeMethodInfo;

    }
    class FuncDelegateHolder<T1, T2, TResult> : DelegateHolder
    {
        static MethodInfo s_invokeMethodInfo = Helper.GetInvokeMethod<FuncDelegateHolder<T1, T2, TResult>>();

        public TResult Invoke(T1 arg1, T2 arg2) => (TResult)_jsFunc.Invoke(arg1, arg2);

        public override DelegateHolder New() => new FuncDelegateHolder<T1, T2, TResult>();

        public override MethodInfo InvokeMethodInfo => s_invokeMethodInfo;
    }
    class FuncDelegateHolder<T1, T2, T3, TResult> : DelegateHolder
    {
        static MethodInfo s_invokeMethodInfo = Helper.GetInvokeMethod<FuncDelegateHolder<T1, T2, T3, TResult>>();
        public TResult Invoke(T1 arg1, T2 arg2, T3 arg3) => (TResult)_jsFunc.Invoke(arg1, arg2, arg3);

        public override DelegateHolder New() => new FuncDelegateHolder<T1, T2, T3, TResult>();

        public override MethodInfo InvokeMethodInfo => s_invokeMethodInfo;
    }
    class FuncDelegateHolder<T1, T2, T3, T4, TResult> : DelegateHolder
    {
        static MethodInfo s_invokeMethodInfo = Helper.GetInvokeMethod<FuncDelegateHolder<T1, T2, T3, T4, TResult>>();

        public TResult Invoke(T1 arg1, T2 arg2, T3 arg3, T4 arg4) => (TResult)_jsFunc.Invoke(arg1, arg2, arg3, arg4);

        public override DelegateHolder New() => new FuncDelegateHolder<T1, T2, T3, T4, TResult>();

        public override MethodInfo InvokeMethodInfo => s_invokeMethodInfo;
    }


    class ActionDelegateHolder : DelegateHolder
    {
        static MethodInfo s_invokeMethodInfo = Helper.GetInvokeMethod<ActionDelegateHolder>();

        public void Invoke() => _jsFunc.Invoke(new object[0]);

        public override DelegateHolder New() => new ActionDelegateHolder();

        public override MethodInfo InvokeMethodInfo => s_invokeMethodInfo;

    }
    class ActionDelegateHolder<T> : DelegateHolder
    {
        static MethodInfo s_invokeMethodInfo = Helper.GetInvokeMethod<ActionDelegateHolder<T>>();

        public void Invoke(T arg) => _jsFunc.Invoke(arg);

        public override DelegateHolder New() => new ActionDelegateHolder<T>();

        public override MethodInfo InvokeMethodInfo => s_invokeMethodInfo;
    }
    class ActionDelegateHolder<T1, T2> : DelegateHolder
    {
        static MethodInfo s_invokeMethodInfo = Helper.GetInvokeMethod<ActionDelegateHolder<T1, T2>>();

        public void Invoke(T1 arg1, T2 arg2) => _jsFunc.Invoke(arg1, arg2);

        public override DelegateHolder New() => new ActionDelegateHolder<T1, T2>();

        public override MethodInfo InvokeMethodInfo => s_invokeMethodInfo;

    }

    class ActionDelegateHolder<T1, T2, T3> : DelegateHolder
    {
        static MethodInfo s_invokeMethodInfo = Helper.GetInvokeMethod<ActionDelegateHolder<T1, T2, T3>>();

        public void Invoke(T1 arg1, T2 arg2, T3 arg3) => _jsFunc.Invoke(arg1, arg2, arg3);

        public override DelegateHolder New() => new ActionDelegateHolder<T1, T2, T3>();

        public override MethodInfo InvokeMethodInfo => s_invokeMethodInfo;

    }
    class ActionDelegateHolder<T1, T2, T3, T4> : DelegateHolder
    {
        static MethodInfo s_invokeMethodInfo = Helper.GetInvokeMethod<ActionDelegateHolder<T1, T2, T3, T4>>();

        public void Invoke(T1 arg1, T2 arg2, T3 arg3, T4 arg4) => _jsFunc.Invoke(arg1, arg2, arg3, arg4);

        public override DelegateHolder New() => new ActionDelegateHolder<T1, T2, T3, T4>();

        public override MethodInfo InvokeMethodInfo => s_invokeMethodInfo;
    }
}