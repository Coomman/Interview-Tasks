#ifndef EXTERNALS
#define EXTERNALS

extern "C"
{
	__declspec(dllexport) int __stdcall str_reverse(const char* raw, char* res);
}

#endif