//BSD 2015, WinterDev
 

////////////////////////////////////////////////////////////////////////////////////////////////////
 
#include <v8.h>
#include <string>
#include <vector> 

#include "vroomjs.h"
#include "mini_bridge.h"


using namespace v8; 
////////////////////////////////////////////////////////////////////////////////////////////////////
 
const int mt_bool=1;
const int mt_int32=2;
const int mt_float=3;
const int mt_double=4;
const int mt_int64=5;
const int mt_string=6; 
const int mt_externalObject=7;


extern "C"{
	
	

	typedef struct MethodCallingArgs{ 
		int numArgs;  
	} MyMethodCallingArgs; 
	 

	typedef struct ExternalMethodReturnResult{
		int resultKind;
		union{
			bool v_bool;
			int int32;
			long long int64; 
			double fl64;
			float fl32;	 
			wchar_t* str_value; 
		}possibleValue;
		int length;
	}MyExternalMethodReturnResult;



	//-----------------------------------------------------
	//typedef  JavascriptContext JSContext;
    typedef  ExternalTypeDefinition ExternalTypeDef;
	typedef  ExternalManagedHandler ExtManagedHandler;
    typedef v8::Locker v8Locker;
 

	typedef void (__stdcall *del01)();

	//simple managed del
	typedef void (__stdcall *del_s)(int oIndex,const wchar_t* methodName,const wchar_t* arg1);
	typedef void (__stdcall *del_s_s)(int oIndex,const wchar_t* methodName,const wchar_t* arg1,const wchar_t* arg2);

	typedef void (__stdcall *del_i4)(int oIndex,const wchar_t* methodName,int arg1);
	typedef void (__stdcall *del_i4_i4)(int oIndex,const wchar_t* methodName,int arg1,int arg2);

	typedef void (__stdcall *del_f4)(int oIndex,const wchar_t* methodName,float arg1);
	typedef void (__stdcall *del_f4_f4)(int oIndex,const wchar_t* methodName,float arg1,float arg2);

	typedef void (__stdcall *del_d8)(int oIndex,const wchar_t* methodName,double arg1);
	typedef void (__stdcall *del_d8_d8)(int oIndex,const wchar_t* methodName,double arg1,double arg2);
	//-------------------------------------------------------------------------------------------
	
	 
	typedef void (__stdcall *del02)(int oIndex,const wchar_t* methodName,MethodCallingArgs* args);
	typedef void (__stdcall *del_JsBridge)(int oIndex,const v8::Arguments* args,ExternalMethodReturnResult* result);
	//-------------------------------------------------------------------------------------------
	  
	EXPORT int GetMiniBridgeVersion();

	EXPORT ExtManagedHandler* CreateWrapperForManagedObject(int mindex,ExternalTypeDef* extTypeDefinition);
	EXPORT void ReleaseWrapper(ExtManagedHandler* externalManagedHandler);
	EXPORT int GetManagedIndex(ExtManagedHandler* externalManagedHandler); 
    //---------------------------------------------------------------------
    //for managed code to register its callback method
	EXPORT void RegisterManagedCallback(void* callback,int callBackKind);   
	//---------------------------------------------------------------------
 
	//create engine with external managed index	 
	/*EXPORT JsContext* CreateEngineContext(int mIndex);
	EXPORT void ReleaseEngineContext(JsContext* engineContext); */
	//---------------------------------------------------------------------
	 
	//EXPORT void RegisterExternalParameter_int32(JsContext* engineContext,const wchar_t* name,int arg);
	//EXPORT void RegisterExternalParameter_double(JsContext* engineContext,const wchar_t* name,double arg);
	//EXPORT void RegisterExternalParameter_float(JsContext* engineContext,const wchar_t* name,float arg);	 
	//EXPORT void RegisterExternalParameter_string(JsContext* engineContext,const wchar_t* name,const wchar_t* arg);
	//EXPORT void RegisterExternalParameter_External(JsContext* engineContext,const wchar_t* name,ExtManagedHandler* arg);
	////---------------------------------------------------------------------
	//EXPORT int EngineContextRun(JsContext* engineContext,const wchar_t* scriptsource);
	//
	////---------------------------------------------------------------------
	//EXPORT v8Locker* EngineContextEnter(JsContext* engineContext);
	//EXPORT void EngineContextExit(JsContext* engineContext,v8Locker* locker);
	////---------------------------------------------------------------------
	 
	//create object template for describing managed type
	//then return type definition handler to managed code
	EXPORT ExternalTypeDefinition* EngineRegisterTypeDefinition(
		JsContext* engineContext,int mIndex, const char* stream,int streamLength); 
	//--------------------------------------------------------------------- 
	EXPORT int ArgGetInt32(const v8::Arguments* args,int index);
	EXPORT int ArgGetString(const v8::Arguments* args,int index, int outputLen, uint16_t* output);
	EXPORT int ArgGetStringLen(const v8::Arguments* args,int index);

	EXPORT int ArgGetAttachDataAsInt32(const v8::Arguments* args);
	//--------------------------------------------------------------------- 
	EXPORT void ArgSetBool(  MyExternalMethodReturnResult* result,bool value); 
	EXPORT void ArgSetInt32(  MyExternalMethodReturnResult* result,int value);
	EXPORT void ArgSetFloat(  MyExternalMethodReturnResult* result,float value);
	EXPORT void ArgSetDouble(  MyExternalMethodReturnResult* result,double value);
	EXPORT void ArgSetString(  MyExternalMethodReturnResult* result,wchar_t* value);
	EXPORT void ArgSetNativeObject(  MyExternalMethodReturnResult* result,int proxyId);
	//--------------------------------------------------------------------- 


	EXPORT int TestCallBack(); 
}
