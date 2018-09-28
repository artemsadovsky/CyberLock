// Программа демонстрирующая использование функции 
// SetLayeredWindowAttributes

// примечания: WinAPI, unicode, w2k, WinXP
// автор: Сапронов Андрей Юрьевич, duan@bk.ru

#define _WIN32_WINNT 0x0500		// для использования функции SetLayeredWindowAttributes и
								// связанных с ней констант
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

// первоначальные значения размеров окна
#define		INITIAL_WND_W	200
#define		INITIAL_WND_H	200

// глобальные переменные
long scrn_w;						// ширина экрана
int wnd_dx=1;						// горизонтальное смещение окна

double dx=0;						// горизонтальное смещение
double DXPS=0.04908738521234051;	// приращение

UINT bWndForm(1);					// текущая форма окна

unsigned char* bmp_cnt = NULL;		// содержимое файла fl
unsigned int bmp_h, bmp_w;			// высота и ширина картинки
BITMAPINFO bmp_info;				// информация о битмапе

const TCHAR szAppName[] = TEXT("LayeredWnds");	// Название приложения
const TCHAR szAppTitle[] = TEXT("LayeredWnds");	// Заголовок главного окна приложения


// функции - обработчики сообщений:
VOID CALLBACK TimerProc(HWND hWnd, UINT uMsg, UINT_PTR idEvent, DWORD dwTime); // таймера
void WndProc_OnPaint(HWND hWnd); // WM_PAINT
void WndProc_OnSize(HWND hWnd, UINT state, int cx, int cy);// WM_SIZE
void WndProc_OnDestroy(HWND hWnd); // WM_DESTROY
void WndProc_OnKeyDown(HWND hWnd, UINT vk, BOOL, UINT cRepeat, UINT flags); // WM_KEYDOWN
void WndProc_OnDropFiles(HWND hWnd, HDROP hDrop); 

// Функция главного окна
LRESULT WINAPI WndProc(HWND hWnd, UINT msg, WPARAM wParam, LPARAM lParam);

// функция изменения формы главного окна
void ChangeWndForm(HWND hWnd, UINT bWndForm);

// загрузка картинки из файла
int LoadBMP(HWND hWnd /*может быть NULL*/, const TCHAR* fn /* имя файла */);

// WinMain
int WINAPI _tWinMain(HINSTANCE hInstance, HINSTANCE, LPSTR, int){


	HWND hWnd;					// хэндл главного окна
	WNDCLASSEX wc;				// класс окна
	MSG msg;					// "сообщение"


	// структуры GDI+ 
    GdiplusStartupInput gdiplusStartupInput;
    ULONG_PTR gdiplusToken;

    // инициализация GDI+
    GdiplusStartup(&gdiplusToken, &gdiplusStartupInput, NULL);

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
	wc.lpfnWndProc = (WNDPROC)WndProc;
	wc.cbClsExtra  = 0;
	wc.cbWndExtra  = 0;
	wc.hInstance = hInstance;
	  
	// загрузка стандартной пиктограммы 
	wc.hIcon = LoadIcon(NULL, IDI_APPLICATION);
	
	// стандартного курсора
	wc.hCursor = LoadCursor(NULL, IDC_ARROW);
	wc.hbrBackground = (HBRUSH) (COLOR_WINDOW);
	wc.lpszClassName = szAppName;

	// Вызываем функцию RegisterClassEx, которая выполняет
	// регистрацию класса окна
	if(!RegisterClassEx(&wc)){
		MessageBox(NULL, _T("RegisterClassEx - failed"), _T("Error"), MB_OK | MB_ICONERROR);
		return FALSE;
	}
    
	// Создаем главное окно приложения CreateWindowEx
	hWnd = CreateWindowEx(/*WS_EX_TRANSPARENT | */WS_EX_LAYERED, szAppName, szAppTitle, WS_VISIBLE | WS_POPUP, 
   		0, 0, INITIAL_WND_H, INITIAL_WND_H, NULL, NULL, hInstance, NULL);
	
	if(!hWnd){
		MessageBox(NULL, _T("CreateWindowEx - failed"), _T("Error"), MB_OK | MB_ICONERROR);
		return FALSE;
	}

	DragAcceptFiles(hWnd, TRUE);	// разрешаем перетаскивание файлов

	MessageBox(NULL, _T("Программа демонстрирующая возможности функции SetLayeredWindowAttributes: \n\n\
F1-F6 - переключение между окнами. \n\n\
Можно перетащить в окно 24 битный bmp файл (считая черный цвет прозрачным)"), _T("Сообщение"), MB_OK);

	// собственно атрибуты прозрачности
	verify(SetLayeredWindowAttributes(hWnd, 0x0, 100, LWA_ALPHA));
	UpdateWindow(hWnd);

	SetTimer(hWnd, 1, 25, TimerProc);		// устанавливаем таймер
	scrn_w = GetSystemMetrics(SM_CXSCREEN);	// сохраняем горизонтальное разрешение экрана

	// Запускаем цикл обработки сообщений
	while(GetMessage(&msg, NULL, 0, 0)){
		TranslateMessage(&msg);
		DispatchMessage(&msg);
	}

	// окончание работы с GDI+
    GdiplusShutdown(gdiplusToken);

	return (int)msg.wParam;
}

// функция загрузки bmp файла 
// для простоты ограничимся загрузкрй 24 битных изображений
int LoadBMP(HWND hWnd, const TCHAR* fn){
	int fl;		// дескриптор открытого *.bmp файла
	fl = _wopen(fn, _O_RDONLY);	// открываем файл для чтения
	if(fl==-1){
		MessageBox(hWnd, _T("Такого файла не существует"), _T("Ошибка: открытие файла"), MB_OK | MB_ICONERROR);
		return -1;
	}
	
	BITMAPFILEHEADER fl_hdr;	// заголовок файла
	
	memset(&fl_hdr, 0x0, sizeof(BITMAPFILEHEADER));
	memset(&bmp_info, 0x0, sizeof(BITMAPINFO));

	// читаем заголовок
	read(fl, &fl_hdr, sizeof(BITMAPFILEHEADER));

	// проверяем
	if(fl_hdr.bfType != 0x4d42){        // 0x42 = "B" 0x4d = "M" 
		MessageBox(hWnd, _T("Это не картинка"), _T("Ошибка: неверный формат файла"), MB_OK | MB_ICONERROR);
		return -1;
	}

	// читаем информацию о битмапе
	read(fl, &bmp_info, sizeof(BITMAPINFO));
	
	if(bmp_info.bmiHeader.biBitCount!=24){
		MessageBox(hWnd, _T("Это не 24 битный bmp"), _T("Ошибка: неверный формат файла"), MB_OK | MB_ICONERROR);
		return -1;
	}

	bmp_h = bmp_info.bmiHeader.biHeight;
	bmp_w = bmp_info.bmiHeader.biWidth;
	
	if(bmp_cnt!=NULL) free(bmp_cnt);

	// выделяем место под содержимое
	bmp_cnt = (unsigned char*)malloc(bmp_h*bmp_w*3);
	// читаем содержимое
	read(fl, bmp_cnt, bmp_h*bmp_w*3);

	// и закрываем файл
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


// таймер - двигает окно по экрану
VOID CALLBACK TimerProc(HWND hWnd, UINT, UINT_PTR, DWORD){
	// старое положение окна
	RECT r; GetWindowRect(hWnd, &r);

	// новое (немного смещенное)
	if(r.right+1>scrn_w || r.left<0){
		wnd_dx=-wnd_dx;
		DXPS=-DXPS;
	}
	r.left+=wnd_dx; r.right+=wnd_dx;

	// устанавливаем новое
	SetWindowPos(hWnd, NULL, r.left, r.top, r.right-r.left, r.bottom-r.top, SWP_SHOWWINDOW);

	// перерисовываем содержимое
	RedrawWindow(hWnd, NULL, NULL, RDW_INVALIDATE | RDW_UPDATENOW | RDW_ERASE );
	
	dx+=DXPS;	// прибавлаем смещение (для следующего раза)
}


// функция окна (распаковщики сообщений см. в windowsx.h)
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

// пользлватель закинул файл
void WndProc_OnDropFiles(HWND hWnd, HDROP hDrop){
	TCHAR fn[0xff];	// буффер для имени файла

	// количество файлов
	UINT fl_cnt = DragQueryFile(hDrop, 0xFFFFFFFF, 0, 0); 

	if(fl_cnt!=1){
		MessageBox(NULL, _T("Можно перетащить только один фйл"), _T("Ошибка"), MB_OK);
		return;
	}

	DragQueryFile(hDrop, 0, fn, 0xff);	// получаем имя файла
	// и загружаем его
	if(LoadBMP(hWnd, fn)!=0) return;

	// и если все нормально меняем форму окна
	ChangeWndForm(hWnd, bWndForm = 6);

}

// обработчик на изменение размеров окна 
void WndProc_OnSize(HWND hWnd, UINT state, int cx, int cy){

//		ChangeWndForm(hWnd, bWndForm);

	return FORWARD_WM_SIZE(hWnd, (WPARAM)(UINT)(state), cx, cy, DefWindowProc);
}

// обработчик на уничтожение
void WndProc_OnDestroy(HWND hWnd){
	PostQuitMessage(0);	// завершение цикла обработки сообщений
	return FORWARD_WM_DESTROY(hWnd, DefWindowProc);
}	

// обработчик на нажатие клавиши
void WndProc_OnKeyDown(HWND hWnd, UINT vk, BOOL, UINT cRepeat, UINT flags){
	// 0x6F - (код клавиши F1)-1;
	// т. о. нажимая на F1-F7 мы получаем значение bWndForm от 1 до 7
	if(1<=vk - 0x6F && vk - 0x6F <=6)
		ChangeWndForm(hWnd, bWndForm = vk - 0x6F);

	return FORWARD_WM_KEYDOWN(hWnd, vk, cRepeat, flags, DefWindowProc);
}

// ф-ция устанавливающая форму окна
void ChangeWndForm(HWND hWnd, UINT bWndForm){

	// полупрозрачное
	if(bWndForm==1){
		SetWindowPos(hWnd, NULL, NULL, NULL, INITIAL_WND_W, INITIAL_WND_H, SWP_NOMOVE | SWP_SHOWWINDOW);
//		verify(SetLayeredWindowAttributes(hWnd, 0x0, 100, LWA_ALPHA));
		verify(SetLayeredWindowAttributes(hWnd, RGB(0xff, 0xff, 0xff), 0x80, LWA_ALPHA));

	}

	// вырезанный "RSDN"
	if(bWndForm==2){
		SetWindowPos(hWnd, NULL, NULL, NULL, INITIAL_WND_W, INITIAL_WND_H, SWP_NOMOVE | SWP_SHOWWINDOW);
		verify(SetLayeredWindowAttributes(hWnd, 0x0, 0, LWA_COLORKEY));
	}

	// вырезанный !"RSDN"
	if(bWndForm==3){
		SetWindowPos(hWnd, NULL, NULL, NULL, INITIAL_WND_W, INITIAL_WND_H, SWP_NOMOVE | SWP_SHOWWINDOW);
		SetLayeredWindowAttributes(hWnd, GetSysColor((IsThemeActive() ? COLOR_WINDOW : COLOR_3DFACE)), 0, LWA_COLORKEY);
	}

	// форма окна из smoke.bmp файла с прозрачным черным цветом
	if(bWndForm==4){
		LoadBMP(hWnd, _T("smoke.bmp"));
		SetWindowPos(hWnd, NULL, NULL, NULL, bmp_info.bmiHeader.biWidth, bmp_info.bmiHeader.biHeight, SWP_NOMOVE | SWP_SHOWWINDOW);
		SetLayeredWindowAttributes(hWnd, 0x0, 0, LWA_COLORKEY);
	}

	// "змейка" рисуем в памяти (в той же где и содержимое картинки находилось), 
	// а потом выводим в окно (для устранения мерцания)
	if(bWndForm==5){
		// устанавливаем необходимые значения для bmp_info
		memset(&bmp_info, 0x0, sizeof(bmp_info));
		
		bmp_info.bmiHeader.biSize = sizeof(bmp_info);
		bmp_info.bmiHeader.biBitCount=8;	// немного другой формат
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


	// при перетаскивании
	if(bWndForm==7){
		SetWindowPos(hWnd, NULL, NULL, NULL, bmp_info.bmiHeader.biWidth, bmp_info.bmiHeader.biHeight, SWP_NOMOVE | SWP_SHOWWINDOW);
		SetLayeredWindowAttributes(hWnd, 0x0, 0, LWA_COLORKEY);
	}
	
	WndProc_OnPaint(hWnd);
}

// перерисовка окна
void WndProc_OnPaint(HWND hWnd){
	HDC hdc; PAINTSTRUCT ps;
	hdc = BeginPaint(hWnd, &ps);
	assert(hdc);
	
	// вывод текста "RSDN"
	if(bWndForm==3||bWndForm==2||bWndForm==1||bWndForm==0){
		// текущее положение окна (клиентской части)
		RECT r; GetClientRect(hWnd, &r);

		// создаем шрифт
		HFONT font = CreateFont(90, 30, 0, 0, 150, 0, 0, 0, ANSI_CHARSET, OUT_DEVICE_PRECIS, 
			CLIP_DEFAULT_PRECIS, ANTIALIASED_QUALITY, DEFAULT_PITCH, _T("Arial"));
		assert(font);
		// выбираем шрифт
		SelectObject(hdc, font);
		// и удаляем
		verify(DeleteFont(font));

		// пишем в соответсвующем режиме
		SetBkMode(hdc, TRANSPARENT);
		DrawText(hdc, _T("RSDN"), 4, &r, DT_VCENTER | DT_CENTER | DT_SINGLELINE );
	}

	// рисуем в памяти... синусойду ("змейка")
	if(bWndForm==5){
 		memset(bmp_cnt, 0x0, INITIAL_WND_W*INITIAL_WND_H);

		for(unsigned int i(0); i<INITIAL_WND_H; i++){
			/*память*/bmp_cnt[INITIAL_WND_W*(/*центрирование*/INITIAL_WND_H/2+
			(unsigned int)(/*высота (масштабирование)*/INITIAL_WND_H*0.12*sin/*змейка*/(i*fabs(DXPS)+dx/*смещение*/)))+i]=127;
		}

		// "биты" -> "устройство"
		SetDIBitsToDevice(hdc, 0, 0, INITIAL_WND_W, INITIAL_WND_H, 
			0, 0, 0, INITIAL_WND_H, bmp_cnt, &bmp_info, DIB_PAL_COLORS);
	}

	// отображение содержимого файла
	if(bWndForm==4||bWndForm==7){
		SetDIBitsToDevice(hdc, 0, 0, bmp_info.bmiHeader.biWidth, bmp_info.bmiHeader.biHeight, 
			0, 0, 0, bmp_info.bmiHeader.biHeight, bmp_cnt, &bmp_info, DIB_PAL_COLORS);
	}

	// можно и средствами GDI+
	if(bWndForm==6){
		RECT rt;
		GetClientRect(hWnd, &rt);
		Rect rect (rt.left, rt.top, rt.right, rt.right);

		// Объявляем графический объект
		Graphics graphics(hdc); 

		// эллипс
		LinearGradientBrush linGrBrush1(rect, 
			Color(0, 0, 0, 0), 
			Color(255, 0, 0, 255),
            LinearGradientModeHorizontal);
		
		graphics.FillEllipse(&linGrBrush1, rt.left, rt.top, rt.right, rt.bottom);
	}

	// конец  рисованию
	EndPaint(hWnd, &ps);
}