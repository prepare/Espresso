//MIT, 2015-present, WinterDev, EngineKit, brezza92
//MIT, 2013, Federico Di Gregorio <fog@initd.org>

using System;
using System.Reflection;
namespace Espresso
{
    public class JsFunction : IDisposable
    {
        readonly JsContext _context;
        readonly IntPtr _funcPtr;
        readonly IntPtr _thisPtr;
        bool _disposed;
        static MethodInfo s_myInvokeMethodInfo;

        static JsFunction()
        {
#if NET20
            s_myInvokeMethodInfo = typeof(JsFunction).GetMethod("Invoke");
#else
            s_myInvokeMethodInfo = typeof(JsFunction).GetRuntimeMethod("Invoke", null);//.GetMethod("Invoke");
#endif
        }
        public JsFunction(JsContext context, IntPtr funcPtr, IntPtr thisPtr)
        {
            if (context == null)
                throw new ArgumentNullException("context");
            if (funcPtr == IntPtr.Zero)
                throw new ArgumentException("can't wrap an empty function (ptr is Zero)", "ptr");

            _context = context;
            _funcPtr = funcPtr;
            _thisPtr = thisPtr;
        }
        public object Invoke(params object[] args)
        {
            return _context.Invoke(_funcPtr, _thisPtr, args);
        }
        public Delegate MakeDelegate(Type targetDelegateType)
        {
            //check if we already has cache delegate for handle the targetDelegateType
            //if not found then create a new one  

            DelegateTemplate foundTemplate;
            if (_context.GetCacheDelegateForType(targetDelegateType, out foundTemplate))
            {
                //found sample
                //the create new holder from sample template
                return foundTemplate.CreateNewDelegate(targetDelegateType, this);
            }
            //----------------------------------------------------------------------------------

#if NET20
            if (targetDelegateType.BaseType != typeof(MulticastDelegate))
            {
                throw new ApplicationException("Not a delegate.");
            }

            MethodInfo invoke = targetDelegateType.GetMethod("Invoke");
            if (invoke == null)
            {
                throw new ApplicationException("Not a delegate.");
            }
#else
            if (targetDelegateType.GetTypeInfo().BaseType != typeof(MulticastDelegate))
            {
                //throw new ApplicationException("Not a delegate.");
                throw new Exception("Not a delegate.");
            }

            MethodInfo invoke = targetDelegateType.GetRuntimeMethod("Invoke", null);//.GetMethod("Invoke");
            if (invoke == null)
            {
                //throw new ApplicationException("Not a delegate.");
                throw new Exception("Not a delegate.");
            }
#endif
            ParameterInfo[] invokeParams = invoke.GetParameters();
            int argCount = invokeParams.Length;
            Type returnType = invoke.ReturnType;
            bool returnVoid = returnType == typeof(void);
            Type[] typelist = null;
            if (returnVoid)
            {
                typelist = new Type[argCount];
                for (int i = 0; i < argCount; ++i)
                {
                    typelist[i] = invokeParams[i].ParameterType;
                }
            }
            else
            {
                typelist = new Type[argCount + 1]; //+1 for return type
                for (int i = 0; i < argCount; ++i)
                {
                    typelist[i] = invokeParams[i].ParameterType;
                }
                typelist[argCount] = returnType;
            }
            //----------------------------------
            //create delegate holder
            //you can add more than 1  
            Type delHolderType = null;
            switch (argCount)
            {
                case 0:
                    {
                        //0 input
                        delHolderType = returnVoid ?
                            typeof(ActionDelegateHolder) :
                            typeof(FuncDelegateHolder<>).MakeGenericType(typelist);

                    }
                    break;
                case 1:
                    {
                        //1 input 
                        delHolderType = returnVoid ?
                            typeof(ActionDelegateHolder<>).MakeGenericType(typelist) :
                            typeof(FuncDelegateHolder<,>).MakeGenericType(typelist);

                    }
                    break;
                case 2:
                    {
                        delHolderType = returnVoid ?
                            typeof(ActionDelegateHolder<,>).MakeGenericType(typelist) :
                            typeof(FuncDelegateHolder<,,>).MakeGenericType(typelist);
                    }
                    break;
                case 3:
                    {
                        delHolderType = returnVoid ?
                            typeof(ActionDelegateHolder<,,>).MakeGenericType(typelist) :
                            typeof(FuncDelegateHolder<,,,>).MakeGenericType(typelist);

                    }
                    break;
                case 4:
                    {
                        delHolderType = returnVoid ?
                            typeof(ActionDelegateHolder<,,,>).MakeGenericType(typelist) :
                            typeof(FuncDelegateHolder<,,,,>).MakeGenericType(typelist);
                    }
                    break;
                default:
                    {
                        //create more if you want
                        throw new NotSupportedException();
                    }
            }
            //----------------------------------
            //create sample 
            DelegateTemplate newTemplate = new DelegateTemplate(
                delHolderType,
                Activator.CreateInstance(delHolderType) as DelegateHolder);
            //cache 
            _context.CacheDelegateForType(targetDelegateType, newTemplate);

            //new delegate created from sample
            return newTemplate.CreateNewDelegate(targetDelegateType, this);

        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
                throw new ObjectDisposedException("JsObject:" + _funcPtr);

            _disposed = true;

            _context.Engine.DisposeObject(_funcPtr);
            if (_thisPtr != IntPtr.Zero)
            {
                _context.Engine.DisposeObject(_thisPtr);
            }
        }

        ~JsFunction()
        {
            if (!_disposed)
                Dispose(false);
        }


    }
}
