//MIT, 2019, WinterDev
#include <js_native_api.h>
#include <iostream>
#include "espresso.h"

#include "node_api.h"
#include "env.h"
#include "env-inl.h"
#include "js_native_api_v8.h"

using namespace v8;

extern "C" {


//---------------
//COPY from nodejs project
// node_napi impl
struct node_napi_env__ : public napi_env__ {
  explicit node_napi_env__(v8::Local<v8::Context> context)
      : napi_env__(context) {
    // CHECK_NOT_NULL(node_env());
  }

  inline node::Environment* node_env() const {
    return node::Environment::GetCurrent(context());
  }

  bool can_call_into_js() const override {
    return node_env()->can_call_into_js();
  }

  v8::Maybe<bool> mark_arraybuffer_as_untransferable(
      v8::Local<v8::ArrayBuffer> ab) const override {
    return ab->SetPrivate(
        context(),
        node_env()->arraybuffer_untransferable_private_symbol(),
        v8::True(isolate));
  }
}; 
	
typedef node_napi_env__* node_napi_env;

static inline napi_env NewEnv(v8::Local<v8::Context> context) {
  node_napi_env result;

  result = new node_napi_env__(context);
  // TODO(addaleax): There was previously code that tried to delete the
  // napi_env when its v8::Context was garbage collected;
  // However, as long as N-API addons using this napi_env are in place,
  // the Context needs to be accessible and alive.
  // Ideally, we'd want an on-addon-unload hook that takes care of this
  // once all N-API addons using this napi_env are unloaded.
  // For now, a per-Environment cleanup hook is the best we can do.
  result->node_env()->AddCleanupHook(
      [](void* arg) { static_cast<napi_env>(arg)->Unref(); },
      static_cast<void*>(result));

  return result;
}
//---------------

EXPORT void CALLCONV
js_test_napi(JsContext* contextPtr,
                                 Persistent<Object>* jsBuff,
                                 int dstIndex,
                                 void* src,
                                 int copyLen,
                                 jsvalue* output) {
  // copy data from other native side and write to js buffer start specific
  // index
  auto ctx = contextPtr->isolate_->GetCurrentContext();
  napi_env env = NewEnv(ctx);
  void* temp_mem = malloc(30);
  napi_value result;
  napi_create_arraybuffer(env, 10, (void**)temp_mem, &result); 
  output->type = JSVALUE_TYPE_EMPTY;  // void
}
}