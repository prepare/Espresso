//BSD 2015, WinterDev
//MIT, 2015-2017, EngineKit, brezza92

////////////////////////////////////////////////////////////////////////////////////////////////////

#include <v8.h>
#include <string>
#include <vector> 

#include "espresso.h" 


using namespace v8;
////////////////////////////////////////////////////////////////////////////////////////////////////


const char MET_ = 0;
const char MET_GETTER = 1;
const char MET_SETTER = 2;

extern "C" {

	//method calling args can be used store set value to a property,

	struct MetCallingArgs {
		uint32_t methodCallKind;
		const v8::FunctionCallbackInfo<Value>* args; //store method input args
		const v8::PropertyCallbackInfo<Value>* accessorInfo; //accessor info for indexer
		Local<Value> setterValue;  //value for set this property 

		//return value to our managed side***
		//this should be one of out MyJsValue ...
		MyJsValue* result1;
		struct jsvalue result;
	};

	typedef void (CALLINGCONVENTION *del02)(int oIndex, const wchar_t* methodName, MetCallingArgs* args);
	typedef void (CALLINGCONVENTION *del_engineSetupCb)(JsEngine* jsEngine, JsContext* enginContext);

	EXPORT ManagedRef* CreateWrapperForManagedObject(JsContext* engineContext, int mindex, ExternalTypeDefinition* extTypeDefinition);
	EXPORT void ReleaseWrapper(ManagedRef* managedObjRef);
	EXPORT int GetManagedIndex(ManagedRef* managedObjRef);
	//---------------------------------------------------------------------
	//for managed code to register its callback method
	EXPORT void RegisterManagedCallback(void* callback, int callBackKind);
	//---------------------------------------------------------------------

	//create object template for describing managed type
	//then return type definition handler to managed code
	EXPORT ExternalTypeDefinition* ContextRegisterTypeDefinition(
		JsContext* jsContext,
		int mIndex,
		const char* stream,
		int streamLength);
	EXPORT void ContextRegisterManagedCallback(
		JsContext* jsContext,
		void* callback,
		int callBackKind);

	//--------------------------------------------------------------------- 
	EXPORT int ArgCount(MetCallingArgs* args);
	//TODO: JS_VALUE
	EXPORT jsvalue ArgGetThis(MetCallingArgs* args);
	//TODO: JS_VALUE
	EXPORT jsvalue ArgGetObject(MetCallingArgs* args, int index);

	//--------------------------------------------------------------------- 
	EXPORT void ResultSetBool(MetCallingArgs* result, bool value);
	EXPORT void ResultSetInt32(MetCallingArgs* result, int value);
	EXPORT void ResultSetFloat(MetCallingArgs* result, float value);
	EXPORT void ResultSetDouble(MetCallingArgs* result, double value);
	EXPORT void ResultSetString(MetCallingArgs* result, wchar_t* value);
	//TODO: JS_VALUE
	EXPORT void ResultSetJsValue(MetCallingArgs* result, jsvalue value);
	//--------------------------------------------------------------------- 

	EXPORT void V8Init();
	EXPORT int TestCallBack();

	
	//this is for espresso-node
	EXPORT int RunJsEngine(int argc, wchar_t *wargv[], void* engine_setupcb);
	void DoEngineSetupCallback(JsEngine* engine, JsContext* jsContext);

}
/////////////////////////////////////////////////////////////////////////////
