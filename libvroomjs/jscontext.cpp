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

#include <vector>
#include <iostream>
#include "vroomjs.h"

using namespace v8;

long js_mem_debug_context_count;

JsContext* JsContext::New(int id, JsEngine *engine)
{
    JsContext* context = new JsContext();
    if (context != NULL) {
		context->id_ = id;
		context->engine_ = engine;
		context->isolate_ = engine->GetIsolate();
        
		Locker locker(context->isolate_);
        Isolate::Scope isolate_scope(context->isolate_);

		context->context_ = new Persistent<Context>(Context::New());
	}
    return context;
}

void JsContext::Dispose()
{
	if(engine_->GetIsolate() != NULL) {
		Locker locker(isolate_);
   	 	Isolate::Scope isolate_scope(isolate_);
		context_->Dispose();            
    	delete context_;
	}
}

jsvalue JsContext::Execute(const uint16_t* str, const uint16_t *resourceName = NULL)
{
    jsvalue v;

    Locker locker(isolate_);
    Isolate::Scope isolate_scope(isolate_);
    (*context_)->Enter();
        
    HandleScope scope;
    TryCatch trycatch;
    
    Handle<String> source = String::New(str);   
	Handle<Script> script;

	if (resourceName != NULL) {
		Handle<String> name = String::New(resourceName);
		script = Script::Compile(source, name);  
	} else {
		script = Script::Compile(source);  
	}

	if (!script.IsEmpty()) {
		Local<Value> result = script->Run();
		
		if (result.IsEmpty())
            v = engine_->ErrorFromV8(trycatch);
        else
            v = engine_->AnyFromV8(result);        
    }
    else {
        v = engine_->ErrorFromV8(trycatch);
    }
            
    (*context_)->Exit();
	return v;     
}

jsvalue JsContext::Execute(JsScript *jsscript)
{
    jsvalue v;

    Locker locker(isolate_);
    Isolate::Scope isolate_scope(isolate_);
    (*context_)->Enter();
        
    HandleScope scope;
    TryCatch trycatch;
   
	Handle<Script> script = (*jsscript->GetScript());

	if (!script.IsEmpty()) {
		Local<Value> result = script->Run();
	
		if (result.IsEmpty())
			v = engine_->ErrorFromV8(trycatch);
		else
			v = engine_->AnyFromV8(result);        
	}

    (*context_)->Exit();
	return v;     
}

jsvalue JsContext::SetVariable(const uint16_t* name, jsvalue value)
{
    Locker locker(isolate_);
    Isolate::Scope isolate_scope(isolate_);
    (*context_)->Enter();
        
    HandleScope scope;
        
    Handle<Value> v = engine_->AnyToV8(value, id_);

    if ((*context_)->Global()->Set(String::New(name), v) == false) {
        // TODO: Return an error if set failed.
    }        

    (*context_)->Exit();
    
    return engine_->AnyFromV8(Null());
}

jsvalue JsContext::GetGlobal() {
	jsvalue v;
    
    Locker locker(isolate_);
    Isolate::Scope isolate_scope(isolate_);
    (*context_)->Enter();
        
    HandleScope scope;
    TryCatch trycatch;
                
    Local<Value> value = (*context_)->Global();
    if (!value.IsEmpty()) {
        v = engine_->AnyFromV8(value);        
    }
    else {
        v = engine_->ErrorFromV8(trycatch);
    }
    
    (*context_)->Exit();
    
    return v;
}

jsvalue JsContext::GetVariable(const uint16_t* name)
{
    jsvalue v;
    
    Locker locker(isolate_);
    Isolate::Scope isolate_scope(isolate_);
    (*context_)->Enter();
        
    HandleScope scope;
    TryCatch trycatch;
                
    Local<Value> value = (*context_)->Global()->Get(String::New(name));
    if (!value.IsEmpty()) {
        v = engine_->AnyFromV8(value);        
    }
    else {
        v = engine_->ErrorFromV8(trycatch);
    }
    
    (*context_)->Exit();
    
    return v;
}

jsvalue JsContext::GetPropertyNames(Persistent<Object>* obj) {
	 jsvalue v;
    
    Locker locker(isolate_);
    Isolate::Scope isolate_scope(isolate_);
    (*context_)->Enter();
        
    HandleScope scope;
    TryCatch trycatch;
                
    Local<Value> value = (*obj)->GetPropertyNames();
    if (!value.IsEmpty()) {
        v = engine_->AnyFromV8(value);        
    }
    else {
        v = engine_->ErrorFromV8(trycatch);
    }
    
    (*context_)->Exit();
    
    return v;
}

jsvalue JsContext::GetPropertyValue(Persistent<Object>* obj, const uint16_t* name)
{
    jsvalue v;
    
    Locker locker(isolate_);
    Isolate::Scope isolate_scope(isolate_);
    (*context_)->Enter();
        
    HandleScope scope;
    TryCatch trycatch;
                
    Local<Value> value = (*obj)->Get(String::New(name));
    if (!value.IsEmpty()) {
        v = engine_->AnyFromV8(value);        
    }
    else {
        v = engine_->ErrorFromV8(trycatch);
    }
    
    (*context_)->Exit();
    
    return v;
}


jsvalue JsContext::SetPropertyValue(Persistent<Object>* obj, const uint16_t* name, jsvalue value)
{
    Locker locker(isolate_);
    Isolate::Scope isolate_scope(isolate_);
    (*context_)->Enter();
        
    HandleScope scope;
        
    Handle<Value> v = engine_->AnyToV8(value, id_);

    if ((*obj)->Set(String::New(name), v) == false) {
        // TODO: Return an error if set failed.
    }          
    	
    (*context_)->Exit();
    
    return engine_->AnyFromV8(Null());
}

jsvalue JsContext::InvokeFunction(Persistent<Function>* func, Persistent<Object>* thisArg, jsvalue args) {
	jsvalue v;
	
    Locker locker(isolate_);
    Isolate::Scope isolate_scope(isolate_);
    (*context_)->Enter();
        
    HandleScope scope;    
    TryCatch trycatch;
  
	Local<Value> prop = *(*func);
    if (prop.IsEmpty() || !prop->IsFunction()) {
        v = engine_->StringFromV8(String::New("isn't a function"));
        v.type = JSVALUE_TYPE_STRING_ERROR;   
    }
	
	Local<Object> reciever = *(*thisArg);
	if (reciever.IsEmpty()) {
		reciever = (*context_)->Global();
	}

    else {
        std::vector<Local<Value> > argv(args.length);
        engine_->ArrayToV8Args(args, id_, &argv[0]);
        // TODO: Check ArrayToV8Args return value (but right now can't fail, right?)                   
        Local<Function> func = Local<Function>::Cast(prop);
		Local<Value> value = func->Call(reciever, args.length, &argv[0]);
        if (!value.IsEmpty()) {
            v = engine_->AnyFromV8(value);        
        }
        else {
            v = engine_->ErrorFromV8(trycatch);
        }         
    }
    
    (*context_)->Exit();
    
    return v;

}


jsvalue JsContext::InvokeProperty(Persistent<Object>* obj, const uint16_t* name, jsvalue args)
{
    jsvalue v;

    Locker locker(isolate_);
    Isolate::Scope isolate_scope(isolate_);
    (*context_)->Enter();
        
    HandleScope scope;    
    TryCatch trycatch;
        
    Local<Value> prop = (*obj)->Get(String::New(name));
    if (prop.IsEmpty() || !prop->IsFunction()) {
        v = engine_->StringFromV8(String::New("property not found or isn't a function"));
        v.type = JSVALUE_TYPE_STRING_ERROR;   
    }
    else {
        std::vector<Local<Value> > argv(args.length);
        engine_->ArrayToV8Args(args, id_, &argv[0]);
        // TODO: Check ArrayToV8Args return value (but right now can't fail, right?)                   
        Local<Function> func = Local<Function>::Cast(prop);
        Local<Value> value = func->Call(*obj, args.length, &argv[0]);
        if (!value.IsEmpty()) {
            v = engine_->AnyFromV8(value);        
        }
        else {
            v = engine_->ErrorFromV8(trycatch);
        }         
    }
    
    (*context_)->Exit();
    
    return v;
}
jsvalue JsContext::ConvAnyFromV8(Handle<Value> value, Handle<Object> thisArg)
{
  return this->engine_->AnyFromV8(value,thisArg);
}






