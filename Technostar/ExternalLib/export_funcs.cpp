#include "export_funcs.h"

#include <string>
using namespace std;

constexpr auto error_code = 666;
constexpr auto success_code = 777;

void swap_chars(string& str, const int first, const int second)
{
	const auto temp = str[first];
	str[first] = str[second];
	str[second] = temp;
}

extern "C" {
	__declspec(dllexport) int __stdcall str_reverse(const char* raw, char* res)
	{
		if (raw == nullptr)
			return error_code;

		auto str = string(raw);

		if (str.length() > 100)
			return error_code;

		for (int i = 0; i < str.length() / 2; ++i)
			swap_chars(str, i, str.length() - i - 1);

		strcpy_s(res, str.size() + 1, str.c_str());

		return success_code;
	}
}