#pragma once

class CPatcherWnd
{
public:
	CPatcherWnd();

	void InitWindow();
	void DestroyWindow();
	void OnProgressUpdate(int nCurSize, int nMaxSize);
	
protected:
};