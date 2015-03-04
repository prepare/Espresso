//BSD 2015, WinterDev
#include <string>
#include "bridge2.h"
#include "vroomjs.h" 
#include "mini_bridge.h"
////////////////////////////////////////////////////////////////////////////////////////////////////
using namespace std;
using namespace v8;


del02 managedListner; //for debug 

del_JsBridge managedJsBridge;
//-----------------


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
			managedJsBridge=  (del_JsBridge)funcPtr;
		}break;
	} 
}

int TestCallBack()
{
	MethodCallingArgs a;
	managedListner(0,L"OKOK001",&a);
	return 1;
}
int LibGetVersion()
{		 
	return 3;
}
void ArgSetBool(MyExternalMethodReturnResult* result,bool value)
{
	result->resultKind = mt_bool;
	result->possibleValue.v_bool = value;
}
void ArgSetInt32(MyExternalMethodReturnResult* result,int value)
{
	result->resultKind =mt_int32;
	result->possibleValue.int32 = value;
}
void ArgSetFloat(MyExternalMethodReturnResult* result,float value)
{	
	result->resultKind =mt_float;
	result->possibleValue.fl32 = value;
}
void ArgSetDouble(MyExternalMethodReturnResult* result,double value)
{
	result->resultKind =mt_double;
	result->possibleValue.fl64 = value;
}
void ArgSetString(MyExternalMethodReturnResult* result,wchar_t* value)
{	
	result->resultKind =mt_string;
	result->possibleValue.str_value = value;
}
void ArgSetNativeObject(MyExternalMethodReturnResult* result,int proxyId)
{
	result->resultKind = mt_int32;
	result->possibleValue.int32 = proxyId; 
}


Persistent<ObjectTemplate> 	createObjectTemplate(){

	HandleScope handleScope;
	Handle<ObjectTemplate> result = ObjectTemplate::New();
	//result->SetInternalFieldCount(1);
	//result->SetNamedPropertyHandler(Getter, Setter);
	//result->SetIndexedPropertyHandler(IndexGetter, IndexSetter); 
	return Persistent<ObjectTemplate>::New(handleScope.Close(result));
} 
Handle<Value> JsFunctionBridge(const Arguments& args)
{	 
	//call to bridge with args 
	//auto data= args.Data(); 


	/* Local<Object> self = info.Holder();
	Local<External> wrap = Local<External>::Cast(self->GetInternalField(0));
	ManagedRef* ref = (ManagedRef*)wrap->Value();
	return scope.Close(ref->GetPropertyValue(name));
	*/

	HandleScope h01;

	if(managedListner)
	{
		//for debug
		managedListner(0,L"data",0);
	} 


	if(managedJsBridge)
	{  	   

		ExternalMethodReturnResult result;
		memset(&result,0,sizeof(ExternalMethodReturnResult));
		result.resultKind =0;//init  
		result.length =0; //init      
		int m_index =  ArgGetAttachDataAsInt32(&args);
		//extract args values
		/*Local<v8::Value> a0= (Local<v8::Value>)args[0];
		if(a0->IsString())
		{	
		auto str01=  a0->ToString();
		auto str01_len= str01->Length();

		wchar_t* a0_str= (wchar_t*)*v8::String::Value(a0->ToString());

		int a02= a0->Int32Value(); 
		}
		int a01= a0->Int32Value(); */ 
		managedJsBridge(m_index, &args,&result); 
		switch(result.resultKind)
		{

		case 1:
			{
				//boolean
				return v8::Boolean::New(result.possibleValue.v_bool);
			}break;
		case 2://int32
			{		 
				//return v8::Persistent<v8::Object>(v8::Int32::New(result.possibleValue.int32));
				//return scope1.Close( v8::Int32::New(result.possibleValue.int32));

				//return hscope.Close(v8::Int32::New(result.possibleValue.int32));
				return h01.Close(v8::Int32::New(result.possibleValue.int32));
				//return  v8::Int32::New(result.possibleValue.int32);
			}		 
		case 3:
			{ //float
				return v8::Number::New(result.possibleValue.fl32);
			}break;
		case 4:
			{ //double
				return v8::Number::New(result.possibleValue.fl64);
			}break;		
		case 5:
			{	//int64
				return v8::Number::New(result.possibleValue.int64);
			}
		case 6:
			{  
				//string  wchar_t*			
				//always send with null terminal char**				 
				return v8::String::New((uint16_t*)result.possibleValue.str_value); 
			}break; 
		case 7:
			{
				////return v8::Persistent<v8::object>(result.pos
				////return v8::Handle<ExternalObject>( result.possibleValue.externalObjectPtr);
				//HandleScope handleScope;
				//Handle<v8::Value> jsdata;
				//auto obj = v8::Object::New();
				//   jsdata= obj;				 
				//   //return v8::Persistent<ExternalObject>::New(v8::Handle<ExternalObject>(0));
				//return handleScope.Close(jsdata);				 
				return v8::Number::New(0);
			}break;
		default:
			{
				return v8::Undefined();
			}
		} 
	}
	return v8::Undefined();
}


ExternalManagedHandler* CreateWrapperForManagedObject(int mIndex, ExternalTypeDefinition* externalTypeDef)
{ 

	HandleScope handleScope;	
	/*HandleScope handleScope2;	
	HandleScope handleScope3;*/
	ExternalManagedHandler* handler= new ExternalManagedHandler(mIndex);

    //create js from template
	if(externalTypeDef)
	{
		if(managedListner){
			managedListner(1,L"handle0",0);
			if((externalTypeDef->handlerToJsObjectTemplate).IsEmpty())
			{
				managedListner(1,L"handle1",0);
			}
			else
			{
				managedListner(1,L"handle2",0);
			}
		}
		//auto a1= externalTypeDef->handlerToJsObjectTemplate->NewInstance();
		handler->v8InstanceHandler=
			Persistent<v8::Object>::New(externalTypeDef->handlerToJsObjectTemplate->NewInstance());
		handler->v8InstanceHandler->SetInternalField(0,External::New(handler));
	}
	 
	return handler;
}
int GetManagedIndex(ExternalManagedHandler* externalManagedHandler)
{
	return  ((ExternalManagedHandler*)externalManagedHandler)->managedIndex;
}
void ReleaseWrapper(ExternalManagedHandler* externalManagedHandler)
{	
	delete externalManagedHandler;
}

//
// 
//int GetManagedIndex(ExternalManagedHandler* externalManagedHandler)
//{
//	return  ((ExternalManagedHandler*)externalManagedHandler)->managedIndex;
//}
//void ReleaseWrapper(ExternalManagedHandler* externalManagedHandler)
//{	
//	delete externalManagedHandler;
//}


//void CallManagedMethod(int mindex,const wchar_t* methodName)
//{	
//	MethodCallingArgs args;
//	args.numArgs =2;
//	managedCallBackPath(mindex,methodName,&args);
//}
//void CallManagedMethodWithArgs(int mindex,const wchar_t* methodName,MethodCallingArgs* args)
//{	 
//	managedCallBackPath(mindex,methodName,args);
//}

////////////////////////////////////////////////////////////////////////////////////////////////////

Handle<Value>
	Getter(Local<String> iName, const AccessorInfo &iInfo)
{
	//name may be method or field 
	
	wstring name = (wchar_t*) *String::Value(iName);
	Handle<External> external = Handle<External>::Cast(iInfo.Holder()->GetInternalField(0));
	ExternalManagedHandler* extHandler=(ExternalManagedHandler*)external->Value();;
   
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

Handle<Value>
	Setter(Local<String> iName, Local<Value> iValue, const AccessorInfo& iInfo)
{

	 
	//name of method or property is sent to here
	wstring name = (wchar_t*) *String::Value(iName);
	//Handle<External> external = Handle<External>::Cast(iInfo.Holder()->GetInternalField(0));
	//Noesis::Javascript::ExternalManagedHandler* exH = (Noesis::Javascript::ExternalManagedHandler*)external->Value();
	 
	return Handle<Value>();
	//JavascriptExternal* wrapper = (JavascriptExternal*) external->Value();

	// set property
	//return wrapper->SetProperty(name, iValue);
	//return 
}

////////////////////////////////////////////////////////////////////////////////////////////////////

Handle<Value>
	IndexGetter(uint32_t iIndex, const AccessorInfo &iInfo)
{

	Handle<External> external = Handle<External>::Cast(iInfo.Holder()->GetInternalField(0));
	//JavascriptExternal* wrapper = (JavascriptExternal*) external->Value();
	//Handle<Value> value;

	//// get property
	//value = wrapper->GetProperty(iIndex);
	//if (!value.IsEmpty())
	//	return value;

	// member not found
	return Handle<Value>();
}

////////////////////////////////////////////////////////////////////////////////////////////////////

Handle<Value> IndexSetter(uint32_t iIndex, Local<Value> iValue, const AccessorInfo &iInfo)
{
	Handle<External> external = Handle<External>::Cast(iInfo.Holder()->GetInternalField(0));
	//JavascriptExternal* wrapper = (JavascriptExternal*) external->Value();
	//Handle<Value> value;

	//// get property
	//value = wrapper->SetProperty(iIndex, iValue);
	//if (!value.IsEmpty())
	//	return value;

	// member not found
	return Handle<Value>();
}

ExternalTypeDefinition* EngineRegisterTypeDefinition(
	JsContext* engineContext, 
	int mIndex,  //managed index of type
	const char* stream,
	int streamLength)
{   

	//use 2 handle scopes ***, otherwise this will error	 

	HandleScope handleScope2;
	HandleScope handleScope; 
	//create new object template
	Handle<ObjectTemplate> objTemplate = ObjectTemplate::New();  
	objTemplate->SetInternalFieldCount(1);//store native instance
	objTemplate->SetNamedPropertyHandler(Getter, Setter);
	objTemplate->SetIndexedPropertyHandler(IndexGetter, IndexSetter);
	//--------------------------------------------------------------

	//read with stream
	BinaryStreamReader binReader(stream,streamLength);
	//--------------------------------------------------------------
	//marker (2 bytes)
	int marker_kind= binReader.ReadInt16();  
	//--------------------------------------------------------------
	if(managedListner){
		managedListner(0,L"typekind",0);
	}
	//---------------------------------------------------------------
	//deserialize data to typedefinition
	//plan: we can use other technique eg. json deserialization 
	//---------------------------------------------------------------

	//this is typename	 
	//--------------------------------------------------------------
	//send type definition handler back to managed side

	ExternalTypeDefinition* externalTypeDef= new ExternalTypeDefinition(mIndex);	
	//1. type id
	int type_id=  binReader.ReadInt16(); 
	//2. typekind( 2 bytes)
	int type_kind= binReader.ReadInt16();  
	//--------------------------------------------------------------
	//3. typename
	//3. typedefinition name(length-prefix unicode)
	 wstring typeDefName= binReader.ReadUtf16String();	 
	if(managedListner){ //--if valid pointer

		managedListner(0,typeDefName.c_str() ,0);
	}
	//4. num of fields
	int nfields= binReader.ReadInt16();
	for(int i=0;i< nfields;++i)
	{
		//in this version..
		//field compose of..
		//field name
		//field type

		int fieldId= binReader.ReadInt16();
		int flags= binReader.ReadInt16();
		std::wstring fieldname= binReader.ReadUtf16String();
		if(managedListner){ //--if valid pointer

			fieldname.append(L"-field");
			managedListner(0,fieldname.c_str() ,0);
		}  

	}   
	//6. num of methods
	int nMethods= binReader.ReadInt16();
	for(int i=0;i<nMethods;++i)
	{
		//method name
		//marker
		//method flags

		int methodId= binReader.ReadInt16();
		int flags= binReader.ReadInt16();
		std::wstring metName= binReader.ReadUtf16String();
		//auto a= v8::Int32::New(1); 
		Handle<FunctionTemplate> funcTemplate=FunctionTemplate::New(JsFunctionBridge,v8::Int32::New(methodId));			 
		//String::New(L"mm")); 
		objTemplate->Set(String::New((uint16_t*)(metName.c_str())),funcTemplate);

		if(managedListner){ //--if valid pointer 
			metName.append(L"-met");
			managedListner(0,metName.c_str() ,0);
		}  
	} 

	externalTypeDef->handlerToJsObjectTemplate = (Persistent<ObjectTemplate>::New(handleScope.Close(objTemplate))); 
	return externalTypeDef; 
} 
//=====================================================
//
//Noesis::Javascript::JavascriptContext* CreateEngineContext(int mIndex)
//{
//	
//	JavascriptContext* jsContext = new JavascriptContext();
//	return jsContext;
//}
//void ReleaseEngineContext(Noesis::Javascript::JavascriptContext* engineContext)
//{
//	//please release unused engine, otherwise-> mem leak	  
//	delete engineContext;
//}
//
//v8::Locker* EngineContextEnter(JsContext* engineContext)
//{	
//	return engineContext->Ent
//}
//void EngineContextExit(JsContext* engineContext,v8::Locker* locker)
//{
//	engineContext->Exit(locker);
//}
//
//int EngineContextRun(Noesis::Javascript::JavascriptContext* engineContext,
//	const wchar_t* scriptsource)
//{
//	//send parameters  and run	
//	SystemOutputObject output; 
//	memset(&output,0,sizeof(SystemOutputObject)); 
//	engineContext->Run2(scriptsource,&output);	 
//
//	return output.int32value;  
//}
//
//void RegisterExternalParameter_int32(JsContext* engineContext,const wchar_t* name,int arg)
//{	 
//	(engineContext)->SetParameter_int32(name,arg);
//}
//void RegisterExternalParameter_double(JsContext* engineContext,const wchar_t* name,double arg)
//{
//	(engineContext)->SetParameter_double(name,arg);
//}
//void RegisterExternalParameter_float(JsContext* engineContext,const wchar_t* name,float arg)
//{
//	(engineContext)->SetParameter_float(name,arg);
//}
//void RegisterExternalParameter_int64(JsContext* engineContext,const wchar_t* name,long long arg)
//{
//	//((JavascriptContext*)engineContext)->SetParameter_int32(name,arg);
//}
//void RegisterExternalParameter_string(JsContext* engineContext,const wchar_t* name,const wchar_t* arg)
//{
//	(engineContext)->SetParameter_string(name,arg);
//}
////RegisterExternalParameter_External
//void RegisterExternalParameter_External(JsContext* engineContext,
//	const wchar_t* name,ExternalManagedHandler* arg)
//{
//	(engineContext)->SetParameter_External(name,arg);
//}


//=====================================================
int ArgGetAttachDataAsInt32(const v8::Arguments* args)
{

	return args->Data()->Int32Value();	 
}

int ArgGetInt32(const v8::Arguments* args,int index)
{	
	return  ((*args)[index])->Int32Value();
}
int ArgGetString(const v8::Arguments* args,int index, int outputLen, uint16_t* output)
{	
	//return (wchar_t*)(*(((*args)[index])->ToString())); 
	//wstring name = (wchar_t*) *v8::String::Value(iName);  
	//wchar_t* ww= (wchar_t*) (*(v8::String::Value(arg)));  
	//return ww; 
	//return (wchar_t*)*v8::String::Value(iValue->ToString()); 
	//return (wchar_t*)(*(((*args)[index])->ToString()));
	//5return (wchar_t*)(*(*(args)[index])->ToString()));

	Local<v8::Value> arg= (Local<v8::Value>)(*args)[index];  
	if(arg->IsString())
	{		
		auto str01=  arg->ToString();	    
		auto strLen= str01->Length();   
		auto copyLen= strLen;
		if(copyLen>outputLen)
		{  
			copyLen= outputLen;
		}	
		//str01->WriteUtf8(output,copyLen);
		str01->Write(output,0,copyLen);
		return strLen;	    
	}  
	return 0;
}
int ArgGetStringLen(const v8::Arguments* args,int index)
{	
	Local<v8::Value> arg= (Local<v8::Value>)(*args)[index];  
	if(arg->IsString())
	{		
		auto str01= arg->ToString();	 
		return str01->Length();   
	}  
	return 0;
}