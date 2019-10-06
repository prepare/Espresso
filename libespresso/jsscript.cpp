//MIT, 2015-2019, EngineKit, brezza92
#include "espresso.h"

long js_mem_debug_script_count;

JsScript *JsScript::New(JsEngine *engine) {
	JsScript *jsscript = new JsScript();
	jsscript->engine_ = engine;
	jsscript->script_ = NULL;
	return jsscript;
}
void JsScript::Compile(const uint16_t* str, const uint16_t *resourceName, jsvalue* output) {

	output->type = 0;
	script_ = engine_->CompileScript(str, resourceName, output);
}

void JsScript::Dispose() {
	Isolate *isolate = engine_->GetIsolate();
	if (isolate != NULL) {
		Locker locker(isolate);
		Isolate::Scope isolate_scope(isolate);
		script_->Reset();
		delete script_;
	}
}
