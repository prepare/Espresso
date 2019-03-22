// Copyright Joyent, Inc. and other Node contributors.
//
// Permission is hereby granted, free of charge, to any person obtaining a
// copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to permit
// persons to whom the Software is furnished to do so, subject to the
// following conditions:
//
// The above copyright notice and this permission notice shall be included
// in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS
// OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN
// NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM,
// DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR
// OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE
// USE OR OTHER DEALINGS IN THE SOFTWARE.

#include "node.h"
#include "../src/libespresso/bridge2.h"
#ifdef _WIN32
#include <VersionHelpers.h>
#include <WinError.h>

int mywinmain(int argc, wchar_t *wargv[]) {
	if (!IsWindows7OrGreater()) {
		fprintf(stderr, "This application is only supported on Windows 7, "
			"Windows Server 2008 R2, or higher.");
		exit(ERROR_EXE_MACHINE_TYPE_MISMATCH);
	}

	// Convert argv to to UTF8
	char** argv = new char*[argc + 1];
	for (int i = 0; i < argc; i++) {
		// Compute the size of the required buffer
		DWORD size = WideCharToMultiByte(CP_UTF8,
			0,
			wargv[i],
			-1,
			nullptr,
			0,
			nullptr,
			nullptr);
		if (size == 0) {
			// This should never happen.
			fprintf(stderr, "Could not convert arguments to utf8.");
			exit(1);
		}
		// Do the actual conversion
		argv[i] = new char[size];
		DWORD result = WideCharToMultiByte(CP_UTF8,
			0,
			wargv[i],
			-1,
			argv[i],
			size,
			nullptr,
			nullptr);
		if (result == 0) {
			// This should never happen.
			fprintf(stderr, "Could not convert arguments to utf8.");
			exit(1);
		}
	}
	argv[argc] = nullptr;
	// Now that conversion is done, we can finally start.
	return node::Start(argc, argv);
}
#else
// UNIX
#ifdef __linux__
#include <elf.h>
#ifdef __LP64__
#define Elf_auxv_t Elf64_auxv_t
#else
#define Elf_auxv_t Elf32_auxv_t
#endif  // __LP64__
extern char** environ;
#endif  // __linux__

namespace node {
	extern bool linux_at_secure;
}  // namespace node

int myunixmain(int argc, char *argv[]) {
#if defined(__linux__)
	char** envp = environ;
	while (*envp++ != nullptr) {}
	Elf_auxv_t* auxv = reinterpret_cast<Elf_auxv_t*>(envp);
	for (; auxv->a_type != AT_NULL; auxv++) {
		if (auxv->a_type == AT_SECURE) {
			node::linux_at_secure = auxv->a_un.a_val;
			break;
		}
	}
#endif
	// Disable stdio buffering, it interacts poorly with printf()
	// calls elsewhere in the program (e.g., any logging from V8.)
	setvbuf(stdout, nullptr, _IONBF, 0);
	setvbuf(stderr, nullptr, _IONBF, 0);
	return node::Start(argc, argv);
}
#endif
//============================================================
del_engineSetupCb jsEngineSetupCb;
del_engineClosingCb jsEngineClosingCb;

extern "C" {
	EXPORT int RunJsEngine(int argc, wchar_t *wargv[], void* engine_setupcb,void* ening_closingcb)
	{
		jsEngineSetupCb = (del_engineSetupCb)engine_setupcb;
        jsEngineClosingCb = (del_engineClosingCb)ening_closingcb;

#ifdef _WIN32
		return mywinmain(argc, wargv);
#else
		//convert from array of wide char* to array of
		// Convert argv to to UTF8
		char** argv = new char*[argc + 1];
		for (int i = 0; i < argc; i++) {
			// Compute the size of the required buffer

			char buffer[255]; //this version we use on
			memset(&buffer, 0, 0);//clear
								  //int ret = wcstombs ( buffer, wargv[i], sizeof(buffer) );

			argv[i] = new char[255];
			int result = wcstombs(buffer, wargv[i], sizeof(buffer));
			if (result == 0) {
				// This should never happen.
				fprintf(stderr, "Could not convert arguments to utf8.");
				exit(1);
			}
		}
		argv[argc] = nullptr; //last one
		return myunixmain(argc, argv);
#endif
	};
}
void DoEngineSetupCallback(JsEngine* engine, JsContext* jsContext) {
	if (jsEngineSetupCb) {
		jsEngineSetupCb(engine, jsContext);
	}
}
void DoEngineClosingCallback(JsEngine* engine, JsContext* jsContext,int exitcode) {
  if (jsEngineClosingCb) {
     jsEngineClosingCb(engine, jsContext, exitcode);
  }
}