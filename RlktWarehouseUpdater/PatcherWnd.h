#pragma once

class CPatcherWnd
{
public:
	CPatcherWnd();

	void InitWindow();
	void DestroyWindow();
	void OnProgressUpdate(int nCurSize, int nMaxSize);

protected:
	HWND m_DlgHwnd;
	HINSTANCE m_DlgHInst;
};

BOOL CALLBACK PatcherWndMsg(HWND hwnd, UINT message, WPARAM wParam, LPARAM lParam);
