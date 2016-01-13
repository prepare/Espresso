
#include "bridge2.h"
#include "vroomjs.h"
#include "v8-debug.h"  
JsContext* global_ctx;

void debug_callback(const v8::Debug::EventDetails& event_details)
{
	v8::DebugEvent a = event_details.GetEvent();
	switch (a) {
	case v8::DebugEvent::Break:
		std::wprintf(L"-------------b-------------");
		break;
	case v8::DebugEvent::AfterCompile:
		std::wprintf(L"-------------AfterCompile-------------");
		//test insert break point here
		global_ctx->SetBreakPoint(3, 0);
		global_ctx->SetBreakPoint(6, 0);
		global_ctx->SetBreakPoint(5, 0);
		break;
	case v8::DebugEvent::BeforeCompile:
		std::wprintf(L"-------------BeforeCompile-------------");
		break;
	case v8::DebugEvent::CompileError:
		std::wprintf(L"-------------CompileError-------------");
		break;
	case v8::DebugEvent::AsyncTaskEvent:
		std::wprintf(L"-------------AsyncTaskEvent-------------");
		break;
	case v8::DebugEvent::Exception:
		std::wprintf(L"-------------Exception-------------");
		break;
	case v8::DebugEvent::NewFunction:
		std::wprintf(L"-------------NewFunction-------------");
		break;
	case v8::DebugEvent::PromiseEvent:
		std::wprintf(L"-------------PromiseEvent-------------");
		break;
	default:
		std::wprintf(L"-------------???-------------");
		break;

	}
} 
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

	v8::DebugEvent a = message.GetEvent();
	 
	switch (a) {
	case v8::DebugEvent::Break:		
		//run next
		v8::Debug::CancelDebugBreak(message.GetIsolate());
		break;

	/*	std::wprintf(L"-------------b2-------------");
		if (totalContinue < 2) {
			totalContinue++;
			global_ctx->DebugContinue();			
		}
		else 
		{
			v8::Debug::CancelDebugBreak(message.GetIsolate());
		}
		break;  */
	}

}
void main()
{
	V8Init();


	//1. create engine
	JsEngine*  jsEngine = JsEngine::New(-1, -1);
	//2. create context
	auto isolate_ = jsEngine->isolate_;

	JsContext* context = JsContext::New(1, jsEngine);
	context->SetDebugHandler(debug_msg_handler, debug_callback);
	global_ctx = context;
	//source file
	uint16_t* s = (uint16_t*)(L" A1();\r\n function A1(){\r\nconsole.log('b');} \r\n function A2(){\r\nconsole.log('c');}\r\n A2();");
	uint16_t* sname = (uint16_t*)"A.js"; //dummy name

	//execute
	context->Execute(s, sname);



}