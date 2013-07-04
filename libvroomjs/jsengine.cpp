#include "vroomjs.h"

JsEngine* JsEngine::New()
{
	JsEngine* engine = new JsEngine();
    if (engine != NULL) 
	{            
		engine->isolate_ = Isolate::New();
	}
	return engine;
}

void JsEngine::Dispose()
{
	isolate_->Dispose();
	isolate_ = NULL;
}