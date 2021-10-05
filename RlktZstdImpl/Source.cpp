#include "stdafx.h"

int main()
{
	size_t const cBuffSize = ZSTD_compressBound(1);
	if (cBuffSize != 1)
		return 1;

	return 0;
}