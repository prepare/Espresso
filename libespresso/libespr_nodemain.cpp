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
int myunixmain(int argc, char *argv[]) {
	// Disable stdio buffering, it interacts poorly with printf()
	// calls elsewhere in the program (e.g., any logging from V8.)
	setvbuf(stdout, nullptr, _IONBF, 0);
	setvbuf(stderr, nullptr, _IONBF, 0);
	return node::Start(argc, argv);
}
#endif

del_engineSetupCb jsEngineSetupCb;

int RunJsEngine(int argc, wchar_t *wargv[], void* engine_setupcb)
{
	jsEngineSetupCb = (del_engineSetupCb)engine_setupcb;
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

void DoEngineSetupCallback(JsEngine* engine, JsContext* jsContext) {
	if (jsEngineSetupCb) {
		jsEngineSetupCb(engine, jsContext);
	}
}