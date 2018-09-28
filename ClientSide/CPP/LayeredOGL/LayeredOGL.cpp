#define _WIN32_WINNT 0x0500		// ��� ������������� ������� SetLayeredWindowAttributes �
// ��������� � ��� ��������

// Windows
#include <windows.h>
#include <windowsx.h>

// GL
#include <GL/gl.h>
#include <GL/glu.h>
#include <GL/glaux.h>

#pragma comment (lib, "opengl32.lib")
#pragma comment (lib, "glu32.lib")
#pragma comment (lib, "glaux.lib")


// trackball
#include "trackball.h"

// CRT
#include <math.h>
#include <assert.h>
#include <tchar.h>

#ifdef  assert
#define	verify(expr) if(!expr) assert(0)
#else verify(expr) expr
#endif

// ������ GLU
GLUquadricObj *quad_obj;
#define QUAD_OBJ_INIT() { if(!quad_obj) quad_obj = gluNewQuadric(); }

const TCHAR szAppName[]=_T("CoolGL");	// �������� ����������
const TCHAR wcWndName[]=_T("WS_EX_LAYERED � OpenGL");		// ����

HDC hDC;			// �������� ���������� �������� ����
HGLRC m_hrc;		// OpenGL �������� �������� ����
int w(300), h(200);	// ������� ����

// ���������� ��� �������� ����� "�����������" ���������
float lx(0), ly(0), cx(0), cy(0), q[4]={3, 3, 3, 1}, m[4][4], buf_q[4]={0, 0, 0, 0};


HDC pdcDIB;					// �������� ���������� � ������
HBITMAP hbmpDIB;			// � ��� ������� ������
void *bmp_cnt(NULL);		// ���������� �������
int cxDIB(0); int cyDIB(0);	// ��� �������
BITMAPINFOHEADER BIH;		// � ���������

void CreateDIB(int, int);	// ������� DIB section � ���������������� ���������
BOOL CreateHGLRC();			// ������� OpenGK ��������

BOOL initSC();				// ������������� �����
BOOL renderSC();			// ���������
void resizeSC(int, int);	// ��������� �������� �����
void draw();				// ��������� �� ��������� ����������

// ������������� �����
BOOL initSC(){
	// ��������� ��������� �����
	float pos[4] = {3,3,3,1};
	
	glEnable(GL_ALPHA_TEST);		// ���� ����
	glEnable(GL_DEPTH_TEST);		// �������
	glEnable(GL_COLOR_MATERIAL);
	
	glEnable(GL_LIGHTING);			// ��������� ���������
	glEnable(GL_LIGHT0);			// ��� ���� GL_LIGHT0 ����������
	
	glEnable(GL_BLEND);				// ���������
	// �� ���� �������
	glBlendFunc(GL_SRC_ALPHA, GL_ONE_MINUS_SRC_ALPHA);

	// ��������� �����
	glLightfv(GL_LIGHT0, GL_POSITION, pos);

	// ���� ������� �������
	glClearColor(0, 0, 0, 0);

	return 0;
}

// ��� �����, �� ��������
BOOL renderSC(){
	glClear(GL_COLOR_BUFFER_BIT | GL_DEPTH_BUFFER_BIT );

	glPushMatrix();

	build_rotmatrix(m, q);
	glMultMatrixf(&m[0][0]);

	glColor3f(0, 1, 1);
	auxSolidTeapot(0.5);

	glPopMatrix();

	// ���������� glFlush ������ SwapBuffers
	glFlush();

	return 0;
}


// ������� DIB section
void CreateDIB(int cx, int cy)
{
	assert(cx > 0); 
	assert(cy > 0);

	// ��������� ������
	cxDIB = cx ;
	cyDIB = cy ;

	// ������� ��������� BITMAPINFOHEADER ����������� ��� DIB
	int iSize = sizeof(BITMAPINFOHEADER);	// ������
	memset(&BIH, 0, iSize);

	BIH.biSize = iSize;	// ������ ���������
	BIH.biWidth = cx;	// ���������
	BIH.biHeight = cy;	// �������
	BIH.biPlanes = 1;	// ���� ����
	BIH.biBitCount = 24;	// 24 bits per pixel
	BIH.biCompression = BI_RGB;	// ��� ������

	// ������� ����� DC � ������
	if(pdcDIB) verify(DeleteDC(pdcDIB));
	pdcDIB = CreateCompatibleDC(NULL);
	assert(pdcDIB);

	// ������� DIB section
	if(hbmpDIB) verify(DeleteObject(hbmpDIB));
	hbmpDIB = CreateDIBSection(
		pdcDIB,			// �������� ����������
		(BITMAPINFO*)&BIH,	// ���������� � �������
		DIB_RGB_COLORS,		// ��������� �����
		&bmp_cnt,		// �������������� ������� (������ �������� �������)
		NULL,	// �� ������������� � ������������ � ������ ������
		0);
    
	assert(hbmpDIB);
    assert(bmp_cnt);

    // ������� ����� ������ ��� ��������� ���������� � ������
    if(hbmpDIB)
        SelectObject(pdcDIB, hbmpDIB);

}

// DIB -> hDC
void draw(HDC pdcDest){

	assert(pdcDIB);
	// ����� ���
    verify(BitBlt(pdcDest, 0, 0, w, h, pdcDIB,
		0, 0, SRCCOPY));

	// � ����� � ��� (����������������)
//	BITMAPINFO bmp_info;
//	memset(&bmp_info, 0x0, sizeof(bmp_info));
//	bmp_info.bmiHeader=BIH;
//	verify(SetDIBitsToDevice(pdcDest, 0, 0, w, h, 
//			0, 0, 0, h, bmp_cnt, &bmp_info, DIB_RGB_COLORS));

}

// ������� �������� OpenGL
BOOL CreateHGLRC(){
	// ��� ������� � ����� ������� ������� (����� ������������ �������)
	// ���� - PFD_DRAW_TO_BITMAP �.�. ����������, ��� ����
	// �������� "� ��������"
	DWORD dwFlags=PFD_SUPPORT_OPENGL | PFD_DRAW_TO_BITMAP;

	// �������� �������������� ������� PIXELFORMATDESCRIPTOR

	PIXELFORMATDESCRIPTOR pfd ;
	memset(&pfd,0, sizeof(PIXELFORMATDESCRIPTOR)) ;
	pfd.nSize = sizeof(PIXELFORMATDESCRIPTOR); // ������
	pfd.nVersion = 1;                       // ����� ������
	pfd.dwFlags =  dwFlags ;				// ����� (��. ����)
	pfd.iPixelType = PFD_TYPE_RGBA ;		// ��� �������
	pfd.cColorBits = 24 ;					// 24 ���� ���������
	pfd.cDepthBits = 32 ;					// 32-������ ������ �������
	pfd.iLayerType = PFD_MAIN_PLANE ;       // ��� ����

   // ������� ��� ������ ��������� ������ ������ �������
   int nPixelFormat = ChoosePixelFormat(pdcDIB, &pfd);
   if (nPixelFormat == 0){
      assert(0);
      return FALSE ;
   }

   // ��������� ���
   BOOL bResult = SetPixelFormat(pdcDIB, nPixelFormat, &pfd);
   if (bResult==FALSE){
      assert(0);
      return FALSE ;
   }

   // ���������� �������� ��� ����������
   m_hrc = wglCreateContext(pdcDIB);
   if (!m_hrc){
      assert(0);
      return FALSE;
   }

   return TRUE;
}

// ������ ������������� ��� ������� � ��������� (��� demo ����������)
// � Viewport �� ��� ����
void resizeSC(int width,int height){
	glViewport(0,0,width,height);
	glMatrixMode(GL_PROJECTION);
	glLoadIdentity();

	glMatrixMode(GL_MODELVIEW );
	glLoadIdentity();
}

// ...
LRESULT CALLBACK WindowFunc(HWND hWnd,UINT msg, WPARAM wParam, LPARAM lParam){
	PAINTSTRUCT ps;
	unsigned int i;

	switch(msg) {
		case WM_ERASEBKGND:
			return 0;	// ��������
		break;

		// ��������
		case WM_CREATE:
			QUAD_OBJ_INIT(); // ������������� GLU
			trackball(q, -0.0, 0.0, 0.0, 0.0); // ������������� ��������
		break;

		// �����������
		case WM_DESTROY:
			if(m_hrc){
				wglMakeCurrent(NULL, NULL);
				wglDeleteContext(m_hrc) ;
			}
			PostQuitMessage(0) ;
		break;

		// �����������
		case WM_PAINT:
			hDC = BeginPaint(hWnd, &ps);
			renderSC();	// OpenGL -> DIB
            draw(hDC);	// DIB -> hDC
			EndPaint(hWnd, &ps);
		break;

		// ��� ��������� ������� ����
		case WM_SIZE:
			// �������� �������������� ����������
			w = LOWORD(lParam); h = HIWORD(lParam);
			
			// ������� ����������� GL ��������
			wglMakeCurrent(NULL, NULL);
			wglDeleteContext(m_hrc);

			// � ������� ����� DIB ��� ������ ������� ����
			CreateDIB(w, h);
			// ����� GL ��������
			CreateHGLRC();
			// � ������ ��� �������
			verify(wglMakeCurrent(pdcDIB, m_hrc));

			initSC();
			resizeSC(w, h);
			
			renderSC();
		break;

		// ������� ��������������
		case WM_MOUSEMOVE:
		
			lx=cx; ly=cy;
			cx = 2.0*(((float)GET_X_LPARAM(lParam)/w)-0.5); 
			cy = 2.0*(((float)GET_Y_LPARAM(lParam)/h)-0.5);

			if(wParam==MK_LBUTTON){

				trackball(buf_q, -lx, ly, -cx, cy);
				add_quats(buf_q, q, q);

				RedrawWindow(hWnd, NULL, NULL, RDW_INVALIDATE | RDW_UPDATENOW | RDW_ERASE );
			}
			
		break;
		default: return DefWindowProc(hWnd,msg,wParam,lParam);
	}

	return 0;
}

// WinMain
int WINAPI _tWinMain(HINSTANCE hThisInst, HINSTANCE hPrevInst, LPSTR str,int nWinMode){
	MSG msg;
	HWND hWnd;
	WNDCLASSEX wc;			// ����� ����

		// ���������, �� ���� �� ��� ���������� �������� �����
	hWnd = FindWindow(szAppName, NULL);
	if(hWnd){
  		// ���� ���� ���������� ���� �������� � �����������,
		// ��������������� ���
		if(IsIconic(hWnd)) ShowWindow(hWnd, SW_RESTORE);

		// ��������� ���� ���������� �� �������� ����
		SetForegroundWindow(hWnd);
		
		MessageBox(NULL, _T("���������� ��� ���������"), _T("Warning"), MB_OK | MB_ICONEXCLAMATION);
		return FALSE;
	}

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
	wc.lpfnWndProc = (WNDPROC)WindowFunc;
	wc.cbClsExtra  = 0;
	wc.cbWndExtra  = 0;
	wc.hInstance = hThisInst;
	  
	// �������� ����������� ����������� 
	wc.hIcon = LoadIcon(NULL, IDI_APPLICATION);
	// ������������ �������
	wc.hCursor = LoadCursor(NULL, IDC_ARROW);
	wc.hbrBackground = (HBRUSH) (COLOR_WINDOW);
	wc.lpszClassName = szAppName;

	if(!RegisterClassEx(&wc)){
		MessageBox(NULL, _T("RegisterClassEx - failed"), _T("Error"), MB_OK | MB_ICONERROR);
		return FALSE;
	}


	hWnd = CreateWindowEx(WS_EX_LAYERED, szAppName, wcWndName,
					WS_VISIBLE | WS_POPUP, 200, 150, w, h,
					NULL, NULL, hThisInst, NULL);


	if(!hWnd){
		MessageBox(NULL, _T("CreateWindowEx - failed"), _T("Error"), MB_OK | MB_ICONERROR);
		return FALSE;
	}

	MessageBox(hWnd, _T("Demo: SetLayeredWindowAttributes + OpenGL\n\n\
����� ������ ���� + ����������� = �������� �����"), _T("���������"), MB_OK);



	verify(SetLayeredWindowAttributes(hWnd, 0x0, 0, LWA_COLORKEY));

	

	// ��������� ���� ��������� ���������
	while(1) 
	{
		while (PeekMessage(&msg,NULL,0,0,PM_NOREMOVE)){
			if (GetMessage(&msg, NULL, 0, 0))
			{
				TranslateMessage(&msg);
				DispatchMessage(&msg);
			}
			else return 0;
		}
	} 

	return (FALSE); 
}