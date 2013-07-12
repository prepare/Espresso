
#include <iostream>
#include "vroomjs.h"

extern "C" jsvalue CALLINGCONVENTION jsvalue_alloc_array(const int32_t length);

static void managed_destroy(Persistent<Value> object, void* parameter)
{
#ifdef DEBUG_TRACE_API
		std::cout << "managed_destroy" << std::endl;
#endif
    HandleScope scope;
    
    Persistent<Object> self = Persistent<Object>::Cast(object);
    Local<External> wrap = Local<External>::Cast(self->GetInternalField(0));
    delete (ManagedRef*)wrap->Value();
    object.Dispose();
}

static Handle<Value> managed_prop_get(Local<String> name, const AccessorInfo& info)
{
#ifdef DEBUG_TRACE_API
		std::cout << "managed_prop_get" << std::endl;
#endif
    HandleScope scope;
    
    Local<Object> self = info.Holder();
    Local<External> wrap = Local<External>::Cast(self->GetInternalField(0));
    ManagedRef* ref = (ManagedRef*)wrap->Value();
    return scope.Close(ref->GetPropertyValue(name));
}

static Handle<Value> managed_prop_set(Local<String> name, Local<Value> value, const AccessorInfo& info)
{
#ifdef DEBUG_TRACE_API
		std::cout << "managed_prop_set" << std::endl;
#endif
    HandleScope scope;
    
    Local<Object> self = info.Holder();
    Local<External> wrap = Local<External>::Cast(self->GetInternalField(0));
    ManagedRef* ref = (ManagedRef*)wrap->Value();
    return scope.Close(ref->SetPropertyValue(name, value));
}

static Handle<Value> managed_call(const Arguments& args)
{
#ifdef DEBUG_TRACE_API
		std::cout << "managed_call" << std::endl;
#endif
    HandleScope scope;
    
    Local<Object> self = args.Holder();
    Local<External> wrap = Local<External>::Cast(self->GetInternalField(0));
    ManagedRef* ref = (ManagedRef*)wrap->Value();
    return scope.Close(ref->Invoke(args));
}

static const int Mega = 1024 * 1024;

JsEngine* JsEngine::New(int32_t max_young_space = -1, int32_t max_old_space = -1)
{
	JsEngine* engine = new JsEngine();
    if (engine != NULL) 
	{            
		engine->isolate_ = Isolate::New();
                engine->isolate_->Enter();
		
		if (max_young_space > 0 && max_old_space > 0) {
			v8::ResourceConstraints constraints;
			constraints.set_max_young_space_size(max_young_space * Mega);
			constraints.set_max_old_space_size(max_old_space * Mega);
		
			v8::SetResourceConstraints(&constraints);
		}

		engine->isolate_->Exit();

            Locker locker(engine->isolate_);
	    Isolate::Scope isolate_scope(engine->isolate_);

		// Setup the template we'll use for all managed object references.
        HandleScope scope;            
        Handle<ObjectTemplate> o = ObjectTemplate::New();
        o->SetInternalFieldCount(1);
        o->SetNamedPropertyHandler(managed_prop_get, managed_prop_set);
        o->SetCallAsFunctionHandler(managed_call);
        Persistent<ObjectTemplate> p = Persistent<ObjectTemplate>::New(o);
        engine->managed_template_ = new Persistent<ObjectTemplate>(p);
	}
	return engine;
}

void JsEngine::TerminateExecution() 
{
	V8::TerminateExecution(isolate_);
}

void JsEngine::DumpHeapStats() 
{
	Locker locker(isolate_);
    	Isolate::Scope isolate_scope(isolate_);

	// gc first.
	while(!V8::IdleNotification()) {};
	
	HeapStatistics stats;
	isolate_->GetHeapStatistics(&stats);
	std::wcout << "Heap size limit " << (stats.heap_size_limit() / Mega) << std::endl;
	std::wcout << "Total heap size " << (stats.total_heap_size() / Mega) << std::endl;
	std::wcout << "Heap size executable " << (stats.total_heap_size_executable() / Mega) << std::endl;
	std::wcout << "Total physical size " << (stats.total_physical_size() / Mega) << std::endl;
	std::wcout << "Used heap size " << (stats.used_heap_size() / Mega) << std::endl;
}

void JsEngine::Dispose()
{
	if (isolate_ != NULL) {
		isolate_->Dispose();
		isolate_ = NULL;
	}
}

jsvalue JsEngine::ErrorFromV8(TryCatch& trycatch)
{
    jsvalue v;

    HandleScope scope;
    
    Local<Value> exception = trycatch.Exception();

    v.type = JSVALUE_TYPE_UNKNOWN_ERROR;        
    v.value.str = 0;
    v.length = 0;

	Local<String> errorString;
	Local<Message> message = trycatch.Message();
	if (!message.IsEmpty()) {
		errorString = message->Get();
	}

	// If this is a managed exception we need to place its ID inside the jsvalue
    // and set the type JSVALUE_TYPE_MANAGED_ERROR to make sure the CLR side will
    // throw on it. Else we just wrap and return the exception Object. Note that
    // this is far from perfect because we ignore both the Message object and the
    // stack stack trace. If the exception is not an object (but just a string,
    // for example) we convert it with toString() and return that as an Exception.
    // TODO: return a composite/special object with stack trace information.
    
    if (exception->IsObject()) {
        Local<Object> obj = Local<Object>::Cast(exception);
        if (obj->InternalFieldCount() == 1) {
			Local<External> wrap = Local<External>::Cast(obj->GetInternalField(0));
			ManagedRef* ref = (ManagedRef*)wrap->Value();
	        v.type = JSVALUE_TYPE_MANAGED_ERROR;
            v.length = ref->Id();
        } else if (!errorString.IsEmpty() && errorString->Length() > 0) {
			v = StringFromV8(errorString);
			v.type = JSVALUE_TYPE_ERROR;  
		} else {
            v = WrappedFromV8(obj);
            v.type = JSVALUE_TYPE_WRAPPED_ERROR;
	    }            
    }
    else if (!exception.IsEmpty()) {
        v = StringFromV8(exception);
        v.type = JSVALUE_TYPE_ERROR;   
    }
    
    return v;
}
    
jsvalue JsEngine::StringFromV8(Handle<Value> value)
{
    jsvalue v;
    
    Local<String> s = value->ToString();
    v.length = s->Length();
    v.value.str = new uint16_t[v.length+1];
    if (v.value.str != NULL) {
        s->Write(v.value.str);
        v.type = JSVALUE_TYPE_STRING;
    }

    return v;
}   

jsvalue JsEngine::WrappedFromV8(Handle<Object> obj)
{
    jsvalue v;
    
    v.type = JSVALUE_TYPE_WRAPPED;

	if (js_object_marshal_type == JSOBJECT_MARSHAL_TYPE_DYNAMIC) {
		 v.length = 0;
        // A Persistent<Object> is exactly the size of an IntPtr, right?
		// If not we're in deep deep trouble (on IA32 and AMD64 should be).
		// We should even cast it to void* because C++ doesn't allow to put
		// it in a union: going scary and scarier here.    
		v.value.ptr = new Persistent<Object>(Persistent<Object>::New(obj));
	} else {
		Local<Array> names = obj->GetOwnPropertyNames();
		v.length = names->Length();
		jsvalue* values = new jsvalue[v.length * 2];
		if (values != NULL) {
			for(int i = 0; i < v.length; i++) {
				int indx = (i * 2);
				Local<Value> key = names->Get(i);
				values[indx] = AnyFromV8(key);
				values[indx+1] = AnyFromV8(obj->Get(key));
			}
			v.value.arr = values;
		}
	}

	return v;
} 

jsvalue JsEngine::ManagedFromV8(Handle<Object> obj)
{
    jsvalue v;
    
	Local<External> wrap = Local<External>::Cast(obj->GetInternalField(0));
    ManagedRef* ref = (ManagedRef*)wrap->Value();
	v.type = JSVALUE_TYPE_MANAGED;
    v.length = ref->Id();
    v.value.str = 0;

    return v;
}
    
jsvalue JsEngine::AnyFromV8(Handle<Value> value)
{
    jsvalue v;
    
    // Initialize to a generic error.
    v.type = JSVALUE_TYPE_UNKNOWN_ERROR;
    v.length = 0;
    v.value.str = 0;
    
    if (value->IsNull() || value->IsUndefined()) {
        v.type = JSVALUE_TYPE_NULL;
    }                
    else if (value->IsBoolean()) {
        v.type = JSVALUE_TYPE_BOOLEAN;
        v.value.i32 = value->BooleanValue() ? 1 : 0;
    }
    else if (value->IsInt32()) {
        v.type = JSVALUE_TYPE_INTEGER;
        v.value.i32 = value->Int32Value();            
    }
    else if (value->IsUint32()) {
        v.type = JSVALUE_TYPE_INDEX;
        v.value.i64 = value->Uint32Value();            
    }
    else if (value->IsNumber()) {
        v.type = JSVALUE_TYPE_NUMBER;
        v.value.num = value->NumberValue();
    }
    else if (value->IsString()) {
        v = StringFromV8(value);
    }
    else if (value->IsDate()) {
        v.type = JSVALUE_TYPE_DATE;
        v.value.num = value->NumberValue();
    }
    else if (value->IsArray()) {
        Handle<Array> object = Handle<Array>::Cast(value->ToObject());
        v.length = object->Length();
        jsvalue* array = new jsvalue[v.length];
        if (array != NULL) {
            for(int i = 0; i < v.length; i++) {
                array[i] = AnyFromV8(object->Get(i));
            }
            v.type = JSVALUE_TYPE_ARRAY;
            v.value.arr = array;
        }
    }
    else if (value->IsFunction()) {
		v.type = JSVALUE_TYPE_NULL; // fix this we just ignore the value.
        // TODO: how do we represent this on the CLR side? Delegate?
    }
    else if (value->IsObject()) {
        Handle<Object> obj = Handle<Object>::Cast(value);
        if (obj->InternalFieldCount() == 1)
            v = ManagedFromV8(obj);
        else
            v = WrappedFromV8(obj);
    }

    return v;
}

Handle<Value> JsEngine::AnyToV8(int32_t contextId, jsvalue v)
{
    if (v.type == JSVALUE_TYPE_NULL) {
        return Null();
    }
    if (v.type == JSVALUE_TYPE_BOOLEAN) {
        return Boolean::New(v.value.i32);
    }
    if (v.type == JSVALUE_TYPE_INTEGER) {
        return Int32::New(v.value.i32);
    }
    if (v.type == JSVALUE_TYPE_NUMBER) {
        return Number::New(v.value.num);
    }
    if (v.type == JSVALUE_TYPE_STRING) {
        return String::New(v.value.str);
    }
    if (v.type == JSVALUE_TYPE_DATE) {
        return Date::New(v.value.num);
    }

    // Arrays are converted to JS native arrays.
    
    if (v.type == JSVALUE_TYPE_ARRAY) {
        Local<Array> a = Array::New(v.length);
        for(int i = 0; i < v.length; i++) {
            a->Set(i, AnyToV8(contextId, v.value.arr[i]));
        }
        return a;        
    }
        
    // This is an ID to a managed object that lives inside the JsContext keep-alive
    // cache. We just wrap it and the pointer to the engine inside an External. A
    // managed error is still a CLR object so it is wrapped exactly as a normal
    // managed object.
    
    if (v.type == JSVALUE_TYPE_MANAGED || v.type == JSVALUE_TYPE_MANAGED_ERROR) {
        ManagedRef* ref = new ManagedRef(this, contextId, v.length);
        Persistent<Object> obj = Persistent<Object>::New((*(managed_template_))->NewInstance());
		obj->SetInternalField(0, External::New(ref));
        obj.MakeWeak(NULL, managed_destroy);
        return obj;
    }

    return Null();
}

int32_t JsEngine::ArrayToV8Args(int32_t contextId, jsvalue value, Handle<Value> preallocatedArgs[])
{
    if (value.type != JSVALUE_TYPE_ARRAY)
        return -1;
        
    for (int i=0 ; i < value.length ; i++) {
        preallocatedArgs[i] = AnyToV8(contextId, value.value.arr[i]);
    }
    
    return value.length;
}

jsvalue JsEngine::ArrayFromArguments(const Arguments& args)
{
    jsvalue v = jsvalue_alloc_array(args.Length());
    
    for (int i=0 ; i < v.length ; i++) {
        v.value.arr[i] = AnyFromV8(args[i]);
    }
    
    return v;
}
