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

// MIT, 2015-2019, EngineKit, brezza92

#include <js_native_api.h>
#include <iostream>
#include "espresso.h"

using namespace v8;

extern "C" {
EXPORT int getVersion() {
  return 160800;
}

EXPORT JsEngine* CALLCONV
jsengine_new(keepalive_remove_f keepalive_remove,
             keepalive_get_property_value_f keepalive_get_property_value,
             keepalive_set_property_value_f keepalive_set_property_value,
             keepalive_valueof_f keepalive_valueof,
             keepalive_invoke_f keepalive_invoke,
             keepalive_delete_property_f keepalive_delete_property,
             keepalive_enumerate_properties_f keepalive_enumerate_properties,
             int32_t max_young_space,
             int32_t max_old_space) {
#ifdef DEBUG_TRACE_API
  std::wcout << "jsengine_new" << std::endl;
#endif
  JsEngine* engine = JsEngine::New(max_young_space, max_old_space);
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
EXPORT JsEngine* CALLCONV jsengine_registerManagedDels(
    JsEngine* engine,
    keepalive_remove_f keepalive_remove,
    keepalive_get_property_value_f keepalive_get_property_value,
    keepalive_set_property_value_f keepalive_set_property_value,
    keepalive_valueof_f keepalive_valueof,
    keepalive_invoke_f keepalive_invoke,
    keepalive_delete_property_f keepalive_delete_property,
    keepalive_enumerate_properties_f keepalive_enumerate_properties) {
#ifdef DEBUG_TRACE_API
  std::wcout << "jsengine_new" << std::endl;
#endif

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

EXPORT void CALLCONV jsengine_terminate_execution(JsEngine* engine) {
#ifdef DEBUG_TRACE_API
  std::wcout << "jsengine_terminate_execution" << std::endl;
#endif

  engine->TerminateExecution();
}

EXPORT void CALLCONV jsengine_dump_heap_stats(JsEngine* engine) {
#ifdef DEBUG_TRACE_API
  std::wcout << "jsengine_dump_heap_stats" << std::endl;
#endif
  engine->DumpHeapStats();
}

EXPORT void CALLCONV js_dump_allocated_items() {
#ifdef DEBUG_TRACE_API
  std::wcout << "js_dump_allocated_items" << std::endl;
#endif
  std::wcout << "Total allocated Js engines " << js_mem_debug_engine_count
             << std::endl;
  std::wcout << "Total allocated Js contexts " << js_mem_debug_context_count
             << std::endl;
  std::wcout << "Total allocated Js scripts " << js_mem_debug_script_count
             << std::endl;
  std::wcout << "Total allocated Managed Refs " << js_mem_debug_managedref_count
             << std::endl;
}

EXPORT void CALLCONV jsengine_dispose(JsEngine* engine) {
#ifdef DEBUG_TRACE_API
  std::wcout << "jsengine_dispose" << std::endl;
#endif
  engine->Dispose();
  delete engine;
}

EXPORT JsContext* CALLCONV jscontext_new(int32_t id, JsEngine* engine) {
#ifdef DEBUG_TRACE_API
  std::wcout << "jscontext_new" << std::endl;
#endif
  JsContext* context = JsContext::New(id, engine);
  return context;
}

EXPORT void CALLCONV jscontext_force_gc() {
#ifdef DEBUG_TRACE_API
  std::wcout << "jscontext_force_gc" << std::endl;
#endif
  // TODO: review here
  // TODO: 0.12.x not have IdleNotification() and not found represent method
  // while(!V8::IdleNotification()) {};
}

EXPORT void CALLCONV jscontext_dispose(JsContext* context) {
#ifdef DEBUG_TRACE_API
  std::wcout << "jscontext_dispose" << std::endl;
#endif
  context->Dispose();
  delete context;
}

EXPORT void CALLCONV jsengine_dispose_object(JsEngine* engine,
                                             Persistent<Object>* obj) {
#ifdef DEBUG_TRACE_API
  std::wcout << "jscontext_dispose_object" << std::endl;
#endif
  if (engine != NULL) {
    engine->DisposeObject(obj);
  }
  delete obj;
}

EXPORT void CALLCONV jscontext_execute(JsContext* context,
                                       const uint16_t* str,
                                       const uint16_t* resourceName,
                                       jsvalue* output) {
#ifdef DEBUG_TRACE_API
  std::wcout << "jscontext_execute" << std::endl;
#endif
  return context->Execute(str, resourceName, output);
}

EXPORT void CALLCONV jscontext_execute_script(JsContext* context,
                                              JsScript* script,
                                              jsvalue* output) {
#ifdef DEBUG_TRACE_API
  std::wcout << "jscontext_execute_script" << std::endl;
#endif
  return context->Execute(script, output);
}
EXPORT void CALLCONV jscontext_get_global(JsContext* context, jsvalue* output) {
#ifdef DEBUG_TRACE_API
  std::wcout << "jscontext_get_global" << std::endl;
#endif
  return context->GetGlobal(output);
}
EXPORT void CALLCONV jscontext_set_variable(JsContext* context,
                                            const uint16_t* name,
                                            jsvalue* value,
                                            jsvalue* output) {
#ifdef DEBUG_TRACE_API
  std::wcout << "jscontext_set_variable" << std::endl;
#endif
  return context->SetVariable(name, value, output);
}
EXPORT void CALLCONV jscontext_get_variable(JsContext* context,
                                            const uint16_t* name,
                                            jsvalue* output) {
#ifdef DEBUG_TRACE_API
  std::wcout << "jscontext_get_variable" << std::endl;
#endif
  return context->GetVariable(name, output);
}

EXPORT void CALLCONV jscontext_get_property_value(JsContext* context,
                                                  Persistent<Object>* obj,
                                                  const uint16_t* name,
                                                  jsvalue* output) {
#ifdef DEBUG_TRACE_API
  std::wcout << "jscontext_get_property_value" << std::endl;
#endif
  return context->GetPropertyValue(obj, name, output);
}
EXPORT void CALLCONV jscontext_set_property_value(JsContext* context,
                                                  Persistent<Object>* obj,
                                                  const uint16_t* name,
                                                  jsvalue* value,
                                                  jsvalue* output) {
#ifdef DEBUG_TRACE_API
  std::wcout << "jscontext_set_property_value" << std::endl;
#endif
  return context->SetPropertyValue(obj, name, value, output);
}

EXPORT void CALLCONV jscontext_get_property_names(JsContext* context,
                                                  Persistent<Object>* obj,
                                                  jsvalue* output) {
#ifdef DEBUG_TRACE_API
  std::wcout << "jscontext_get_property_names" << std::endl;
#endif
  return context->GetPropertyNames(obj, output);
}

EXPORT void CALLCONV jscontext_invoke_property(JsContext* context,
                                               Persistent<Object>* obj,
                                               const uint16_t* name,
                                               jsvalue* args,
                                               jsvalue* output) {
#ifdef DEBUG_TRACE_API
  std::wcout << "jscontext_invoke_property" << std::endl;
#endif
  return context->InvokeProperty(obj, name, args, output);
}
// TODO: JS_VALUE
EXPORT void CALLCONV jscontext_invoke(JsContext* context,
                                      Persistent<Function>* funcArg,
                                      Persistent<Object>* thisArg,
                                      jsvalue* args,
                                      jsvalue* output) {
#ifdef DEBUG_TRACE_API
  std::wcout << "jscontext_invoke" << std::endl;
#endif
  return context->InvokeFunction(funcArg, thisArg, args, output);
}

EXPORT JsScript* CALLCONV jsscript_new(JsEngine* engine) {
#ifdef DEBUG_TRACE_API
  std::wcout << "jsscript_new" << std::endl;
#endif
  // create on native heap
  JsScript* script = JsScript::New(engine);
  return script;
}

EXPORT void CALLCONV jsscript_dispose(JsScript* script) {
#ifdef DEBUG_TRACE_API
  std::wcout << "jsscript_dispose" << std::endl;
#endif
  script->Dispose();
  delete script;
}
EXPORT void CALLCONV jsscript_compile(JsScript* script,
                                      const uint16_t* str,
                                      const uint16_t* resourceName,
                                      jsvalue* output) {
#ifdef DEBUG_TRACE_API
  std::wcout << "jsscript_compile" << std::endl;
#endif
  return script->Compile(str, resourceName, output);
}

EXPORT void CALLCONV jsvalue_buffer_get_info(JsContext* contextPtr,
                                             Persistent<Object>* jsBuff,
                                             jsvalue* output_buffer_kind,
                                             jsvalue* output_buffer_len) {
  // from https://nodejs.org/dist/latest-v12.x/docs/api/buffer.html
  // Buffer instances are also Uint8Array instances.
  // However, there are subtle incompatibilities with TypedArray

  // https://stackoverflow.com/questions/51511307/nodejs-conversion-from-buffer-data-to-byte-array

  v8::Local<v8::Value> js_buffer = jsBuff->Get(contextPtr->isolate_);

  if (js_buffer->IsArrayBuffer()) {
    //
    Local<ArrayBuffer> arrBuff = Local<ArrayBuffer>::Cast(js_buffer);

    output_buffer_kind->type = JSVALUE_TYPE_INTEGER;
    output_buffer_kind->i32 = 1;

    output_buffer_len->type = JSVALUE_TYPE_INTEGER;
    output_buffer_len->i32 = arrBuff->ByteLength();

  } else if (js_buffer->IsUint8Array()) {
    // node'js js buffer
    Local<Uint8Array> arrBuff = Local<Uint8Array>::Cast(js_buffer);

    output_buffer_kind->type = JSVALUE_TYPE_INTEGER;
    output_buffer_kind->i32 = 0;

    output_buffer_len->type = JSVALUE_TYPE_INTEGER;
    output_buffer_len->i32 = arrBuff->ByteLength();
  } else {
    // TODO: review other buffer type

    // node'js js buffer
    Local<Uint8Array> arrBuff = Local<Uint8Array>::Cast(js_buffer);

    output_buffer_kind->type = JSVALUE_TYPE_INTEGER;
    output_buffer_kind->i32 = 0;

    output_buffer_len->type = JSVALUE_TYPE_INTEGER;
    output_buffer_len->i32 = arrBuff->ByteLength();
  }
}

EXPORT void CALLCONV jsvalue_buffer_get_len(JsContext* contextPtr,
                                            Persistent<Object>* jsBuff,
                                            int buffer_kind,
                                            jsvalue* output) {
  // from https://nodejs.org/dist/latest-v12.x/docs/api/buffer.html
  // Buffer instances are also Uint8Array instances.
  // However, there are subtle incompatibilities with TypedArray

  // https://stackoverflow.com/questions/51511307/nodejs-conversion-from-buffer-data-to-byte-array

  v8::Local<v8::Value> js_buffer = jsBuff->Get(contextPtr->isolate_);
  switch (buffer_kind) {
    default: {
      output->type = JSVALUE_TYPE_ERROR;
      output->i32 = 0;
    } break;
    case 0: {
      Local<Uint8Array> arrBuff = Local<Uint8Array>::Cast(js_buffer);
      output->type = JSVALUE_TYPE_INTEGER;
      output->i32 = arrBuff->ByteLength();
    } break;
    case 1: {
      Local<ArrayBuffer> arrBuff = Local<ArrayBuffer>::Cast(js_buffer);
      output->type = JSVALUE_TYPE_INTEGER;
      output->i32 = arrBuff->ByteLength();
    } break;
  }
}

EXPORT void CALLCONV jsvalue_buffer_copy_buffer_data(JsContext* contextPtr,
                                                     Persistent<Object>* jsBuff,
                                                     int buffer_kind,
                                                     void* dstMem,
                                                     int len,
                                                     jsvalue* output) {
  // copy data from js-side to dstMem (other native side)
  v8::Local<v8::Value> js_buffer = jsBuff->Get(contextPtr->isolate_);
  switch (buffer_kind) {
    default: {
      output->type = JSVALUE_TYPE_ERROR;
      output->i32 = 0;
    } break;
    case 0: {
      Local<Uint8Array> arrBuff = Local<Uint8Array>::Cast(js_buffer);
      arrBuff->CopyContents(dstMem, len);
      output->type = JSVALUE_TYPE_EMPTY;  // void
    } break;
    case 1: {
      Local<ArrayBuffer> arrBuff = Local<ArrayBuffer>::Cast(js_buffer);
      // copy content
      void* rawBufferData = arrBuff->GetBackingStore()->Data();
      //memcpy_s(dstMem, len, rawBufferData, arrBuff->ByteLength());
      memcpy(dstMem, rawBufferData, arrBuff->ByteLength());
    } break;
  }
}

EXPORT void CALLCONV
jsvalue_buffer_write_buffer_data(JsContext* contextPtr,
                                 Persistent<Object>* jsBuff,
                                 int buffer_kind,
                                 int dstIndex,
                                 void* src,
                                 int copyLen,
                                 jsvalue* output) {
  // copy data from other native side and write to js buffer start specific
  // index

  v8::Local<v8::Value> js_buffer = jsBuff->Get(contextPtr->isolate_);
  switch (buffer_kind) {
    default: {
      output->type = JSVALUE_TYPE_ERROR;
      output->i32 = 0;
    } break;
    case 0: {
      Local<Uint8Array> arrBuff = Local<Uint8Array>::Cast(js_buffer);
      /*v8::Local<v8::ArrayBuffer> underlyingBuff = arrBuff->Buffer();
      void* underlying_data1 = underlyingBuff->GetBackingStore()->Data();*/
       
      void* underlying_data1 = arrBuff->Buffer()->GetBackingStore()->Data();

      int buff_len = arrBuff->ByteLength();
      if (dstIndex > -1) {
        if (dstIndex + copyLen <= buff_len) {
          // check in range
          char* dst1 = ((char*)underlying_data1) + dstIndex;
          //memcpy_s((void*)dst1, buff_len, src, copyLen);
          memcpy((void*)dst1, src, arrBuff->ByteLength());
          output->type = JSVALUE_TYPE_EMPTY;  // void
          output->i32 = 0;
          return;
        }
      }
      output->type = JSVALUE_TYPE_MEM_ERROR;
      output->i32 = 0;
    } break;
    case 1: {
      Local<ArrayBuffer> arrBuff = Local<ArrayBuffer>::Cast(js_buffer);
      // copy content
      void* rawBufferData = arrBuff->GetBackingStore()->Data();
      int buff_len = arrBuff->ByteLength();
      if (dstIndex > -1) {
        if (dstIndex + copyLen <= buff_len) {
          // check in range
          char* dst1 = ((char*)rawBufferData) + dstIndex;
          //memcpy_s((void*)dst1, buff_len, src, copyLen);
          memcpy((void*)dst1, src, copyLen);
          output->type = JSVALUE_TYPE_EMPTY;  // void
          output->i32 = 0;
          return;
        }
      }
      output->type = JSVALUE_TYPE_MEM_ERROR;
      output->i32 = 0; 

    } break;
  }
   

 
  // output->type = JSVALUE_TYPE_EMPTY;  // void
}

EXPORT void CALLCONV jsvalue_alloc_string(const uint16_t* str,
                                          int len,
                                          jsvalue* output) {
#ifdef DEBUG_TRACE_API
  std::wcout << "jsvalue_alloc_string" << std::endl;
#endif

  // create on native heap
  uint16_t* newstr = new uint16_t[len + 1];  //+1 for null-terminated string
  if (newstr != NULL) {
    // alloc succeed
    //memcpy_s(newstr, (len + 1) * 2, str, len * 2);
    memcpy(newstr, str, len * 2);
    /*for (int i = length - 1; i >= 0; --i)
    {
            newstr[i] = newstr[i];
    }*/

    // last one, close with null character
    // newstr[length] = '\0'; //null-terminated string
    newstr[len] = 0;
    //----------------------------------
    output->type = JSVALUE_TYPE_STRING;
    output->ptr = newstr;  // assign
    output->i32 = len;
  } else {
    // alloc error
    output->type = JSVALUE_TYPE_MEM_ERROR;
    output->i32 = 0;
  }
}
EXPORT void CALLCONV jsvalue_alloc_array(const int32_t length,
                                         jsvalue* output) {
#ifdef DEBUG_TRACE_API
  std::wcout << "jsvalue_alloc_array" << std::endl;
#endif
  jsvalue* newarr = (jsvalue*)calloc(length, sizeof(jsvalue));
  if (newarr != NULL) {
    // alloc succeed
    output->ptr = newarr;
    output->type = JSVALUE_TYPE_ARRAY;
    output->i32 = length;
  } else {
    // alloc error
    output->type = JSVALUE_TYPE_MEM_ERROR;
    output->i32 = 0;
  }
}
EXPORT void CALLCONV jsvalue_dispose(jsvalue* value) {
#ifdef DEBUG_TRACE_API
  std::wcout << "jsvalue_dispose" << std::endl;
#endif

  switch (value->type) {
    case JSVALUE_TYPE_STRING:
    case JSVALUE_TYPE_STRING_ERROR: {
      if (value->ptr != NULL) {
        delete[] value->ptr;
      }
    } break;
    case JSVALUE_TYPE_ARRAY:
    case JSVALUE_TYPE_FUNCTION: {
      jsvalue* arr = (jsvalue*)value->ptr;
      for (int i = value->i32 - 1; i >= 0; --i) {
        jsvalue_dispose((arr + i));
      }
      if (arr != NULL) {
        delete value->ptr;
      }
    } break;
    case JSVALUE_TYPE_DICT: {
      jsvalue* arr = (jsvalue*)value->ptr;
      for (int i = (value->i32 * 2) - 1; i >= 0; --i) {  // key-value
        jsvalue_dispose(arr + i);
      }
      if (arr != NULL) {
        delete[] value->ptr;
      }
    } break;
    case JSVALUE_TYPE_ERROR: {
      jserror* error = (jserror*)value->ptr;
      jsvalue_dispose(error->resource);
      jsvalue_dispose(error->message);
      jsvalue_dispose(error->exception);
      delete error;

    } break;
  }
}
}
