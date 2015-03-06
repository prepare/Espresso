//BSD 2015, WinterDev
#include <string>

#include <v8.h>
////////////////////////////////////////////////////////////////////////////////////////////////////
using namespace std;
using namespace v8;
#include "bridge2.h"

del02 managedListner; //for debug 
del_JsBridge managedJsBridge;
//-----------------
int GetMiniBridgeVersion(){return 7;};
//
//Persistent<ObjectTemplate> 	createObjectTemplate(){
//	HandleScope handleScope;
//	Handle<ObjectTemplate> result = ObjectTemplate::New();
//	//result->SetInternalFieldCount(1);
//	//result->SetNamedPropertyHandler(Getter, Setter);
//	//result->SetIndexedPropertyHandler(IndexGetter, IndexSetter); 
//	return Persistent<ObjectTemplate>::New(handleScope.Close(result));
//}


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
};

int TestCallBack()
{
	MetCallingArgs a;
	memset(&a,0,sizeof(MetCallingArgs));
	managedListner(0,L"OKOK001",&a);
	return 1;
};

void ResultSetBool(MetCallingArgs* callingArgs,bool value)
{
	callingArgs->resultKind = mt_bool;
	callingArgs->possibleValue.v_bool = value;
};
void ResultSetInt32(MetCallingArgs* callingArgs,int value)
{
	callingArgs->resultKind =mt_int32;
	callingArgs->possibleValue.int32 = value;
};
void ResultSetFloat(MetCallingArgs* callingArgs,float value)
{	
	callingArgs->resultKind =mt_float;
	callingArgs->possibleValue.fl32 = value;
};
void ResultSetDouble(MetCallingArgs* callingArgs,double value)
{
	callingArgs->resultKind =mt_double;
	callingArgs->possibleValue.fl64 = value;
};
void ResultSetString(MetCallingArgs* callingArgs,wchar_t* value)
{	
	callingArgs->resultKind =mt_string;
	callingArgs->possibleValue.str_value = value;
};
void ResultSetNativeObject(MetCallingArgs* callingArgs,int proxyId)
{
	callingArgs->resultKind = mt_int32;
	callingArgs->possibleValue.int32 = proxyId; 
};

 
Handle<Value> JsFunctionBridge(const Arguments& args)
{	 
	//call to bridge with args  
	HandleScope h01; 
	//if(managedListner)
	//{
	//	//for debug
	//	managedListner(0,L"data",0);
	//}  
	if(managedJsBridge)
	{  	   

		MetCallingArgs callingArgs;
		memset(&callingArgs,0,sizeof(MetCallingArgs));		 
		callingArgs.args = &args;
		 
		managedJsBridge(args.Data()->Int32Value(),//method index
			MET_, //method kind
			&callingArgs); 

		switch(callingArgs.resultKind)
		{	

			case mt_bool:
				{
					//boolean
					return h01.Close(v8::Boolean::New(callingArgs.possibleValue.v_bool));
				}break;
			case mt_int32://int32
				{	 
					return h01.Close(v8::Int32::New(callingArgs.possibleValue.int32)); 
				}		 
			case mt_float:
				{   //float
					return h01.Close(v8::Number::New(callingArgs.possibleValue.fl32));
				}break;
			case mt_double:
				{   //double
					return h01.Close(v8::Number::New(callingArgs.possibleValue.fl64));
				}break;		
			case mt_int64:
				{	
					//int64
					return h01.Close(v8::Number::New(callingArgs.possibleValue.int64));
				}
			case mt_string:
				{  
					//string  wchar_t*			
					//always send with null terminal char**				 
					return h01.Close(v8::String::New((uint16_t*)callingArgs.possibleValue.str_value)); 
				}break; 
			case mt_externalObject:
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
					return h01.Close(v8::Undefined());
				}
		} 
	}
	return h01.Close(v8::Undefined());
};


ManagedObjRef* JsContext::CreateWrapperForManagedObject(int mIndex, ExternalTypeDefinition* externalTypeDef)
{ 

	Locker locker(isolate_);
	Isolate::Scope isolate_scope(isolate_);
	(*context_)->Enter();


	HandleScope handleScope;	 
	ManagedObjRef* handler= new ManagedObjRef(mIndex);

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
};

ManagedObjRef* CreateWrapperForManagedObject(JsContext* engineContext,int mIndex, ExternalTypeDefinition* externalTypeDef)
{ 
	return engineContext->CreateWrapperForManagedObject(mIndex,externalTypeDef); 
}; 

int GetManagedIndex(ManagedObjRef* externalManagedHandler)
{
	return  ((ManagedObjRef*)externalManagedHandler)->managedIndex;
};
void ReleaseWrapper(ManagedObjRef* externalManagedHandler)
{	
	delete externalManagedHandler;
} 


Handle<Value>
	Getter(Local<String> iName, const AccessorInfo &iInfo)
{
	//name may be method or field 

	wstring name = (wchar_t*) *String::Value(iName);
	
	Handle<External> external = Handle<External>::Cast(iInfo.Holder()->GetInternalField(0));
	ManagedObjRef* extHandler=(ManagedObjRef*)external->Value();;

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
};

////////////////////////////////////////////////////////////////////////////////////////////////////
Handle<Value> 
	DoGetterProperty(Local<String> propertyName,const AccessorInfo& info) 
{

	HandleScope h01;  

	int m_index  = info.Data()->Int32Value();	 
	Handle<External> external = Handle<External>::Cast(info.Holder()->GetInternalField(0));
	ManagedObjRef* extHandler=(ManagedObjRef*)external->Value();;

	
	if(managedJsBridge)
	{  	   

		MetCallingArgs callingArgs;
		memset(&callingArgs,0,sizeof(MetCallingArgs)); 
		 
		managedJsBridge(m_index,MET_GETTER, &callingArgs); 

		switch(callingArgs.resultKind)
		{

		case mt_bool:
			{
				//boolean
				return h01.Close(v8::Boolean::New(callingArgs.possibleValue.v_bool));
			}break;
		case mt_int32://int32
			{	 
				return h01.Close(v8::Int32::New(callingArgs.possibleValue.int32)); 
			}		 
		case mt_float:
			{   //float
				return h01.Close(v8::Number::New(callingArgs.possibleValue.fl32));
			}break;
		case mt_double:
			{   //double
				return h01.Close(v8::Number::New(callingArgs.possibleValue.fl64));
			}break;		
		case mt_int64:
			{	//int64
				return h01.Close(v8::Number::New(callingArgs.possibleValue.int64));
			}
		case mt_string:
			{  
				//string  wchar_t*			
				//always send with null terminal char**				 
				return h01.Close(v8::String::New((uint16_t*)callingArgs.possibleValue.str_value)); 
			}break; 
		case mt_externalObject:
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
				return h01.Close(v8::Undefined());
			}
		} 
	}
	return h01.Close(v8::Undefined()); 
}


void DoSetterProperty(Local<String> propertyName,
                     Local<Value> value,
                     const AccessorInfo& info)
{
	HandleScope h01;  

	int m_index  = info.Data()->Int32Value();	 
	Handle<External> external = Handle<External>::Cast(info.Holder()->GetInternalField(0));
	ManagedObjRef* extHandler=(ManagedObjRef*)external->Value();;
	
	if(managedJsBridge)
	{  	   

		MetCallingArgs callingArgs;
		memset(&callingArgs,0,sizeof(MetCallingArgs)); 
		 
		
		managedJsBridge(m_index,MET_SETTER, &callingArgs); 
	} 
}

Handle<Value>
Setter(Local<String> iName, Local<Value> iValue, const AccessorInfo& iInfo)
{


	//name of method or property is sent to here
	wstring name = (wchar_t*) *String::Value(iName);
	//Handle<External> external = Handle<External>::Cast(iInfo.Holder()->GetInternalField(0));
	//Noesis::Javascript::ManagedObjRef* exH = (Noesis::Javascript::ManagedObjRef*)external->Value();

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
		//field 
		objTemplate->SetAccessor(String::New((uint16_t*)(fieldname.c_str())),
			DoGetterProperty,
			DoSetterProperty,
			v8::Int32::New(fieldId));  
	}   
	//6. num of methods
	int nMethods= binReader.ReadInt16();
	for(int i=0;i<nMethods;++i)
	{  
		int flags= binReader.ReadInt16();
		int methodId= binReader.ReadInt16(); 

		std::wstring metName= binReader.ReadUtf16String(); 
		Handle<FunctionTemplate> funcTemplate=FunctionTemplate::New(JsFunctionBridge,v8::Int32::New(methodId));	 
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
		objTemplate->SetAccessor(String::New((uint16_t*)(propName.c_str())),
			DoGetterProperty,
			DoSetterProperty,
			v8::Int32::New(property_id)); 
	} 

	//objTemplate->SetNamedPropertyHandler(Getter, Setter);
	//objTemplate->SetIndexedPropertyHandler(IndexGetter, IndexSetter); 

	externalTypeDef->handlerToJsObjectTemplate = (Persistent<ObjectTemplate>::New(handleScope.Close(objTemplate))); 


	(*context_)->Exit();

	return externalTypeDef; 
} 

ExternalTypeDefinition* ContextRegisterTypeDefintion(
	JsContext* context, 
	int mIndex,  //managed index of type
	const char* stream,
	int streamLength)
{   
	return context->RegisterTypeDefinition(mIndex,stream,streamLength); 
}  

int ArgGetInt32(MetCallingArgs* args,int index)
{	
	return  ((*(args->args))[index])->Int32Value();
}
int ArgGetString(MetCallingArgs* args,int index, int outputLen, uint16_t* output)
{	

	Local<v8::Value> arg= (Local<v8::Value>)(*(args->args))[index];  
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
int ArgGetStringLen(MetCallingArgs* args,int index)
{	
	Local<v8::Value> arg= (Local<v8::Value>)(*(args->args))[index];  
	if(arg->IsString())
	{		
		auto str01= arg->ToString();	 
		return str01->Length();   
	}  
	return 0;
}
//======================================================
ManagedObjRef::ManagedObjRef(int mIndex)
{
	this->managedIndex = mIndex;
}

//====================================================== 
ExternalTypeDefinition::ExternalTypeDefinition(int mIndex)
{
	this->managedIndex = mIndex;
}
void ExternalTypeDefinition:: ReadTypeDefinitionFromStream(BinaryStreamReader* reader)
{ 
}
//====================================================== 