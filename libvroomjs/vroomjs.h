// This file is part of the VroomJs library.
//
// Author:
//     Federico Di Gregorio <fog@initd.org>
//
// Copyright (c) 2013 
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

#ifndef LIBVROOMJS_H
#define LIBVROOMJS_H 

#include <v8.h>
#include <stdlib.h>
#include <stdint.h>

using namespace v8;

#define JSOBJECT_MARSHAL_TYPE_DYNAMIC       1
#define JSOBJECT_MARSHAL_TYPE_DICTIONARY    2

// jsvalue (JsValue on the CLR side) is a struct that can be easily marshaled
// by simply blitting its value (being only 16 bytes should be quite fast too).

#define JSVALUE_TYPE_UNKNOWN_ERROR  -1
#define JSVALUE_TYPE_NULL            0
#define JSVALUE_TYPE_BOOLEAN         1
#define JSVALUE_TYPE_INTEGER         2
#define JSVALUE_TYPE_NUMBER          3
#define JSVALUE_TYPE_STRING          4
#define JSVALUE_TYPE_DATE            5
#define JSVALUE_TYPE_INDEX           6
#define JSVALUE_TYPE_ARRAY          10
#define JSVALUE_TYPE_ERROR          11
#define JSVALUE_TYPE_MANAGED        12
#define JSVALUE_TYPE_MANAGED_ERROR  13
#define JSVALUE_TYPE_WRAPPED        14
#define JSVALUE_TYPE_WRAPPED_ERROR  15

#ifdef _WIN32 
#define EXPORT __declspec(dllexport)
#else 
#define EXPORT
#endif

#ifdef _WIN32 
#define CALLINGCONVENTION __stdcall
#else 
#define CALLINGCONVENTION
#endif

extern int32_t js_object_marshal_type;

extern "C" 
{
    struct jsvalue
    {
        // 8 bytes is the maximum CLR alignment; by putting the union first and a
        // int64_t inside it we make (almost) sure the offset of 'type' will always
        // be 8 and the total size 16. We add a check to JsContext_new anyway.
        
        union 
        {
            int32_t     i32;
            int64_t     i64;
            double      num;
            void       *ptr;
            uint16_t   *str;
            jsvalue    *arr;
        } value;
        
        int32_t         type;
        int32_t         length; // Also used as slot index on the CLR side.
    };
    
   EXPORT void CALLINGCONVENTION jsvalue_dispose(jsvalue value);
}

class JsEngine;
class JsContext;

// The only way for the C++/V8 side to call into the CLR is to use the function
// pointers (CLR, delegates) defined below.

extern "C" 
{
    // We don't have a keepalive_add_f because that is managed on the managed side.
    // Its definition would be "int (*keepalive_add_f) (ManagedRef obj)".
    
    typedef void (CALLINGCONVENTION *keepalive_remove_f) (int context, int id);
    typedef jsvalue (CALLINGCONVENTION *keepalive_get_property_value_f) (int context, int id, uint16_t* name);
    typedef jsvalue (CALLINGCONVENTION *keepalive_set_property_value_f) (int context, int id, uint16_t* name, jsvalue value);
    typedef jsvalue (CALLINGCONVENTION *keepalive_invoke_f) (int context, int id, jsvalue args);
}


class JsContext {
 public:
    static JsContext* New(int32_t id, JsEngine *engine);
 
    
    // Called by bridge to execute JS from managed code.
    jsvalue Execute(const uint16_t* str);    
	jsvalue GetGlobal();
    jsvalue GetVariable(const uint16_t* name);
    jsvalue SetVariable(const uint16_t* name, jsvalue value);
	jsvalue GetPropertyNames(Persistent<Object>* obj);
    jsvalue GetPropertyValue(Persistent<Object>* obj, const uint16_t* name);
    jsvalue SetPropertyValue(Persistent<Object>* obj, const uint16_t* name, jsvalue value);
    jsvalue InvokeProperty(Persistent<Object>* obj, const uint16_t* name, jsvalue args);
    
  
    // Dispose a Persistent<Object> that was pinned on the CLR side by JsObject.
    void DisposeObject(Persistent<Object>* obj);
    
    void Dispose();
                
	int32_t GetId() { return id_; }

 private:             
    inline JsContext() {}
	int32_t id_;
    Isolate *isolate_;
	JsEngine *engine_;
	Persistent<Context> *context_;
};

// JsEngine is a single isolated v8 interpreter and is the referenced as an IntPtr
// by the JsEngine on the CLR side.
class JsEngine {
public:
	static JsEngine *New(int32_t max_young_space, int32_t max_old_space);

	inline void SetRemoveDelegate(keepalive_remove_f delegate) { keepalive_remove_ = delegate; }
    inline void SetGetPropertyValueDelegate(keepalive_get_property_value_f delegate) { keepalive_get_property_value_ = delegate; }
    inline void SetSetPropertyValueDelegate(keepalive_set_property_value_f delegate) { keepalive_set_property_value_ = delegate; }
    inline void SetInvokeDelegate(keepalive_invoke_f delegate) { keepalive_invoke_ = delegate; }
    
	void TerminateExecution();

    // Call delegates into managed code.
    inline void CallRemove(int32_t context, int id) { 
		keepalive_remove_(context, id); 
	}
    inline jsvalue CallGetPropertyValue(int32_t context, int32_t id, uint16_t* name) {
		return keepalive_get_property_value_(context, id, name);
	}
    inline jsvalue CallSetPropertyValue(int32_t context, int32_t id, uint16_t* name, jsvalue value) { 
		return keepalive_set_property_value_(context, id, name, value); 
	}
    inline jsvalue CallInvoke(int32_t context, int32_t id, jsvalue args) { 
		return keepalive_invoke_(context, id, args); 
	}
  

	// Conversions. Note that all the conversion functions should be called
    // with an HandleScope already on the stack or sill misarabily fail.
    Handle<Value> AnyToV8(int32_t contextId, jsvalue value); 
    jsvalue ErrorFromV8(TryCatch& trycatch);
    jsvalue StringFromV8(Handle<Value> value);
    jsvalue WrappedFromV8(Handle<Object> obj);
    jsvalue ManagedFromV8(Handle<Object> obj);
    jsvalue AnyFromV8(Handle<Value> value);
    
    // Needed to create an array of args on the stack for calling functions.
    int32_t ArrayToV8Args(int32_t contextId, jsvalue value, Handle<Value> preallocatedArgs[]);     
    
    // Converts JS function Arguments to an array of jsvalue to call managed code.
    jsvalue ArrayFromArguments(const Arguments& args);
    

	void Dispose();
	
	void DumpHeapStats();
	Isolate *GetIsolate() { return isolate_; }

	Persistent<ObjectTemplate> *managed_template_;

private:
	inline JsEngine() {}
	Isolate *isolate_;
    keepalive_remove_f keepalive_remove_;
    keepalive_get_property_value_f keepalive_get_property_value_;
    keepalive_set_property_value_f keepalive_set_property_value_;
    keepalive_invoke_f keepalive_invoke_;
};

class ManagedRef {
 public:
    inline explicit ManagedRef(JsEngine *engine, int contextId, int id) : engine_(engine), id_(id) {
		contextId_ = contextId;
	}
    
    inline int32_t Id() { return id_; }
    
    Handle<Value> GetPropertyValue(Local<String> name);
    Handle<Value> SetPropertyValue(Local<String> name, Local<Value> value);
    Handle<Value> Invoke(const Arguments& args);
    
    ~ManagedRef() { engine_->CallRemove(contextId_, id_); }
    
 private:
    ManagedRef() {}
	JsEngine* engine_;
    int32_t contextId_;
	int32_t id_;
};

#endif
