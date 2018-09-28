// ��������� ��������������� ������������� ������� 
// SetLayeredWindowAttributes

// ����������: WinAPI, unicode, w2k, WinXP
// �����: �������� ������ �������, duan@bk.ru

#define _WIN32_WINNT 0x0500		// ��� ������������� ������� SetLayeredWindowAttributes �
								// ��������� � ��� ��������
#if _MSC_VER > 1000
#pragma once
#endif // _MSC_VER > 1000

#include "Stdafx.h"
// Windows
#include <windows.h>
#include <windowsx.h>

#include <uxtheme.h>
#pragma comment (lib, "Uxtheme.lib")

#include <Gdiplus.h>
using namespace Gdiplus;
#pragma comment(lib, "GdiPlus.lib")
 

// CRT
#include <io.h>
#include <fcntl.h>
#include <assert.h>
#include <tchar.h>
#include <math.h>
#include <crtdbg.h>	

#ifdef  assert
#define	verify(exp) if(!exp) assert(0)
#else verify(exp) exp
#endif

// �������������� �������� �������� ����
#define		INITIAL_WND_W	200
#define		INITIAL_WND_H	200

// ���������� ����������
long scrn_w;						// ������ ������
int wnd_dx=1;						// �������������� �������� ����

double dx=0;						// �������������� ��������
double DXPS=0.04908738521234051;	// ����������

UINT bWndForm(1);					// ������� ����� ����

unsigned char* bmp_cnt = NULL;		// ���������� ����� fl
unsigned int bmp_h, bmp_w;			// ������ � ������ ��������
BITMAPINFO bmp_info;				// ���������� � �������

const TCHAR szAppName[] = TEXT("LayeredWnds");	// �������� ����������
const TCHAR szAppTitle[] = TEXT("LayeredWnds");	// ��������� �������� ���� ����������


// ������� - ����������� ���������:
VOID CALLBACK TimerProc(HWND hWnd, UINT uMsg, UINT_PTR idEvent, DWORD dwTime); // �������
void WndProc_OnPaint(HWND hWnd); // WM_PAINT
void WndProc_OnSize(HWND hWnd, UINT state, int cx, int cy);// WM_SIZE
void WndProc_OnDestroy(HWND hWnd); // WM_DESTROY
void WndProc_OnKeyDown(HWND hWnd, UINT vk, BOOL, UINT cRepeat, UINT flags); // WM_KEYDOWN
void WndProc_OnDropFiles(HWND hWnd, HDROP hDrop); 

// ������� �������� ����
LRESULT WINAPI WndProc(HWND hWnd, UINT msg, WPARAM wParam, LPARAM lParam);

// ������� ��������� ����� �������� ����
void ChangeWndForm(HWND hWnd, UINT bWndForm);

// �������� �������� �� �����
int LoadBMP(HWND hWnd /*����� ���� NULL*/, const TCHAR* fn /* ��� ����� */);

// WinMain
int WINAPI _tWinMain(HINSTANCE hInstance, HINSTANCE, LPSTR, int){


	HWND hWnd;					// ����� �������� ����
	WNDCLASSEX wc;				// ����� ����
	MSG msg;					// "���������"


	// ��������� GDI+ 
    GdiplusStartupInput gdiplusStartupInput;
    ULONG_PTR gdiplusToken;

    // ������������� GDI+
    GdiplusStartup(&gdiplusToken, &gdiplusStartupInput, NULL);

	// ������������ ����� ����
	memset(&wc, 0, sizeof(wc));
  
	//	���� wc.cbSize	� wc.hIconSm ���������� � ���������
	//	WNDCLASSEX, ������� ����� ������������ ���
	//	����������� ������ ���� � Win95
	wc.cbSize = sizeof(WNDCLASSEX);

	// �������� ����� �� ����������� ������
	wc.hIconSm = LoadIcon(NULL, IDI_APPLICATION);
  
	// ���������� ��������� WNDCLASSEX
	wc.style = CS_HREDRAW | CS_VREDRAW;
	wc.lpfnWndProc = (WNDPROC)WndProc;
	wc.cbClsExtra  = 0;
	wc.cbWndExtra  = 0;
	wc.hInstance = hInstance;
	  
	// �������� ����������� ����������� 
	wc.hIcon = LoadIcon(NULL, IDI_APPLICATION);
	
	// ������������ �������
	wc.hCursor = LoadCursor(NULL, IDC_ARROW);
	wc.hbrBackground = (HBRUSH) (COLOR_WINDOW);
	wc.lpszClassName = szAppName;

	// �������� ������� RegisterClassEx, ������� ���������
	// ����������� ������ ����
	if(!RegisterClassEx(&wc)){
		MessageBox(NULL, _T("RegisterClassEx - failed"), _T("Error"), MB_OK | MB_ICONERROR);
		return FALSE;
	}
    
	// ������� ������� ���� ���������� CreateWindowEx
	hWnd = CreateWindowEx(/*WS_EX_TRANSPARENT | */WS_EX_LAYERED, szAppName, szAppTitle, WS_VISIBLE | WS_POPUP, 
   		0, 0, INITIAL_WND_H, INITIAL_WND_H, NULL, NULL, hInstance, NULL);
	
	if(!hWnd){
		MessageBox(NULL, _T("CreateWindowEx - failed"), _T("Error"), MB_OK | MB_ICONERROR);
		return FALSE;
	}

	DragAcceptFiles(hWnd, TRUE);	// ��������� �������������� ������

	MessageBox(NULL, _T("��������� ��������������� ����������� ������� SetLayeredWindowAttributes: \n\n\
F1-F6 - ������������ ����� ������. \n\n\
����� ���������� � ���� 24 ������ bmp ���� (������ ������ ���� ����������)"), _T("���������"), MB_OK);

	// ���������� �������� ������������
	verify(SetLayeredWindowAttributes(hWnd, 0x0, 100, LWA_ALPHA));
	UpdateWindow(hWnd);

	SetTimer(hWnd, 1, 25, TimerProc);		// ������������� ������
	scrn_w = GetSystemMetrics(SM_CXSCREEN);	// ��������� �������������� ���������� ������

	// ��������� ���� ��������� ���������
	while(GetMessage(&msg, NULL, 0, 0)){
		TranslateMessage(&msg);
		DispatchMessage(&msg);
	}

	// ��������� ������ � GDI+
    GdiplusShutdown(gdiplusToken);

	return (int)msg.wParam;
}

// ������� �������� bmp ����� 
// ��� �������� ����������� ��������� 24 ������ �����������
int LoadBMP(HWND hWnd, const TCHAR* fn){
	int fl;		// ���������� ��������� *.bmp �����
	fl = _wopen(fn, _O_RDONLY);	// ��������� ���� ��� ������
	if(fl==-1){
		MessageBox(hWnd, _T("������ ����� �� ����������"), _T("������: �������� �����"), MB_OK | MB_ICONERROR);
		return -1;
	}
	
	BITMAPFILEHEADER fl_hdr;	// ��������� �����
	
	memset(&fl_hdr, 0x0, sizeof(BITMAPFILEHEADER));
	memset(&bmp_info, 0x0, sizeof(BITMAPINFO));

	// ������ ���������
	read(fl, &fl_hdr, sizeof(BITMAPFILEHEADER));

	// ���������
	if(fl_hdr.bfType != 0x4d42){        // 0x42 = "B" 0x4d = "M" 
		MessageBox(hWnd, _T("��� �� ��������"), _T("������: �������� ������ �����"), MB_OK | MB_ICONERROR);
		return -1;
	}

	// ������ ���������� � �������
	read(fl, &bmp_info, sizeof(BITMAPINFO));
	
	if(bmp_info.bmiHeader.biBitCount!=24){
		MessageBox(hWnd, _T("��� �� 24 ������ bmp"), _T("������: �������� ������ �����"), MB_OK | MB_ICONERROR);
		return -1;
	}

	bmp_h = bmp_info.bmiHeader.biHeight;
	bmp_w = bmp_info.bmiHeader.biWidth;
	
	if(bmp_cnt!=NULL) free(bmp_cnt);

	// �������� ����� ��� ����������
	bmp_cnt = (unsigned char*)malloc(bmp_h*bmp_w*3);
	// ������ ����������
	read(fl, bmp_cnt, bmp_h*bmp_w*3);

	// � ��������� ����
	close(fl);

	unsigned char buf;
	// RGB -> BGR ???
	unsigned int i(0);
	for(; i<bmp_h*bmp_w*3; i+=3){
		buf=bmp_cnt[i+2];
		bmp_cnt[i+2]=bmp_cnt[i];
		bmp_cnt[i]=buf;

		buf=bmp_cnt[i+1];
		bmp_cnt[i+1]=bmp_cnt[i+2];
		bmp_cnt[i+2]=buf;
	}

	return 0;
}


// ������ - ������� ���� �� ������
VOID CALLBACK TimerProc(HWND hWnd, UINT, UINT_PTR, DWORD){
	// ������ ��������� ����
	RECT r; GetWindowRect(hWnd, &r);

	// ����� (������� ���������)
	if(r.right+1>scrn_w || r.left<0){
		wnd_dx=-wnd_dx;
		DXPS=-DXPS;
	}
	r.left+=wnd_dx; r.right+=wnd_dx;

	// ������������� �����
	SetWindowPos(hWnd, NULL, r.left, r.top, r.right-r.left, r.bottom-r.top, SWP_SHOWWINDOW);

	// �������������� ����������
	RedrawWindow(hWnd, NULL, NULL, RDW_INVALIDATE | RDW_UPDATENOW | RDW_ERASE );
	
	dx+=DXPS;	// ���������� �������� (��� ���������� ����)
}


// ������� ���� (������������ ��������� ��. � windowsx.h)
LRESULT WINAPI WndProc(HWND hWnd, UINT msg, WPARAM wParam, LPARAM lParam)
{
	switch(msg)
	{
		HANDLE_MSG(hWnd, WM_SIZE, WndProc_OnSize);
		HANDLE_MSG(hWnd, WM_DESTROY, WndProc_OnDestroy);
		HANDLE_MSG(hWnd, WM_KEYDOWN, WndProc_OnKeyDown);
		HANDLE_MSG(hWnd, WM_PAINT, WndProc_OnPaint);
		HANDLE_MSG(hWnd, WM_DROPFILES, WndProc_OnDropFiles);

		default:
		return(DefWindowProc(hWnd, msg, wParam, lParam));
	}
}

// ������������ ������� ����
void WndProc_OnDropFiles(HWND hWnd, HDROP hDrop){
	TCHAR fn[0xff];	// ������ ��� ����� �����

	// ���������� ������
	UINT fl_cnt = DragQueryFile(hDrop, 0xFFFFFFFF, 0, 0); 

	if(fl_cnt!=1){
		MessageBox(NULL, _T("����� ���������� ������ ���� ���"), _T("������"), MB_OK);
		return;
	}

	DragQueryFile(hDrop, 0, fn, 0xff);	// �������� ��� �����
	// � ��������� ���
	if(LoadBMP(hWnd, fn)!=0) return;

	// � ���� ��� ��������� ������ ����� ����
	ChangeWndForm(hWnd, bWndForm = 6);

}

// ���������� �� ��������� �������� ���� 
void WndProc_OnSize(HWND hWnd, UINT state, int cx, int cy){

//		ChangeWndForm(hWnd, bWndForm);

	return FORWARD_WM_SIZE(hWnd, (WPARAM)(UINT)(state), cx, cy, DefWindowProc);
}

// ���������� �� �����������
void WndProc_OnDestroy(HWND hWnd){
	PostQuitMessage(0);	// ���������� ����� ��������� ���������
	return FORWARD_WM_DESTROY(hWnd, DefWindowProc);
}	

// ���������� �� ������� �������
void WndProc_OnKeyDown(HWND hWnd, UINT vk, BOOL, UINT cRepeat, UINT flags){
	// 0x6F - (��� ������� F1)-1;
	// �. �. ������� �� F1-F7 �� �������� �������� bWndForm �� 1 �� 7
	if(1<=vk - 0x6F && vk - 0x6F <=6)
		ChangeWndForm(hWnd, bWndForm = vk - 0x6F);

	return FORWARD_WM_KEYDOWN(hWnd, vk, cRepeat, flags, DefWindowProc);
}

// �-��� ��������������� ����� ����
void ChangeWndForm(HWND hWnd, UINT bWndForm){

	// ��������������
	if(bWndForm==1){
		SetWindowPos(hWnd, NULL, NULL, NULL, INITIAL_WND_W, INITIAL_WND_H, SWP_NOMOVE | SWP_SHOWWINDOW);
//		verify(SetLayeredWindowAttributes(hWnd, 0x0, 100, LWA_ALPHA));
		verify(SetLayeredWindowAttributes(hWnd, RGB(0xff, 0xff, 0xff), 0x80, LWA_ALPHA));

	}

	// ���������� "RSDN"
	if(bWndForm==2){
		SetWindowPos(hWnd, NULL, NULL, NULL, INITIAL_WND_W, INITIAL_WND_H, SWP_NOMOVE | SWP_SHOWWINDOW);
		verify(SetLayeredWindowAttributes(hWnd, 0x0, 0, LWA_COLORKEY));
	}

	// ���������� !"RSDN"
	if(bWndForm==3){
		SetWindowPos(hWnd, NULL, NULL, NULL, INITIAL_WND_W, INITIAL_WND_H, SWP_NOMOVE | SWP_SHOWWINDOW);
		SetLayeredWindowAttributes(hWnd, GetSysColor((IsThemeActive() ? COLOR_WINDOW : COLOR_3DFACE)), 0, LWA_COLORKEY);
	}

	// ����� ���� �� smoke.bmp ����� � ���������� ������ ������
	if(bWndForm==4){
		LoadBMP(hWnd, _T("smoke.bmp"));
		SetWindowPos(hWnd, NULL, NULL, NULL, bmp_info.bmiHeader.biWidth, bmp_info.bmiHeader.biHeight, SWP_NOMOVE | SWP_SHOWWINDOW);
		SetLayeredWindowAttributes(hWnd, 0x0, 0, LWA_COLORKEY);
	}

	// "������" ������ � ������ (� ��� �� ��� � ���������� �������� ����������), 
	// � ����� ������� � ���� (��� ���������� ��������)
	if(bWndForm==5){
		// ������������� ����������� �������� ��� bmp_info
		memset(&bmp_info, 0x0, sizeof(bmp_info));
		
		bmp_info.bmiHeader.biSize = sizeof(bmp_info);
		bmp_info.bmiHeader.biBitCount=8;	// ������� ������ ������
		bmp_info.bmiHeader.biCompression = BI_RGB;
		bmp_info.bmiHeader.biWidth=INITIAL_WND_W;
		bmp_info.bmiHeader.biHeight=INITIAL_WND_H;
		bmp_info.bmiHeader.biPlanes=1;

		if(bmp_cnt!=NULL) free(bmp_cnt);
		bmp_cnt = (unsigned char*)malloc(INITIAL_WND_W*INITIAL_WND_H);

		SetWindowPos(hWnd, NULL, NULL, NULL, INITIAL_WND_W, INITIAL_WND_H, SWP_NOMOVE | SWP_SHOWWINDOW);
		SetLayeredWindowAttributes(hWnd, 0x0, 0xFF, LWA_COLORKEY);
	}

	// GDI+ demo
	if(bWndForm==6){
		SetWindowPos(hWnd, NULL, NULL, NULL, bmp_info.bmiHeader.biWidth, bmp_info.bmiHeader.biHeight, SWP_NOMOVE | SWP_SHOWWINDOW);
		SetLayeredWindowAttributes(hWnd, GetSysColor((IsThemeActive() ? COLOR_WINDOW : COLOR_3DFACE)), 0xFF, LWA_COLORKEY);
	}


	// ��� ��������������
	if(bWndForm==7){
		SetWindowPos(hWnd, NULL, NULL, NULL, bmp_info.bmiHeader.biWidth, bmp_info.bmiHeader.biHeight, SWP_NOMOVE | SWP_SHOWWINDOW);
		SetLayeredWindowAttributes(hWnd, 0x0, 0, LWA_COLORKEY);
	}
	
	WndProc_OnPaint(hWnd);
}

// ����������� ����
void WndProc_OnPaint(HWND hWnd){
	HDC hdc; PAINTSTRUCT ps;
	hdc = BeginPaint(hWnd, &ps);
	assert(hdc);
	
	// ����� ������ "RSDN"
	if(bWndForm==3||bWndForm==2||bWndForm==1||bWndForm==0){
		// ������� ��������� ���� (���������� �����)
		RECT r; GetClientRect(hWnd, &r);

		// ������� �����
		HFONT font = CreateFont(90, 30, 0, 0, 150, 0, 0, 0, ANSI_CHARSET, OUT_DEVICE_PRECIS, 
			CLIP_DEFAULT_PRECIS, ANTIALIASED_QUALITY, DEFAULT_PITCH, _T("Arial"));
		assert(font);
		// �������� �����
		SelectObject(hdc, font);
		// � �������
		verify(DeleteFont(font));

		// ����� � �������������� ������
		SetBkMode(hdc, TRANSPARENT);
		DrawText(hdc, _T("RSDN"), 4, &r, DT_VCENTER | DT_CENTER | DT_SINGLELINE );
	}

	// ������ � ������... ��������� ("������")
	if(bWndForm==5){
 		memset(bmp_cnt, 0x0, INITIAL_WND_W*INITIAL_WND_H);

		for(unsigned int i(0); i<INITIAL_WND_H; i++){
			/*������*/bmp_cnt[INITIAL_WND_W*(/*�������������*/INITIAL_WND_H/2+
			(unsigned int)(/*������ (���������������)*/INITIAL_WND_H*0.12*sin/*������*/(i*fabs(DXPS)+dx/*��������*/)))+i]=127;
		}

		// "����" -> "����������"
		SetDIBitsToDevice(hdc, 0, 0, INITIAL_WND_W, INITIAL_WND_H, 
			0, 0, 0, INITIAL_WND_H, bmp_cnt, &bmp_info, DIB_PAL_COLORS);
	}

	// ����������� ����������� �����
	if(bWndForm==4||bWndForm==7){
		SetDIBitsToDevice(hdc, 0, 0, bmp_info.bmiHeader.biWidth, bmp_info.bmiHeader.biHeight, 
			0, 0, 0, bmp_info.bmiHeader.biHeight, bmp_cnt, &bmp_info, DIB_PAL_COLORS);
	}

	// ����� � ���������� GDI+
	if(bWndForm==6){
		RECT rt;
		GetClientRect(hWnd, &rt);
		Rect rect (rt.left, rt.top, rt.right, rt.right);

		// ��������� ����������� ������
		Graphics graphics(hdc); 

		// ������
		LinearGradientBrush linGrBrush1(rect, 
			Color(0, 0, 0, 0), 
			Color(255, 0, 0, 255),
            LinearGradientModeHorizontal);
		
		graphics.FillEllipse(&linGrBrush1, rt.left, rt.top, rt.right, rt.bottom);
	}

	// �����  ���������
	EndPaint(hWnd, &ps);
}