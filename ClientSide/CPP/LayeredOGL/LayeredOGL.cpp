#define _WIN32_WINNT 0x0500		// для использования функции SetLayeredWindowAttributes и
// связанных с ней констант

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

// объект GLU
GLUquadricObj *quad_obj;
#define QUAD_OBJ_INIT() { if(!quad_obj) quad_obj = gluNewQuadric(); }

const TCHAR szAppName[]=_T("CoolGL");	// название приложения
const TCHAR wcWndName[]=_T("WS_EX_LAYERED и OpenGL");		// окна

HDC hDC;			// контекст устройства текущего окна
HGLRC m_hrc;		// OpenGL контекст текущего окна
int w(300), h(200);	// размеры окна

// информация для вращения сцены "виртуальным" трекболом
float lx(0), ly(0), cx(0), cy(0), q[4]={3, 3, 3, 1}, m[4][4], buf_q[4]={0, 0, 0, 0};


HDC pdcDIB;					// контекст устройства в памяти
HBITMAP hbmpDIB;			// и его текущий битмап
void *bmp_cnt(NULL);		// содержимое битмапа
int cxDIB(0); int cyDIB(0);	// его размеры
BITMAPINFOHEADER BIH;		// и заголовок

void CreateDIB(int, int);	// создаем DIB section с соответствующими размерами
BOOL CreateHGLRC();			// создаем OpenGK контекст

BOOL initSC();				// инициализация сцены
BOOL renderSC();			// рендеринг
void resizeSC(int, int);	// изменение размеров сцены
void draw();				// рисование на контексте устройства

// инициализация сцены
BOOL initSC(){
	// положение источника света
	float pos[4] = {3,3,3,1};
	
	glEnable(GL_ALPHA_TEST);		// алфа тест
	glEnable(GL_DEPTH_TEST);		// глубины
	glEnable(GL_COLOR_MATERIAL);
	
	glEnable(GL_LIGHTING);			// разрешаем освещение
	glEnable(GL_LIGHT0);			// вот этим GL_LIGHT0 источником
	
	glEnable(GL_BLEND);				// смешиваем
	// по этой формуле
	glBlendFunc(GL_SRC_ALPHA, GL_ONE_MINUS_SRC_ALPHA);

	// параметры света
	glLightfv(GL_LIGHT0, GL_POSITION, pos);

	// цвет очистки буффера
	glClearColor(0, 0, 0, 0);

	return 0;
}

// что хотим, то рендерим
BOOL renderSC(){
	glClear(GL_COLOR_BUFFER_BIT | GL_DEPTH_BUFFER_BIT );

	glPushMatrix();

	build_rotmatrix(m, q);
	glMultMatrixf(&m[0][0]);

	glColor3f(0, 1, 1);
	auxSolidTeapot(0.5);

	glPopMatrix();

	// используем glFlush вместо SwapBuffers
	glFlush();

	return 0;
}


// создаем DIB section
void CreateDIB(int cx, int cy)
{
	assert(cx > 0); 
	assert(cy > 0);

	// сохраняем размер
	cxDIB = cx ;
	cyDIB = cy ;

	// создаем структуру BITMAPINFOHEADER описывающую наш DIB
	int iSize = sizeof(BITMAPINFOHEADER);	// размер
	memset(&BIH, 0, iSize);

	BIH.biSize = iSize;	// размер структуры
	BIH.biWidth = cx;	// геометрия
	BIH.biHeight = cy;	// битмапа
	BIH.biPlanes = 1;	// один план
	BIH.biBitCount = 24;	// 24 bits per pixel
	BIH.biCompression = BI_RGB;	// без сжатия

	// создаем новый DC в памяти
	if(pdcDIB) verify(DeleteDC(pdcDIB));
	pdcDIB = CreateCompatibleDC(NULL);
	assert(pdcDIB);

	// создаем DIB section
	if(hbmpDIB) verify(DeleteObject(hbmpDIB));
	hbmpDIB = CreateDIBSection(
		pdcDIB,			// контекст ускройства
		(BITMAPINFO*)&BIH,	// информация о битмапе
		DIB_RGB_COLORS,		// параметры цвета
		&bmp_cnt,		// местоположение буффера (память выделяет система)
		NULL,	// не привязываемся к отображаемым в память файлам
		0);
    
	assert(hbmpDIB);
    assert(bmp_cnt);

    // выберем новый битмап для контекста устройства в памяти
    if(hbmpDIB)
        SelectObject(pdcDIB, hbmpDIB);

}

// DIB -> hDC
void draw(HDC pdcDest){

	assert(pdcDIB);
	// можно так
    verify(BitBlt(pdcDest, 0, 0, w, h, pdcDIB,
		0, 0, SRCCOPY));

	// а можно и так (расскоментируйте)
//	BITMAPINFO bmp_info;
//	memset(&bmp_info, 0x0, sizeof(bmp_info));
//	bmp_info.bmiHeader=BIH;
//	verify(SetDIBitsToDevice(pdcDest, 0, 0, w, h, 
//			0, 0, 0, h, bmp_cnt, &bmp_info, DIB_RGB_COLORS));

}

// создаем контекст OpenGL
BOOL CreateHGLRC(){
	// вот пожалуй и самое главное отличие (после расположения буффера)
	// флаг - PFD_DRAW_TO_BITMAP т.е. показываем, что надо
	// рисовать "в картинку"
	DWORD dwFlags=PFD_SUPPORT_OPENGL | PFD_DRAW_TO_BITMAP;

	// заполним соответсвующим образом PIXELFORMATDESCRIPTOR

	PIXELFORMATDESCRIPTOR pfd ;
	memset(&pfd,0, sizeof(PIXELFORMATDESCRIPTOR)) ;
	pfd.nSize = sizeof(PIXELFORMATDESCRIPTOR); // размер
	pfd.nVersion = 1;                       // номер версии
	pfd.dwFlags =  dwFlags ;				// флаги (см. выше)
	pfd.iPixelType = PFD_TYPE_RGBA ;		// тип пикселя
	pfd.cColorBits = 24 ;					// 24 бита напиксель
	pfd.cDepthBits = 32 ;					// 32-битный буффер глубины
	pfd.iLayerType = PFD_MAIN_PLANE ;       // тип слоя

   // выберем для нашего контекста данный формат пикселя
   int nPixelFormat = ChoosePixelFormat(pdcDIB, &pfd);
   if (nPixelFormat == 0){
      assert(0);
      return FALSE ;
   }

   // установим его
   BOOL bResult = SetPixelFormat(pdcDIB, nPixelFormat, &pfd);
   if (bResult==FALSE){
      assert(0);
      return FALSE ;
   }

   // собственно контекст для рендеринга
   m_hrc = wglCreateContext(pdcDIB);
   if (!m_hrc){
      assert(0);
      return FALSE;
   }

   return TRUE;
}

// просто устанавливаем все матрицы в единичные (для demo достаточно)
// и Viewport на все окно
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
			return 0;	// заглушка
		break;

		// создание
		case WM_CREATE:
			QUAD_OBJ_INIT(); // инициализация GLU
			trackball(q, -0.0, 0.0, 0.0, 0.0); // инициализация трекбола
		break;

		// уничтожение
		case WM_DESTROY:
			if(m_hrc){
				wglMakeCurrent(NULL, NULL);
				wglDeleteContext(m_hrc) ;
			}
			PostQuitMessage(0) ;
		break;

		// перерисовка
		case WM_PAINT:
			hDC = BeginPaint(hWnd, &ps);
			renderSC();	// OpenGL -> DIB
            draw(hDC);	// DIB -> hDC
			EndPaint(hWnd, &ps);
		break;

		// при изменении размера окна
		case WM_SIZE:
			// изменяем соответсвующие переменные
			w = LOWORD(lParam); h = HIWORD(lParam);
			
			// убиваем предъидущий GL контекст
			wglMakeCurrent(NULL, NULL);
			wglDeleteContext(m_hrc);

			// и создаем новый DIB для нового размера окна
			CreateDIB(w, h);
			// новый GL контекст
			CreateHGLRC();
			// и делаем его текущим
			verify(wglMakeCurrent(pdcDIB, m_hrc));

			initSC();
			resizeSC(w, h);
			
			renderSC();
		break;

		// трекбол преобразование
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
	WNDCLASSEX wc;			// класс окна

		// Проверяем, не было ли это приложение запущено ранее
	hWnd = FindWindow(szAppName, NULL);
	if(hWnd){
  		// Если окно приложения было свернуто в пиктограмму,
		// восстанавливаем его
		if(IsIconic(hWnd)) ShowWindow(hWnd, SW_RESTORE);

		// Выдвигаем окно приложения на передний план
		SetForegroundWindow(hWnd);
		
		MessageBox(NULL, _T("Приложение уже запущенно"), _T("Warning"), MB_OK | MB_ICONEXCLAMATION);
		return FALSE;
	}

	// Регистрируем класс окна
	memset(&wc, 0, sizeof(wc));
  
	//	Поля wc.cbSize	и wc.hIconSm определены в структуре
	//	WNDCLASSEX, которой можно пользоваться для
	//	регистрации класса окна в Win95
	wc.cbSize = sizeof(WNDCLASSEX);

	// загрузка одной из стандартных иконок
	wc.hIconSm = LoadIcon(NULL, IDI_APPLICATION);
  
	// заполнение структуры WNDCLASSEX
	wc.style = CS_HREDRAW | CS_VREDRAW;
	wc.lpfnWndProc = (WNDPROC)WindowFunc;
	wc.cbClsExtra  = 0;
	wc.cbWndExtra  = 0;
	wc.hInstance = hThisInst;
	  
	// загрузка стандартной пиктограммы 
	wc.hIcon = LoadIcon(NULL, IDI_APPLICATION);
	// стандартного курсора
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
левая кнопка мыши + перемещение = вращение сцены"), _T("Сообщение"), MB_OK);



	verify(SetLayeredWindowAttributes(hWnd, 0x0, 0, LWA_COLORKEY));

	

	// запускаем цикл обработки сообщений
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