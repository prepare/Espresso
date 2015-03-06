//BSD 2015, WinterDev


////////////////////////////////////////////////////////////////////////////////////////////////////

#include <v8.h>
#include <string>
#include <vector> 

#include "vroomjs.h" 


using namespace v8; 
////////////////////////////////////////////////////////////////////////////////////////////////////

const int mt_bool=1;
const int mt_int32=2;
const int mt_float=3;
const int mt_double=4;
const int mt_int64=5;
const int mt_string=6; 
const int mt_externalObject=7;
//-----------------------------
const int MET_=0;
const int MET_GETTER=1;
const int MET_SETTER=2;

extern "C"{



 
	typedef struct MetCallingArgs{
		
		//-----------------------
		//calling args 
		const v8::Arguments* args;
		//-----------------------
		//return result 
		int resultKind;
		union{
			bool v_bool;
			int int32;
			long long int64; 
			double fl64;
			float fl32;	 
			wchar_t* str_value; 
		}possibleValue;
		int length;//str_length
		jsvalue* v;

	} MetCallingArgs_;
	   


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


	typedef void (__stdcall *del02)(int oIndex,const wchar_t* methodName,MetCallingArgs* args);
	typedef void (__stdcall *del_JsBridge)(int mIndex,int methodKind,MetCallingArgs* result);
	//-------------------------------------------------------------------------------------------

	 
	EXPORT ManagedObjRef* CreateWrapperForManagedObject(JsContext* engineContext,int mindex,ExternalTypeDefinition* extTypeDefinition);
	EXPORT void ReleaseWrapper(ManagedObjRef* managedObjRef);
	EXPORT int GetManagedIndex(ManagedObjRef* managedObjRef); 
	//---------------------------------------------------------------------
	//for managed code to register its callback method
	EXPORT void RegisterManagedCallback(void* callback,int callBackKind);   
	//---------------------------------------------------------------------
	 
	//create object template for describing managed type
	//then return type definition handler to managed code
	EXPORT ExternalTypeDefinition* ContextRegisterTypeDefintion(
		JsContext* engineContext,
		int mIndex, 
		const char* stream,
		int streamLength); 
	//--------------------------------------------------------------------- 
	EXPORT int ArgGetInt32(MetCallingArgs* args,int index);
	EXPORT int ArgGetString(MetCallingArgs* args,int index, int outputLen, uint16_t* output);
	EXPORT int ArgGetStringLen(MetCallingArgs* args,int index);
	 
	//--------------------------------------------------------------------- 
	EXPORT void ResultSetBool(MetCallingArgs* result,bool value); 
	EXPORT void ResultSetInt32(MetCallingArgs* result,int value);
	EXPORT void ResultSetFloat(MetCallingArgs* result,float value);
	EXPORT void ResultSetDouble(MetCallingArgs* result,double value);
	EXPORT void ResultSetString(MetCallingArgs* result,wchar_t* value);
	EXPORT void ResultSetNativeObject(MetCallingArgs* result,int proxyId);
	//--------------------------------------------------------------------- 


	EXPORT int TestCallBack(); 
}
