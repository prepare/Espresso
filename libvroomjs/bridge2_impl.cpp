//BSD 2015, WinterDev
#include <string>

#include <v8.h>
////////////////////////////////////////////////////////////////////////////////////////////////////
using namespace std;
using namespace v8; 
#include "bridge2.h"

del02 managedListner; //for debug 


void RegisterManagedCallback(void* funcPtr,int callbackKind)
{	
	switch(callbackKind)
	{
	case 0:
		{
			managedListner= (del02)funcPtr;
		}break;
	case 1:
		{

		}break;
	} 
} 

int TestCallBack()
{
	MetCallingArgs a;
	memset(&a,0,sizeof(MetCallingArgs));
	managedListner(0,L"OKOK001",&a);
	return 1;
} 

void ResultSetBool(MetCallingArgs* callingArgs,bool value)
{
	jsvalue result;
	result.type = JSVALUE_TYPE_BOOLEAN;
	result.value.i32 =  value ? 1: 0;
	callingArgs->result =  result;
} 
void ResultSetInt32(MetCallingArgs* callingArgs,int value)
{  
	jsvalue result;
	result.type = JSVALUE_TYPE_INTEGER;
	result.value.i32 =  value;
	callingArgs->result =  result; 
} 
void ResultSetFloat(MetCallingArgs* callingArgs,float value)
{		
	jsvalue result;
	result.type = JSVALUE_TYPE_NUMBER;
	result.value.num =  value;
	callingArgs->result =  result;  
}
void ResultSetDouble(MetCallingArgs* callingArgs,double value)
{ 
	jsvalue result;
	result.type = JSVALUE_TYPE_NUMBER;
	result.value.num =  value;
	callingArgs->result =  result; 
}
void ResultSetString(MetCallingArgs* callingArgs,wchar_t* value)
{	
	jsvalue result;
	result.type = JSVALUE_TYPE_STRING;
	result.value.str =(uint16_t*)value;
	callingArgs->result =  result;  
} 
 
void ResultSetJsValue(MetCallingArgs* callingArgs,jsvalue value)
{	
	callingArgs->result =  value;   
} 

ManagedRef* JsContext::CreateWrapperForManagedObject(int mIndex, ExternalTypeDefinition* externalTypeDef)
{ 

	Locker locker(isolate_);
	Isolate::Scope isolate_scope(isolate_);
	(*context_)->Enter();


	HandleScope handleScope;	 
	ManagedRef* handler= new ManagedRef(this->engine_,this->id_,mIndex,true);

	//create js from template
	if(externalTypeDef)
	{
		/*if(managedListner){
		managedListner(1,L"handle0",0);
		if((externalTypeDef->handlerToJsObjectTemplate).IsEmpty())
		{
		managedListner(1,L"handle1",0);
		}
		else
		{
		managedListner(1,L"handle2",0);
		}
		}*/
		//auto a1= externalTypeDef->handlerToJsObjectTemplate->NewInstance();
		handler->v8InstanceHandler=
			Persistent<v8::Object>::New(externalTypeDef->handlerToJsObjectTemplate->NewInstance());
		handler->v8InstanceHandler->SetInternalField(0,External::New(handler));
	}
	(*context_)->Exit();
	return handler;
}

ManagedRef* CreateWrapperForManagedObject(JsContext* engineContext,int mIndex, ExternalTypeDefinition* externalTypeDef)
{ 
	return engineContext->CreateWrapperForManagedObject(mIndex,externalTypeDef); 
} 

int GetManagedIndex(ManagedRef* externalManagedHandler)
{
	return  ((ManagedRef*)externalManagedHandler)->Id();
}
void ReleaseWrapper(ManagedRef* externalManagedHandler)
{	
	delete externalManagedHandler;
} 


Handle<Value>
	Getter(Local<String> iName, const AccessorInfo &iInfo)
{
	//name may be method or field 

	wstring name = (wchar_t*) *String::Value(iName);

	Handle<External> external = Handle<External>::Cast(iInfo.Holder()->GetInternalField(0));
	ManagedRef* extHandler=(ManagedRef*)external->Value();;

	//JavascriptExternal* wrapper = (JavascriptExternal*) external->Value();
	//Handle<Function> function;
	//Handle<Value> value;

	//// get method
	//function = wrapper->GetMethod(name);
	//if (!function.IsEmpty())
	//	return function;  // good value or exception

	//// As for GetMethod().
	//if (wrapper->GetProperty(name, value))
	//	return value;  // good value or exception

	//// map toString with ToString
	//if (wstring((wchar_t*) *String::Value(iName)) == L"toString")
	//{
	//	function = wrapper->GetMethod(L"ToString");
	//	if (!function.IsEmpty())
	//		return function;
	//}

	//// member not found
	//if ((wrapper->GetOptions() & SetParameterOptions::RejectUnknownProperties) == SetParameterOptions::RejectUnknownProperties)
	//	return v8::ThrowException(JavascriptInterop::ConvertToV8("Unknown member: " + gcnew System::String((wchar_t*) *String::Value(iName))));
	return Handle<Value>();
}

////////////////////////////////////////////////////////////////////////////////////////////////////
Handle<Value> DoMethodCall(const Arguments& args)
{	 
	//call to bridge with args  
	HandleScope h01; 
	 
	MetCallingArgs callingArgs;
	memset(&callingArgs,0,sizeof(MetCallingArgs));		 
	callingArgs.args = &args;
	callingArgs.methodCallKind = MET_;

	Local<v8::External> ext= Local<v8::External>::Cast( args.Data());
	CallingContext* cctx =  (CallingContext*)ext->Value();  

	int m_index = cctx->mIndex; 

	cctx->ctx->myMangedCallBack(m_index,//method index
		MET_, //method kind
		&callingArgs); 

	return cctx->ctx->AnyToV8(callingArgs.result);
	 
}
////////////////////////////////////////////////////////////////////////////////////////////////////
Handle<Value> DoGetterProperty(Local<String> propertyName,const AccessorInfo& info) 
{

	HandleScope h01;  	

	//wstring name = (wchar_t*) *String::Value(propertyName);
	Local<v8::External> ext= Local<v8::External>::Cast( info.Data());
	CallingContext* cctx =  (CallingContext*)ext->Value(); 
	 
	 
	int m_index = cctx->mIndex;
	Handle<External> external = Handle<External>::Cast(info.Holder()->GetInternalField(0));
	ManagedRef* extHandler=(ManagedRef*)external->Value();; 

	MetCallingArgs callingArgs;
	memset(&callingArgs,0,sizeof(MetCallingArgs));  
	callingArgs.accessorInfo = &info;
	callingArgs.methodCallKind = MET_GETTER; 
	 	
	cctx->ctx->myMangedCallBack(m_index,MET_GETTER, &callingArgs); 
	
	//close and return value
	return h01.Close(cctx->ctx->AnyToV8(callingArgs.result));
}

void DoSetterProperty(Local<String> propertyName,
	Local<Value> value,
	const AccessorInfo& info)
{
	 
	HandleScope h01;  

	Local<v8::External> ext= Local<v8::External>::Cast( info.Data());
	CallingContext* cctx =  (CallingContext*)ext->Value(); 

	//int m_index  = info.Data()->Int32Value();	 
	int m_index = cctx->mIndex;
	Handle<External> external = Handle<External>::Cast(info.Holder()->GetInternalField(0));
	ManagedRef* extHandler=(ManagedRef*)external->Value();
 
	Handle<Object> obj= Handle<Object>::Cast(info.Holder()->GetInternalField(0));
	MetCallingArgs callingArgs;
	memset(&callingArgs,0,sizeof(MetCallingArgs)); 
	callingArgs.accessorInfo = &info;
    callingArgs.methodCallKind = MET_SETTER; 
	callingArgs.setterValue = value; 
	cctx->ctx->myMangedCallBack(m_index,MET_SETTER, &callingArgs); 
}

Handle<Value> Setter(Local<String> iName, Local<Value> iValue, const AccessorInfo& iInfo)
{
	//TODO: implement this ...
	HandleScope h01;  
	//name of method or property is sent to here
	wstring name = (wchar_t*) *String::Value(iName);
	//Handle<External> external = Handle<External>::Cast(iInfo.Holder()->GetInternalField(0));
	//Noesis::Javascript::ManagedRef* exH = (Noesis::Javascript::ManagedRef*)external->Value();

	return  h01.Close(Handle<Value>());
	//JavascriptExternal* wrapper = (JavascriptExternal*) external->Value();

	// set property
	//return wrapper->SetProperty(name, iValue);
	//return 
}

////////////////////////////////////////////////////////////////////////////////////////////////////

Handle<Value> IndexGetter(uint32_t iIndex, const AccessorInfo &iInfo)
{		
	
	HandleScope h01;  
	//TODO: implement this ...
	Handle<External> external = Handle<External>::Cast(iInfo.Holder()->GetInternalField(0));
	//JavascriptExternal* wrapper = (JavascriptExternal*) external->Value();
	//Handle<Value> value;

	//// get property
	//value = wrapper->GetProperty(iIndex);
	//if (!value.IsEmpty())
	//	return value;

	// member not found
	return h01.Close( Handle<Value>());
} 
//////////////////////////////////////////////////////////////////////////////////////////////////// 
Handle<Value> IndexSetter(uint32_t iIndex, Local<Value> iValue, const AccessorInfo &iInfo)
{	
	HandleScope h01;  
	Handle<External> external = Handle<External>::Cast(iInfo.Holder()->GetInternalField(0));
	//JavascriptExternal* wrapper = (JavascriptExternal*) external->Value();
	//Handle<Value> value;

	//// get property
	//value = wrapper->SetProperty(iIndex, iValue);
	//if (!value.IsEmpty())
	//	return value;

	// member not found
	return h01.Close(Handle<Value>());
}
void JsContext::RegisterManagedCallback(void* callback,int callBackKind)
{
	this->myMangedCallBack = (del_JsBridge)callback; 
}

ExternalTypeDefinition* JsContext::RegisterTypeDefinition(int mIndex,const char* stream,int streamLength)
{

	Locker locker(isolate_);
	Isolate::Scope isolate_scope(isolate_);
	(*context_)->Enter(); 

	//use 2 handle scopes ***, otherwise this will error	 

	HandleScope handleScope2;
	HandleScope handleScope; 
	//create new object template
	Handle<ObjectTemplate> objTemplate = ObjectTemplate::New();  
	objTemplate->SetInternalFieldCount(1);//store native instance

	//--------------------------------------------------------------

	//read with stream
	BinaryStreamReader binReader(stream,streamLength);
	//--------------------------------------------------------------
	//marker (2 bytes)
	int marker_kind= binReader.ReadInt16();  
	//--------------------------------------------------------------
	/*if(managedListner){
	managedListner(0,L"typekind",0);
	}*/
	//---------------------------------------------------------------
	//deserialize data to typedefinition
	//plan: we can use other technique eg. json deserialization 
	//---------------------------------------------------------------

	//this is typename	 
	//--------------------------------------------------------------
	//send type definition handler back to managed side

	ExternalTypeDefinition* externalTypeDef= new ExternalTypeDefinition(mIndex);	
	//1.  typekind( 2 bytes)
	int type_kind= binReader.ReadInt16();  
	//2. typeid
	int type_id=  binReader.ReadInt16(); 	
	//--------------------------------------------------------------
	//3. typename
	//3. typedefinition name(length-prefix unicode)
	wstring typeDefName= binReader.ReadUtf16String();	 
	//if(managedListner){ //--if valid pointer

	//	managedListner(0,typeDefName.c_str() ,0);
	//}

	//4. num of fields 
	int nfields= binReader.ReadInt16(); 

	for(int i=0;i< nfields;++i)
	{

		int flags= binReader.ReadInt16();
		int fieldId= binReader.ReadInt16();		
		std::wstring fieldname= binReader.ReadUtf16String(); 
		////field 
		//objTemplate->SetAccessor(String::New((uint16_t*)(fieldname.c_str())),
		//	DoGetterProperty,
		//	DoSetterProperty,
		//	v8::Int32::New(fieldId));  
	}   
	//6. num of methods
	int nMethods= binReader.ReadInt16();
	for(int i=0;i<nMethods;++i)
	{  
		int flags= binReader.ReadInt16();
		int methodId= binReader.ReadInt16();  
		std::wstring metName= binReader.ReadUtf16String();  


		CallingContext* callingContext= new CallingContext();		 
		callingContext->ctx = this;
		callingContext->mIndex = methodId;		
		auto wrap = v8::External::New(callingContext);

		Handle<FunctionTemplate> funcTemplate= FunctionTemplate::New(DoMethodCall,wrap);	 
		objTemplate->Set(String::New((uint16_t*)(metName.c_str())),funcTemplate);

		//if(managedListner){ //--if valid pointer 
		//	metName.append(L"-met");
		//	managedListner(0,metName.c_str() ,0);
		//}  
	} 

	//7. properties and indexer

	int nProperties = binReader.ReadInt16();
	for(int i=0;i<nProperties;++i)
	{
		//read pair getter/setter
		int flags_getter= binReader.ReadInt16();
		int property_id= binReader.ReadInt16(); 
		//name
		std::wstring propName= binReader.ReadUtf16String(); 

		CallingContext* callingContext= new CallingContext();		 
		callingContext->ctx = this;
		callingContext->mIndex = property_id;		
		auto wrap = v8::External::New(callingContext);

		objTemplate->SetAccessor(String::New((uint16_t*)(propName.c_str())),
			DoGetterProperty,
			DoSetterProperty,
			wrap);   
	} 

	//objTemplate->SetNamedPropertyHandler(Getter, Setter);
	//objTemplate->SetIndexedPropertyHandler(IndexGetter, IndexSetter); 

	externalTypeDef->handlerToJsObjectTemplate = (Persistent<ObjectTemplate>::New(handleScope.Close(objTemplate))); 


	(*context_)->Exit();

	return externalTypeDef; 
} 

ExternalTypeDefinition* ContextRegisterTypeDefinition(
	JsContext* jsContext, 
	int mIndex,  //managed index of type
	const char* stream,
	int streamLength)
{		 

	return jsContext->RegisterTypeDefinition(mIndex,stream,streamLength); 	 

}  
void ContextRegisterManagedCallback(JsContext* jsContext,void* callback,int callBackKind)
{
	jsContext->RegisterManagedCallback(callback,callBackKind);
}
 

jsvalue ArgGetObject(MetCallingArgs* args,int index)
{		
	switch(args->methodCallKind)
	{	
		case MET_SETTER:
			{	
				//1 arg
				Local<v8::External> ext= Local<v8::External>::Cast(args->accessorInfo->Data());
				Handle<Object> obj= Handle<Object>::Cast(args->accessorInfo->This());

				CallingContext* cctx =  (CallingContext*)ext->Value();  
				return cctx->ctx->ConvAnyFromV8(args->setterValue,obj);
				 
			}break; 
		case MET_: 
			{	
				Local<v8::External> ext= Local<v8::External>::Cast( args->args->Data());
				CallingContext* cctx =  (CallingContext*)ext->Value(); 

				Local<v8::Value> arg= (Local<v8::Value>)(*(args->args))[index];  
				Handle<Object> obj= Handle<Object>::Cast(args->args->This());
				return cctx->ctx->ConvAnyFromV8(arg,obj);
			}
	} 
	jsvalue v; 
				// Initialize to a generic error.
	v.type = JSVALUE_TYPE_NULL;
	v.length = 0;
	v.value.str = 0; 
	return v;
}
jsvalue ArgGetThis(MetCallingArgs* args)
{	
	if(args->accessorInfo == NULL)
	{
		Local<v8::External> ext= Local<v8::External>::Cast(args->args->Data());
		CallingContext* cctx =  (CallingContext*)ext->Value(); 
		
		Handle<Object> obj= Handle<Object>::Cast(args->args->This());
		return cctx->ctx->ConvAnyFromV8(obj,obj);
	}
	else
	{	
		//use accessor
		//for getter/setter
		Local<v8::External> ext= Local<v8::External>::Cast(args->accessorInfo->Data());
		CallingContext* cctx =  (CallingContext*)ext->Value(); 
		
		Handle<Object> obj= Handle<Object>::Cast(args->accessorInfo->This());
		return cctx->ctx->ConvAnyFromV8(obj,obj);
	} 
} 
 
//====================================================== 
ExternalTypeDefinition::ExternalTypeDefinition(int mIndex)
{
	this->managedIndex = mIndex;
}
void ExternalTypeDefinition:: ReadTypeDefinitionFromStream(BinaryStreamReader* reader)
{ 
}
int ArgCount(MetCallingArgs* args)
{
	 

	switch(args->methodCallKind)
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