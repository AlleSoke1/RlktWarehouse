#include "stdafx.h"
#include "PatcherWnd.h"

#include "resource.h"

#include <Commctrl.h>

HWND g_Hwnd = NULL;

CPatcherWnd::CPatcherWnd()
	: m_DlgHInst(NULL)
	, m_DlgHwnd(NULL)
{
}

void CPatcherWnd::InitWindow()
{
	g_Hwnd = GetActiveWindow();
	m_DlgHwnd = CreateDialog(m_DlgHInst, MAKEINTRESOURCE(IDD_DIALOG1), 0, PatcherWndMsg);
	ShowWindow(m_DlgHwnd, 1);
}

void CPatcherWnd::DestroyWindow()
{
	EndDialog(m_DlgHwnd, 1);
}

void CPatcherWnd::OnProgressUpdate(int nCurSize, int nTotalSize)
{
	if (nTotalSize == 0) return;

	//Update progress bar
	int iProgress = nCurSize / nTotalSize;
	SendMessage(GetDlgItem(g_Hwnd, IDC_PROGRESS1), PBM_SETPOS, (int)iProgress, 0);

	//Update remaining size text
	TCHAR ProgressText[64];
	wsprintf(ProgressText, L"%d / %d", nCurSize, nTotalSize);
	SetWindowText(GetDlgItem(g_Hwnd, IDD_SIZE), ProgressText);
}

BOOL CALLBACK PatcherWndMsg(HWND hwnd, UINT message, WPARAM wParam, LPARAM lParam)
{
	if (!g_Hwnd)
		g_Hwnd = hwnd;

	switch (message)
	{
		case WM_INITDIALOG:
		{
			//Center window
			RECT rc;
			GetWindowRect(hwnd, &rc);

			int xPos = (GetSystemMetrics(SM_CXSCREEN) - rc.right) / 2;
			int yPos = (GetSystemMetrics(SM_CYSCREEN) - rc.bottom) / 2;

			SetWindowPos(hwnd, 0, xPos, yPos, 0, 0, SWP_NOZORDER | SWP_NOSIZE);
		}
	}
	return TRUE;
}