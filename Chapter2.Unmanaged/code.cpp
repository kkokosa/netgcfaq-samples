#include "pch.h"
#include "code.h"

__declspec(dllexport) int fnUnmanaged(int argument)
{
    return *static_cast<int*>(nullptr);
}