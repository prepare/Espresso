//MIT, 2015-2017, WinterDev, EngineKit, brezza92

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



typedef void (CALLINGCONVENTION *del02)(int oIndex, const char16_t* methodName, MetCallingArgs* args);
typedef void (CALLINGCONVENTION *del_engineSetupCb)(JsEngine* jsEngine, JsContext* enginContext);
typedef void(CALLINGCONVENTION* del_engineClosingCb)(JsEngine* jsEngine, JsContext* enginContext,int exitCode);

extern "C" {

	EXPORT int TestCallBack();
	EXPORT void V8Init();
	//---------------------------------------------------------------------
	//for managed code to register its callback method
	EXPORT void RegisterManagedCallback(void* callback, int callBackKind);
	//---------------------------------------------------------------------


	EXPORT ManagedRef* CreateWrapperForManagedObject(JsContext* engineContext, int mindex, ExternalTypeDefinition* extTypeDefinition);
	EXPORT void ReleaseWrapper(ManagedRef* managedObjRef);
	EXPORT int GetManagedIndex(ManagedRef* managedObjRef);

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
	EXPORT void ArgGetThis(MetCallingArgs* args, jsvalue* output);
	EXPORT void ArgGetObject(MetCallingArgs* args, int index, jsvalue* output);

	//--------------------------------------------------------------------- 
	EXPORT void ResultSetBool(MetCallingArgs* result, bool value);
	EXPORT void ResultSetInt32(MetCallingArgs* result, int value);
	EXPORT void ResultSetFloat(MetCallingArgs* result, float value);
	EXPORT void ResultSetDouble(MetCallingArgs* result, double value);
	EXPORT void ResultSetString(MetCallingArgs* result, uint16_t* value);
	EXPORT void ResultSetValue(MetCallingArgs* result, jsvalue* value);

	EXPORT void ResultSetManagedObjectIndex(MetCallingArgs* result, int32_t managedObjectIndex);

	EXPORT void ResultSetJsNull(MetCallingArgs* result);
	EXPORT void ResultSetJsVoid(MetCallingArgs* result);
	//--------------------------------------------------------------------- 
	 
}
/////////////////////////////////////////////////////////////////////////////
