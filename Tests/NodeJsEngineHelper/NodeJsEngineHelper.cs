//MIT, 2019-present, WinterDev 
using System;
using System.Collections.Generic;

namespace Espresso
{
    public delegate string LoadMainSrcFile();
    public delegate string NodeJsExecSessionSetup(NodeJsExecSession session);

    public class NodeJsExecSession
    {
        readonly JsEngine Engine;
        readonly JsContext Context;
        internal NodeJsExecSession(JsEngine engine, JsContext ctx)
        {
            Engine = engine;
            Context = ctx;
        }
        public void SetExternalObj<T>(string name, T obj) where T : class
        {
            Context.SetVariableAutoWrap<T>(name, obj);
        }
    }

    public static class NodeJsEngineHelper
    {
        const string LIB_ESPRESSO_CLASS = "LibEspressionClass";        
        const string LOAD_MAIN_SRC_FILE = "LoadMainSrcFile";
        const string LIB_ESPRESSO = "LibEspresso";

        public static void Run(string[] parameters, NodeJsExecSessionSetup nodeExecSession)
        {
            //------------ 
            NodeJsEngine.Run(parameters, (eng, ctx) =>
            {
                //-------------
                //this LibEspressoClass object is need,
                //so node can talk with us,
                //-------------
                JsTypeDefinition jstypedef = new JsTypeDefinition(LIB_ESPRESSO_CLASS);
                NodeJsExecSession nodeJsExecSession = new NodeJsExecSession(eng, ctx);
                string mainSrc = nodeExecSession(nodeJsExecSession);

                jstypedef.AddMember(new JsMethodDefinition(LOAD_MAIN_SRC_FILE, args =>
                 {
                     args.SetResult(mainSrc);
                 }));
                if (!jstypedef.IsRegisterd)
                {
                    ctx.RegisterTypeDefinition(jstypedef);
                }
                //----------
                //then register this as x***       
                //this object is just an instance for reference        
                ctx.SetVariableFromAny(LIB_ESPRESSO, ctx.CreateWrapper(new object(), jstypedef));
            });
        }
        public static void Run(NodeJsExecSessionSetup nodeExecSession)
        {
            //------------ 
            NodeJsEngine.Run((eng, ctx) =>
            {
                //-------------
                //this LibEspressoClass object is need,
                //so node can talk with us,
                //-------------

                JsTypeDefinition jstypedef = new JsTypeDefinition(LIB_ESPRESSO_CLASS);
                NodeJsExecSession nodeJsExecSession = new NodeJsExecSession(eng, ctx);
                string mainSrc = nodeExecSession(nodeJsExecSession);

                jstypedef.AddMember(new JsMethodDefinition(LOAD_MAIN_SRC_FILE, args =>
                {
                    args.SetResult(mainSrc);
                }));
                if (!jstypedef.IsRegisterd)
                {
                    ctx.RegisterTypeDefinition(jstypedef);
                }
                //----------
                //then register this as x***       
                //this object is just an instance for reference        
                ctx.SetVariableFromAny(LIB_ESPRESSO, ctx.CreateWrapper(new object(), jstypedef));
            });
        }
        public static void Run(LoadMainSrcFile loadMainSrcFile)
        {
            //------------ 
            NodeJsEngine.Run((eng, ctx) =>
            {
                //-------------
                //this LibEspressoClass object is need,
                //so node can talk with us,
                //-------------
                JsTypeDefinition jstypedef = new JsTypeDefinition(LIB_ESPRESSO_CLASS);
                jstypedef.AddMember(new JsMethodDefinition(LOAD_MAIN_SRC_FILE, args =>
                {
                    args.SetResult(loadMainSrcFile());
                }));
                if (!jstypedef.IsRegisterd)
                {
                    ctx.RegisterTypeDefinition(jstypedef);
                }
                //----------
                //then register this as x***       
                //this object is just an instance for reference        
                ctx.SetVariableFromAny(LIB_ESPRESSO, ctx.CreateWrapper(new object(), jstypedef));
            });

        }
    }
}