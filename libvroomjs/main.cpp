
#include "bridge2.h"
#include "vroomjs.h"
#include "v8-debug.h" 

void debug_msg_handler(const v8::Debug::Message& message)
{
	//this method is called when v8 exec debugger statement


	//for json protocol
	auto jsonMsg = message.GetJSON();
	String::Value v(jsonMsg);

	uint16_t* buff = (uint16_t*)*v;
	const wchar_t* buff2 = (wchar_t*)buff;
	std::wstring wstr = buff2;
	std::wprintf(wstr.c_str());

}
void main()
{
	V8Init();


	//1. create engine
	JsEngine*  jsEngine = JsEngine::New(-1, -1);
	//2. create context
	auto isolate_ = jsEngine->isolate_;
	JsContext* context = JsContext::New(1, jsEngine);
	
	context->SetDebugHandler(debug_msg_handler);
	 

	//source file
	uint16_t* s = (uint16_t*)(L"debugger;");
	uint16_t* sname = (uint16_t*)"A";
	//execute
	context->Execute(s, sname);

}