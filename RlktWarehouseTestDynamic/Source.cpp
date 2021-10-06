#include <Windows.h>
#include "WarehouseLib.h"

int main()
{
	Initialize(0, 1, "127.0.0.1", 9666);

	while (1)
	{
		Sleep(100);
	}

	return 0;
}