//MIT, 2015-present, WinterDev, EngineKit, brezza92

using System;
using System.Collections.Generic;
using System.Reflection;

namespace Espresso
{

    public abstract class JsTypeDefinitionBuilder
    {
        internal JsTypeDefinition BuildTypeDefinition(Type t)
        {
            return this.OnBuildRequest(t);
        }
        protected abstract JsTypeDefinition OnBuildRequest(Type t);
    }

    class DefaultJsTypeDefinitionBuilder : JsTypeDefinitionBuilder
    {

        /// <summary>
        /// group of overload method that has the same arg count
        /// </summary>
        class SameArgMethodList
        {
            JsMethodDefinition _onlyOneMember;
            List<JsMethodDefinition> _members;
            readonly int _argCount;
            public SameArgMethodList(int argCount, JsMethodDefinition onlyOneMember)
            {
                _argCount = argCount;
                _onlyOneMember = onlyOneMember;
            }
            public void AddMore(JsMethodDefinition anotherMethod)
            {
                if (_members == null)
                {
                    _members = new List<JsMethodDefinition>();
                    _members.Add(_onlyOneMember);
                    _onlyOneMember = null;
                }
                _members.Add(anotherMethod);
            }

            static int CalculateTypeDepth(Type testType, Type baseType)
            {
                int depthLevel = 0;
                if (testType == baseType)
                {
                    return 0;
                }
                else
                {
                    Type curType = GetBaseType(testType);
                    if (curType == null)
                    {
                        //no base type
                        return depthLevel;
                    }
                    depthLevel++;
                    while ((curType != baseType) && curType != typeof(object))
                    {
                        curType = GetBaseType(curType); //go next level
                        depthLevel++;
                        if (curType == typeof(object))
                        {
                            break;
                        }
                    }
                    return depthLevel;
                }
            }
            public JsMethodDefinition SelectBestMethod(Type[] inputArgTypes)
            {
                if (_onlyOneMember != null)
                {
                    return _onlyOneMember;
                }
                //------------------------------               
                //TODO: check performance here 
                //TODO: review How Roslyn select best method ***
                //-----------------------------
                //best fit? 

                int memberCount = _members.Count;
                int[] bestScores = new int[memberCount];
                int maxBestScore = -1;
                int candidateMaxIndex = 0;
                bool hasOnly1Candidate = true;

                for (int m = memberCount - 1; m >= 0; --m)
                {
                    JsMethodDefinition met = _members[m];
                    int convScore = 0;

                    for (int argNo = _argCount - 1; argNo >= 0; --argNo)
                    {
                        //pick one arg
                        Type inputArgType = inputArgTypes[argNo];
                        Type parType = met.Parameters[argNo].ParameterType;
                        if (inputArgType == null)
                        {
                            //if input value is null
                            //special mx
                            convScore += CalculateTypeDepth(parType, typeof(object));
                        }
                        else if (parType == inputArgType)
                        {
                            //exact type 
                            convScore += 255;
                        }
                        else if (IsAssignable(parType, inputArgType))
                        {
                            //assignable
                            //check inheritance depth?
                            //find conversion energy (depth)
                            convScore += CalculateTypeDepth(inputArgType, parType);
                        }
                        else
                        {
                            //can't assign
                            //can't use this method 
                            convScore = -1;
                            argNo = -1; //not check more => go next method 
                        }
                    }

                    bestScores[m] = convScore; //store value , for later use ,TODO: review here
                    if (convScore > maxBestScore)
                    {
                        maxBestScore = convScore;
                        candidateMaxIndex = m;
                        hasOnly1Candidate = true;
                    }
                    else if (convScore == maxBestScore)
                    {
                        hasOnly1Candidate = false;
                    }
                }
                //-----------------------------
                //find max best score
                //TODO: check if we have more than 1 best value 
                //-----------------------------
                if (maxBestScore < 0)
                {
                    return null;
                }
                if (hasOnly1Candidate)
                {
                    return _members[candidateMaxIndex];
                }
                //TODO: review ...

                //find member with max score
                JsMethodDefinition[] candidateMethods = new JsMethodDefinition[memberCount];
                int n = 0;
                for (int i = memberCount - 1; i >= 0; --i)
                {
                    if (bestScores[i] == maxBestScore)
                    {
                        candidateMethods[n] = _members[i];
                        n++;
                    }
                }

                if (n > 1)
                {
                    //more than 1 best method
                }
                else
                {
                    //only 1
                    return candidateMethods[0];
                }
                return null;
            }
        }


        /// <summary>
        /// group of overload methods ( that have the same)
        /// </summary>
        class JsMethodGroup
        {

            internal List<JsMethodDefinition> members;
            internal JsMethodDefinition onlyOneMember;

            Dictionary<int, SameArgMethodList> sameArgNumMethodLists = null;

            public JsMethodGroup(string name, JsMethodDefinition firstMember)
            {
                this.Name = name;
                this.onlyOneMember = firstMember;
            }
            public void AddMoreMember(JsMethodDefinition anotherMethod)
            {
                if (members == null)
                {
                    members = new List<JsMethodDefinition>();
                    members.Add(onlyOneMember);
                    onlyOneMember = null;
                }
                members.Add(anotherMethod);
            }
            public int MemberCount
            {
                get
                {
                    if (members == null) { return 1; }
                    else
                    {
                        return members.Count;
                    }
                }
            }
            public string Name { get; private set; }


            public virtual JsMethodDefinition GetJsMethod()
            {
                if (onlyOneMember != null)
                {
                    return onlyOneMember;
                }
                else
                {
                    //if we have > 1 member
                    //create a 'wrapper' method for all method in this group                     
                    //------
                    //analyze method based on arg count and the arg type
                    //quite complex
                    //------  
                    //this is a default .... resolver
                    //you can provide a special method overload resolution 
                    //for each method group

                    if (sameArgNumMethodLists == null)
                    {
                        sameArgNumMethodLists = new Dictionary<int, SameArgMethodList>();
                        int j = members.Count;
                        for (int i = 0; i < j; ++i)
                        {
                            JsMethodDefinition met = members[i];
                            ParameterInfo[] pars = met.Parameters;
                            if (pars != null)
                            {
                                SameArgMethodList existingMethods;
                                int count = pars.Length;
                                if (!sameArgNumMethodLists.TryGetValue(count, out existingMethods))
                                {
                                    existingMethods = new SameArgMethodList(count, met);
                                    sameArgNumMethodLists.Add(count, existingMethods);
                                }
                                else
                                {
                                    existingMethods.AddMore(met);
                                }
                            }
                            else
                            {
                                //delegate base?
                                //
                                //TODO : review this again
                                return null;
                            }
                        }
                    }
                    return new JsMethodDefinition(this.Name, args =>
                    {

                        //how to select the best over method based on input args:
                        //this resolve at runtime=> may be slow
                        //TODO: review here again

                        //num of arg not match
                        //TODO &
                        //LIMITATION: this version dose NOT support
                        //1. default parameter
                        //2. param args 
                        //invoke method
                        var thisArg = args.GetThisArg();
                        //actual input arg count
                        int actualArgCount = args.ArgCount;

                        SameArgMethodList foundMets;
                        if (!sameArgNumMethodLists.TryGetValue(actualArgCount, out foundMets))
                        {
                            //so if num of arg not match the return
                            args.SetResultUndefined(); //?
                            return;
                        }

                        //-----------
                        //select best method by checking each vars
                        //TODO: how to find  hints ?
                        //best match args
                        object[] inputArgs = new object[actualArgCount];
                        Type[] inputArgsTypes = new Type[actualArgCount];
                        for (int i = 0; i < actualArgCount; ++i)
                        {
                            object arg_v = args.GetArgAsObject(i);
                            inputArgs[i] = arg_v;
                            if (arg_v == null)
                            {
                                //review here again
                                inputArgsTypes[i] = null;//null value
                            }
                            else
                            {
                                inputArgsTypes[i] = arg_v.GetType();
                            }
                        }
                        JsMethodDefinition selectedMet = foundMets.SelectBestMethod(inputArgsTypes);
                        if (selectedMet == null)
                        {
                            //if not found
                            //TODO: review here , throw exception?                     
                            args.SetResultUndefined(); //?
                            return;
                        }

                        selectedMet.InvokeMethod(args);
                    });
                }
            }
        }
        protected override JsTypeDefinition OnBuildRequest(Type t)
        {
            JsTypeDefinition typedefinition = new JsTypeDefinition(t.Name);
            //-------
            //only instance /public method /prop***  
            Dictionary<string, JsMethodGroup> methodGroups = new Dictionary<string, JsMethodGroup>();
            foreach (MethodInfo met in GetMehodIter(t, BindingFlags.Instance | BindingFlags.Public))
            {
                var jsMethodDef = new JsMethodDefinition(met.Name, met);
                JsMethodGroup existingGroup;
                if (!methodGroups.TryGetValue(met.Name, out existingGroup))
                {
                    //create new one
                    existingGroup = new JsMethodGroup(met.Name, jsMethodDef);
                    methodGroups.Add(met.Name, existingGroup);
                }
                else
                {
                    existingGroup.AddMoreMember(jsMethodDef);
                }
            }
            //-----------------
            foreach (JsMethodGroup metGroup in methodGroups.Values)
            {
                JsMethodDefinition selectedMethod = metGroup.GetJsMethod();
                if (selectedMethod != null)
                {

                    typedefinition.AddMember(metGroup.GetJsMethod());
                }
            }
            //-----------------
            foreach (var property in GetPropertyIter(t, System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public))
            {
                typedefinition.AddMember(new JsPropertyDefinition(property.Name, property));
            }

            return typedefinition;
        }

        static IEnumerable<MethodInfo> GetMehodIter(Type t, BindingFlags flags)
        {
#if NET20
            return t.GetMethods(flags);
#else
            return t.GetTypeInfo().GetMethods(flags);
#endif
        }
        static IEnumerable<PropertyInfo> GetPropertyIter(Type t, BindingFlags flags)
        {
#if NET20
            return t.GetProperties(flags);
#else
            return t.GetTypeInfo().GetProperties(flags);
#endif
        }
        static Type GetBaseType(Type t)
        {
#if NET20
            return t.BaseType; 
#else
            return t.GetTypeInfo().BaseType;
#endif
        }
        static bool IsValueType(Type t)
        {
#if NET20
                return t.IsValueType; 
#else
            return t.GetTypeInfo().IsValueType;
#endif
        }
        static bool IsAssignable(Type dest, Type src)
        {
#if NET20
                return dest.IsAssignableFrom(src);
#else
            return dest.GetTypeInfo().IsAssignableFrom(src);
#endif
        }
    }
}