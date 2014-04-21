// This file is part of the VroomJs library.
//
// Author:
//     Federico Di Gregorio <fog@initd.org>
//
// Copyright © 2013 Federico Di Gregorio <fog@initd.org>
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

#include <iostream>
#include "vroomjs.h"

using namespace v8;

int32_t js_object_marshal_type;

extern "C" 
{
	EXPORT void CALLINGCONVENTION js_set_object_marshal_type(int32_t type)
    {
#ifdef DEBUG_TRACE_API
		std::wcout << "js_object_marshal_type " << type << std::endl;
#endif
	    js_object_marshal_type = type;
    }

	EXPORT JsEngine* CALLINGCONVENTION jsengine_new(keepalive_remove_f keepalive_remove, 
                           keepalive_get_property_value_f keepalive_get_property_value,
                           keepalive_set_property_value_f keepalive_set_property_value,
						   keepalive_valueof_f keepalive_valueof,
                           keepalive_invoke_f keepalive_invoke,
						   keepalive_delete_property_f keepalive_delete_property,
						   keepalive_enumerate_properties_f keepalive_enumerate_properties,
						   int32_t max_young_space, int32_t max_old_space) 
	{
#ifdef DEBUG_TRACE_API
		std::wcout << "jsengine_new" << std::endl;
#endif
		JsEngine *engine = JsEngine::New(max_young_space, max_old_space);
		if (engine != NULL) {
            engine->SetRemoveDelegate(keepalive_remove);
            engine->SetGetPropertyValueDelegate(keepalive_get_property_value);
            engine->SetSetPropertyValueDelegate(keepalive_set_property_value);
			engine->SetValueOfDelegate(keepalive_valueof);
            engine->SetInvokeDelegate(keepalive_invoke);
			engine->SetDeletePropertyDelegate(keepalive_delete_property);
			engine->SetEnumeratePropertiesDelegate(keepalive_enumerate_properties);
        }
		return engine;
	}

	EXPORT void CALLINGCONVENTION jsengine_terminate_execution(JsEngine* engine) {
#ifdef DEBUG_TRACE_API
                std::wcout << "jsengine_terminate_execution" << std::endl;
#endif

		engine->TerminateExecution();
	}

    EXPORT void CALLINGCONVENTION jsengine_dump_heap_stats(JsEngine* engine) {
#ifdef DEBUG_TRACE_API
                std::wcout << "jsengine_dump_heap_stats" << std::endl;
#endif
		engine->DumpHeapStats();
	}

	EXPORT void CALLINGCONVENTION js_dump_allocated_items() {
#ifdef DEBUG_TRACE_API
                std::wcout << "js_dump_allocated_items" << std::endl;
#endif
		std::wcout << "Total allocated Js engines " << js_mem_debug_engine_count << std::endl;
		std::wcout << "Total allocated Js contexts " << js_mem_debug_context_count << std::endl;
		std::wcout << "Total allocated Js scripts " << js_mem_debug_script_count << std::endl;
		std::wcout << "Total allocated Managed Refs " << js_mem_debug_managedref_count << std::endl;
	}

	EXPORT void CALLINGCONVENTION jsengine_dispose(JsEngine* engine)
    {
#ifdef DEBUG_TRACE_API
		std::wcout << "jsengine_dispose" << std::endl;
#endif
        engine->Dispose();        
        delete engine;
    }

    EXPORT JsContext* CALLINGCONVENTION jscontext_new(int32_t id, JsEngine *engine)
    {
#ifdef DEBUG_TRACE_API
		std::wcout << "jscontext_new" << std::endl;
#endif
        JsContext* context = JsContext::New(id, engine);
        return context;
    }

	EXPORT void CALLINGCONVENTION jscontext_force_gc()
    {
#ifdef DEBUG_TRACE_API
		std::wcout << "jscontext_force_gc" << std::endl;
#endif
        while(!V8::IdleNotification()) {};
    }

    EXPORT void jscontext_dispose(JsContext* context)
    {
#ifdef DEBUG_TRACE_API
		std::wcout << "jscontext_dispose" << std::endl;
#endif
        context->Dispose();        
        delete context;
    }
    
    EXPORT void CALLINGCONVENTION jsengine_dispose_object(JsEngine* engine, Persistent<Object>* obj)
    {
#ifdef DEBUG_TRACE_API
		std::wcout << "jscontext_dispose_object" << std::endl;
#endif
        if (engine != NULL) {
            engine->DisposeObject(obj);
		}
		delete obj;
    }     
    
    EXPORT jsvalue CALLINGCONVENTION jscontext_execute(JsContext* context, const uint16_t* str, const uint16_t *resourceName)
    {
#ifdef DEBUG_TRACE_API
		std::wcout << "jscontext_execute" << std::endl;
#endif
        return context->Execute(str, resourceName);
    }

	EXPORT jsvalue CALLINGCONVENTION jscontext_execute_script(JsContext* context, JsScript *script)
    {
#ifdef DEBUG_TRACE_API
		std::wcout << "jscontext_execute_script" << std::endl;
#endif
        return context->Execute(script);
    }

	EXPORT jsvalue CALLINGCONVENTION jscontext_get_global(JsContext* context)
    {
#ifdef DEBUG_TRACE_API
		std::wcout << "jscontext_get_global" << std::endl;
#endif
        return context->GetGlobal();
    }
	
    EXPORT jsvalue CALLINGCONVENTION jscontext_set_variable(JsContext* context, const uint16_t* name, jsvalue value)
    {
#ifdef DEBUG_TRACE_API
		std::wcout << "jscontext_set_variable" << std::endl;
#endif
        return context->SetVariable(name, value);
    }

    EXPORT jsvalue CALLINGCONVENTION jscontext_get_variable(JsContext* context, const uint16_t* name)
    {
#ifdef DEBUG_TRACE_API
		std::wcout << "jscontext_get_variable" << std::endl;
#endif
        return context->GetVariable(name);
    }

    EXPORT jsvalue CALLINGCONVENTION jscontext_get_property_value(JsContext* context, Persistent<Object>* obj, const uint16_t* name)
    {
#ifdef DEBUG_TRACE_API
		std::wcout << "jscontext_get_property_value" << std::endl;
#endif
        return context->GetPropertyValue(obj, name);
    }
    
    EXPORT jsvalue CALLINGCONVENTION jscontext_set_property_value(JsContext* context, Persistent<Object>* obj, const uint16_t* name, jsvalue value)
    {
#ifdef DEBUG_TRACE_API
		std::wcout << "jscontext_set_property_value" << std::endl;
#endif
        return context->SetPropertyValue(obj, name, value);
    }    

	EXPORT jsvalue CALLINGCONVENTION jscontext_get_property_names(JsContext* context, Persistent<Object>* obj)
    {
#ifdef DEBUG_TRACE_API
		std::wcout << "jscontext_get_property_names" << std::endl;
#endif
        return context->GetPropertyNames(obj);
    }    
	    
    EXPORT jsvalue CALLINGCONVENTION jscontext_invoke_property(JsContext* context, Persistent<Object>* obj, const uint16_t* name, jsvalue args)
    {
#ifdef DEBUG_TRACE_API
		std::wcout << "jscontext_invoke_property" << std::endl;
#endif
        return context->InvokeProperty(obj, name, args);
    }        

	  EXPORT jsvalue CALLINGCONVENTION jscontext_invoke(JsContext* context, Persistent<Function>* funcArg, Persistent<Object>* thisArg, jsvalue args)
    {
#ifdef DEBUG_TRACE_API
		std::wcout << "jscontext_invoke" << std::endl;
#endif
        return context->InvokeFunction(funcArg, thisArg, args);
    }        

	 EXPORT JsScript* CALLINGCONVENTION jsscript_new(JsEngine *engine)
    {
#ifdef DEBUG_TRACE_API
		std::wcout << "jsscript_new" << std::endl;
#endif
        JsScript* script = JsScript::New(engine);
        return script;
    }

	 EXPORT void CALLINGCONVENTION jsscript_dispose(JsScript *script)
    {
#ifdef DEBUG_TRACE_API
		std::wcout << "jsscript_dispose" << std::endl;
#endif
        script->Dispose();
		delete script;
    }

	  EXPORT jsvalue CALLINGCONVENTION jsscript_compile(JsScript* script, const uint16_t* str, const uint16_t *resourceName)
    {
#ifdef DEBUG_TRACE_API
		std::wcout << "jsscript_compile" << std::endl;
#endif
		return script->Compile(str, resourceName);
    }

    EXPORT jsvalue CALLINGCONVENTION jsvalue_alloc_string(const uint16_t* str)
    {
#ifdef DEBUG_TRACE_API
		std::wcout << "jsvalue_alloc_string" << std::endl;
#endif
        jsvalue v;
    
        int length = 0;
        while (str[length] != '\0')
            length++;
          
        v.length = length;
        v.value.str = new uint16_t[length+1];
        if (v.value.str != NULL) {
            for (int i=0 ; i < length ; i++)
                 v.value.str[i] = str[i];
            v.value.str[length] = '\0';
            v.type = JSVALUE_TYPE_STRING;
        }

        return v;
    }    
    
    EXPORT jsvalue CALLINGCONVENTION jsvalue_alloc_array(const int32_t length)
    {
#ifdef DEBUG_TRACE_API
		std::wcout << "jsvalue_alloc_array" << std::endl;
#endif
        jsvalue v;
          
        v.value.arr = new jsvalue[length];
        if (v.value.arr != NULL) {
            v.length = length;
            v.type = JSVALUE_TYPE_ARRAY;
        }

        return v;
    }        
                
    EXPORT void CALLINGCONVENTION jsvalue_dispose(jsvalue value)
    {
#ifdef DEBUG_TRACE_API
		std::wcout << "jsvalue_dispose" << std::endl;
#endif
        if (value.type == JSVALUE_TYPE_STRING || value.type == JSVALUE_TYPE_STRING_ERROR) {
            if (value.value.str != NULL) {
				delete[] value.value.str;
			}
        }
		else if (value.type == JSVALUE_TYPE_ARRAY || value.type == JSVALUE_TYPE_FUNCTION) {
		    for (int i=0 ; i < value.length ; i++) {
                jsvalue_dispose(value.value.arr[i]);
			}
            if (value.value.arr != NULL) {
                delete[] value.value.arr;
			}
        }
		else if (value.type == JSVALUE_TYPE_DICT) {
			for (int i=0 ; i < value.length * 2; i++) {
                jsvalue_dispose(value.value.arr[i]);
			}
            if (value.value.arr != NULL) {
                delete[] value.value.arr;
			}
		}
		else if (value.type == JSVALUE_TYPE_ERROR) {
			jserror *error = (jserror*)value.value.ptr;
			jsvalue_dispose(error->resource);
			jsvalue_dispose(error->message);
			jsvalue_dispose(error->exception);
			delete error;
		}
    }       
}
