#include "stdafx.h"
#include "PatcherWnd.h"


CPatcherWnd::CPatcherWnd()
{
}


void CPatcherWnd::InitWindow()
{
}

void CPatcherWnd::DestroyWindow()
{
}

void CPatcherWnd::OnProgressUpdate(int nCurSize, int nTotalSize)
{
	if (nTotalSize == 0) return;

	int iProgress = nCurSize / nTotalSize;
}