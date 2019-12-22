//MIT, 2015-2017, WinterDev, EngineKit, brezza92
#include <string>

#include <v8.h>
#include "libplatform/libplatform.h"
#include <cstring>

////////////////////////////////////////////////////////////////////////////////////////////////////
using namespace std;
using namespace v8;

#include "espresso.h"
#include "bridge2.h"

del02 managedListner; //for debug 


void RegisterManagedCallback(void* funcPtr, int callbackKind)
{
	switch (callbackKind)
	{
	case 0:
	{
		managedListner = (del02)funcPtr;
	}break;
	}
}

int TestCallBack()
{
	MetCallingArgs a;
	memset(&a, 0, sizeof(MetCallingArgs));


	std::u16string u16str = u"OKOK001";
	managedListner(0, u16str.c_str(), &a);
	return 1;
}

void ResultSetBool(MetCallingArgs* callingArgs, bool value)
{
	callingArgs->result.type = JSVALUE_TYPE_BOOLEAN;
	callingArgs->result.i32 = value ? 1 : 0;
}
void ResultSetInt32(MetCallingArgs* callingArgs, int value)
{
	//TODO: JS_VALUE

	callingArgs->result.type = JSVALUE_TYPE_INTEGER;
	callingArgs->result.i32 = value;
}
void ResultSetFloat(MetCallingArgs* callingArgs, float value)
{
	callingArgs->result.type = JSVALUE_TYPE_NUMBER;
	callingArgs->result.num = value;
}
void ResultSetDouble(MetCallingArgs* callingArgs, double value)
{
	callingArgs->result.type = JSVALUE_TYPE_NUMBER;
	callingArgs->result.num = value;
}
void ResultSetString(MetCallingArgs* callingArgs, uint16_t* value)
{
	//TODO: review here
	callingArgs->result.type = JSVALUE_TYPE_STRING;
	callingArgs->result.ptr = value;
}
void ResultSetJsNull(MetCallingArgs* callingArgs)
{
	callingArgs->result.type = JSVALUE_TYPE_NULL;
}
void ResultSetJsVoid(MetCallingArgs* callingArgs)
{
	callingArgs->result.type = JSVALUE_TYPE_EMPTY;
}
void ResultSetManagedObjectIndex(MetCallingArgs* callingArgs, int32_t managedObjectIndex)
{
	//TODO: review here
	callingArgs->result.type = JSVALUE_TYPE_MANAGED;
	callingArgs->result.i32 = managedObjectIndex;
}
void ResultSetValue(MetCallingArgs* callingArgs, jsvalue* value)
{
	//copy all value to
	callingArgs->result = *value;
}

ManagedRef* JsContext::CreateWrapperForManagedObject(int mIndex, ExternalTypeDefinition* externalTypeDef)
{

	Locker locker(isolate_);
	Isolate::Scope isolate_scope(isolate_);
	HandleScope scope(isolate_);
	Local<Context> ctx = Local<Context>::New(isolate_, *context_);
	ctx->Enter();

	HandleScope handleScope(isolate_);
	ManagedRef* handler = new ManagedRef(this->engine_, this->id_, mIndex, true);

	//create js from template
	if (externalTypeDef)
	{

		Local<ObjectTemplate> objTemplate = Local<ObjectTemplate>::New(isolate_, externalTypeDef->handlerToJsObjectTemplate);
		Local<Object> instance;
		objTemplate->NewInstance(ctx).ToLocal(&instance);
		handler->v8InstanceHandler.Reset(isolate_, instance);
		Local<Object> hd = Local<v8::Object>::New(isolate_, handler->v8InstanceHandler);
		hd->SetInternalField(0, External::New(isolate_, handler));
	}

	ctx->Exit();
	return handler;
}

ManagedRef* CreateWrapperForManagedObject(JsContext* engineContext, int mIndex, ExternalTypeDefinition* externalTypeDef)
{
	return engineContext->CreateWrapperForManagedObject(mIndex, externalTypeDef);
}

int GetManagedIndex(ManagedRef* externalManagedHandler)
{
	return  ((ManagedRef*)externalManagedHandler)->Id();
}
void ReleaseWrapper(ManagedRef* externalManagedHandler)
{
	delete externalManagedHandler;
}


void DoMethodCall(const FunctionCallbackInfo<Value>& args)
{
	//call to bridge with args  
	HandleScope h01(args.GetIsolate());

	MetCallingArgs callingArgs;
	memset(&callingArgs, 0, sizeof(MetCallingArgs));
	callingArgs.args = &args;
	callingArgs.methodCallKind = MET_;

	Local<v8::External> ext = Local<v8::External>::Cast(args.Data());
	CallingContext* cctx = (CallingContext*)ext->Value();

	int m_index = cctx->mIndex;

	cctx->ctx->myMangedCallBack(m_index,//method index
		MET_, //method kind
		&callingArgs);

	args.GetReturnValue().Set(cctx->ctx->AnyToV8(&callingArgs.result));

}
////////////////////////////////////////////////////////////////////////////////////////////////////
void DoGetterProperty(Local<String> propertyName, const PropertyCallbackInfo<Value>& info)
{

	HandleScope h01(info.GetIsolate());


	Local<v8::External> ext = Local<v8::External>::Cast(info.Data());
	CallingContext* cctx = (CallingContext*)ext->Value();

	int m_index = cctx->mIndex;
	Handle<External> external = Handle<External>::Cast(info.Holder()->GetInternalField(0));
	ManagedRef* extHandler = (ManagedRef*)external->Value();;

	MetCallingArgs callingArgs;
	memset(&callingArgs, 0, sizeof(MetCallingArgs));
	callingArgs.accessorInfo = &info;
	callingArgs.methodCallKind = MET_GETTER;

	cctx->ctx->myMangedCallBack(m_index, MET_GETTER, &callingArgs);

	//close and return value
	info.GetReturnValue().Set(cctx->ctx->AnyToV8(&callingArgs.result));
}

void DoSetterProperty(Local<String> propertyName,
	Local<Value> value,
	const PropertyCallbackInfo<Value>& info)
{

	HandleScope h01(info.GetIsolate());

	Local<v8::External> ext = Local<v8::External>::Cast(info.Data());
	CallingContext* cctx = (CallingContext*)ext->Value();


	int m_index = cctx->mIndex;
	Handle<External> external = Handle<External>::Cast(info.Holder()->GetInternalField(0));
	ManagedRef* extHandler = (ManagedRef*)external->Value();

	Handle<Object> obj = Handle<Object>::Cast<Value>(info.Holder()->GetInternalField(0));
	MetCallingArgs callingArgs;
	memset(&callingArgs, 0, sizeof(MetCallingArgs));
	callingArgs.accessorInfo = &info;
	callingArgs.methodCallKind = MET_SETTER;
	callingArgs.setterValue = value;
	cctx->ctx->myMangedCallBack(m_index, MET_SETTER, &callingArgs);
}

void JsContext::RegisterManagedCallback(void* callback, int callBackKind)
{
	this->myMangedCallBack = (del_JsBridge)callback;
}

ExternalTypeDefinition* JsContext::RegisterTypeDefinition(int mIndex, const char* stream, int streamLength)
{

	Locker locker(isolate_);
	Isolate::Scope isolate_scope(isolate_);
	HandleScope scope(isolate_);
	Local<Context> ctx = Local<Context>::New(isolate_, *context_);
	ctx->Enter();
	//use 2 handle scopes ***, otherwise this will error	  

	EscapableHandleScope handleScope(isolate_);
	//create new object template
	Handle<ObjectTemplate> objTemplate = ObjectTemplate::New(isolate_);
	objTemplate->SetInternalFieldCount(1);//store native instance 
	//--------------------------------------------------------------

	//read with stream
	BinaryStreamReader binReader(stream, streamLength);
	//--------------------------------------------------------------
	//marker (2 bytes)
	int marker_kind = binReader.ReadInt16();
	//--------------------------------------------------------------
	//send type definition handler back to managed side 
	ExternalTypeDefinition* externalTypeDef = new ExternalTypeDefinition(mIndex);
	//1.  typekind( 2 bytes)
	int type_kind = binReader.ReadInt16();
	//2. typeid
	int type_id = binReader.ReadInt16();
	//--------------------------------------------------------------
	//3. typename
	//3. typedefinition name(length-prefix unicode)
	std::u16string typeDefName = binReader.ReadUtf16String();

	//4. num of fields 
	int nfields = binReader.ReadInt16();

	for (int i = 0; i < nfields; ++i)
	{
		//we not support in this version ?
		int flags = binReader.ReadInt16();
		int fieldId = binReader.ReadInt16();
		std::u16string fieldname = binReader.ReadUtf16String();
		//accessor ?

	}
	//6. num of methods
	int nMethods = binReader.ReadInt16();
	for (int i = 0; i < nMethods; ++i)
	{
		int flags = binReader.ReadInt16();
		int methodId = binReader.ReadInt16();
		std::u16string metName = binReader.ReadUtf16String();


		CallingContext* callingContext = new CallingContext();
		callingContext->ctx = this;
		callingContext->mIndex = methodId;
		auto wrap = v8::External::New(isolate_, callingContext);

		Handle<FunctionTemplate> funcTemplate = FunctionTemplate::New(isolate_, DoMethodCall, wrap);
		objTemplate->Set(String::NewFromTwoByte(isolate_, (uint16_t*)(metName.c_str())), funcTemplate);

	}

	//7. properties and indexer 
	int nProperties = binReader.ReadInt16();
	for (int i = 0; i < nProperties; ++i)
	{
		//read pair getter/setter
		int flags_getter = binReader.ReadInt16();
		int property_id = binReader.ReadInt16();
		//name
		std::u16string propName = binReader.ReadUtf16String();

		CallingContext* callingContext = new CallingContext();
		callingContext->ctx = this;
		callingContext->mIndex = property_id;
		auto wrap = v8::External::New(isolate_, callingContext);

		objTemplate->SetAccessor(String::NewFromTwoByte(isolate_, (uint16_t*)(propName.c_str())),
			DoGetterProperty,
			(AccessorSetterCallback)DoSetterProperty,
			wrap);
	}
	externalTypeDef->handlerToJsObjectTemplate.Reset(isolate_, objTemplate);
	ctx->Exit();
	return externalTypeDef;
}

ExternalTypeDefinition* ContextRegisterTypeDefinition(
	JsContext* jsContext,
	int mIndex,  //managed index of type
	const char* stream,
	int streamLength)

{
	return jsContext->RegisterTypeDefinition(mIndex, stream, streamLength);

}
void ContextRegisterManagedCallback(JsContext* jsContext, void* callback, int callBackKind)
{
	jsContext->RegisterManagedCallback(callback, callBackKind);
}


void ArgGetObject(MetCallingArgs* args, int index, jsvalue* output)
{
	switch (args->methodCallKind)
	{
	case MET_SETTER:
	{
		//1 arg
		Local<v8::External> ext = Local<v8::External>::Cast(args->accessorInfo->Data());
		Handle<Object> obj = Handle<Object>::Cast(args->accessorInfo->This());
		CallingContext* cctx = (CallingContext*)ext->Value();
		cctx->ctx->ConvAnyFromV8(args->setterValue, obj, output);
		return;
	}
	case MET_:
	{
		Local<v8::External> ext = Local<v8::External>::Cast(args->args->Data());
		CallingContext* cctx = (CallingContext*)ext->Value();

		Local<v8::Value> arg = (Local<v8::Value>)(*(args->args))[index];
		Handle<Object> obj = Handle<Object>::Cast(args->args->This());
		cctx->ctx->ConvAnyFromV8(arg, obj, output);
		return;
	}
	default:
		output->type = JSVALUE_TYPE_NULL;//null?, review again
		break;
	}

}
void ArgGetThis(MetCallingArgs* args, jsvalue* output)
{
	if (args->accessorInfo == NULL)
	{
		Local<v8::External> ext = Local<v8::External>::Cast(args->args->Data());
		CallingContext* cctx = (CallingContext*)ext->Value();

		Handle<Object> obj = Handle<Object>::Cast(args->args->This());
		cctx->ctx->ConvAnyFromV8(obj, Handle<Object>(), output);
		return;
	}
	else
	{
		//use accessor
		//for getter/setter
		Local<v8::External> ext = Local<v8::External>::Cast(args->accessorInfo->Data());
		CallingContext* cctx = (CallingContext*)ext->Value();

		Handle<Object> obj = Handle<Object>::Cast(args->accessorInfo->This());
		return cctx->ctx->ConvAnyFromV8(obj, obj, output);
	}
}
//====================================================== 
ExternalTypeDefinition::ExternalTypeDefinition(int mIndex)
{
	this->managedIndex = mIndex;
}
void ExternalTypeDefinition::ReadTypeDefinitionFromStream(BinaryStreamReader* reader)
{
}
int ArgCount(MetCallingArgs* args)
{
	switch (args->methodCallKind)
	{
	case MET_SETTER:
	{
		//1 arg
		return 1;
	}break;
	case MET_:
	{
		return args->args->Length();
	}
	}
	return 0;
}
//======================================================  

static v8::Platform* default_platform;
static bool Initialized = false;

void V8Init(char* location)
{
	if (Initialized) return;
	const int thread_pool_size = 4;
	/*auto p = V8::*/
	v8::V8::InitializeICUDefaultLocation(location);
	v8::V8::InitializeExternalStartupData(location);
	default_platform = v8::platform::NewDefaultPlatform(thread_pool_size).release();
	V8::InitializePlatform(default_platform);
	V8::Initialize();
	Initialized = true;
}
