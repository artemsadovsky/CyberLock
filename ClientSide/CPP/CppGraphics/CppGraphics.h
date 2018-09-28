// CppGraphics.h

#pragma once
// Windows

using namespace System;

namespace CppGraphics {

	public ref class Drawer
	{
	public:
		Drawer();
		~Drawer();
		int Get(void);
		void Draw(void);
	private:
		int number;
		//HWND win_pw;        // Хэндл окна PW
		//HWND frame;         // Хэндл нашего фрейма
		int frLeft;     // Левый отступ фрейма от границы окна-родителя
		int frTop;      // Отступ сверху от границы окна-родителя
		int frWidth;    // Ширина фрейма
		int frHeight;   // Высота фрейма
	};
}
